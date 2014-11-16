using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PopupMessage : MonoBehaviour
{
	private static PopupMessage instance;

	public PopupMessageInstance popupPrefab;
	public float textBuffer = 5f;

	void Awake()
	{
		instance = this;
	}

	public static void CreatePopup(Vector3 newPosition, string newText, Sprite newImage = null, bool followPlayer = false)
	{
		newPosition.z = instance.transform.position.z;

		PopupMessageInstance popupInstance = Instantiate(instance.popupPrefab, newPosition, Quaternion.identity) as PopupMessageInstance;
		popupInstance.transform.parent = instance.transform;
		popupInstance.transform.SetAsFirstSibling();
		popupInstance.followPlayer = followPlayer;

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
	}
}
