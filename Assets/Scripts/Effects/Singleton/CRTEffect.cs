using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class CRTEffect : MonoBehaviour 
{
	private static CRTEffect instance;

	public float defaultFade = 0.7f;
	public float borderBuffer = -64f;
	public float borderZeroed = -32f;
	public float distortionAmount = 0.2f;
	public float noiseIntensity = 3.5f;
	public Vector2 scanlineOffset = new Vector2(0f, 100f);
	public Image crtBorder;

	private Vector2 defaultScanlines;
	private CRT crtShader;
	private NoiseAndGrain noiseShader;

	public static CRTEffect Instance
	{
		get { return instance; }
	}

	private void Awake()
	{
		instance = this;

		defaultScanlines = new Vector2(Screen.height + scanlineOffset.x, Screen.height + scanlineOffset.y);
		crtShader = Camera.main.GetComponent<CRT>();
		noiseShader = Camera.main.GetComponent<NoiseAndGrain>();
		noiseShader.intensityMultiplier = noiseIntensity;
		crtBorder.color = new Color(crtBorder.color.r, crtBorder.color.g, crtBorder.color.b, 1f);
	}

	public void StartCRT(float fadeTime, float scanlinesStart = -1f, float scanlinesEnd = -1f, Ease easeType = Ease.OutSine)
	{
		Vector2 scanlines = (scanlinesStart == -1 || scanlinesEnd == -1) ? defaultScanlines : new Vector2(scanlinesStart, scanlinesEnd);

		EnableCRTShader();

		DOTween.To(UpdateCRTBorder, borderBuffer, borderZeroed, fadeTime)
			.SetEase(easeType)
			.SetUpdate(true);
		DOTween.To(UpdateCRTScanlines, scanlines.x, scanlines.y, fadeTime)
			.SetEase(easeType)
			.SetUpdate(true);
		DOTween.To(UpdateCRTShader, 0f, distortionAmount, fadeTime)
			.SetEase(Ease.OutQuint)
			.SetUpdate(true);
	}

	public void EndCRT(float fadeTime, float scanlinesStart = -1f, float scanlinesEnd = -1f, Ease easeType = Ease.InSine)
	{
		Vector2 scanlines = (scanlinesStart == -1 || scanlinesEnd == -1) ? defaultScanlines : new Vector2(scanlinesStart, scanlinesEnd);

		if (!crtShader.enabled)
		{
			EnableCRTShader();
		}

		DOTween.To(UpdateCRTBorder, borderZeroed, borderBuffer, fadeTime)
			.SetEase(Ease.OutCirc)
			.SetUpdate(true);
		DOTween.To(UpdateCRTScanlines, scanlines.y, scanlines.x, fadeTime)
			.SetEase(easeType)
			.SetUpdate(true);
		DOTween.To(UpdateCRTShader, distortionAmount, 0f, fadeTime)
			.SetEase(Ease.OutQuint)
			.SetUpdate(true)
			.OnComplete(DisableCRTShader);
	}

	public void AnimateScanlines(float fadeTime, float scanlinesEnd, Ease easeType)
	{
		if (!crtShader.enabled)
		{
			StartCRT(fadeTime, defaultScanlines.x, scanlinesEnd, easeType);
		}
		else
		{
			DOTween.To(UpdateCRTScanlines, crtShader.TextureSize, scanlinesEnd, fadeTime)
				.SetEase(easeType)
				.SetUpdate(true);
		}
	}

	public void UpdateResolution(int height)
	{
		defaultScanlines = new Vector2(height + scanlineOffset.x, height + scanlineOffset.y);
		crtShader.TextureSize = defaultScanlines.y;
	}

	private void UpdateCRTBorder(float newValue)
	{
		crtBorder.rectTransform.offsetMin = new Vector2(newValue, newValue);
		crtBorder.rectTransform.offsetMax = new Vector2(-newValue, -newValue);
	}

	private void UpdateCRTShader(float newValue)
	{
		crtShader.Distortion = newValue;
	}

	private void UpdateCRTScanlines(float newValue)
	{
		crtShader.TextureSize = newValue;
	}

	public void EnableCRTShader()
	{
		crtShader.enabled = true;
		noiseShader.enabled = true;
		crtBorder.fillCenter = true;
	}

	public void DisableCRTShader()
	{
		crtShader.enabled = false;
		noiseShader.enabled = false;
		crtBorder.fillCenter = false;
	}
}
