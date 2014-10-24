using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Pause : MonoBehaviour 
{
	public float fadeTime = 0.7f;
	public CanvasGroup fadeOverlays;
	public EasyJoystick[] JoysticksToDisable;

	private bool paused = false;
	private bool canPause = true;
	private AudioSource[] sounds;

	void Update()
	{
		#if MOBILE_INPUT
		if (CrossPlatformInputManager.GetButton("Pause") && canPause)
		#else
		if (CrossPlatformInputManager.GetButtonDown("Pause") && canPause)
		#endif
		{
			sounds = FindObjectsOfType<AudioSource>();

			if (!paused)
			{
				paused = true;
				canPause = false;
				TimeWarpEffect.StartWarp(0f, fadeTime, sounds);
				CRTEffect.StartCRT(fadeTime);

				iTween.ValueTo(gameObject, iTween.Hash("from", 0f,
													   "to", 1f,
													   "time", fadeTime,
													   "easetype", iTween.EaseType.easeOutQuint,
													   "onupdate", "UpdateOverlayAlpha",
													   "oncomplete", "EnablePausing",
													   "ignoretimescale", true));

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
				canPause = false;
				TimeWarpEffect.EndWarp(fadeTime, sounds);
				CRTEffect.EndCRT(fadeTime);

				iTween.ValueTo(gameObject, iTween.Hash("from", 1f,
													   "to", 0f,
													   "time", fadeTime,
													   "easetype", iTween.EaseType.easeOutQuint,
													   "onupdate", "UpdateOverlayAlpha",
													   "oncomplete", "EnablePausing",
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

	private void EnablePausing()
	{
		canPause = true;
	}

	private void UpdateOverlayAlpha(float newValue)
	{
		fadeOverlays.alpha = newValue;
	}
}
