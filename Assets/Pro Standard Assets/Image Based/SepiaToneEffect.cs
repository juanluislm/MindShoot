using UnityEngine;
using System.Collections;

[AddComponentMenu("Image Effects/Sepia Tone")]
public class SepiaToneEffect : ImageEffectBase {

	// Called by camera to apply image effect
	void OnRenderImage (RenderTexture source, RenderTexture destination) {
		ImageEffects.RenderDistortion (material, source, destination, 0, Vector3.zero, 0, 1);
	}
}
