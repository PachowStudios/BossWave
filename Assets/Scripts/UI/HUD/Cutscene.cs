using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Cutscene : MonoBehaviour
{
	private static Cutscene instance;

	public float fadeTime = 0.5f;

	private bool cutsceneActive = false;
	private Animator anim;

	public static Cutscene Instance
	{
		get { return instance; }
	}

	private void Awake()
	{
		instance = this;

		anim = GetComponent<Animator>();

		cutsceneActive = false;
	}

	public void StartCutscene(bool disableInput = false)
	{
		if (!cutsceneActive)
		{
			if (disableInput)
			{
				PlayerControl.Instance.DisableInput();
			}

			cutsceneActive = true;
			anim.SetTrigger("Start");
		}
		else
		{
			Debug.LogError("Tried to start a cutscene when one was already active!");
		}
	}

	public void EndCutscene(bool enableInput = false)
	{
		if (cutsceneActive)
		{
			if (enableInput && PlayerControl.Instance.IsInputDisabled())
			{
				PlayerControl.Instance.EnableInput();
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
