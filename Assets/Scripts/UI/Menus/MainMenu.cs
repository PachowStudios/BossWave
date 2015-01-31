using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public class MainMenu : MonoBehaviour 
{
	public EventSystem eventSystem;

	public float startDelay = 1f;
	public float fadeTime = 2f;
	public float nodeMoveSpeed = 2f;
	public float interactableThreshold = 0.75f;
	public float overlayVisibility = 0.5f;

	public Image logo;
	public CanvasGroup blackOverlay;

	private Slider volumeSlider;
	private Slider fovSlider;

	#if !MOBILE_INPUT
	private Toggle fullscreenToggle;
	private ResolutionSelector resolutionSelector;
	#endif

	private CanvasGroup menu;
	private RectTransform rectTransform;

	void Awake()
	{
		DOTween.Init();

		volumeSlider = transform.FindSubChild("Volume").GetComponent<Slider>();
		fovSlider = transform.FindSubChild("FOV").GetComponent<Slider>();

		#if !MOBILE_INPUT
		fullscreenToggle = transform.FindSubChild("Fullscreen").GetComponent<Toggle>();
		resolutionSelector = transform.FindSubChild("Resolution").GetComponent<ResolutionSelector>();
		#endif

		menu = GetComponent<CanvasGroup>();
		rectTransform = GetComponent<RectTransform>();

		menu.interactable = false;

		LoadPrefs();
	}

	void Start()
	{
		Time.timeScale = 1f;
		Time.fixedDeltaTime = TimeWarpEffect.Instance.DefaultFixedTimestep;

		StartCoroutine(ShowMenu());
	}

	public void GoToNode(string node)
	{
		rectTransform.DOAnchorPos(-transform.FindSubChild(node).GetComponent<RectTransform>().anchoredPosition, nodeMoveSpeed)
			.SetEase(Ease.OutQuint)
			.SetUpdate(true);
	}

	public void SelectObject(GameObject gameObject)
	{
		eventSystem.SetSelectedGameObject(gameObject, new BaseEventData(eventSystem));
	}

	public void LoadLevel(string levelName)
	{
		if (!Application.isLoadingLevel)
		{
			StartCoroutine(HideMenu(levelName));
		}
	}

	public void SetVolume(float newVolume)
	{
		AudioListener.volume = Mathf.Abs(newVolume);
	}

	public void SetFOV(float newFOV)
	{
		ScaleWidthCamera.Instance.FOV = Mathf.Abs((int)newFOV);
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

	public void ResetPrefs()
	{
		PlayerPrefs.DeleteAll();
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
			ScaleWidthCamera.Instance.FOV = Mathf.Abs((int)fovSlider.value);
		}

		#if !MOBILE_INPUT
		if (PlayerPrefs.HasKey("Settings/Fullscreen"))
		{
			fullscreenToggle.isOn = PlayerPrefs.GetInt("Settings/Fullscreen") == 1 ? true : false;
			Screen.fullScreen = fullscreenToggle.isOn;
		}
		#endif
	}

	private IEnumerator ShowMenu()
	{
		yield return new WaitForSeconds(startDelay);
		
		logo.GetComponent<Animator>().SetTrigger("Start");
		CRTEffect.Instance.StartCRT(fadeTime);
		DOTween.To(UpdateMenuAlpha, 0f, 1f, fadeTime)
			.SetEase(Ease.OutQuint)
			.SetUpdate(true);
		blackOverlay.alpha = 1f;
		blackOverlay.DOFade(overlayVisibility, fadeTime)
			.SetEase(Ease.OutQuint)
			.SetUpdate(true);
	}

	private IEnumerator HideMenu(string levelName = "none")
	{
		if (levelName != "none" && levelName != "Exit")
		{
			CRTEffect.Instance.AnimateScanlines(fadeTime, 0f, Ease.OutSine);

			AsyncOperation async = Application.LoadLevelAsync(levelName);
			async.allowSceneActivation = false;

			yield return StartCoroutine(Extensions.WaitForRealSeconds(fadeTime));

			DOTween.Kill();
			async.allowSceneActivation = true;
		}
		else if (levelName == "Exit")
		{
			Application.Quit();
		}
	}

	private void UpdateMenuAlpha(float newValue)
	{
		menu.alpha = newValue;
		menu.interactable = newValue >= interactableThreshold;
		menu.blocksRaycasts = newValue >= interactableThreshold;
	}
}
