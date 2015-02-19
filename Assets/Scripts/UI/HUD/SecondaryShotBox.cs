using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class SecondaryShotBox : MonoBehaviour
{
	#region Fields
	private static SecondaryShotBox instance;

	public Image bar;
	public Image icon;

	public float fadeTime = 0.5f;
	public float showY = 1f;
	public float hideY = -3f;
	public float cooldownDamping = 0.5f;
	public Gradient barGradient;

	private bool showing = false;
	private bool overrideShowing = false;

	private float cooldownPercent;
	private float originalCooldownWidth;
	private Vector2 cooldownVelocity = Vector3.zero;

	private CanvasGroup canvasGroup;
	private RectTransform rectTransform;
	#endregion

	#region Public Properties
	public static SecondaryShotBox Instance
	{
		get { return instance; }
	}
	#endregion

	#region MonoBehaviour
	private void Awake()
	{
		instance = this;

		canvasGroup = GetComponent<CanvasGroup>();
		rectTransform = GetComponent<RectTransform>();

		originalCooldownWidth = bar.rectTransform.sizeDelta.x;
	}

	private void OnGUI()
	{
		if (PlayerControl.Instance.Gun.secondaryShot && !overrideShowing)
		{
			if (!showing)
			{
				Show();
			}

			cooldownPercent = Mathf.Clamp(PlayerControl.Instance.Gun.secondaryTimer / PlayerControl.Instance.Gun.secondaryCooldown, 0f, 1f);
			icon.sprite = PlayerControl.Instance.Gun.secondaryIcon;
		}
		else
		{
			if (showing)
			{
				Hide();
				overrideShowing = false;
			}

			cooldownPercent = 0f;
		}

		bar.rectTransform.sizeDelta = Vector2.SmoothDamp(bar.rectTransform.sizeDelta, new Vector2(originalCooldownWidth * cooldownPercent, bar.rectTransform.sizeDelta.y), ref cooldownVelocity, cooldownDamping);
		bar.color = barGradient.Evaluate(cooldownPercent);
	}
	#endregion

	#region Public Methods
	public void Show(float time = 0f, bool fade = false)
	{
		if (!showing)
		{
			time = (time == 0f) ? fadeTime : time;

			if (fade)
			{
				canvasGroup.DOFade(1f, time);	
			}
			else
			{
				rectTransform.DOAnchorPos(new Vector2(rectTransform.anchoredPosition.x, showY), time);
			}

			showing = true;
			overrideShowing = false;
		}
	}

	public void Hide(float time = 0f, bool fade = false)
	{
		if (showing)
		{
			time = (time == 0f) ? fadeTime : time;

			if (fade)
			{
				canvasGroup.DOFade(0f, time);
			}
			else
			{
				rectTransform.DOAnchorPos(new Vector2(rectTransform.anchoredPosition.x, hideY), time);
			}
			
			showing = false;
			overrideShowing = true;
		}
	}
	#endregion
}
