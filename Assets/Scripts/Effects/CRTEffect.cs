using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CRTEffect : MonoBehaviour 
{
	public float borderBuffer = -32f;
	public float distortionAmount = 0.2f;
	public float noiseIntensity = 3.5f;
	public Vector2 gamma = new Vector2(1f, 2.2f);
	public Image crtBorder;

	private static Vector2 scanlines;
	private static CRT crtShader;
	private static NoiseAndGrain noiseShader;

	private static CRTEffect instance;

	void Awake()
	{
		instance = this;

		scanlines = new Vector2(Screen.height, Screen.height + 100f);
		crtShader = Camera.main.GetComponent<CRT>();
		noiseShader = Camera.main.GetComponent<NoiseAndGrain>();
		noiseShader.intensityMultiplier = noiseIntensity;
	}

	public static void StartCRT(float fadeTime)
	{
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
														"easetype", iTween.EaseType.easeOutSine,
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

	public static void EndCRT(float fadeTime)
	{
		iTween.ValueTo(instance.gameObject, iTween.Hash("from", 0f,
														"to", instance.borderBuffer,
														"time", fadeTime,
														"easetype", iTween.EaseType.easeOutCirc,
														"onupdate", "UpdateCRTBorder",
														"ignoretimescale", true));
		iTween.ValueTo(instance.gameObject, iTween.Hash("from", scanlines.y,
														"to", scanlines.x,
														"time", fadeTime,
														"easetype", iTween.EaseType.easeInSine,
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

	public static void UpdateResolution(int height)
	{
		scanlines = new Vector2(Screen.height, Screen.height + 100f);
		crtShader.TextureSize = scanlines.y;
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

	private void EnableCRTShader()
	{
		crtShader.enabled = true;
		noiseShader.enabled = true;
		crtBorder.fillCenter = true;
	}

	private void DisableCRTShader()
	{
		crtShader.enabled = false;
		noiseShader.enabled = false;
		crtBorder.fillCenter = false;
	}
}
