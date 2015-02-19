using UnityEngine;
using System.Collections;
using DG.Tweening;

public class LevelSelect : MonoBehaviour
{
	#region Fields
	public bool introCRT = true;
	public float fadeInTime = 2f;
	public AudioSource mainMusic;
	#endregion

	#region MonoBehaviour
	private void Start()
	{
		mainMusic.pitch = 0f;
		mainMusic.Play();

		if (introCRT)
		{
			Time.timeScale = 0f;
			Time.fixedDeltaTime = 0f;
			TimeWarpEffect.Instance.EndWarp(fadeInTime, new AudioSource[] { mainMusic }, Ease.InOutSine);
			CRTEffect.Instance.EndCRT(fadeInTime, 0f, Screen.height, Ease.InOutSine);
		}
		else
		{
			Time.timeScale = 1f;
			Time.fixedDeltaTime = TimeWarpEffect.Instance.DefaultFixedTimestep;
			mainMusic.pitch = 1f;
		}
	}
	#endregion
}
