// Upgrade NOTE: replaced 'samplerRECT' with 'sampler2D'
// Upgrade NOTE: replaced 'texRECT' with 'tex2D'

Shader "Hidden/Sepiatone Effect" {
Properties {
	_MainTex ("Base (RGB)", RECT) = "white" {}
}

Category {
	SubShader {
		Pass {
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }
				
CGPROGRAM
// profiles arbfp1
// vertex vert_img
// fragment frag
// fragmentoption ARB_fog_exp2
// fragmentoption ARB_precision_hint_fastest 
#include "UnityCG.cginc"

uniform sampler2D _MainTex : register(s0);

float4 frag (v2f_img i) : COLOR
{	
	float4 original = tex2D(_MainTex, i.uv);
	
	// get intensity value (Y part of YIQ color space)
	float Y = dot (float3(0.299, 0.587, 0.114), original.rgb);

	// Convert to Sepia Tone by adding constant
	float4 sepiaConvert = float4 (0.191, -0.054, -0.221, 0.0);
	float4 output = sepiaConvert + Y;
	output.a = original.a;
	
	return output;
}
ENDCG
			SetTexture [_MainTex] {}
		}
	}
}

Fallback off

}