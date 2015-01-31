using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;

public class PopupSwapGunInstance : MonoBehaviour 
{
	public float timeLimit = 5f;
	public bool swapAfterTime = false;
	public float appearTime = 0.25f;
	public float disappearTime = 0.5f;
	public float disappearScale = 1.5f;
	public float distance = 1f;

	[HideInInspector]
	public Gun newGunPrefab;

	private float timer;
	private float yOffset = 0f;
	private bool selectionMade = false;

	private CanvasGroup canvasGroup;
	private CanvasGroup newGun;
	private CanvasGroup oldGun;
	private CanvasGroup timerAlpha;
	private Text timerText;

	void Awake()
	{
		canvasGroup = GetComponent<CanvasGroup>();
		newGun = transform.FindChild("New").GetComponent<CanvasGroup>();
		oldGun = transform.FindChild("Old").GetComponent<CanvasGroup>();
		timerAlpha = transform.FindChild("Timer").GetComponent<CanvasGroup>();
		timerText = transform.FindChild("Timer").GetComponent<Text>();

		timer = timeLimit;
	}

	void Start()
	{
		Appear();
	}

	void Update()
	{
		if (!selectionMade)
		{
			if (CrossPlatformInputManager.GetButtonDown("NewGun"))
			{
				selectionMade = true;
				SwapGuns(true);
			}
			else if (CrossPlatformInputManager.GetButtonDown("OldGun"))
			{
				selectionMade = true;
				SwapGuns(false);
			}
		}
	}

	void FixedUpdate()
	{
		transform.position = PlayerControl.Instance.PopupMessagePoint + new Vector3(0f, yOffset, 0f);

		if (timer > 0)
		{
			timer -= Time.fixedDeltaTime;
			timerText.text = timer.ToString("F1");
		}
		else if (!selectionMade)
		{
			selectionMade = true;
			SwapGuns(swapAfterTime);
		}
	}

	public void DisappearNoSelection()
	{
		canvasGroup.alpha = 1f;
		yOffset = 0f;

		canvasGroup.DOFade(0f, appearTime * 0.6f)
			.SetEase(Ease.InQuad)
			.SetDelay(appearTime * 0.4f);
		DOTween.To(() => yOffset, x => yOffset = x, distance * 2f, appearTime)
			.SetEase(Ease.OutQuint);

		Destroy(gameObject, appearTime);
	}

	private void SwapGuns(bool swap)
	{
		if (swap)
		{
			PlayerControl.Instance.SwapGun(newGunPrefab);
		}

		Disappear(swap);
	}

	private void Appear()
	{
		canvasGroup.alpha = 0f;
		yOffset = -distance;

		canvasGroup.DOFade(1f, appearTime * 0.6f)
			.SetEase(Ease.InQuad);
		DOTween.To(() => yOffset, x => yOffset = x, 0f, appearTime)
			.SetEase(Ease.OutBack);
	}

	private void Disappear(bool swap)
	{
		Transform selectedTransform = swap ? newGun.transform : oldGun.transform;

		DOTween.To(() => newGun.alpha, x => newGun.alpha = x, 0f, disappearTime * (swap ? 0.6f : 0.5f))
			.SetEase(Ease.InQuad)
			.SetDelay(disappearTime * (swap ? 0.4f : 0f));
		DOTween.To(() => oldGun.alpha, x => oldGun.alpha = x, 0f, disappearTime * (swap ? 0.5f : 0.6f))
			.SetEase(Ease.InQuad)
			.SetDelay(disappearTime * (swap ? 0f : 0.4f));
		selectedTransform.DOScale(new Vector3(disappearScale, disappearScale, 1f), disappearTime)
			.SetEase(Ease.InCubic);
		timerAlpha.DOFade(0f, disappearTime * 0.5f)
			.SetEase(Ease.InQuad);

		Destroy(gameObject, disappearTime);
	}
}
