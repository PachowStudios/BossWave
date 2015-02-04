using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ResolutionSelector : MonoBehaviour 
{
	public Text buttonText;

	private List<Resolution> resolutions;
	private int selected = 0;

	private void Awake()
	{
		resolutions = Screen.resolutions.ToList<Resolution>();

		if (PlayerPrefs.HasKey("Settings/ResolutionWidth") && PlayerPrefs.HasKey("Settings/ResolutionHeight"))
		{
			Resolution savedRes = new Resolution();

			savedRes.width = PlayerPrefs.GetInt("Settings/ResolutionWidth");
			savedRes.height = PlayerPrefs.GetInt("Settings/ResolutionHeight");
			savedRes.refreshRate = Screen.currentResolution.refreshRate;

			if (resolutions.Contains(savedRes))
			{
				Screen.SetResolution(savedRes.width, savedRes.height, Screen.fullScreen);
			}
		}		
	}

	private void Start()
	{
		Resolution windowRes = new Resolution();

		windowRes.width = Screen.width;
		windowRes.height = Screen.height;
		windowRes.refreshRate = Screen.currentResolution.refreshRate;

		selected = resolutions.FindIndex(Resolution => Resolution.Equals(windowRes));
		selected = Mathf.Clamp(selected, 0, resolutions.Count - 1);

		SetText();
	}
	
	public void CycleResolutions()
	{
		if (selected == resolutions.Count - 1)
		{
			selected = 0;
		}
		else
		{
			selected++;
		}

		PlayerPrefs.SetInt("Settings/ResolutionWidth", resolutions[selected].width);
		PlayerPrefs.SetInt("Settings/ResolutionHeight", resolutions[selected].height);

		SetText();
	}

	public void SetResolution()
	{
		Screen.SetResolution(resolutions[selected].width, resolutions[selected].height, Screen.fullScreen);
		CRTEffect.Instance.UpdateResolution(resolutions[selected].height);
	}

	private void SetText()
	{
		buttonText.text = resolutions[selected].width + "x" + resolutions[selected].height;
	}
}
