using UnityEngine;

public class ReflectionRenderTexture : MonoBehaviour
{
	public float	m_ClipPlaneOffset = 0.01F;
	public bool		m_ReflectUpperSide = true;
	public bool		m_DisablePixelLights = true;

	int				m_RestorePixelLightCount;
	Camera			m_SourceCamera; // The camera we are going to reflect
 
	void LateUpdate ()
	{
		// Use main camera for reflection
		m_SourceCamera = Camera.main;
		
		// Figure out if we can do reflection/refraction
		if (!RenderTexture.enabled)
			camera.enabled = false; // no RTs - can't do
		else if (!Graphics.supportsVertexProgram)
			camera.enabled = false; // no vertex programs - can't do
		else if (camera.targetTexture == null)
		{
			Debug.Log ("No Render Texture assigned! Disabling reflection.");
			camera.enabled = false;
		}
		else if (!m_SourceCamera)
		{
			Debug.Log ("Reflection rendering requires that a Camera that is tagged \"MainCamera\"! Disabling reflection.");
			camera.enabled = false;
		}
		else
		{
			camera.enabled = true;
		}
	}

	void OnPreCull ()
	{
		m_SourceCamera = Camera.main;
		if( m_SourceCamera )
		{			
			// find out the reflection plane: position and normal in world space
			Vector3 pos = transform.position;
			Vector3 normal = transform.TransformDirection (Vector3.up);
			
			// if we're reflecting upper side, need to reflect the source camera
			// around reflection plane
			if (m_ReflectUpperSide)
			{
				float d = -Vector3.Dot (normal, pos) - m_ClipPlaneOffset;
				Vector4 reflectionPlane = new Vector4 (normal.x, normal.y, normal.z, d);
			
				Matrix4x4 reflection = Matrix4x4.zero;
				CalculateReflectionMatrix (ref reflection, reflectionPlane);
				camera.worldToCameraMatrix = m_SourceCamera.worldToCameraMatrix * reflection;
			}
			// else just use the source camera
			else
			{
				camera.worldToCameraMatrix = m_SourceCamera.worldToCameraMatrix;
			}
		
			// Setup oblique projection matrix so that near plane is our reflection
			// plane. This way we clip everything below/above it for free.
			Vector4 clipPlane = CameraSpacePlane( pos, normal );
			Matrix4x4 projection = m_SourceCamera.projectionMatrix;
			CalculateObliqueMatrix (ref projection, clipPlane);
			camera.projectionMatrix = projection;
		}
		else
		{
			camera.ResetWorldToCameraMatrix ();
		}
	}

	void OnPreRender ()
	{
		// If we're reflecting upper side, we need to revert backface culling
		if (m_ReflectUpperSide)
			GL.SetRevertBackfacing (true);
	
		if( m_DisablePixelLights )
		{
			m_RestorePixelLightCount = Light.pixelLightCount;
			Light.pixelLightCount = 0;
		}
	}
	
	void OnPostRender ()
	{
		// If we were reflecting upper side, restore the backface culling
		if (m_ReflectUpperSide)
			GL.SetRevertBackfacing (false);
			
		if( m_DisablePixelLights )
			Light.pixelLightCount = m_RestorePixelLightCount;
	}

	// Extended sign: returns -1, 0 or 1 based on sign of a
	private static float sgn(float a)
	{
        if (a > 0.0F) return 1.0F;
        if (a < 0.0F) return -1.0F;
        return 0.0F;
	}
	
	// Given position/normal of the plane, calculates plane in camera space.
	private Vector4 CameraSpacePlane (Vector3 pos, Vector3 normal)
	{
		float sideSign = m_ReflectUpperSide ? 1.0f : -1.0f;
		
		Vector3 offsetPos = pos + normal * (m_ClipPlaneOffset * sideSign);
		Matrix4x4 m = camera.worldToCameraMatrix;
		Vector3 cpos = m.MultiplyPoint( offsetPos );
		Vector3 cnormal = m.MultiplyVector( normal ).normalized * sideSign;
		return new Vector4( cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos,cnormal) );
	}
	
	// Adjusts the given projection matrix so that near plane is the given clipPlane
	// clipPlane is given in camera space. See article in GPG5.
	private static void CalculateObliqueMatrix (ref Matrix4x4 projection, Vector4 clipPlane)
	{
		Vector4 q;  
        q.x = (sgn(clipPlane.x) + projection[8]) / projection[0];
        q.y = (sgn(clipPlane.y) + projection[9]) / projection[5];
        q.z = -1.0F;
        q.w = (1.0F + projection[10]) / projection[14];
        
        Vector4 c = clipPlane * (2.0F / (Vector4.Dot (clipPlane, q)));
        
        projection[2] = c.x;
        projection[6] = c.y;
        projection[10] = c.z + 1.0F;
        projection[14] = c.w;
	}

	// Calculates reflection matrix around the given plane
	private static void CalculateReflectionMatrix (ref Matrix4x4 reflectionMat, Vector4 plane)
	{
	    reflectionMat.m00 = (1F - 2F*plane[0]*plane[0]);
	    reflectionMat.m01 = (   - 2F*plane[0]*plane[1]);
	    reflectionMat.m02 = (   - 2F*plane[0]*plane[2]);
	    reflectionMat.m03 = (   - 2F*plane[3]*plane[0]);

	    reflectionMat.m10 = (   - 2F*plane[1]*plane[0]);
	    reflectionMat.m11 = (1F - 2F*plane[1]*plane[1]);
	    reflectionMat.m12 = (   - 2F*plane[1]*plane[2]);
	    reflectionMat.m13 = (   - 2F*plane[3]*plane[1]);
	
    	reflectionMat.m20 = (   - 2F*plane[2]*plane[0]);
    	reflectionMat.m21 = (   - 2F*plane[2]*plane[1]);
    	reflectionMat.m22 = (1F - 2F*plane[2]*plane[2]);
    	reflectionMat.m23 = (   - 2F*plane[3]*plane[2]);

    	reflectionMat.m30 = 0F;
    	reflectionMat.m31 = 0F;
    	reflectionMat.m32 = 0F;
    	reflectionMat.m33 = 1F;
	}
}