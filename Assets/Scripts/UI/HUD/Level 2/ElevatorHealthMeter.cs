using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public sealed class ElevatorHealthMeter : MonoBehaviour
{
	#region Fields
	private static ElevatorHealthMeter instance;

	public Mask healthMask;

	public float fadeTime = 0.5f;
	public float healthDamping = 0.5f;

	private bool showing = true;

	private float originalHealthHeight;
	private Vector2 healthVelocity = Vector2.zero;

	private CanvasGroup canvasGroup;
	#endregion

	#region Public Properties
	public static ElevatorHealthMeter Instance
	{ get { return instance; } }
	#endregion

	#region MonoBehaviour
	private void Awake()
	{
		instance = this;

		canvasGroup = GetComponent<CanvasGroup>();

		originalHealthHeight = healthMask.rectTransform.sizeDelta.y;
	}

	private void Update()
	{
		healthMask.rectTransform.sizeDelta = Vector2.SmoothDamp(healthMask.rectTransform.sizeDelta,
																new Vector2(healthMask.rectTransform.sizeDelta.x,
																			originalHealthHeight * Level2.Instance.elevator.HealthPercent),
																ref healthVelocity,
																healthDamping);
	}
	#endregion

	#region Public Methods
	public void Show(float fadeTime = 0f)
	{
		if (showing)
			return;

		fadeTime = (fadeTime == 0f) ? this.fadeTime : fadeTime;
		canvasGroup.DOFade(1f, fadeTime);

		showing = true;
	}

	public void Hide(float fadeTime = 0f)
	{
		if (!showing)
			return;

		fadeTime = (fadeTime == 0f) ? this.fadeTime : fadeTime;
		canvasGroup.DOFade(0f, fadeTime);

		showing = false;
	}
	#endregion
}
