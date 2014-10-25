using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class MainMenu : MonoBehaviour 
{
	public EventSystem eventSystem;
	public Slider volumeSlider;

	public float startDelay = 1f;
	public float fadeTime = 1.5f;
	public float nodeMoveSpeed = 2f;

	private CanvasGroup menu;
	private iTweenPath menuPath;

	void Awake()
	{
		LoadPrefs();

		menu = GetComponent<CanvasGroup>();
		menuPath = GetComponent<iTweenPath>();
	}

	void Start()
	{
		StartCoroutine(ShowMenu());
	}

	public void GoToNode(int node)
	{
		menu.gameObject.MoveTo(menuPath.nodes[node], nodeMoveSpeed, 0f, EaseType.easeOutQuint);
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

	private void LoadPrefs()
	{
		if (PlayerPrefs.HasKey("Settings/Volume"))
		{
			AudioListener.volume = PlayerPrefs.GetFloat("Settings/Volume");
			volumeSlider.value = -AudioListener.volume;
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
}
