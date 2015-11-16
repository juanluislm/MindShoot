using UnityEngine;
using System.Collections;

[AddComponentMenu("Image Effects/Grayscale")]
public class GrayscaleEffect : ImageEffectBase {
	public Texture  textureRamp;
	public float    rampOffset;

	// Called by camera to apply image effect
	void OnRenderImage (RenderTexture source, RenderTexture destination) {
		material.SetTexture("_RampTex", textureRamp);
		material.SetFloat("_RampOffset", rampOffset);
		ImageEffects.RenderDistortion (material, source, destination, 0, Vector3.zero, 0, 1);
	}
}