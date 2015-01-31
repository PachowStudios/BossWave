using UnityEngine;
using System.Collections;
using DG.Tweening;

public class ComboMeter : MonoBehaviour 
{
	private static ComboMeter instance;

	public float fadeTime = 0.5f;

	private bool showing = true;

	private CanvasGroup canvasGroup;
	private Animator anim;

	public static ComboMeter Instance
	{
		get { return instance; }
	}

	private void Awake()
	{
		instance = this;

		canvasGroup = GetComponent<CanvasGroup>();
		anim = GetComponent<Animator>();
	}

	private void OnGUI()
	{
		anim.SetInteger("Combo", PlayerControl.Instance.Combo);
	}

	public void Show(float time = 0f)
	{
		if (!showing)
		{
			time = (time == 0f) ? fadeTime : time;
			canvasGroup.DOFade(1f, time);

			showing = true;
		}
	}

	public void Hide(float time = 0f)
	{
		if (showing)
		{
			time = (time == 0f) ? fadeTime : time;
			canvasGroup.DOFade(0f, time);

			showing = false;
		}
	}
}
