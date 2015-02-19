using UnityEngine;
using System.Collections;
using DG.Tweening;

public class TimeWarpEffect : MonoBehaviour
{
	#region Fields
	private static TimeWarpEffect instance;

	public float defaultFixedTimestep = 0.0166667f;

	private AudioSource[] allSounds;
	#endregion

	#region Public Properties
	public static TimeWarpEffect Instance
	{
		get { return instance; }
	}

	public float DefaultFixedTimestep
	{
		get { return defaultFixedTimestep; }
	}
	#endregion

	#region MonoBehaviour
	private void Awake()
	{
		instance = this;
		allSounds = null;
	}
	#endregion

	#region Internal Helper Methods
	private void UpdateValues(float newValue)
	{
		Time.timeScale = newValue;

		if (newValue <= 1f)
		{
			Time.fixedDeltaTime = defaultFixedTimestep * newValue;
		}

		if (allSounds != null)
		{
			foreach (AudioSource sound in allSounds)
			{
				sound.pitch = newValue;
			}
		}
	}
	#endregion

	#region Public Methods
	public void Warp(float timeScale, float length, float fadeTime, AudioSource[] sounds = null)
	{
		allSounds = sounds;

		Sequence sequence = DOTween.Sequence().SetEase(Ease.OutQuint).SetUpdate(true);
		sequence.Append(DOTween.To(UpdateValues, Time.timeScale, timeScale, fadeTime))
			.AppendInterval(length)
			.Append(DOTween.To(UpdateValues, timeScale, 1f, fadeTime));
	}

	public void StartWarp(float timeScale, float fadeTime, AudioSource[] sounds = null, Ease easeType = Ease.OutQuint)
	{
		allSounds = sounds;

		DOTween.To(UpdateValues, Time.timeScale, timeScale, fadeTime)
			.SetEase(easeType)
			.SetUpdate(true);
	}

	public void EndWarp(float fadeTime, AudioSource[] sounds = null, Ease easeType = Ease.InSine)
	{
		allSounds = sounds;

		DOTween.To(UpdateValues, Time.timeScale, 1f, fadeTime)
			.SetEase(easeType)
			.SetUpdate(true);
	}
	#endregion
}
