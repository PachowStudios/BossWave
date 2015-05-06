using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public class Cutscene : MonoBehaviour
{
	#region Fields
	private static Cutscene instance;

	public float fadeTime = 0.5f;
	public float showY = 2f;
	public float hideY = 0f;
	public CanvasGroup canvasGroup;
	public RectTransform topBar;
	public RectTransform bottomBar;

	private bool showing = false;
	#endregion

	#region Public Properties
	public static Cutscene Instance
	{
		get { return instance; }
	}
	#endregion

	#region MonoBehaviour
	private void Awake()
	{
		instance = this;
	}
	#endregion

	#region Public Methods
	public void StartCutscene(bool disableInput = false)
	{
		if (showing)
			return;

		canvasGroup.DOFade(1f, fadeTime);
		topBar.DOAnchorPos(new Vector2(topBar.anchoredPosition.x, showY), fadeTime);
		bottomBar.DOAnchorPos(new Vector2(bottomBar.anchoredPosition.x, -showY), fadeTime);
		HideUI(fadeTime);

		if (disableInput)
			PlayerControl.Instance.DisableInput();

		showing = true;
	}

	public void EndCutscene(bool enableInput = false)
	{
		if (!showing)
			return;

		canvasGroup.DOFade(0f, fadeTime);
		topBar.DOAnchorPos(new Vector2(topBar.anchoredPosition.x, hideY), fadeTime);
		bottomBar.DOAnchorPos(new Vector2(bottomBar.anchoredPosition.x, hideY), fadeTime);
		ShowUI(fadeTime);

		if (enableInput && PlayerControl.Instance.IsInputDisabled)
			PlayerControl.Instance.EnableInput();

		showing = false;
	}

	public void ShowUI(float fadeTime)
	{
		HealthDisplay.Instance.Show(fadeTime);
		ComboMeter.Instance.Show(fadeTime);
		SecondaryShotBox.Instance.Show(fadeTime, true);
		LevelManager.Instance.ShowUI(fadeTime);
		//Timer.Instance.Show(fadeTime);
	}

	public void HideUI(float fadeTime)
	{
		HealthDisplay.Instance.Hide(fadeTime);
		ComboMeter.Instance.Hide(fadeTime);
		SecondaryShotBox.Instance.Hide(fadeTime, true);
		LevelManager.Instance.HideUI(fadeTime);
		//Timer.Instance.Hide(fadeTime);
	}
	#endregion
}
