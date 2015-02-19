using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class BossWaveTimer : MonoBehaviour
{
	#region Fields
	private static BossWaveTimer instance;

	public float fadeTime = 0.5f;
	public float showY = -1.5f;
	public float hideY = 3f;

	private bool showing = false;

	private Text text;
	private CanvasGroup canvasGroup;
	#endregion

	#region Public Properties
	public static BossWaveTimer Instance
	{
		get { return instance; }
	}

	public float Timer
	{ get; set; }
	#endregion

	#region MonoBehaviour
	private void Awake()
	{
		instance = this;

		text = GetComponent<Text>();
		canvasGroup = GetComponent<CanvasGroup>();

		Timer = 0f;
	}

	private void OnGUI()
	{
		text.text = Timer.ToString("F2");
	}
	#endregion

	#region Public Methods
	public void Show(float time = 0f)
	{
		if (!showing)
		{
			time = (time == 0f) ? fadeTime : time;
			canvasGroup.alpha = 1f;
			text.rectTransform.DOAnchorPos(new Vector2(text.rectTransform.anchoredPosition.x, showY), time);

			showing = true;
		}
	}

	public void Hide(float time = 0f)
	{
		if (showing)
		{
			time = (time == 0f) ? fadeTime : time;
			text.rectTransform.DOAnchorPos(new Vector2(text.rectTransform.anchoredPosition.x, hideY), time)
				.OnComplete(() => canvasGroup.alpha = 0f);

			showing = false;
		}
	}
	#endregion
}
