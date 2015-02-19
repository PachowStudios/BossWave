using UnityEngine;
using System.Collections;
using DG.Tweening;

public class ComboMeter : MonoBehaviour
{
	#region Fields
	private static ComboMeter instance;

	public float fadeTime = 0.5f;

	private bool showing = true;

	private CanvasGroup canvasGroup;
	private Animator anim;
	#endregion

	#region Public Properties
	public static ComboMeter Instance
	{
		get { return instance; }
	}
	#endregion

	#region MonoBehaviour
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
	#endregion

	#region Public Methods
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
	#endregion
}
