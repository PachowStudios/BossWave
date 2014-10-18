using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Pause : MonoBehaviour 
{
	public float fadeTime = 0.3f;
	public EasyJoystick[] JoysticksToDisable;

	private bool paused = false;
	private AudioSource[] sounds;
	private CanvasGroup pauseScreen;

	void Awake()
	{
		pauseScreen = GetComponent<CanvasGroup>();
	}

	void Update()
	{
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
		pauseScreen.alpha = newValue;
	}
}
