using UnityEngine;
using System.Collections;

public class TimeWarpEffect : MonoBehaviour
{
	static private TimeWarpEffect instance;

	void Awake()
	{
		instance = this;
	}

	public static void Warp(float timeScale, float length, AudioSource[] sounds = null)
	{
		instance.StopAllCoroutines();
		instance.StartCoroutine(instance.WarpCoroutine(timeScale, length, sounds));
	}

	public static void StartWarp(float timeScale, AudioSource[] sounds = null)
	{
		instance.StopAllCoroutines();
		instance.StartCoroutine(instance.StartWarpCoroutine(timeScale, sounds));
	}

	public static void EndWarp(AudioSource[] sounds = null)
	{
		instance.StopAllCoroutines();
		instance.StartCoroutine(instance.EndWarpCoroutine(sounds));
	}

	private IEnumerator WarpCoroutine(float timeScale, float length, AudioSource[] sounds)
	{
		float startTime = Time.realtimeSinceStartup;

		while (Time.timeScale > timeScale + 0.01f)
		{
			Time.timeScale = Mathf.Lerp(Time.timeScale, timeScale, 0.2f);

			if (sounds != null)
			{
				foreach (AudioSource sound in sounds)
				{
					sound.pitch = Time.timeScale;
				}
			}

			yield return new WaitForSeconds(0.01f * Time.timeScale);
		}

		Time.timeScale = timeScale;

		yield return new WaitForSeconds((length - ((Time.realtimeSinceStartup - startTime) * 2f)) * Time.timeScale);

		while (Time.timeScale < 0.99f)
		{
			Time.timeScale = Mathf.Lerp(Time.timeScale, 1f, 0.2f);

			if (sounds != null)
			{
				foreach (AudioSource sound in sounds)
				{
					sound.pitch = Time.timeScale;
				}
			}

			yield return new WaitForSeconds(0.01f * Time.timeScale);
		}

		Time.timeScale = 1f;

		if (sounds != null)
		{
			foreach (AudioSource sound in sounds)
			{
				sound.pitch = 1f;
			}
		}
	}

	private IEnumerator StartWarpCoroutine(float timeScale, AudioSource[] sounds)
	{
		while (Time.timeScale > timeScale + 0.01f)
		{
			Time.timeScale = Mathf.Lerp(Time.timeScale, timeScale, 0.2f);

			if (sounds != null)
			{
				foreach (AudioSource sound in sounds)
				{
					sound.pitch = Time.timeScale;
				}
			}

			yield return new WaitForSeconds(0.01f * Time.timeScale);
		}

		Time.timeScale = timeScale;

		if (sounds != null)
		{
			foreach (AudioSource sound in sounds)
			{
				sound.pitch = timeScale;
			}
		}
	}

	private IEnumerator EndWarpCoroutine(AudioSource[] sounds)
	{
		while (Time.timeScale < 0.99f)
		{
			Time.timeScale = Mathf.Lerp(Time.timeScale, 1f, 0.2f);

			if (sounds != null)
			{
				foreach (AudioSource sound in sounds)
				{
					sound.pitch = Time.timeScale;
				}
			}

			yield return new WaitForSeconds(0.01f * Time.deltaTime);
		}

		Time.timeScale = 1f;

		if (sounds != null)
		{
			foreach (AudioSource sound in sounds)
			{
				sound.pitch = 1f;
			}
		}
	}
}
