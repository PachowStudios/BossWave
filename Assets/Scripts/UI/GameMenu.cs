using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class GameMenu : MonoBehaviour 
{
	public EventSystem eventSystem;
	public float fadeTime = 0.7f;
	public CanvasGroup pauseOverlay;
	public CanvasGroup gameOverOverlay;
	public Selectable gameOverSelect;
	public EasyJoystick[] JoysticksToDisable;

	private bool paused = false;
	private bool canPause = true;
	private bool gameOver = false;
	private AudioSource[] sounds;

	private PlayerControl player;

	void Awake()
	{
		player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();
	}

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
				Fade(0f, 1f, "UpdatePauseAlpha", true);

				SetJoysticks(false);
			}
			else
			{
				paused = false;
				canPause = false;
				TimeWarpEffect.EndWarp(fadeTime, sounds);
				CRTEffect.EndCRT(fadeTime);
				Fade(1f, 0f, "UpdatePauseAlpha", true);

				SetJoysticks(true);
			}
		}

		if (!gameOver && player.health <= 0f)
		{
			gameOver = true;
			canPause = false;

			CRTEffect.StartCRT(fadeTime);
			Fade(0f, 1f, "UpdateGameOverAlpha", false);
		}
	}

	public void SelectObject(GameObject gameObject)
	{
		eventSystem.SetSelectedGameObject(gameObject, new BaseEventData(eventSystem));
	}

	private void Fade(float from, float to, string updateMethod, bool setPause)
	{
		iTween.ValueTo(gameObject, iTween.Hash("from", from,
											   "to", to,
											   "time", fadeTime,
											   "easetype", iTween.EaseType.easeOutQuint,
											   "onupdate", updateMethod,
											   "oncomplete", "EnablePausing",
											   "oncompleteparams", iTween.Hash("enabled", setPause),
											   "ignoretimescale", true));
	}

	private void EnablePausing(bool enabled)
	{
		canPause = enabled;
	}

	private void SetJoysticks(bool enabled)
	{
		#if MOBILE_INPUT
		foreach (EasyJoystick joystick in JoysticksToDisable)
		{
			joystick.enable = enabled;
		}
		#endif
	}

	private void UpdatePauseAlpha(float newValue)
	{
		pauseOverlay.alpha = newValue;
	}

	private void UpdateGameOverAlpha(float newValue)
	{
		gameOverOverlay.alpha = newValue;
	}
}
