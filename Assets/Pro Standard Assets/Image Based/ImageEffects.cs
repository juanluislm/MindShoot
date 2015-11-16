using UnityEngine;
using System;

/// Blendning modes use by the UnityImage.Blit functions.
public enum BlendMode {
	Copy,			
	Multiply, 
	MultiplyDouble, 
	Add, 
	AddSmoooth, 
	Blend
}

/// A Utility class for performing various image based rendering tasks.
[AddComponentMenu("")]
public class ImageEffects {
	static Material[] m_BlitMaterials = {null, null, null, null, null, null};
	
	static public Material GetBlitMaterial (BlendMode mode) {
		int index = (int)mode;
		
		if (m_BlitMaterials[index] != null)
			return m_BlitMaterials[index];
			
		// Blit Copy Material
		m_BlitMaterials[0] = new Material (
			"Shader \"BlitCopy\" {\n"	+
			"	SubShader { Pass {\n" +
			" 		ZTest Always Cull Off ZWrite Off\n" +
			"		SetTexture [__RenderTex] { combine texture}"	+
			"	}}\n"	 +
			"}"
		);
		// Blit Multiply
		m_BlitMaterials[1] = new Material (
			"Shader \"BlitMultiply\" {\n"	+
			"	SubShader { Pass {\n" +
			"		Blend DstColor Zero\n" + 
			" 		ZTest Always Cull Off ZWrite Off\n" +
			"		SetTexture [__RenderTex] { combine texture }"	+
			"	}}\n"	 +
			"}"
		);
		// Blit Multiply 2X
		m_BlitMaterials[2] = new Material (
			"Shader \"BlitMultiplyDouble\" {\n"	+
			"	SubShader { Pass {\n" +
			"		Blend DstColor SrcColor\n" + 
			" 		ZTest Always Cull Off ZWrite Off\n" +
			"		SetTexture [__RenderTex] { combine texture }"	+
			"	}}\n"	 +
			"}"
		);
		// Blit Add
		m_BlitMaterials[3] = new Material (
			"Shader \"BlitAdd\" {\n"	+
			"	SubShader { Pass {\n" +
			"		Blend One One\n" + 
			" 		ZTest Always Cull Off ZWrite Off\n" +
			"		SetTexture [__RenderTex] { combine texture }"	+
			"	}}\n"	 +
			"}"
		);
		// Blit AddSmooth
		m_BlitMaterials[4] = new Material (
			"Shader \"BlitAddSmooth\" {\n"	+
			"	SubShader { Pass {\n" +
			"		Blend OneMinusDstColor One\n" + 
			" 		ZTest Always Cull Off ZWrite Off\n" +
			"		SetTexture [__RenderTex] { combine texture }"	+
			"	}}\n"	 +
			"}"
		);
		// Blit Blend
		m_BlitMaterials[5] = new Material (
			"Shader \"BlitBlend\" {\n"	+
			"	SubShader { Pass {\n" +
			"		Blend SrcAlpha OneMinusSrcAlpha\n" + 
			" 		ZTest Always Cull Off ZWrite Off\n" +
			"		SetTexture [__RenderTex] { combine texture }"	+
			"	}}\n"	 +
			"}"
		);
		return m_BlitMaterials[index];
	}
	
	/// DownSample from 1 texture to another. 
	/// This function will downsample from /source/ to /dest/. It ensures that /dest/ gets properly
	/// sampled. 
	///
	/// This function is typically used as part of a blur. First you downsample, then you blur the 
	/// downsampled version, then you blit the blurred, downsampled image on to the main display.
	/// The stretching done as part of this process is not visible because of the blur.
	/// The contents of
	public static void DownSample (RenderTexture source, RenderTexture dest) {
		float w = source.width, h = source.height;
		float wFactor = w / dest.width, hFactor = h / dest.height;
		float maxFactor = wFactor > hFactor ? wFactor : hFactor;
		
		if (maxFactor <= 1) {	// If the destination texture is larger than the source, we just blit. 
			// Should log a performance warning, I guess.
			Blit (source, dest);
			return;
		} 
		
		// Ok - we only need to blit one time.
		if (maxFactor >= .5) {
			Blit (source,dest);
			return;
		}
		
		// Here we go into pingpong mode.
		do {
			
		} while (w > dest.width && h > dest.height);
	}
		
	/// Copies one render texture onto another.
	///  This function copies /source/ onto /dest/, optionally using a custom blend mode.
	/// If /blendMode/ is left out, the default operation is simply to copy one texture on to another.
	/// This function will copy the whole source texture on to the whole destination texture. If the sizes differ, 
	/// the image in the source texture will get stretched to fit.
	/// The source and destination textures cannot be the same.
	public static void Blit (RenderTexture source, RenderTexture dest, BlendMode blendMode) {
		Blit (source, new Rect (0,0,1,1), dest, new Rect (0,0,1,1), blendMode);
	}
	public static void Blit (RenderTexture source, RenderTexture dest) {		
		Blit (source, dest, BlendMode.Copy);
	}

	/// Copies one render texture onto another.
	public static void Blit (RenderTexture source, Rect sourceRect, RenderTexture dest, Rect destRect, BlendMode blendMode) {
		// Make the destination texture the target for all rendering
		RenderTexture.active = dest;  		
		// Assign the source texture to a property from a shader
		source.SetGlobalShaderProperty ("__RenderTex");	
		// Set up the simple Matrix
		GL.PushMatrix ();
		GL.LoadOrtho ();
		Material blitMaterial = GetBlitMaterial(blendMode);
		for (int i = 0; i < blitMaterial.passCount; i++) {
			blitMaterial.SetPass (i);
			DrawGrid(1, 1);
		}
		GL.PopMatrix ();
	}
	
	public static void RenderDistortion (Material material, RenderTexture source, RenderTexture destination, float angle, Vector3 center, float radius, int subdivisions)
	{
		RenderTexture.active = destination;
		
		angle *= Mathf.Deg2Rad;
		Matrix4x4 rotationMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.EulerAngles (0, 0, angle), Vector3.one);
		Vector4 pixelCenter = new Vector4 (source.width * center.x, source.height * center.y, 1, 1);
		radius *= source.height;
		
		material.SetMatrix("_RotationMatrix", rotationMatrix);
		material.SetVector("_Center", pixelCenter);
		material.SetFloat("_Radius", radius);
		material.SetFloat("_Angle", angle);
		material.SetTexture("_MainTex", source);
		
		GL.PushMatrix ();
		GL.LoadOrtho ();
		
		for (int i = 0; i < material.passCount; i++) {
			material.SetPass (i);
			ImageEffects.DrawGrid(subdivisions, subdivisions);
		}
		GL.PopMatrix ();
	}
	
	public static void DrawGrid (int xSize, int ySize)
	{
		GL.Begin (GL.QUADS);
		
		float xDelta = 1.0F / xSize;
		float yDelta = 1.0F / ySize;
		
		for (int y=0;y<xSize;y++)
		{
			for (int x=0;x<ySize;x++)
			{
				GL.TexCoord2 ((x+0) * xDelta, (y+0) * yDelta); GL.Vertex3 ((x+0) * xDelta, (y+0) * yDelta, 0.1f);
				GL.TexCoord2 ((x+1) * xDelta, (y+0) * yDelta); GL.Vertex3 ((x+1) * xDelta, (y+0) * yDelta, 0.1f);
				GL.TexCoord2 ((x+1) * xDelta, (y+1) * yDelta); GL.Vertex3 ((x+1) * xDelta, (y+1) * yDelta, 0.1f);
				GL.TexCoord2 ((x+0) * xDelta, (y+1) * yDelta); GL.Vertex3 ((x+0) * xDelta, (y+1) * yDelta, 0.1f);
			}
		}
		GL.End();
	}
}