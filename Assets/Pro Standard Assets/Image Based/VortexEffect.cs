using UnityEngine;
using System.Collections;

[AddComponentMenu("Image Effects/Vortex")]
public class VortexEffect : ImageEffectBase {
	public float    radius = .4F;
	public float    angle = 50;
	public Vector3  center = new Vector3 (0.5F, 0.5F, 0);

	// Called by camera to apply image effect
	void OnRenderImage (RenderTexture source, RenderTexture destination) {
		ImageEffects.RenderDistortion (material, source, destination, angle, center, radius, 1);
	}
}