// Upgrade NOTE: replaced 'PositionFog()' with multiply of UNITY_MATRIX_MVP by position
// Upgrade NOTE: replaced 'V2F_POS_FOG' with 'float4 pos : SV_POSITION'

Shader "FX/WaterPlane (reflective&refractive)" { 
Properties {
	_WaveScale ("Wave scale", Range (0.02,0.15)) = .07
	_ReflDistort ("Reflection distort", Range (0,1.5)) = 0.5
	_RefrDistort ("Refraction distort", Range (0,1.5)) = 0.4
	_RefrColor ("Refraction color", COLOR)  = ( .34, .85, .92, 1)
	_horizonColor ("Fallback horizon color", COLOR)  = ( .172 , .463 , .435 , 1)
	_ReflectionTex ("Environment Reflection", 2D) = ""
	_RefractionTex ("Environment Refraction", 2D) = ""
	_Fresnel ("Fresnel (A) ", 2D) = ""
	_BumpMap ("Bumpmap (RGB) ", 2D) = ""
	_ColorControl ("Fallback color ramp (RGB) fresnel (A) ", 2D) = ""
	_ColorControlCube ("Fallback color cube (RGB) fresnel (A)", Cube) = "" { TexGen CubeReflect }
	_MainTex ("Fallback texture", 2D) = ""
}

// -----------------------------------------------------------
// ARB fragment program

Subshader { 
	Pass {
		
CGPROGRAM
// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct appdata members vertex,normal)
#pragma exclude_renderers d3d11 xbox360
// profiles arbfp1
// fragment frag
// vertex vert
// fragmentoption ARB_precision_hint_fastest 
// fragmentoption ARB_fog_exp2

#include "UnityCG.cginc"

// Wave speed (map1 x,y; map2 x,y)
uniform float4 WaveSpeed = float4(19,9,-16,-7);
uniform float _WaveScale;
uniform float _ReflDistort;
uniform float _RefrDistort;

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

v2f vert(appdata v)
{
	v2f o;
	o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
	
	// scroll bump waves
	float4 temp;
	temp.xyzw = (v.vertex.xzxz + _Time.x * WaveSpeed.xyzw) * _WaveScale;
	o.bumpuv[0] = temp.xy * float2(.4, .45);
	o.bumpuv[1] = temp.wz;
	
	// object space view direction (will normalize per pixel)
	o.viewDir.xzy = ObjSpaceViewDir(v.vertex);
	
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
sampler2D _RefractionTex : register(s1);
sampler2D _BumpMap : register(s2);
sampler2D _Fresnel : register(s3);
uniform float4 _RefrColor;

half4 frag( v2f i ) : COLOR
{
	i.viewDir = normalize(i.viewDir);
	
	// combine two scrolling bumpmaps into one
	half3 bump1 = tex2D( _BumpMap, i.bumpuv[0] ).rgb;
	half3 bump2 = tex2D( _BumpMap, i.bumpuv[1] ).rgb;
	half3 bump = bump1 + bump2 - 1;
	
	// lookup fresnel
	half fresnelFac = dot( i.viewDir, bump );
	half fresnel = tex2D( _Fresnel, float2(fresnelFac,fresnelFac) ).a;
	
	// perturb reflection/refraction UVs by bumpmap, and lookup colors
	float4 uv1 = i.ref; uv1.xyz += bump * _ReflDistort;
	float4 uv2 = i.ref; uv2.xyz -= bump * _RefrDistort;
	half4 refl = tex2Dproj( _ReflectionTex, uv1 );
	half4 refr = tex2Dproj( _RefractionTex, uv2 ) * _RefrColor;
	
	// final color is between refracted and reflected based on fresnel	
	half4 color;
	color = lerp( refr, refl, fresnel );
	return color;
}
ENDCG

			SetTexture [_ReflectionTex] {}
			SetTexture [_RefractionTex] {}
			SetTexture [_BumpMap] {}
			SetTexture [_Fresnel] {}
  		}
	}

	Fallback "FX/WaterPlane (reflective)", 1
}
