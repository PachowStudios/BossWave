using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using DG.Tweening;

public class Timer : MonoBehaviour
{
	#region Fields
	private static Timer instance;

	public float fadeTime = 0.5f;
	public float showY = -1.5f;
	public float hideY = 3f;

	private bool showing = false;
	private Tween tween = null;
	private float time = 0f;

	private Text text;
	private CanvasGroup canvasGroup;
	#endregion

	#region Public Properties
	public static Timer Instance
	{ get { return instance; } }

	public float Time
	{ get { return time; } }
	#endregion

	#region MonoBehaviour
	private void Awake()
	{
		instance = this;

		text = GetComponent<Text>();
		canvasGroup = GetComponent<CanvasGroup>();
	}

	private void Update()
	{
		text.text = time.ToString("F2");
	}
	#endregion

	#region Public Methods
	public void StartTimer(float startTime, float? duration = null, bool hideOnComplete = false, Action onCompleteCallback = null)
	{
		if (tween != null && tween.IsPlaying())
			tween.Kill();

		time = startTime;

		tween = DOTween.To(() => time, t => time = t, 0f, duration ?? startTime)
			.SetEase(Ease.Linear);

		if (onCompleteCallback != null)
			tween.OnComplete(() =>
			{
				onCompleteCallback();

				if (hideOnComplete)
					Hide();
			});

		Show();
	}

	public void StopTimer()
	{
		if (tween == null)
			return;

		tween.Kill();
	}

	public void Show(float time = 0f)
	{
		if (showing)
			return;

		time = (time == 0f) ? fadeTime : time;
		canvasGroup.alpha = 1f;
		text.rectTransform.DOAnchorPos(new Vector2(text.rectTransform.anchoredPosition.x, showY), time);

		showing = true;
	}

	public void Hide(float time = 0f)
	{
		if (!showing)
			return;

		time = (time == 0f) ? fadeTime : time;
		text.rectTransform.DOAnchorPos(new Vector2(text.rectTransform.anchoredPosition.x, hideY), time)
			.OnComplete(() => canvasGroup.alpha = 0f);

		showing = false;
	}
	#endregion
}
