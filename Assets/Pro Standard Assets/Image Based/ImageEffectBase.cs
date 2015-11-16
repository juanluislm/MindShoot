using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Camera))]
[AddComponentMenu("")]
public class ImageEffectBase : MonoBehaviour {
	/// Provides a shader property that is set in the inspector
	/// and a material instantiated from the shader
	public Shader   shader;
	private Material m_Material;

	protected void Start ()
	{
		// Disable the image effect if the shader can't
		// run on the users graphics card
		if (!shader.isSupported)
			enabled = false;
	}

	protected Material material {
		get {
			if (m_Material == null) 
				m_Material = new Material (shader);
			return m_Material;
		} 
	}
}