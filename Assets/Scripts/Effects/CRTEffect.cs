using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class CRTEffect : MonoBehaviour 
{
	public float defaultFade = 0.7f;
	public float borderBuffer = -64f;
	public float borderZeroed = -32f;
	public float distortionAmount = 0.2f;
	public float noiseIntensity = 3.5f;
	public Vector2 gamma = new Vector2(1f, 2.2f);
	public Image crtBorder;

	private static Vector2 defaultScanlines;
	private static CRT crtShader;
	private static NoiseAndGrain noiseShader;

	private static CRTEffect instance;

	void Awake()
	{
		instance = this;

		defaultScanlines = new Vector2(Screen.height, Screen.height + 100f);
		crtShader = Camera.main.GetComponent<CRT>();
		noiseShader = Camera.main.GetComponent<NoiseAndGrain>();
		noiseShader.intensityMultiplier = noiseIntensity;
	}

	public static void StartCRT(float fadeTime, float scanlinesStart = -1f, float scanlinesEnd = -1f, Ease easeType = Ease.OutSine)
	{
		Vector2 scanlines = (scanlinesStart == -1 || scanlinesEnd == -1) ? defaultScanlines : new Vector2(scanlinesStart, scanlinesEnd);

		instance.EnableCRTShader();

		DOTween.To(instance.UpdateCRTBorder, instance.borderBuffer, instance.borderZeroed, fadeTime)
			.SetEase(easeType)
			.SetUpdate(true);
		DOTween.To(instance.UpdateCRTScanlines, scanlines.x, scanlines.y, fadeTime)
			.SetEase(easeType)
			.SetUpdate(true);
		DOTween.To(instance.UpdateCRTGamma, instance.gamma.x, instance.gamma.x, fadeTime)
			.SetEase(Ease.OutQuint)
			.SetUpdate(true);
		DOTween.To(instance.UpdateCRTShader, 0f, instance.distortionAmount, fadeTime)
			.SetEase(Ease.OutQuint)
			.SetUpdate(true);
	}

	public static void EndCRT(float fadeTime, float scanlinesStart = -1f, float scanlinesEnd = -1f, Ease easeType = Ease.InSine)
	{
		Vector2 scanlines = (scanlinesStart == -1 || scanlinesEnd == -1) ? defaultScanlines : new Vector2(scanlinesStart, scanlinesEnd);

		if (!crtShader.enabled)
		{
			instance.EnableCRTShader();
		}

		DOTween.To(instance.UpdateCRTBorder, instance.borderZeroed, instance.borderBuffer, fadeTime)
			.SetEase(Ease.OutCirc)
			.SetUpdate(true);
		DOTween.To(instance.UpdateCRTScanlines, scanlines.y, scanlines.x, fadeTime)
			.SetEase(easeType)
			.SetUpdate(true);
		DOTween.To(instance.UpdateCRTGamma, instance.gamma.y, instance.gamma.x, fadeTime)
			.SetEase(Ease.OutQuint)
			.SetUpdate(true);
		DOTween.To(instance.UpdateCRTShader, instance.distortionAmount, 0f, fadeTime)
			.SetEase(Ease.OutQuint)
			.SetUpdate(true)
			.OnComplete(instance.DisableCRTShader);
	}

	public static void AnimateScanlines(float fadeTime, float scanlinesEnd, Ease easeType)
	{
		if (!crtShader.enabled)
		{
			StartCRT(fadeTime, defaultScanlines.x, scanlinesEnd, easeType);
		}
		else
		{
			DOTween.To(instance.UpdateCRTScanlines, crtShader.TextureSize, scanlinesEnd, fadeTime)
				.SetEase(easeType)
				.SetUpdate(true);
		}
	}

	public static void UpdateResolution(int height)
	{
		defaultScanlines = new Vector2(height, height + 100f);
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

	private void UpdateCRTGamma(float newValue)
	{
		crtShader.OutputGamma = newValue;
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
