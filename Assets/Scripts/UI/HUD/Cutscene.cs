using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public class Cutscene : MonoBehaviour
{
	private static Cutscene instance;

	public float fadeTime = 0.5f;
	public float showY = 2f;
	public float hideY = 0f;
	public CanvasGroup canvasGroup;
	public RectTransform topBar;
	public RectTransform bottomBar;

	private bool showing = false;

	public static Cutscene Instance
	{
		get { return instance; }
	}

	private void Awake()
	{
		instance = this;
	}

	public void Show(bool disableInput = false)
	{
		if (!showing)
		{
			canvasGroup.DOFade(1f, fadeTime);
			topBar.DOAnchorPos(new Vector2(topBar.anchoredPosition.x, showY), fadeTime);
			bottomBar.DOAnchorPos(new Vector2(bottomBar.anchoredPosition.x, -showY), fadeTime);

			HealthDisplay.Instance.Hide(fadeTime);
			ComboMeter.Instance.Hide(fadeTime);
			SecondaryShotBox.Instance.Hide(fadeTime, true);

			if (disableInput)
			{
				PlayerControl.Instance.DisableInput();
			}

			showing = true;
		}
	}

	public void Hide(bool enableInput = false)
	{
		if (showing)
		{
			canvasGroup.DOFade(0f, fadeTime);
			topBar.DOAnchorPos(new Vector2(topBar.anchoredPosition.x, hideY), fadeTime);
			bottomBar.DOAnchorPos(new Vector2(bottomBar.anchoredPosition.x, hideY), fadeTime);

			HealthDisplay.Instance.Show(fadeTime);
			ComboMeter.Instance.Show(fadeTime);
			SecondaryShotBox.Instance.Show(fadeTime, true);

			if (enableInput && PlayerControl.Instance.IsInputDisabled())
			{
				PlayerControl.Instance.EnableInput();
			}

			showing = false;
		}
	}

	
}
