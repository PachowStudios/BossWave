using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MainMenu : MonoBehaviour 
{
	public EventSystem eventSystem;
	public Slider volumeSlider;

	public float startDelay = 1f;
	public float fadeTime = 1.5f;
	public float nodeMoveSpeed = 2f;

	private CanvasGroup menu;
	private RectTransform rectTransform;

	void Awake()
	{
		LoadPrefs();

		menu = GetComponent<CanvasGroup>();
		rectTransform = GetComponent<RectTransform>();
	}

	void Start()
	{
		StartCoroutine(ShowMenu());
	}

	public void GoToNode(string node)
	{
		iTween.ValueTo(gameObject, iTween.Hash("from", rectTransform.anchoredPosition.x,
											   "to", -transform.FindChild(node).GetComponent<RectTransform>().anchoredPosition.x,
											   "time", nodeMoveSpeed,
											   "easetype", iTween.EaseType.easeOutQuint,
											   "onupdate", "GoToUpdateX",
											   "ignoretimescale", true));

		iTween.ValueTo(gameObject, iTween.Hash("from", rectTransform.anchoredPosition.y,
											   "to", -transform.FindChild(node).GetComponent<RectTransform>().anchoredPosition.y,
											   "time", nodeMoveSpeed,
											   "easetype", iTween.EaseType.easeOutQuint,
											   "onupdate", "GoToUpdateY",
											   "ignoretimescale", true));
	}

	public void SelectObject(GameObject gameObject)
	{
		eventSystem.SetSelectedGameObject(gameObject, new BaseEventData(eventSystem));
	}

	public void LoadLevel(string levelName)
	{
		StartCoroutine(HideMenu(levelName));
	}

	public void SetVolume(float newVolume)
	{
		newVolume = Mathf.Abs(newVolume);

		PlayerPrefs.SetFloat("Settings/Volume", newVolume);
		AudioListener.volume = newVolume;
	}

	public void SetFullscreen(bool active)
	{
		PlayerPrefs.SetInt("Settings/Fullscreen", active ? 1 : 0);
		Screen.fullScreen = active;
	}

	private void LoadPrefs()
	{
		if (PlayerPrefs.HasKey("Settings/Volume"))
		{
			AudioListener.volume = PlayerPrefs.GetFloat("Settings/Volume");
			volumeSlider.value = -AudioListener.volume;
		}

		if (PlayerPrefs.HasKey("Settings/Fullscreen"))
		{
			Screen.fullScreen = PlayerPrefs.GetInt("Settings/Fullscreen") == 1 ? true : false;
		}
	}

	private IEnumerator ShowMenu()
	{
		yield return new WaitForSeconds(startDelay);
		CRTEffect.StartCRT(fadeTime);
		iTween.ValueTo(gameObject, iTween.Hash("from", 0f,
											   "to", 1f,
											   "time", fadeTime,
											   "easetype", iTween.EaseType.easeOutQuint,
											   "onupdate", "UpdateMenuAlpha",
											   "ignoretimescale", true));		
	}

	private IEnumerator HideMenu(string levelName = "none")
	{
		CRTEffect.EndCRT(fadeTime);
		iTween.ValueTo(gameObject, iTween.Hash("from", 1f,
											   "to", 0f,
											   "time", fadeTime,
											   "easetype", iTween.EaseType.easeOutQuint,
											   "onupdate", "UpdateMenuAlpha",
											   "ignoretimescale", true));

		yield return new WaitForSeconds(fadeTime);

		if (levelName != "none" && levelName != "Exit")
		{	
			Application.LoadLevel(levelName);
		}
		else if (levelName == "Exit")
		{
			Application.Quit();
		}
	}

	private void UpdateMenuAlpha(float newValue)
	{
		menu.alpha = newValue;
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
