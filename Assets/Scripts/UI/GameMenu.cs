using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;

public class GameMenu : MonoBehaviour 
{
	public EventSystem eventSystem;

	public float fadeTime = 0.7f;
	public float loadTime = 2f;
	public float gameOverDelay = 1f;
	public float nodeMoveSpeed = 2f;
	public float interactableThreshold = 0.75f;
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
	private Slider fovSlider;

	#if	!MOBILE_INPUT
	private Toggle fullscreenToggle;
	private ResolutionSelector resolutionSelector;
	#endif

	private RectTransform rectTransform;

	void Awake()
	{
		volumeSlider = transform.FindSubChild("Volume").GetComponent<Slider>();
		fovSlider = transform.FindSubChild("FOV").GetComponent<Slider>();

		#if !MOBILE_INPUT
		fullscreenToggle = transform.FindSubChild("Fullscreen").GetComponent<Toggle>();
		resolutionSelector = transform.FindSubChild("Resolution").GetComponent<ResolutionSelector>();
		#endif

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
				Fade(0f, 1f, UpdatePauseAlpha, true);

				SetJoysticks(false);
			}
			else
			{
				paused = false;
				canPause = false;
				GoToNode("Pause Menu");
				TimeWarpEffect.EndWarp(fadeTime, sounds);
				CRTEffect.EndCRT(fadeTime);
				Fade(1f, 0f, UpdatePauseAlpha, true);

				SetJoysticks(true);
			}
		}

		if (!gameOver && PlayerControl.instance.Dead)
		{
			gameOver = true;
			canPause = false;

			StartCoroutine(GameOver());

			SetJoysticks(false);
		}
	}

	public void LoadLevel(string levelName)
	{
		if (!Application.isLoadingLevel)
		{
			levelName = (levelName == "Retry") ? Application.loadedLevelName : levelName;

			sounds = FindObjectsOfType<AudioSource>();

			TimeWarpEffect.StartWarp(0f, loadTime, sounds, Ease.OutSine);
			CRTEffect.AnimateScanlines(loadTime, 0f, Ease.OutSine);
			StartCoroutine(LoadLevelCoroutine(levelName));
		}
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
		rectTransform.DOAnchorPos(-transform.FindSubChild(node).GetComponent<RectTransform>().anchoredPosition, nodeMoveSpeed)
			.SetEase(Ease.OutQuint)
			.SetUpdate(true);
	}

	public void SelectObject(Selectable selectable)
	{
		eventSystem.SetSelectedGameObject(selectable.gameObject, new BaseEventData(eventSystem));
	}

	public void SetVolume(float newVolume)
	{
		AudioListener.volume = Mathf.Abs(newVolume);
	}

	public void SetFOV(float newFOV)
	{
		ScaleWidthCamera.FOV = Mathf.Abs((int)newFOV);
	}

	public void ApplySettings()
	{
		PlayerPrefs.SetFloat("Settings/Volume", volumeSlider.value);
		PlayerPrefs.SetInt("Settings/FOV", (int)fovSlider.value);

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
			volumeSlider.value = PlayerPrefs.GetFloat("Settings/Volume");
			AudioListener.volume = Mathf.Abs(volumeSlider.value);
		}

		if (PlayerPrefs.HasKey("Settings/FOV"))
		{
			fovSlider.value = PlayerPrefs.GetInt("Settings/FOV");
			ScaleWidthCamera.FOV = Mathf.Abs((int)fovSlider.value);
		}

		#if !MOBILE_INPUT
		if (PlayerPrefs.HasKey("Settings/Fullscreen"))
		{
			fullscreenToggle.isOn = PlayerPrefs.GetInt("Settings/Fullscreen") == 1 ? true : false;
			Screen.fullScreen = fullscreenToggle.isOn;
		}
		#endif
	}

	private IEnumerator GameOver()
	{
		yield return new WaitForSeconds(gameOverDelay);

		SelectObject(gameOverSelect);
		CRTEffect.StartCRT(fadeTime);
		Fade(0f, 1f, UpdateGameOverAlpha, false);
	}

	private void Fade(float from, float to, DOSetter<float> updateMethod, bool setPause)
	{
		DOTween.To(updateMethod, from, to, fadeTime)
			.SetEase(Ease.OutQuint)
			.SetUpdate(true)
			.OnComplete(() => EnablePausing(setPause));
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
		pauseOverlay.interactable = newValue >= interactableThreshold;
		pauseOverlay.blocksRaycasts = newValue >= interactableThreshold;
	}

	private void UpdateGameOverAlpha(float newValue)
	{
		gameOverOverlay.alpha = newValue;
		gameOverOverlay.interactable = newValue >= interactableThreshold;
		gameOverOverlay.blocksRaycasts = newValue >= interactableThreshold;
	}
}
