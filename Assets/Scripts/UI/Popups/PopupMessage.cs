using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PopupMessage : MonoBehaviour
{
	private static PopupMessage instance;

	public PopupMessageInstance popupPrefab;
	public float textBuffer = 5f;

	public static PopupMessage Instance
	{
		get { return instance; }
	}

	private void Awake()
	{
		instance = this;
	}

	public void CreatePopup(Vector3 newPosition, string newText, Sprite newImage = null, bool followPlayer = false)
	{
		newPosition.z = transform.position.z;

		PopupMessageInstance popupInstance = Instantiate(popupPrefab, newPosition, Quaternion.identity) as PopupMessageInstance;
		popupInstance.transform.SetParent(transform);
		popupInstance.transform.SetAsFirstSibling();
		popupInstance.followPlayer = followPlayer;

		RectTransform instanceRect = popupInstance.GetComponent<RectTransform>();
		Image instanceImage = popupInstance.GetComponentInChildren<Image>();
		Text instanceText = popupInstance.GetComponentInChildren<Text>();

		float imageWidth;
		float currentTextBuffer;

		if (newImage != null)
		{
			instanceImage.sprite = newImage;
			imageWidth = instanceRect.sizeDelta.y * (instanceImage.sprite.bounds.size.x / instanceImage.sprite.bounds.size.y);
			currentTextBuffer = textBuffer;
		}
		else
		{
			imageWidth = 0f;
			currentTextBuffer = 0f;
		}

		instanceText.text = newText;

		instanceText.rectTransform.offsetMin = new Vector2(imageWidth + currentTextBuffer, 0);
		popupInstance.transform.localScale = popupPrefab.transform.localScale;

		instanceRect.sizeDelta = new Vector2(imageWidth + currentTextBuffer + instanceText.preferredWidth, instanceRect.sizeDelta.y);
	}
}
