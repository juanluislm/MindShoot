// Upgrade NOTE: replaced 'glstate.matrix.mvp' with 'UNITY_MATRIX_MVP'
// Upgrade NOTE: replaced 'glstate.matrix.texture[0]' with 'UNITY_MATRIX_TEXTURE0'
// Upgrade NOTE: replaced 'samplerRECT' with 'sampler2D'
// Upgrade NOTE: replaced 'texRECT' with 'tex2D'

Shader "Hidden/Glow Downsample" {

Properties {
	_Color ("Color", color) = (1,1,1,0)
}

CGINCLUDE
#include "UnityCG.cginc"

struct v2f {
	float4 pos : POSITION;
	float4 uv[4] : TEXCOORD0;
};

v2f vert (appdata_img v)
{
	v2f o;
	o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
	float4 uv;
	uv.xy = MultiplyUV (UNITY_MATRIX_TEXTURE0, v.texcoord);
	uv.zw = 0;
	float off = 1.0;
	o.uv[0] = uv + float4(-off,-off,0,1);
	o.uv[1] = uv + float4( off,-off,0,1);
	o.uv[2] = uv + float4( off, off,0,1);
	o.uv[3] = uv + float4(-off, off,0,1);
	return o;
}
ENDCG


Category {
	ZTest Always Cull Off ZWrite Off
	
	// -----------------------------------------------------------
	// ARB fragment program
	
	Subshader { 
		Pass {
		
CGPROGRAM
// profiles arbfp1
// vertex vert
// fragment frag
// fragmentoption ARB_precision_hint_fastest

sampler2D __RenderTex : register(s0);
float4 _Color;

half4 frag( v2f i ) : COLOR
{
	half4 c;
	c  = tex2D( __RenderTex, i.uv[0].xy );
	c += tex2D( __RenderTex, i.uv[1].xy );
	c += tex2D( __RenderTex, i.uv[2].xy );
	c += tex2D( __RenderTex, i.uv[3].xy );
	c /= 4;
	c.rgb *= _Color.rgb;
	c.rgb *= (c.a + _Color.a);
	c.a = 0;
	return c;
}
ENDCG

			SetTexture [__RenderTex] {}
			SetTexture [__RenderTex] {}
			SetTexture [__RenderTex] {}
			SetTexture [__RenderTex] {}
		}
	}
			
	// -----------------------------------------------------------
	// Radeon 9000
	
	Subshader {
		Pass {


CGPROGRAM
// vertex vert
// use the same vertex program as in FP path
ENDCG

			
			// average 2x2 samples
			SetTexture [__RenderTex] {constantColor (0,0,0,0.25) combine texture * constant alpha}
			SetTexture [__RenderTex] {constantColor (0,0,0,0.25) combine texture * constant + previous}
			SetTexture [__RenderTex] {constantColor (0,0,0,0.25) combine texture * constant + previous}
			SetTexture [__RenderTex] {constantColor (0,0,0,0.25) combine texture * constant + previous}
			// apply glow tint and add additional glow
			SetTexture [__RenderTex] {constantColor[_Color] combine previous * constant, previous + constant}
			SetTexture [__RenderTex] {constantColor (0,0,0,0) combine previous * previous alpha, constant}
		}
	}
}

Fallback off

}
