using UnityEngine;
using System.Collections;

public class LevelSelect : MonoBehaviour 
{
	public float fadeInTime = 2f;
	public AudioSource mainMusic;

	void Start()
	{
		mainMusic.pitch = 0f;
		mainMusic.Play();

		Time.timeScale = 0f;
		TimeWarpEffect.EndWarp(fadeInTime, new AudioSource[] { mainMusic }, iTween.EaseType.easeInOutSine);
		CRTEffect.EndCRT(fadeInTime, Screen.height, 0f, iTween.EaseType.easeInOutSine);
	}
}
