using UnityEngine;
using System.Collections;

public class LevelSelect : MonoBehaviour 
{
	public bool introCRT = true;
	public float fadeInTime = 2f;
	public AudioSource mainMusic;

	void Start()
	{
		mainMusic.pitch = 0f;
		mainMusic.Play();

		if (introCRT)
		{
			Time.timeScale = 0f;
			TimeWarpEffect.EndWarp(fadeInTime, new AudioSource[] { mainMusic }, iTween.EaseType.easeInOutSine);
			CRTEffect.EndCRT(fadeInTime, Screen.height, 0f, iTween.EaseType.easeInOutSine);
		}
		else
		{
			Time.timeScale = 1f;
			mainMusic.pitch = 1f;
		}
	}
}
