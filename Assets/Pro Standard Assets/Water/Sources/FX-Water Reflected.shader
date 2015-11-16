// Upgrade NOTE: replaced 'PositionFog()' with multiply of UNITY_MATRIX_MVP by position
// Upgrade NOTE: replaced 'V2F_POS_FOG' with 'float4 pos : SV_POSITION'

Shader "FX/WaterPlane (reflective)" {
Properties {
	_WaveScale ("Wave scale", Range (0.02,0.15)) = .07
	_ReflDistort ("Reflection distort", Range (0,1.5)) = 0.5
	_horizonColor ("Fallback horizon color", COLOR)  = ( .172 , .463 , .435 , 1)
	_ReflectionTex ("Environment Reflection", 2D) = ""
	_ColorControl ("Color ramp (RGB) fresnel (A) ", 2D) = ""
	_BumpMap ("Waves Bumpmap (RGB) ", 2D) = ""
	_ColorControlCube ("Fallback color cube (RGB) fresnel (A)", Cube) = "" { TexGen CubeReflect }
	_MainTex ("Fallback texture", 2D) = ""
}  

CGINCLUDE
// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct appdata members vertex,normal)
#pragma exclude_renderers d3d11 xbox360
// -----------------------------------------------------------
// This section is included in all program sections below

#include "UnityCG.cginc"

// Wave speed (map1 x,y; map2 x,y)
uniform float4 WaveSpeed = float4(19,9,-16,-7);
uniform float _WaveScale;
uniform float _ReflDistort;

struct appdata {
	float4 vertex; 
	float3 normal; 
};

struct v2f {
	float4 pos : SV_POSITION;
	float4 ref : TEXCOORD0;
	float2 bumpuv[2] : TEXCOORD1;
	float3 viewDir : TEXCOORD3;
}; 
ENDCG

// -----------------------------------------------------------
// ARB fragment program

Subshader {
	Pass {
	
CGPROGRAM
// profiles arbfp1
// fragment frag
// vertex vert
// fragmentoption ARB_precision_hint_fastest 
// fragmentoption ARB_fog_exp2

v2f vert(appdata v)
{
	v2f o;
	o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
	
	// scroll bump waves
	float4 temp;
	temp.xyzw = (v.vertex.xzxz + _Time.x * WaveSpeed.xyzw) * _WaveScale;
	o.bumpuv[0] = temp.xy * float2(.4, .45);
	o.bumpuv[1] = temp.wz;
	
	// object space view direction
	o.viewDir.xzy = normalize( ObjSpaceViewDir(v.vertex) );
	
	// calculate the reflection vector
	float4x4 mat= float4x4 (
		.5, 0, 0,.5,
		 0,.5, 0,.5,
		 0, 0,.5,.5,
		 0, 0, 0, 1
	);	
	o.ref = mul (mat, o.pos);
	
	return o;
}

sampler2D _ReflectionTex : register(s0);
sampler2D _BumpMap : register(s1);
sampler2D _ColorControl : register(s2);

half4 frag( v2f i ) : COLOR
{
	// combine two scrolling bumpmaps into one
	half3 bump1 = tex2D( _BumpMap, i.bumpuv[0] ).rgb;
	half3 bump2 = tex2D( _BumpMap, i.bumpuv[1] ).rgb;
	half3 bump = bump1 + bump2 - 1;
	
	// lookup "refracted" water color and fresnel
	half fresnel = dot( i.viewDir, bump );
	half4 water = tex2D( _ColorControl, float2(fresnel,fresnel) );
	
	// perturb reflection UVs by bumpmap, and lookup reflected color
	float4 uv = i.ref; uv.xyz += bump * _ReflDistort;	
	half4 refl = tex2Dproj( _ReflectionTex, uv );
	
	// final color is between "refracted" and reflected based on fresnel
	half4 color;
	color.rgb = lerp( water.rgb, refl.rgb, water.a );
	color.a = refl.a * water.a;
	
	return color;
}
ENDCG

		SetTexture [_ReflectionTex] {}
		SetTexture [_BumpMap] {}
		SetTexture [_ColorControl] {}
	}
}

// -----------------------------------------------------------
// Radeon 9000

#warning Upgrade NOTE: SubShader commented out because of manual shader assembly
/*Subshader {
	Pass {
	
CGPROGRAM
// vertex vert

v2f vert(appdata v)
{
	v2f o;
	o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
	
	// scroll bump waves
	float4 temp;
	temp.xyzw = (v.vertex.xzxz + _Time.x * WaveSpeed.xyzw) * _WaveScale;
	o.bumpuv[0] = temp.xy * float2(.4, .45);
	o.bumpuv[1] = temp.wz;
	
	// object space view direction
	o.viewDir.xzy = normalize( ObjSpaceViewDir(v.vertex) );
	
	// calculate the reflection vector
	float4x4 mat= float4x4 (
		.5, 0, 0,.5,
		 0,.5, 0,.5,
		 0, 0,.5,.5,
		 0, 0, 0, 1
	);	
	o.ref = mul (mat, o.pos);
	
	return o;
}
ENDCG

Program "" {
SubProgram {
	Local 0, ([_ReflDistort],0,0,0)

"!!ATIfs1.0
StartConstants;
	CONSTANT c0 = program.local[0];
EndConstants;

StartPrelimPass;
	PassTexCoord r0, t0.stq_dq; # reflection vector
	SampleMap r1, t1.str;	# bump1
	SampleMap r2, t2.str;	# bump2
	PassTexCoord r3, t3.str;
	
	ADD r2.half, r1.bias, r2.bias;	# bump = bump1 + bump2 - 1
	DOT3 r3, r2.2x, r3;			# fresnel: dot (bump, viewer-pos)
	# add less offset because it's purely screenspace; big ones look bad
	MAD r0.rg, r2, c0.r, r0;	# uv += bump * strength; add less because it's not perspective
EndPass;

StartOutputPass;
	SampleMap r0, r0.str;		# reflection color
 	SampleMap r3, r3.str;		# water color/fresnel

	LERP r0.rgb, r3.a, r0, r3;	# between water and reflected based on fresnel
	MUL r0.a, r0.a, r3.a;
EndPass;
" 
}
}
		SetTexture [_ReflectionTex] {}
		SetTexture [_BumpMap] {}
		SetTexture [_BumpMap] {}
		SetTexture [_ColorControl] {}
	}
}*/

// -----------------------------------------------------------
//  Fallback to non-reflective for older cards or Unity non-Pros

Fallback "FX/WaterPlane", 1

}
