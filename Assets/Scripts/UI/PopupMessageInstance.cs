using UnityEngine;
using System.Collections;

public class PopupMessageInstance : MonoBehaviour 
{
	public float time = 1f;
	public float distance = 1f;

	[HideInInspector]
	public bool followPlayer = false;

	private Vector3 startingPosition;
	private float yOffset = 0f;

	private CanvasGroup canvasGroup;

	void Awake()
	{
		startingPosition = transform.position;

		canvasGroup = GetComponent<CanvasGroup>();
	}

	void Start()
	{
		Appear();
	}

	void FixedUpdate()
	{
		if (followPlayer)
		{
			transform.position = PlayerControl.instance.popupMessagePoint.position + new Vector3(0f, yOffset, 0f);
		}

		else
		{
			transform.position = startingPosition + new Vector3(0f, yOffset, 0f);
		}
	}

	public void Appear()
	{
		iTween.ValueTo(gameObject, iTween.Hash("from", 0f,
											   "to", 1f,
											   "time", time * 0.15f,
										       "easetype", iTween.EaseType.easeInQuad,
											   "onupdate", "UpdateAlpha"));

		iTween.ValueTo(gameObject, iTween.Hash("from", -distance,
											   "to", 0f,
											   "time", time * 0.25f,
											   "easetype", iTween.EaseType.easeOutBack,
											   "onupdate", "UpdatePosition"));

		iTween.ValueTo(gameObject, iTween.Hash("delay", time * 0.75f,
											   "from", 1f,
											   "to", 0f,
											   "time", time * 0.25f,
											   "easetype", iTween.EaseType.easeInQuad,
											   "onupdate", "UpdateAlpha"));

		Destroy(gameObject, time);
	}

	private void UpdateAlpha(float newValue)
	{
		canvasGroup.alpha = newValue;
	}

	private void UpdatePosition(float newValue)
	{
		yOffset = newValue;
	}
}
