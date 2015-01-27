using UnityEngine;
using System.Collections;
using DG.Tweening;

public class TimeWarpEffect : MonoBehaviour
{
	private static TimeWarpEffect instance;

	public float defaultFixedTimestep = 0.0166667f;

	private static AudioSource[] allSounds;

	public static float DefaultFixedTimestep
	{
		get { return instance.defaultFixedTimestep; }
	}

	void Awake()
	{
		instance = this;
		allSounds = null;
	}

	public static void Warp(float timeScale, float length, float fadeTime, AudioSource[] sounds = null)
	{
		allSounds = sounds;

		Sequence sequence = DOTween.Sequence().SetEase(Ease.OutQuint).SetUpdate(true);
		sequence.Append(DOTween.To(instance.UpdateValues, Time.timeScale, timeScale, fadeTime))
			.AppendInterval(length)
			.Append(DOTween.To(instance.UpdateValues, timeScale, 1f, fadeTime));
	}

	public static void StartWarp(float timeScale, float fadeTime, AudioSource[] sounds = null, Ease easeType = Ease.OutQuint)
	{
		allSounds = sounds;

		DOTween.To(instance.UpdateValues, Time.timeScale, timeScale, fadeTime)
			.SetEase(easeType)
			.SetUpdate(true);
	}

	public static void EndWarp(float fadeTime, AudioSource[] sounds = null, Ease easeType = Ease.InSine)
	{
		allSounds = sounds;

		DOTween.To(instance.UpdateValues, Time.timeScale, 1f, fadeTime)
			.SetEase(easeType)
			.SetUpdate(true);
	}

	private void UpdateValues(float newValue)
	{
		Time.timeScale = newValue;
		Time.fixedDeltaTime = defaultFixedTimestep * newValue;

		if (allSounds != null)
		{
			foreach (AudioSource sound in allSounds)
			{
				sound.pitch = newValue;
			}
		}
	}
}
