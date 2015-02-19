using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;

public class GameMenu : MonoBehaviour
{
	#region Fields
	private static GameMenu instance;

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

	private bool paused = false;
	private bool canPause = true;
	private bool gameOver = false;
	private AudioSource[] sounds;

	private Slider volumeSlider;
	private Toggle fullscreenToggle;
	private ResolutionSelector resolutionSelector;

	private RectTransform rectTransform;
	#endregion

	#region Public Properties
	public static GameMenu Instance
	{
		get { return instance; }
	}
	#endregion

	#region MonoBehaviour
	private void Awake()
	{
		instance = this;

		volumeSlider = transform.FindSubChild("Volume").GetComponent<Slider>();
		fullscreenToggle = transform.FindSubChild("Fullscreen").GetComponent<Toggle>();
		resolutionSelector = transform.FindSubChild("Resolution").GetComponent<ResolutionSelector>();

		rectTransform = GetComponent<RectTransform>();

		pauseOverlay.interactable = false;

		if (gameOverOverlay != null)
		{
			gameOverOverlay.interactable = false;
		}

		LoadPrefs();
	}

	private void Update()
	{
		if (CrossPlatformInputManager.GetButtonDown("Pause") && canPause)
		{
			sounds = FindObjectsOfType<AudioSource>();

			if (!paused)
			{
				paused = true;
				canPause = false;
				PlayerControl.Instance.DisableInput();
				SelectObject(pauseSelect);
				TimeWarpEffect.Instance.StartWarp(0f, fadeTime, sounds);
				CRTEffect.Instance.StartCRT(fadeTime);
				Fade(0f, 1f, UpdatePauseAlpha, true);
			}
			else
			{
				paused = false;
				canPause = false;
				PlayerControl.Instance.EnableInput();
				GoToNode("Pause Menu");
				TimeWarpEffect.Instance.EndWarp(fadeTime, sounds);
				CRTEffect.Instance.EndCRT(fadeTime);
				Fade(1f, 0f, UpdatePauseAlpha, true);
			}
		}

		if (!gameOver && PlayerControl.Instance.Dead)
		{
			gameOver = true;
			canPause = false;

			StartCoroutine(GameOver());
		}
	}
	#endregion

	#region Internal Helper Methods
	private void LoadPrefs()
	{
		if (PlayerPrefs.HasKey("Settings/Volume"))
		{
			volumeSlider.value = PlayerPrefs.GetFloat("Settings/Volume");
			AudioListener.volume = Mathf.Abs(volumeSlider.value);
		}

		if (PlayerPrefs.HasKey("Settings/Fullscreen"))
		{
			fullscreenToggle.isOn = PlayerPrefs.GetInt("Settings/Fullscreen") == 1 ? true : false;
			Screen.fullScreen = fullscreenToggle.isOn;
		}
	}

	private IEnumerator GameOver()
	{
		yield return new WaitForSeconds(gameOverDelay);

		SelectObject(gameOverSelect);
		CRTEffect.Instance.StartCRT(fadeTime);
		Fade(0f, 1f, UpdateGameOverAlpha, false);
	}

	private void Fade(float from, float to, DOSetter<float> updateMethod, bool setPause)
	{
		DOTween.To(updateMethod, from, to, fadeTime)
			.SetEase(Ease.OutQuint)
			.SetUpdate(true)
			.OnComplete(() => EnablePausing(setPause));
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
	#endregion

	#region Public Methods
	public void EnablePausing(bool enabled)
	{
		canPause = enabled;
	}

	public void LoadLevel(string levelName)
	{
		if (!Application.isLoadingLevel)
		{
			levelName = (levelName == "Retry") ? Application.loadedLevelName : levelName;

			sounds = FindObjectsOfType<AudioSource>();

			TimeWarpEffect.Instance.StartWarp(0f, loadTime, sounds, Ease.OutSine);
			CRTEffect.Instance.AnimateScanlines(loadTime, 0f, Ease.OutSine);
			StartCoroutine(LoadLevelCoroutine(levelName));
		}
	}

	private IEnumerator LoadLevelCoroutine(string levelName)
	{
		AsyncOperation async = Application.LoadLevelAsync(levelName);
		async.allowSceneActivation = false;

		yield return StartCoroutine(Extensions.WaitForRealSeconds(loadTime - (loadTime * 0.05f)));

		DOTween.Kill();
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

	public void ApplySettings()
	{
		PlayerPrefs.SetFloat("Settings/Volume", volumeSlider.value);
		resolutionSelector.SetResolution();
		PlayerPrefs.SetInt("Settings/Fullscreen", fullscreenToggle.isOn ? 1 : 0);
		Screen.fullScreen = fullscreenToggle.isOn;
	}
	#endregion
}
