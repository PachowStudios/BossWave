using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class SecondaryShotBox : MonoBehaviour 
{
	private static SecondaryShotBox instance;

	public Image bar;
	public Image icon;

	public float fadeTime = 0.5f;
	public float cooldownDamping = 0.5f;
	public Gradient barGradient;
	public float showY = 1f;
	public float hideY = -3f;

	private bool showing = false;

	private float cooldownPercent;
	private Vector3 cooldownVelocity = Vector3.zero;

	private CanvasGroup canvasGroup;
	private RectTransform rectTransform;

	public static SecondaryShotBox Instance
	{
		get { return instance; }
	}

	private void Awake()
	{
		instance = this;

		canvasGroup = GetComponent<CanvasGroup>();
		rectTransform = GetComponent<RectTransform>();
	}

	private void OnGUI()
	{
		if (PlayerControl.Instance.Gun.secondaryShot)
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
			}

			cooldownPercent = 0f;
		}

		bar.transform.localScale = Vector3.SmoothDamp(bar.transform.localScale, new Vector3(cooldownPercent, 1f, 1f), ref cooldownVelocity, cooldownDamping);
		bar.color = barGradient.Evaluate(bar.transform.localScale.x);
	}

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
				rectTransform.DOAnchorPos(new Vector2(rectTransform.anchoredPosition.x, Instance.showY), time);
			}

			showing = true;
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
				rectTransform.DOAnchorPos(new Vector2(rectTransform.anchoredPosition.x, Instance.hideY), time);
			}

			showing = false;
		}
	}
}
