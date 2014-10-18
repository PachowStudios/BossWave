using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Pause : MonoBehaviour 
{
	public float pauseOverlayDuration = 0.3f;

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
													   "time", pauseOverlayDuration,
 													   "easetype", iTween.EaseType.easeOutQuint,
													   "onupdate", "updateOverlayAlpha", 
													   "ignoretimescale", true));
				TimeWarpEffect.StartWarp(0f, sounds);
			}
			else
			{
				paused = false;
				TimeWarpEffect.EndWarp(sounds);
				iTween.ValueTo(gameObject, iTween.Hash("from", 1f,
													   "to", 0f,
													   "time", pauseOverlayDuration,
													   "easetype", iTween.EaseType.easeOutQuint,
													   "onupdate", "updateOverlayAlpha",
													   "ignoretimescale", true));
			}
		}
	}

	private void updateOverlayAlpha(float newValue)
	{
		pauseScreen.alpha = newValue;
	}
}
