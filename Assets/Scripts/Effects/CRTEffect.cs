using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CRTEffect : MonoBehaviour 
{
	public float defaultFade = 0.7f;
	public float borderBuffer = -32f;
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

	public static void StartCRT(float fadeTime, float scanlinesStart = -1f, float scanlinesEnd = -1f, iTween.EaseType easeType = iTween.EaseType.easeOutSine)
	{
		Vector2 scanlines = (scanlinesStart == -1 || scanlinesEnd == -1) ? defaultScanlines : new Vector2(scanlinesStart, scanlinesEnd);

		instance.EnableCRTShader();
		
		iTween.ValueTo(instance.gameObject, iTween.Hash("from", instance.borderBuffer,
													    "to", 0f,
													    "time", fadeTime,
													    "easetype", iTween.EaseType.easeOutCirc,
													    "onupdate", "UpdateCRTBorder",
													    "ignoretimescale", true));
		iTween.ValueTo(instance.gameObject, iTween.Hash("from", scanlines.x,
														"to", scanlines.y,
														"time", fadeTime,
														"easetype", easeType,
														"onupdate", "UpdateCRTScanlines",
														"ignoretimescale", true));
		iTween.ValueTo(instance.gameObject, iTween.Hash("from", instance.gamma.x,
														"to", instance.gamma.y,
														"time", fadeTime,
														"easetype", iTween.EaseType.easeOutQuint,
														"onupdate", "UpdateCRTGamma",
														"ignoretimescale", true));
		iTween.ValueTo(instance.gameObject, iTween.Hash("from", 0f,
														"to", instance.distortionAmount,
														"time", fadeTime,
														"easetype", iTween.EaseType.easeOutQuint,
														"onupdate", "UpdateCRTShader",
														"ignoretimescale", true));
	}

	public static void EndCRT(float fadeTime, float scanlinesStart = -1f, float scanlinesEnd = -1f, iTween.EaseType easeType = iTween.EaseType.easeInSine)
	{
		Vector2 scanlines = (scanlinesStart == -1 || scanlinesEnd == -1) ? defaultScanlines : new Vector2(scanlinesStart, scanlinesEnd);

		if (!crtShader.enabled)
		{
			instance.EnableCRTShader();
		}

		iTween.ValueTo(instance.gameObject, iTween.Hash("from", 0f,
														"to", instance.borderBuffer,
														"time", fadeTime,
														"easetype", iTween.EaseType.easeOutCirc,
														"onupdate", "UpdateCRTBorder",
														"ignoretimescale", true));
		iTween.ValueTo(instance.gameObject, iTween.Hash("from", scanlines.y,
														"to", scanlines.x,
														"time", fadeTime,
														"easetype", easeType,
														"onupdate", "UpdateCRTScanlines",
														"ignoretimescale", true));
		iTween.ValueTo(instance.gameObject, iTween.Hash("from", instance.gamma.y,
														"to", instance.gamma.x,
														"time", fadeTime,
														"easetype", iTween.EaseType.easeOutQuint,
														"onupdate", "UpdateCRTGamma",
														"ignoretimescale", true));
		iTween.ValueTo(instance.gameObject, iTween.Hash("from", instance.distortionAmount,
														"to", 0f,
														"time", fadeTime,
														"easetype", iTween.EaseType.easeOutQuint,
														"onupdate", "UpdateCRTShader",
														"oncomplete", "DisableCRTShader",
														"ignoretimescale", true));
	}

	public static void AnimateScanlines(float fadeTime, float scanlinesEnd, iTween.EaseType easeType)
	{
		if (!crtShader.enabled)
		{
			StartCRT(fadeTime, defaultScanlines.x, scanlinesEnd, easeType);
		}
		else
		{
			iTween.ValueTo(instance.gameObject, iTween.Hash("from", crtShader.TextureSize,
															"to", scanlinesEnd,
															"time", fadeTime,
															"easetype", easeType,
															"onupdate", "UpdateCRTScanlines",
															"ignoretimescale", true));
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
