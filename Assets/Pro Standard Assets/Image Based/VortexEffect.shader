// Upgrade NOTE: replaced 'glstate.matrix.mvp' with 'UNITY_MATRIX_MVP'
// Upgrade NOTE: replaced 'glstate.matrix.texture[0]' with 'UNITY_MATRIX_TEXTURE0'
// Upgrade NOTE: replaced 'samplerRECT' with 'sampler2D'
// Upgrade NOTE: replaced 'texRECT' with 'tex2D'

Shader "Hidden/Twist Effect" {
Properties {
	_MainTex ("Base (RGB)", RECT) = "white" {}
}

Category
{
	SubShader
	{
		Pass
		{
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

CGPROGRAM
// profiles arbfp1
// vertex vert
// fragment frag
// fragmentoption ARB_fog_exp2
// fragmentoption ARB_precision_hint_fastest 

#include "UnityCG.cginc"

uniform sampler2D _MainTex : register(s0);
uniform float _Angle;
uniform float4 _Center;
uniform float _Radius;

v2f_img vert (appdata_img v)
{
	v2f_img o;
	o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
	o.uv = MultiplyUV (UNITY_MATRIX_TEXTURE0, v.texcoord) - _Center.xy;
	return o;
}

float4 frag (v2f_img i) : COLOR
{
	float2 offset = i.uv;
	float angle = 1.0 - (length(offset) / _Radius);
	angle = max (0, angle);
	angle = angle * angle * _Angle;
	float cosLength, sinLength;
	sincos (angle, sinLength, cosLength);
	
	float2 uv;
	uv.x = cosLength * offset[0] - sinLength * offset[1];
	uv.y = sinLength * offset[0] + cosLength * offset[1];
	uv += _Center.xy;
	
	return tex2D(_MainTex, uv);
}
ENDCG

			SetTexture [_MainTex] {}
		}
	}
}

Fallback off

}