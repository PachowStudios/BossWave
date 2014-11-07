using UnityEngine;
using System.Collections;

public class PopupSwapGunInstance : MonoBehaviour 
{
	public float appearTime = 0.25f;
	public float disappearTime = 0.5f;
	public float disappearScale = 1.5f;
	public float distance = 1f;

	[HideInInspector]
	public Gun newGunPrefab;

	private float yOffset = 0f;
	private bool selectionMade = false;

	private CanvasGroup canvasGroup;
	private CanvasGroup newGun;
	private CanvasGroup oldGun;

	void Awake()
	{
		canvasGroup = GetComponent<CanvasGroup>();
		newGun = transform.FindChild("New").GetComponent<CanvasGroup>();
		oldGun = transform.FindChild("Old").GetComponent<CanvasGroup>();
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
		transform.position = PlayerControl.instance.popupMessagePoint.position + new Vector3(0f, yOffset, 0f);
	}

	private void SwapGuns(bool swap)
	{
		if (swap)
		{
			PlayerControl.instance.SwapGun(newGunPrefab);
		}

		Disappear(swap);
	}

	private void Appear()
	{
		iTween.ValueTo(gameObject, iTween.Hash("from", 0f,
											   "to", 1f,
											   "time", appearTime * 0.6f,
											   "easetype", iTween.EaseType.easeInQuad,
											   "onupdate", "UpdateAlpha"));

		iTween.ValueTo(gameObject, iTween.Hash("from", -distance,
											   "to", 0f,
											   "time", appearTime,
											   "easetype", iTween.EaseType.easeOutBack,
											   "onupdate", "UpdatePosition"));
	}

	private void Disappear(bool swap)
	{
		string selected = swap ? "New" : "Old";
		string notSelected = swap ? "Old" : "New";

		iTween.ValueTo(gameObject, iTween.Hash("from", 1f,
												"to", 0f,
												"delay", disappearTime * 0.4f,
												"time", disappearTime * 0.6f,
												"easetype", iTween.EaseType.easeInQuad,
												"onupdate", "Update" + selected + "Alpha"));

		iTween.ValueTo(gameObject, iTween.Hash("from", 1f,
												"to", disappearScale,
												"time", disappearTime,
												"easetype", iTween.EaseType.easeInCubic,
												"onupdate", "Update" + selected + "Scale"));

		iTween.ValueTo(gameObject, iTween.Hash("from", 1f,
												"to", 0f,
												"time", disappearTime * 0.5f,
												"easetype", iTween.EaseType.easeInQuad,
												"onupdate", "Update" + notSelected + "Alpha"));

		Destroy(gameObject, disappearTime);
	}

	private void UpdateAlpha(float newValue)
	{
		canvasGroup.alpha = newValue;
	}

	private void UpdatePosition(float newValue)
	{
		yOffset = newValue;
	}

	private void UpdateNewAlpha(float newValue)
	{
		newGun.alpha = newValue;
	}

	private void UpdateNewScale(float newValue)
	{
		newGun.transform.localScale = new Vector3(newValue, newValue, 1f);
	}

	private void UpdateOldAlpha(float newValue)
	{
		oldGun.alpha = newValue;
	}

	private void UpdateOldScale(float newValue)
	{
		oldGun.transform.localScale = new Vector3(newValue, newValue, 1f);
	}
}
