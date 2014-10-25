using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainMenu : MonoBehaviour 
{
	public CanvasGroup menu;
	public float startDelay = 1f;
	public float fadeTime = 1.5f;

	void Start()
	{
		StartCoroutine(ShowMenu());
	}

	public void LoadLevel(string levelName)
	{
		StartCoroutine(HideMenu(levelName));
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
