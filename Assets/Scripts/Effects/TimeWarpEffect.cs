using UnityEngine;
using System.Collections;

public class TimeWarpEffect : MonoBehaviour
{
	static private TimeWarpEffect instance;
	static private AudioSource[] allSounds;

	void Awake()
	{
		instance = this;
		allSounds = null;
	}

	public static void Warp(float timeScale, float length, float fadeTime, AudioSource[] sounds = null)
	{
		allSounds = sounds;
		iTween.ValueTo(instance.gameObject, iTween.Hash("from", Time.timeScale,
														"to", timeScale,
														"time", fadeTime,
														"easetype", iTween.EaseType.easeOutQuint,
														"onupdate", "UpdateValues",
														"oncomplete", "RevertWarp",
														"oncompleteparams", iTween.Hash("length", length, "fadeTime", fadeTime), 
														"ignoretimescale", true));
	}

	public static void StartWarp(float timeScale, float fadeTime, AudioSource[] sounds = null)
	{
		allSounds = sounds;
		iTween.ValueTo(instance.gameObject, iTween.Hash("from", Time.timeScale,
														"to", timeScale,
													    "time", fadeTime,
														"easetype", iTween.EaseType.easeOutQuint,
														"onupdate", "UpdateValues",
														"ignoretimescale", true));
	}

	public static void EndWarp(float fadeTime, AudioSource[] sounds = null)
	{
		allSounds = sounds;
		iTween.ValueTo(instance.gameObject, iTween.Hash("from", Time.timeScale,
														"to", 1f,
														"time", fadeTime,
														"easetype", iTween.EaseType.easeInSine,
														"onupdate", "UpdateValues",
														"ignoretimescale", true));
	}

	private void RevertWarp(Hashtable vals)
	{
		iTween.ValueTo(instance.gameObject, iTween.Hash("delay", vals["length"],
														"from", Time.timeScale,
														"to", 1f,
														"time", vals["fadeTime"],
														"easetype", iTween.EaseType.easeOutQuint,
														"onupdate", "UpdateValues",
														"ignoretimescale", true));
	}

	private void UpdateValues(float newValue)
	{
		Time.timeScale = newValue;

		if (allSounds != null)
		{
			foreach (AudioSource sound in allSounds)
			{
				sound.pitch = newValue;
			}
		}
	}
}
