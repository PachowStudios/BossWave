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
	public Color flashColor = Color.red;
	public float flashSpeed = 0.5f;

	private bool showing = false;
	private float time = 0f;
	private Tween timerTween = null;
	private Tween flashTween = null;

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
		if (timerTween != null)
			text.text = time.ToString("F2");
	}
	#endregion

	#region Public Methods
	public void StartTimer(float startTime, float? duration = null, float? flashTime = null, bool hideOnComplete = false, Action onCompleteCallback = null)
	{
		if (timerTween != null && timerTween.IsPlaying())
			timerTween.Kill();

		if (flashTween != null && flashTween.IsPlaying())
			flashTween.Kill();

		time = startTime;
		duration = duration ?? startTime;

		timerTween = DOTween.To(() => time, t => time = t, 0f, duration.Value)
			.SetEase(Ease.Linear);

		if (flashTime.HasValue)
			flashTween = text.DOColor(flashColor, flashSpeed)
				.SetEase(Ease.InSine)
				.SetDelay(duration.Value - flashTime.Value)
				.SetLoops((int)(flashTime.Value / flashSpeed) - 1, LoopType.Yoyo)
				.OnComplete(() =>
					{
						text.DOColor(flashColor, time)
							.SetEase(Ease.InSine);
					});

		timerTween.OnComplete(() =>
		{
			if (onCompleteCallback != null)
				onCompleteCallback();

			if (hideOnComplete)
				Hide();
		});

		Show();
	}

	public void StopTimer(bool hide = false)
	{
		if (timerTween != null && timerTween.IsPlaying())
			timerTween.Kill();

		if (flashTween != null && timerTween.IsPlaying())
			flashTween.Kill();

		if (hide)
			Hide();
	}

	public void Show(float time = 0f)
	{
		if (showing)
			return;

		time = (time == 0f) ? fadeTime : time;
		canvasGroup.alpha = 1f;
		text.color = Color.white;
		text.rectTransform.DOAnchorPos(new Vector2(text.rectTransform.anchoredPosition.x, showY), time);

		showing = true;
	}

	public void Hide(float time = 0f)
	{
		if (!showing)
			return;

		time = (time == 0f) ? fadeTime : time;
		text.rectTransform.DOAnchorPos(new Vector2(text.rectTransform.anchoredPosition.x, hideY), time)
			.OnComplete(() =>
				{
					canvasGroup.alpha = 0f;
					text.color = Color.white;
				});

		showing = false;
	}
	#endregion
}
