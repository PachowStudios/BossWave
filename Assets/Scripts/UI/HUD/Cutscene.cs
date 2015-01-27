using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Cutscene : MonoBehaviour
{
	private static bool cutsceneActive = false;

	private static Animator anim;

	void Awake()
	{
		anim = GetComponent<Animator>();
	}

	public static void StartCutscene(bool disableInput = false)
	{
		if (!cutsceneActive)
		{
			if (disableInput)
			{
				PlayerControl.instance.DisableInput();
			}

			cutsceneActive = true;
			anim.SetTrigger("Start");
		}
		else
		{
			Debug.LogError("Tried to start a cutscene when one was already active!");
		}
	}

	public static void EndCutscene(bool enableInput = false)
	{
		if (cutsceneActive)
		{
			if (enableInput && PlayerControl.instance.IsInputDisabled())
			{
				PlayerControl.instance.EnableInput();
			}

			cutsceneActive = false;
			anim.SetTrigger("End");
		}
		else
		{
			Debug.LogError("Tried to end a cutscene when there weren't any active!");
		}
	}
}
