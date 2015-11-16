// Upgrade NOTE: replaced 'glstate.matrix.mvp' with 'UNITY_MATRIX_MVP'

// Glass with fresnel reflection & transparency...
// Tested on: ATI Radeon Mobility
CGINCLUDE
// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct appdata members vertex,normal)
#pragma exclude_renderers d3d11 xbox360

uniform float4 _Color;
uniform float4 _horizonColor = {0.615,0.796,0.876,1};

// Wave speed (map1 x, y, map2 x, y)
uniform float4 BumpParm = float4(1,1,1,1);
uniform float _bScale;
uniform float _BumpPeturb;

struct appdata {
	float4 vertex; 
	float3 normal; 
};

ENDCG

SHADER "FX/WaterPlane" { 
	Properties {
		_Color ("Water color", COLOR)  = ( .172 , .463 , .435 , 1)
		_horizonColor ("horizon color", COLOR)  = ( .172 , .463 , .435 , 1)
		_bScale ("Bump scale", Range (0,1)) = .5
		_BumpPeturb ("Bump strength", Range (0,300)) = 200
		_BumpPeturb2 ("Bump strength2", Range (0,1)) = 1
		_MainTex ("Main texture", 2D) = ""
		_ColorControl ("Color ramp (RGB) fresnel (A) ", 2D) = ""
		_ColorControlCube ("Color cube (RGB) fresnel (A) ", Cube) = "" 
		_BumpMap ("Bumpmap (RGB) ", 2D) = ""
	}  
	// -----------------------------------------------------------
	// ARB_FP
	// -----------------------------------------------------------
	#warning Upgrade NOTE: SubShader commented out because of manual shader assembly
/*SUBSHADER { 
		Pass { 

CGPROGRAM
// vertex vert
#include "UnityCG.cginc"
struct v2f {
	float4 pos : POSITION;
	float4 ref : TEXCOORD0;
	float4 bumpuv[2] : TEXCOORD1;
	float3 normal : TEXCOORD3;
	float fog : FOGC; 
}; 


v2f vert(appdata v) {
	v2f o;
	// HACK FOR OCEAN WATER: fade out waves...
	float4 s;

	o.pos = mul(UNITY_MATRIX_MVP, v.vertex.xyzw);
	o.fog = o.pos.z;

	// wave the bumpuvs back & forth
	// ------------------------------------------------
	float4 temp;
	temp.xyzw = v.vertex.xzxz  * _bScale;   // twirled base coords
	temp.xyzw += _Time.x * BumpParm.xyzw;		// scroll them
	o.bumpuv[0] = temp * float4 (.4, .5, 1, 1); 			// scale & assign
	o.bumpuv[1].xy = temp.wz;

	// Get the fresnel
	// ------------------------------------------------
	o.normal.xzy = normalize (_ObjectSpaceCameraPos.xyz - v.vertex.xyz);

	// Calculate the reflection vectors
	// ------------------------------------------------
	// basics: Get the screenspace UV coords
	// TODO: normal-based pertubation

	float4x4 mat= float4x4 (
		128,0,0,128,// - _BumpPeturb / 4,
		0,-128,0,128,// - _BumpPeturb / 4,
		0,0,.5,.5,
		0,0,0,1
	);

	o.ref = mul (mat, o.pos);

	return o;
}
ENDCG
Program "" {
SubProgram {
  Local 0, ([_BumpPeturb],0,0,0)
  Local 1, [_horizonColor]

  "!!ARBfp1.0
OPTION ARB_precision_hint_fastest;
OPTION ARB_fog_exp2;
PARAM c[2] = { program.local[0..1] };
PARAM c3 = { 1, 0,0,0 };

TEMP R0;
TEMP R1;
TEX R0, fragment.texcoord[1], texture[1], 2D;
TEX R1, fragment.texcoord[2], texture[2], 2D;
ADD R0, R0, R1;
SUB R0, R0, c3.xxxx;			# totalbump = bump1 + bump2 - 1

DP3 R1, fragment.texcoord[3], R0;	# fresnel = dot (viewer-pos, totalbump)

#MAD R0.xyzw, R0, c[0].xxxy, fragment.texcoord[0];	# uv = _BumpAmt * totalbump + uv

TEX R1, R1, texture[3], 2D;		# lookup water color & reflection strength

#TXP R0, R0, texture[0], RECT;	# lookup reflection
LRP result.color.rgb, R1.a, c[1], R1;
#LRP result.color.rgb, R1.a, R0, R1;
MUL result.color.a, R0.a, R1.a;
END
  	"
}}

			SetTexture [_BumpMap] { combine texture }
			SetTexture [_BumpMap] {combine texture, previous alpha * texture alpha  }
			SetTexture [_BumpMap] {combine texture, previous alpha * texture alpha  }
			SetTexture [_ColorControl] {combine texture, previous alpha * texture alpha  }
  		}
	}*/

	//  ------------------------------------------------------------
	// R9000
	//  ------------------------------------------------------------
	#warning Upgrade NOTE: SubShader commented out because of manual shader assembly
/*SUBSHADER { 
		Pass { 
CGPROGRAM
// vertex vert
#include "UnityCG.cginc"
struct v2f {
	float4 pos : POSITION;
	float4 ref : TEXCOORD0;
	float4 bumpuv[2] : TEXCOORD1;
	float3 normal : TEXCOORD3;
	float fog : FOGC; 
}; 


v2f vert(appdata v) {
	v2f o;
	// HACK FOR OCEAN WATER: fade out waves...
	float4 s;

	o.pos = mul(UNITY_MATRIX_MVP, v.vertex.xyzw);
	o.fog = o.pos.z;

	// wave the bumpuvs back & forth
	// ------------------------------------------------
	float4 temp;
	temp.xyzw = v.vertex.xzxz  * _bScale;   // twirled base coords
	temp.xyzw += _Time.x * BumpParm.xyzw;		// scroll them
	o.bumpuv[0] = temp * float4 (.4, .5, 1, 1); 			// scale & assign
	o.bumpuv[1].xy = temp.wz;

	// Get the fresnel
	// ------------------------------------------------
//	float fresfac = 1 - normalize ((_ObjectSpaceCameraPos - v.vertex.xyz)).y;
//	o.fresnel = lerp (_Color, _horizonColor, fresfac * fresfac);

	o.normal.xzy = normalize (_ObjectSpaceCameraPos.xyz - v.vertex.xyz);

	// Calculate the reflection vectors
	// ------------------------------------------------
	// basics: Get the screenspace UV coords
	// TODO: normal-based pertubation

	float4x4 mat= float4x4 (
		4,0,0,4,
		0,-4,0,4,
		0,0,.5,.5,
		0,0,0,1
	);
	o.ref = mul (mat, o.pos);

	return o;
}
ENDCG

Program "" {
SubProgram {
  Local 0, ([_BumpPeturb2],0,0,0)
  Local 1, [_horizonColor]

"!!ATIfs1.0
StartConstants;
	CONSTANT c0 = program.local[0];
	CONSTANT c1 = program.local[1];
	CONSTANT c2 = { 1, 0.0, 0.03225, 1 };
EndConstants;

StartPrelimPass;
	PassTexCoord r0, t0.stq_dq;
	SampleMap r1, t1.str;	# normalMap1
	SampleMap r2, t2.str;	# normalMap2
	PassTexCoord r3, t3.str;
	# r0.rg = Reflection coord
	ADD r2, r1.bias, r2.bias;		# totalbump = bump1 + bump2

	DOT3 r3, r2, r3;				# r4.a = fresnel  [ dot (totalbump, viewer-pos)
	
#	MAD r0.rg, r2, c0.r, r0;		# uv += totalbump

#	MOV r0.b, c2.b;				# use perspective divide to scale refmap up to size
	
EndPass;

StartOutputPass;
 	SampleMap r3, r3.str;

	LERP r0.rgb, r3.a, c1, r3;	# 	Fade in reflection
	MUL r0.a, r0.a, r3.a;		# Make glossed reflections glow
EndPass;
" 
}
}
			SetTexture [_BumpMap] { combine texture }
			SetTexture [_BumpMap] {combine texture, previous alpha * texture alpha  }
			SetTexture [_BumpMap] {combine texture, previous alpha * texture alpha  }
			SetTexture [_ColorControl] {combine texture, previous alpha * texture alpha  }
  		}
	}*/
	//  ------------------------------------------------------------
	// R7k, Geforce3,4
	//  ------------------------------------------------------------
	SUBSHADER {
		Pass { 
			SetTexture [_ColorControlCube] {combine texture   Matrix [_Reflection] }
			SetTexture [_BumpMap] {
				combine previous, previous +- texture
			}
			SetTexture [_BumpMap] { 
				ConstantColor [_horizonColor]
				combine constant lerp (previous) previous, previous * texture DOUBLE
			}
      		}
	}
	//  ------------------------------------------------------------
	// Geforce MX 
	//  ------------------------------------------------------------
	SUBSHADER { 
		Pass { 
			SetTexture [_ColorControlCube] {combine texture   Matrix [_Reflection] }
			SetTexture [_BumpMap] {
				combine previous, previous +- texture
			}
      		}
		Pass { 
			Blend DstAlpha OneMinusDstAlpha
			Color [_horizonColor]
      		}
	}
	//  ------------------------------------------------------------
	// Rage128 
	//  ------------------------------------------------------------
	SUBSHADER { 
		Pass { 
			SetTexture [_MainTex] {
				constantColor (.1,.1,.1, 1)
				combine texture * constant alpha
			}
			SetTexture [_BumpMap] {
				combine previous * texture alpha
			}
      		}
	}
}
