using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PopupMessage : MonoBehaviour
{
	public PopupMessageInstance popupPrefab;
	public float textBuffer = 5f;
	public float time = 1f;
	public float distance = 50f;

	private static PopupMessage instance;

	void Awake()
	{
		instance = this;
	}

	public static void CreatePopup(Vector3 newPosition, string newText, Sprite newImage = null)
	{
		newPosition.z = instance.transform.position.z;

		PopupMessageInstance popupInstance = Instantiate(instance.popupPrefab, newPosition, Quaternion.identity) as PopupMessageInstance;
		popupInstance.transform.parent = instance.transform;
		RectTransform instanceRect = popupInstance.GetComponent<RectTransform>();
		Image instanceImage = popupInstance.GetComponentInChildren<Image>();
		Text instanceText = popupInstance.GetComponentInChildren<Text>();

		float imageWidth;
		float textBuffer;

		if (newImage != null)
		{
			instanceImage.sprite = newImage;
			imageWidth = instanceRect.sizeDelta.y * (instanceImage.sprite.bounds.size.x / instanceImage.sprite.bounds.size.y);
			textBuffer = instance.textBuffer;
		}
		else
		{
			imageWidth = 0f;
			textBuffer = 0f;
		}

		instanceText.text = newText;

		instanceText.rectTransform.offsetMin = new Vector2(imageWidth + instance.textBuffer, 0);
		popupInstance.transform.localScale = instance.popupPrefab.transform.localScale;

		instanceRect.sizeDelta = new Vector2(imageWidth + textBuffer + instanceText.preferredWidth, instanceRect.sizeDelta.y);

		popupInstance.Animate(instance.time, instance.distance);
	}
}
