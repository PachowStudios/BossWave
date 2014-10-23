using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Pause : MonoBehaviour 
{
	public float fadeTime = 0.3f;
	public float borderBuffer = -50f;
	public float distortionAmount = 0.2f;
	public Vector2 scanlines = new Vector2(671f, 768f);
	public Vector2 gamma = new Vector2(1f, 2.2f);
	public CanvasGroup fadeOverlays;
	public Image crtBorder;
	public EasyJoystick[] JoysticksToDisable;

	private bool paused = false;
	private AudioSource[] sounds;

	private CRT crtShader;

	void Awake()
	{
		crtShader = Camera.main.GetComponent<CRT>();
	}

	void Update()
	{
		if (paused)
		{
			EnableCRTShader();
		}

		#if MOBILE_INPUT
		if (CrossPlatformInputManager.GetButton("Pause"))
		#else
		if (CrossPlatformInputManager.GetButtonDown("Pause"))
		#endif
		{
			sounds = FindObjectsOfType<AudioSource>();

			if (!paused)
			{
				paused = true;
				iTween.ValueTo(gameObject, iTween.Hash("from", 0f, 
													   "to", 1f, 
													   "time", fadeTime,
 													   "easetype", iTween.EaseType.easeOutQuint,
													   "onupdate", "UpdateOverlayAlpha", 
													   "ignoretimescale", true));
				iTween.ValueTo(gameObject, iTween.Hash("from", borderBuffer,
													   "to", 0f,
													   "time", fadeTime,
													   "easetype", iTween.EaseType.easeOutCirc,
													   "onupdate", "UpdateCRTBorder",
													   "ignoretimescale", true));
				iTween.ValueTo(gameObject, iTween.Hash("from", scanlines.x,
													   "to", scanlines.y,
													   "time", fadeTime,
													   "easetype", iTween.EaseType.easeOutSine,
													   "onupdate", "UpdateCRTScanlines",
													   "ignoretimescale", true));
				iTween.ValueTo(gameObject, iTween.Hash("from", gamma.x,
													   "to", gamma.y,
													   "time", fadeTime,
													   "easetype", iTween.EaseType.easeOutQuint,
													   "onupdate", "UpdateCRTGamma",
													   "ignoretimescale", true));
				iTween.ValueTo(gameObject, iTween.Hash("from", 0f,
													   "to", distortionAmount,
													   "time", fadeTime,
													   "easetype", iTween.EaseType.easeOutQuint,
													   "onupdate", "UpdateCRTShader",
													   "ignoretimescale", true));
				TimeWarpEffect.StartWarp(0f, fadeTime, sounds);

				#if MOBILE_INPUT
				foreach (EasyJoystick joystick in JoysticksToDisable)
				{
					joystick.enable = false;
				}
				#endif
			}
			else
			{
				paused = false;
				TimeWarpEffect.EndWarp(fadeTime, sounds);
				iTween.ValueTo(gameObject, iTween.Hash("from", 1f,
													   "to", 0f,
													   "time", fadeTime,
													   "easetype", iTween.EaseType.easeOutQuint,
													   "onupdate", "UpdateOverlayAlpha",
													   "ignoretimescale", true));
				iTween.ValueTo(gameObject, iTween.Hash("from", 0f,
													   "to", borderBuffer,
													   "time", fadeTime,
													   "easetype", iTween.EaseType.easeOutCirc,
													   "onupdate", "UpdateCRTBorder",
													   "ignoretimescale", true));
				iTween.ValueTo(gameObject, iTween.Hash("from", scanlines.y,
													   "to", scanlines.x,
													   "time", fadeTime,
													   "easetype", iTween.EaseType.easeInSine,
													   "onupdate", "UpdateCRTScanlines",
													   "ignoretimescale", true));
				iTween.ValueTo(gameObject, iTween.Hash("from", gamma.y,
													   "to", gamma.x,
													   "time", fadeTime,
													   "easetype", iTween.EaseType.easeOutQuint,
													   "onupdate", "UpdateCRTGamma",
													   "ignoretimescale", true));
				iTween.ValueTo(gameObject, iTween.Hash("from", distortionAmount,
													   "to", 0f,
													   "time", fadeTime,
													   "easetype", iTween.EaseType.easeOutQuint,
													   "onupdate", "UpdateCRTShader",
													   "oncomplete", "DisableCRTShader",
													   "ignoretimescale", true));
				#if MOBILE_INPUT
				foreach (EasyJoystick joystick in JoysticksToDisable)
				{
					joystick.enable = true;
				}
				#endif
			}
		}
	}

	private void UpdateOverlayAlpha(float newValue)
	{
		fadeOverlays.alpha = newValue;
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
		crtBorder.fillCenter = true;
	}

	private void DisableCRTShader()
	{
		crtShader.enabled = false;
		crtBorder.fillCenter = false;
	}
}
