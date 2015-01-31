using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public class HealthDisplay : MonoBehaviour
{
	private static HealthDisplay instance;

	public Image face;
	public Image bar;
	public Text score;
	public Text microchips;

	public float fadeTime = 0.5f;
	public int scoreDigits = 12;
	public int microchipsDigits = 5;
	public float textDamping = 1f;
	public float healthDamping = 1f;
	public List<Sprite> healthFaces;

	private bool showing = true;

	private float healthPercent;
	private float originalHealthWidth;
	private Vector2 healthVelocity = Vector2.zero;

	private float scoreValue = 0f;
	private float scoreVelocity = 0f;

	private float microchipsValue = 0f;
	private float microchipsVelocity = 0f;

	private CanvasGroup canvasGroup;

	public static HealthDisplay Instance
	{
		get { return instance; }
	}

	private void Awake()
	{
		instance = this;

		canvasGroup = GetComponent<CanvasGroup>();

		originalHealthWidth = bar.rectTransform.sizeDelta.x;
	}

	private void OnGUI()
	{
		healthPercent = Mathf.Clamp(PlayerControl.Instance.Health / PlayerControl.Instance.maxHealth, 0f, 1f);

		face.sprite = healthFaces[Mathf.Clamp((int)(healthFaces.Count * healthPercent), 0, healthFaces.Count - 1)];

		bar.rectTransform.sizeDelta = Vector2.SmoothDamp(bar.rectTransform.sizeDelta, new Vector2(originalHealthWidth * healthPercent, bar.rectTransform.sizeDelta.y), ref healthVelocity, healthDamping);
		scoreValue = Mathf.SmoothDamp(scoreValue, PlayerControl.Instance.Score, ref scoreVelocity, textDamping);
		microchipsValue = Mathf.SmoothDamp(microchipsValue, PlayerControl.Instance.Microchips, ref microchipsVelocity, textDamping);

		score.text = Mathf.RoundToInt(scoreValue).ToString().PadLeft(scoreDigits, '0');
		microchips.text = Mathf.RoundToInt(microchipsValue).ToString().PadLeft(microchipsDigits, '0');
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
