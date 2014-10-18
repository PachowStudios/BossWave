using UnityEngine;
using System.Collections;

public class TimeWarpEffect : MonoBehaviour
{
	static private TimeWarpEffect instance;

	void Awake()
	{
		instance = this;
	}

	public static void Warp(float timeScale, float length, AudioSource music = null)
	{
		instance.StopAllCoroutines();
		instance.StartCoroutine(instance.WarpCoroutine(timeScale, length, music));
	}

	public static void StartWarp(float timeScale, AudioSource music = null)
	{
		instance.StopAllCoroutines();
		instance.StartCoroutine(instance.StartWarpCoroutine(timeScale, music));
	}

	public static void EndWarp(AudioSource music = null)
	{
		instance.StopAllCoroutines();
		instance.StartCoroutine(instance.EndWarpCoroutine(music));
	}

	private IEnumerator WarpCoroutine(float timeScale, float length, AudioSource music)
	{
		float startTime = Time.realtimeSinceStartup;

		while (Time.timeScale > timeScale + 0.01f)
		{
			Time.timeScale = Mathf.Lerp(Time.timeScale, timeScale, 0.2f);

			if (music != null)
			{
				music.pitch = Time.timeScale;
			}

			yield return new WaitForSeconds(0.01f * Time.timeScale);
		}

		Time.timeScale = timeScale;

		yield return new WaitForSeconds((length - ((Time.realtimeSinceStartup - startTime) * 2f)) * Time.timeScale);

		while (Time.timeScale < 0.99f)
		{
			Time.timeScale = Mathf.Lerp(Time.timeScale, 1f, 0.2f);

			if (music != null)
			{
				music.pitch = Time.timeScale;
			}

			yield return new WaitForSeconds(0.01f * Time.timeScale);
		}

		Time.timeScale = 1f;
	}

	private IEnumerator StartWarpCoroutine(float timeScale, AudioSource music)
	{
		while (Time.timeScale > timeScale + 0.01f)
		{
			Time.timeScale = Mathf.Lerp(Time.timeScale, timeScale, 0.2f);

			if (music != null)
			{
				music.pitch = Time.timeScale;
			}

			yield return new WaitForSeconds(0.01f * Time.timeScale);
		}

		Time.timeScale = timeScale;
		music.pitch = timeScale;
	}

	private IEnumerator EndWarpCoroutine(AudioSource music)
	{
		while (Time.timeScale < 0.99f)
		{
			Time.timeScale = Mathf.Lerp(Time.timeScale, 1f, 0.2f);

			if (music != null)
			{
				music.pitch = Time.timeScale;
			}

			yield return new WaitForSeconds(0.01f * Time.deltaTime);
		}

		Time.timeScale = 1f;
		music.pitch = 1f;
	}
}
