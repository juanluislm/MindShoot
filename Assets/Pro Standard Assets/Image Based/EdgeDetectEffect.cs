using UnityEngine;
using System.Collections;

// This class implements Edge Detection using a Roberts cross filter.
[AddComponentMenu("Image Effects/Edge Detection")]
public class EdgeDetectEffect : ImageEffectBase
{
	public float threshold = 0.2F;
	
	// Called by camera to apply image effect
	void OnRenderImage (RenderTexture source, RenderTexture destination)
	{
		material.SetFloat("_Treshold", threshold);
		ImageEffects.RenderDistortion (material, source, destination, 0, Vector3.zero, 0, 1);
	}
}
