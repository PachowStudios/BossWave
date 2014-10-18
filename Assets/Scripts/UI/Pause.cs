using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Pause : MonoBehaviour 
{
	public float fadeTime = 0.3f;

	private bool paused = false;
	private AudioSource[] sounds;

	private CanvasGroup pauseScreen;

	void Awake()
	{
		pauseScreen = GetComponent<CanvasGroup>();
	}

	void Update()
	{
		if (CrossPlatformInputManager.GetButtonDown("Pause"))
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
			}
		}
	}

	private void UpdateOverlayAlpha(float newValue)
	{
		pauseScreen.alpha = newValue;
	}
}
