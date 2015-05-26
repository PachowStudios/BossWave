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
	public float nodeMoveSpeed = 2f;
	public float interactableThreshold = 0.75f;
	public float scoreTallySpeed = 10000f;
	public float gameOverDelay = 1f;
	public float gameWinDelay = 1f;
	public CanvasGroup pauseOverlay;
	public CanvasGroup gameOverOverlay;
	public CanvasGroup gameWinOverlay;
	public Selectable pauseSelect;
	public Selectable gameOverSelect;
	public Selectable gameWinSelect;
	public Text gameOverReason;
	public Text scoreText;

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
			Pause(!paused);
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

	private void Pause(bool startPause)
	{
		paused = startPause;
		canPause = false;
		sounds = FindObjectsOfType<AudioSource>();

		if (startPause)
		{
			PlayerControl.Instance.DisableInput();
			SelectObject(pauseSelect);
			TimeWarpEffect.Instance.StartWarp(0f, fadeTime, sounds);
			CRTEffect.Instance.StartCRT(fadeTime);
			Fade(0f, 1f, pauseOverlay, true);
		}
		else
		{
			PlayerControl.Instance.EnableInput();
			GoToNode("Pause Menu");
			TimeWarpEffect.Instance.EndWarp(fadeTime, sounds);
			CRTEffect.Instance.EndCRT(fadeTime);
			Fade(1f, 0f, pauseOverlay, true);
		}
	}

	private void Fade(float from, float to, CanvasGroup fadeGroup, bool setPause)
	{
		fadeGroup.alpha = from;
		fadeGroup.DOFade(to, fadeTime)
			.SetEase(Ease.OutQuint)
			.SetUpdate(true)
			.OnUpdate(() =>
			{
				fadeGroup.interactable = fadeGroup.alpha >= interactableThreshold;
				fadeGroup.blocksRaycasts = fadeGroup.alpha >= interactableThreshold;
			})
			.OnComplete(() => EnablePausing(setPause));
	}
	#endregion

	#region Public Methods
	public IEnumerator GameWin()
	{
		if (gameOver)
			yield return null;

		gameOver = true;
		canPause = false;

		yield return new WaitForSeconds(gameWinDelay);

		SelectObject(gameWinSelect);
		CRTEffect.Instance.StartCRT(fadeTime);
		Cutscene.Instance.HideUI(fadeTime);
		Fade(0f, 1f, gameWinOverlay, false);

		yield return new WaitForSeconds(fadeTime);

		DOTween.To(s => scoreText.text = "Score:   " + Mathf.RoundToInt(s).ToString().PadLeft(HealthDisplay.Instance.scoreDigits, '0'), 
				   0f, 
				   PlayerControl.Instance.Score, 
				   scoreTallySpeed)
			.SetSpeedBased(true)
			.SetEase(Ease.InQuint);
	}

	public IEnumerator GameOver(string reason = "")
	{
		if (gameOver)
			yield return null;

		gameOver = true;
		canPause = false;

		yield return new WaitForSeconds(gameOverDelay);

		PlayerControl.Instance.DisableInput();
		SelectObject(gameOverSelect);
		gameOverReason.text = reason;
		CRTEffect.Instance.StartCRT(fadeTime);
		Cutscene.Instance.HideUI(fadeTime);
		Fade(0f, 1f, gameOverOverlay, false);
	}

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
