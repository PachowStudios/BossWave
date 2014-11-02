using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class GameMenu : MonoBehaviour 
{
	public EventSystem eventSystem;
	public float fadeTime = 0.7f;
	public float loadTime = 2f;
	public float nodeMoveSpeed = 2f;
	public CanvasGroup pauseOverlay;
	public CanvasGroup gameOverOverlay;
	public Selectable pauseSelect;
	public Selectable gameOverSelect;
	public EasyJoystick[] JoysticksToDisable;

	private bool paused = false;
	private bool canPause = true;
	private bool gameOver = false;
	private AudioSource[] sounds;

	private Slider volumeSlider;

	#if	!MOBILE_INPUT
	private Toggle fullscreenToggle;
	private ResolutionSelector resolutionSelector;
	#endif

	private PlayerControl player;
	private RectTransform rectTransform;

	void Awake()
	{
		volumeSlider = transform.FindSubChild("Volume").GetComponent<Slider>();

		#if !MOBILE_INPUT
		fullscreenToggle = transform.FindSubChild("Fullscreen").GetComponent<Toggle>();
		resolutionSelector = transform.FindSubChild("Resolution").GetComponent<ResolutionSelector>();
		#endif

		player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();
		rectTransform = GetComponent<RectTransform>();

		pauseOverlay.interactable = false;

		if (gameOverOverlay != null)
		{
			gameOverOverlay.interactable = false;
		}

		LoadPrefs();
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
				SelectObject(pauseSelect);
				TimeWarpEffect.StartWarp(0f, fadeTime, sounds);
				CRTEffect.StartCRT(fadeTime);
				Fade(0f, 1f, "UpdatePauseAlpha", true);

				SetJoysticks(false);
			}
			else
			{
				paused = false;
				canPause = false;
				GoToNode("Pause Menu");
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

			SelectObject(gameOverSelect);
			CRTEffect.StartCRT(fadeTime);
			Fade(0f, 1f, "UpdateGameOverAlpha", false);

			SetJoysticks(false);
		}
	}

	public void LoadLevel(string levelName)
	{
		sounds = FindObjectsOfType<AudioSource>();

		TimeWarpEffect.StartWarp(0f, loadTime, sounds, iTween.EaseType.easeOutSine);
		CRTEffect.AnimateScanlines(loadTime, 0f, iTween.EaseType.easeOutSine);
		StartCoroutine(LoadLevelCoroutine(levelName));
	}

	private IEnumerator LoadLevelCoroutine(string levelName)
	{
		AsyncOperation async = Application.LoadLevelAsync(levelName);
		async.allowSceneActivation = false;

		yield return StartCoroutine(Extensions.WaitForRealSeconds(loadTime - (loadTime * 0.05f)));

		async.allowSceneActivation = true;
	}

	public void GoToNode(string node)
	{
		iTween.ValueTo(gameObject, iTween.Hash("from", rectTransform.anchoredPosition.x,
											   "to", -transform.FindSubChild(node).GetComponent<RectTransform>().anchoredPosition.x,
											   "time", nodeMoveSpeed,
											   "easetype", iTween.EaseType.easeOutQuint,
											   "onupdate", "GoToUpdateX",
											   "ignoretimescale", true));

		iTween.ValueTo(gameObject, iTween.Hash("from", rectTransform.anchoredPosition.y,
											   "to", -transform.FindSubChild(node).GetComponent<RectTransform>().anchoredPosition.y,
											   "time", nodeMoveSpeed,
											   "easetype", iTween.EaseType.easeOutQuint,
											   "onupdate", "GoToUpdateY",
											   "ignoretimescale", true));
	}

	public void SelectObject(Selectable selectable)
	{
		eventSystem.SetSelectedGameObject(selectable.gameObject, new BaseEventData(eventSystem));
	}

	public void SetVolume(float newVolume)
	{
		newVolume = Mathf.Abs(newVolume);

		PlayerPrefs.SetFloat("Settings/Volume", newVolume);
		AudioListener.volume = newVolume;
	}

	public void ApplySettings()
	{
		#if !MOBILE_INPUT
		resolutionSelector.SetResolution();
		PlayerPrefs.SetInt("Settings/Fullscreen", fullscreenToggle.isOn ? 1 : 0);
		Screen.fullScreen = fullscreenToggle.isOn;
		#endif
	}

	private void LoadPrefs()
	{
		if (PlayerPrefs.HasKey("Settings/Volume"))
		{
			AudioListener.volume = PlayerPrefs.GetFloat("Settings/Volume");
			volumeSlider.value = -AudioListener.volume;
		}

		#if !MOBILE_INPUT
		if (PlayerPrefs.HasKey("Settings/Fullscreen"))
		{
			Screen.fullScreen = PlayerPrefs.GetInt("Settings/Fullscreen") == 1 ? true : false;
			fullscreenToggle.isOn = Screen.fullScreen;
		}
		#endif
	}

	private void Fade(float from, float to, string updateMethod, bool setPause)
	{
		iTween.ValueTo(gameObject, iTween.Hash("from", from,
											   "to", to,
											   "time", fadeTime,
											   "easetype", iTween.EaseType.easeOutQuint,
											   "onupdate", updateMethod,
											   "oncomplete", "EnablePausing",
											   "oncompleteparams", setPause,
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
		pauseOverlay.interactable = newValue == 1f;
		pauseOverlay.blocksRaycasts = newValue == 1f;
	}

	private void UpdateGameOverAlpha(float newValue)
	{
		gameOverOverlay.alpha = newValue;
		gameOverOverlay.interactable = newValue == 1f;
		gameOverOverlay.blocksRaycasts = newValue == 1f;
	}

	private void GoToUpdateX(float newValue)
	{
		rectTransform.anchoredPosition = new Vector2(newValue, rectTransform.anchoredPosition.y);
	}

	private void GoToUpdateY(float newValue)
	{
		rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, newValue);
	}
}
