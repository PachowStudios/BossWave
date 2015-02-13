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

	public CanvasGroup newCanvasGroup;
	public CanvasGroup oldCanvasGroup;
	public Image newGun;
	public Image oldGun;
	public Text newStats;
	public Text oldStats;
	public Text timerText;

	[HideInInspector]
	public Gun newGunPrefab;

	private float timer;
	private float yOffset = 0f;
	private bool selectionMade = false;

	private CanvasGroup canvasGroup;

	private void Awake()
	{
		canvasGroup = GetComponent<CanvasGroup>();

		timer = timeLimit;
	}

	private void Start()
	{
		newGun.sprite = newGunPrefab.SpriteRenderer.sprite;
		newStats.text = newGunPrefab.projectile.damage + "\n" +
						newGunPrefab.FireRate + "\n" +
						newGunPrefab.projectile.knockback.x;

		Appear();
	}

	private void Update()
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

	private void FixedUpdate()
	{
		transform.position = PlayerControl.Instance.PopupMessagePoint + new Vector3(0f, yOffset, 0f);

		if (timer > 0)
		{
			timer -= Time.fixedDeltaTime;
			timerText.text = timer.ToString("F2");
		}
		else if (!selectionMade)
		{
			selectionMade = true;
			SwapGuns(swapAfterTime);
		}
	}

	private void OnGUI()
	{
		if (!selectionMade)
		{
			oldGun.sprite = PlayerControl.Instance.Gun.SpriteRenderer.sprite;
			oldStats.text = PlayerControl.Instance.Gun.projectile.damage + "\n" +
							PlayerControl.Instance.Gun.FireRate + "\n" +
							PlayerControl.Instance.Gun.projectile.knockback.x;
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
			.SetEase(Ease.OutQuint)
			.OnComplete(() => Destroy(gameObject));
	}

	private void SwapGuns(bool swap)
	{
		if (swap)
		{
			PlayerControl.Instance.AddGun(newGunPrefab);
			CurrentGunName.Instance.Show(newGunPrefab.gunName, newGunPrefab.Color);
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

		newCanvasGroup.DOFade(0f, disappearTime * (swap ? 0.6f : 0.5f))
			.SetEase(Ease.InQuad)
			.SetDelay(disappearTime * (swap ? 0.4f : 0f));
		oldCanvasGroup.DOFade(0f, disappearTime * (swap ? 0.5f : 0.6f))
			.SetEase(Ease.InQuad)
			.SetDelay(disappearTime * (swap ? 0f : 0.4f));
		selectedTransform.DOScale(new Vector3(disappearScale, disappearScale, 1f), disappearTime)
			.SetEase(Ease.InCubic);
		timerText.DOFade(0f, disappearTime * 0.5f)
			.SetEase(Ease.InQuad);

		Destroy(gameObject, disappearTime);
	}
}
