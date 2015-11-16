using UnityEngine;
using System.Collections;

// TBD: if some shader is not supported, turn off!

[AddComponentMenu("Image Effects/Contrast Stretch")]
public class ContrastStretchEffect : MonoBehaviour
{
	/// Adaptation speed - percents per frame, if playing at 30FPS.
	/// Default is 0.02 (2% each 1/30s).
	public float adaptationSpeed = 0.02f;
	
	/// If our scene is really dark (or really bright), we might not want to
	/// stretch its contrast to the full range.
	/// limitMinimum=0, limitMaximum=1 is the same as not applying the effect at all.
	/// limitMinimum=1, limitMaximum=0 is always stretching colors to full range.
	
	/// The limit on the minimum luminance (0...1) - we won't go above this.
	public float limitMinimum = 0.2f;
	
	/// The limit on the maximum luminance (0...1) - we won't go below this.
	public float limitMaximum = 0.6f;
	
	
	// To maintain adaptation levels over time, we need two 1x1 render textures
	// and ping-pong between them.
	private RenderTexture[] adaptRenderTex = new RenderTexture[2];
	private int curAdaptIndex = 0;
	
	
	// Computes scene luminance (grayscale) image
	public Shader   shaderLum;
	private Material m_materialLum;
	protected Material materialLum {
		get {
			if( m_materialLum == null )
				m_materialLum = new Material(shaderLum);
			return m_materialLum;
		}
	}
	
	// Reduces size of the image by 2x2, while computing maximum/minimum values.
	// By repeatedly applying this shader, we reduce the initial luminance image
	// to 1x1 image with minimum/maximum luminances found.
	public Shader   shaderReduce;
	private Material m_materialReduce;
	protected Material materialReduce {
		get {
			if( m_materialReduce == null )
				m_materialReduce = new Material(shaderReduce);
			return m_materialReduce;
		}
	}
	
	// Adaptation shader - gradually "adapts" minimum/maximum luminances,
	// based on currently adapted 1x1 image and the actual 1x1 image of the current scene.
	public Shader   shaderAdapt;
	private Material m_materialAdapt;
	protected Material materialAdapt {
		get {
			if( m_materialAdapt == null )
				m_materialAdapt = new Material(shaderAdapt);
			return m_materialAdapt;
		}
	}
	
	// Final pass - stretches the color values of the original scene, based on currently
	// adpated minimum/maximum values.
	public Shader   shaderApply;
	private Material m_materialApply;
	protected Material materialApply {
		get {
			if( m_materialApply == null )
				m_materialApply = new Material(shaderApply);
			return m_materialApply;
		}
	}
	
	void Start()
	{
		// TBD: disable if some shaders are not supported
	}
	
	void OnEnable()
	{		
		for( int i = 0; i < 2; ++i )
		{
			if( !adaptRenderTex[i] )
				adaptRenderTex[i] = new RenderTexture( 1, 1, 32 );
		}
	}
	
	void OnDisable()
	{
		for( int i = 0; i < 2; ++i )
		{
			Destroy( adaptRenderTex[i] );
			adaptRenderTex[i] = null;
		}
	}
	

	/// Apply the filter	
	void OnRenderImage (RenderTexture source, RenderTexture destination)
	{
		// Blit to smaller RT and convert to luminance on the way
		const int TEMP_RATIO = 1; // 4x4 smaller
		RenderTexture rtTempSrc = RenderTexture.GetTemporary(source.width/TEMP_RATIO, source.height/TEMP_RATIO);
		ImageEffects.RenderDistortion( materialLum, source, rtTempSrc, 0, Vector3.zero, 0, 1 );
		
		// Repeatedly reduce this image in size, computing min/max luminance values
		// In the end we'll have 1x1 image with min/max luminances found.
		const int FINAL_SIZE = 1;
		//const int FINAL_SIZE = 1;
		while( rtTempSrc.width > FINAL_SIZE || rtTempSrc.height > FINAL_SIZE )
		{
			const int REDUCE_RATIO = 2; // our shader does 2x2 reduction
			int destW = rtTempSrc.width / REDUCE_RATIO;
			if( destW < FINAL_SIZE ) destW = FINAL_SIZE;
			int destH = rtTempSrc.height / REDUCE_RATIO;
			if( destH < FINAL_SIZE ) destH = FINAL_SIZE;
			RenderTexture rtTempDst = RenderTexture.GetTemporary(destW,destH);
			ImageEffects.RenderDistortion( materialReduce, rtTempSrc, rtTempDst, 0, Vector3.zero, 0, 1 );
			
			// Release old src temporary, and make new temporary the source
			RenderTexture.ReleaseTemporary( rtTempSrc );
			rtTempSrc = rtTempDst;
		}
		
		// Update viewer's adaptation level
		CalculateAdaptation( rtTempSrc );
		
		// Apply contrast strech to the original scene, using currently adapted parameters
		materialApply.SetTexture("_AdaptTex", adaptRenderTex[curAdaptIndex] );
		ImageEffects.RenderDistortion( materialApply, source, destination, 0, Vector3.zero, 0, 1 );
		
		RenderTexture.ReleaseTemporary( rtTempSrc );
	}
	
	
	/// Helper function to do gradual adaptation to min/max luminances
	private void CalculateAdaptation( Texture curTexture )
	{
		int prevAdaptIndex = curAdaptIndex;
		curAdaptIndex = (curAdaptIndex+1) % 2;
		
		// Adaptation speed is expressed in percents/frame, based on 30FPS.
		// Calculate the adaptation lerp, based on current FPS.
		float adaptLerp = 1.0f - Mathf.Pow( 1.0f - adaptationSpeed, 30.0f * Time.deltaTime );
		const float kMinAdaptLerp = 0.01f;
		adaptLerp = Mathf.Clamp( adaptLerp, kMinAdaptLerp, 1 );
		
		materialAdapt.SetTexture("_CurTex", curTexture );
		materialAdapt.SetVector("_AdaptParams", new Vector4(
			adaptLerp,
			limitMinimum,
			limitMaximum,
			0.0f
		));
		ImageEffects.RenderDistortion( materialAdapt,
			adaptRenderTex[prevAdaptIndex],
			adaptRenderTex[curAdaptIndex],
			0, Vector3.zero, 0, 1 );
	}
}
