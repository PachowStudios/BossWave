using UnityEngine;
using System.Collections;

public class Pause : MonoBehaviour 
{
	private bool paused = false;
	private AudioSource[] sounds;

	void Update()
	{
		if (CrossPlatformInputManager.GetButtonDown("Pause"))
		{
			sounds = FindObjectsOfType<AudioSource>();

			if (!paused)
			{
				paused = true;
				TimeWarpEffect.StartWarp(0f, sounds);
			}
			else
			{
				paused = false;
				TimeWarpEffect.EndWarp(sounds);
			}
		}
	}
}
