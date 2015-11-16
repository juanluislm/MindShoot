// Upgrade NOTE: replaced 'samplerRECT' with 'sampler2D'
// Upgrade NOTE: replaced 'texRECT' with 'tex2D'

Shader "Hidden/Edge Detect X" {
Properties {
	_MainTex ("Base (RGB)", RECT) = "white" {}
	_Treshold ("Treshold", Float) = 0.2
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
uniform float _Treshold;


half4 frag (v2f_img i) : COLOR
{
	half4 original = tex2D(_MainTex, i.uv);

	// a very simple cross gradient filter
	half3 p1 = original.rgb;
	half3 p2 = tex2D( _MainTex, i.uv + float2(-1,-1) ).rgb;
	half3 p3 = tex2D( _MainTex, i.uv + float2(+1,-1) ).rgb;
	
	half4 diff;
	diff.rgb = p1*2 - p2 - p3;
	diff.a = 0;
	half len = length(diff);
	
	float4 output;
	if( len >= _Treshold )
		output = float4(0, 0, 0, original.a);
	else
		output = original;
	return output;
}
ENDCG
			SetTexture [_MainTex] {}
		}
	}
}

Fallback off

}