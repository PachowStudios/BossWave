using UnityEngine;
using System.Collections;

public class PopupMessageInstance : MonoBehaviour 
{
	private RectTransform rectTransform;
	private CanvasGroup canvasGroup;

	void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
		canvasGroup = GetComponent<CanvasGroup>();
	}

	public void Animate(float time, float distance)
	{
		iTween.ValueTo(gameObject, iTween.Hash("from", 0f,
											   "to", 1f,
											   "time", time * 0.15f,
										       "easetype", iTween.EaseType.easeInQuad,
											   "onupdate", "UpdateAlpha"));

		iTween.ValueTo(gameObject, iTween.Hash("from", rectTransform.anchoredPosition.y,
											   "to", rectTransform.anchoredPosition.y + distance,
											   "time", time * 0.25f,
											   "easetype", iTween.EaseType.easeOutBack,
											   "onupdate", "UpdatePosition"));

		iTween.ValueTo(gameObject, iTween.Hash("delay", time * 0.75f,
											   "from", 1f,
											   "to", 0f,
											   "time", time * 0.25f,
											   "easetype", iTween.EaseType.easeInQuad,
											   "onupdate", "UpdateAlpha",
											   "oncomplete", "Destroy",
											   "oncompleteparams", gameObject));
	}

	private void UpdateAlpha(float newValue)
	{
		canvasGroup.alpha = newValue;
	}

	private void UpdatePosition(float newValue)
	{
		rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, newValue);
	}
}
