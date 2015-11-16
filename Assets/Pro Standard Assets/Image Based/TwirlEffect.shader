// Upgrade NOTE: replaced 'samplerRECT' with 'sampler2D'
// Upgrade NOTE: replaced 'texRECT' with 'tex2D'

Shader "Hidden/Twirt Effect Shader" {
Properties {
	_MainTex ("Base (RGB)", RECT) = "white" {}
	_Radius ("Radius", float) = 100
	_Center ("Center", Color) = (1,1,1,1)
}

Category {
	SubShader {
		Pass {
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }
				
CGPROGRAM
// profiles arbfp1
// fragment frag
// fragmentoption ARB_fog_exp2
// fragmentoption ARB_precision_hint_fastest 
// vertex vert_img
#include "UnityCG.cginc"

uniform sampler2D _MainTex;
uniform float4 _Center;
uniform float _Radius;
uniform float4x4 _RotationMatrix;

float4 frag (v2f_img i) : COLOR
{
	float2 offset = i.uv - _Center.xy;
	float2 distortedOffset = MultiplyUV (_RotationMatrix, offset.xy);
	float t = min (1, length(offset) / _Radius);
	
	offset = lerp (distortedOffset, offset, t);
	offset += _Center.xy;
	
	return tex2D(_MainTex, offset);
}
ENDCG

			SetTexture [_MainTex] {}
		}
	}
}

Fallback off

}
