/*
 * 
 * OLDTVFilter.cs 
 * Use this file to create a awesome old tv effect.
 * 
 * Version 1.00
 * 
 * Developed by Vortex Game Studios LTDA ME. (http://www.vortexstudios.com)
 * Authors:		Alexandre Ribeiro de Sa (@themonkeytail)
 * 
 */


using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class OLDTVFilter : MonoBehaviour {
	public Texture2D tvMask;
	public Texture2D tvNoise;
	public Texture2D tvStatic;

	[Range(-1.0f, 1.0f)]
	public float tubeDistortion = 0.05f;
	[Range(-1.0f, 1.0f)]
	public float chromaticAberration = 0.002f;

	[Range(0.0f, 1.0f)]
	public float scanlineMagnetude = 0.5f;
	public float scanlineSize = 640f;


	[Range(-1.0f, 1.0f)]
	public float staticMagnetude = 0.05f;
	[Range(-1.0f, 1.0f)]
	public float noiseMagnetude = 0.05f;

	public Material materialTv;
	public Material materialTvMobile;

	private Material _tvFilter;

	void OnRenderImage (RenderTexture source, RenderTexture destination) {		
		#if UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_BLACKBERRY
			_tvFilter = materialTvMobile;
		#else
			_tvFilter = materialTv;
		#endif

		// Just a TV mast overlay
		_tvFilter.SetTexture( "_TvMask", tvMask );

		// We use this texture to create both noise and static effect
		_tvFilter.SetTexture( "_TvNoise", tvNoise );
		_tvFilter.SetTexture( "_TvStatic", tvStatic );

		_tvFilter.SetFloat( "_Noise", Random.Range(-1.0f, 1.0f) );
		_tvFilter.SetFloat( "_tD", tubeDistortion );
		_tvFilter.SetFloat( "_cA", chromaticAberration );
		_tvFilter.SetFloat( "_lM", scanlineMagnetude );
		_tvFilter.SetFloat( "_lS", scanlineSize );
		_tvFilter.SetFloat( "_sM", staticMagnetude );
		_tvFilter.SetFloat( "_nM", Random.Range(-1.0f, 1.0f) * noiseMagnetude );

		Graphics.Blit(source, destination, _tvFilter);
	}	
}
