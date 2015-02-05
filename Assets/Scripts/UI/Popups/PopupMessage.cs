using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PopupMessage : MonoBehaviour
{
	private static PopupMessage instance;

	public PopupMessageInstance popupPrefab;
	public float textBuffer = 0.4f;

	public static PopupMessage Instance
	{
		get { return instance; }
	}

	private void Awake()
	{
		instance = this;
	}

	public void CreatePopup(Vector3 newPosition, string newText = "", Sprite newImage = null, bool followPlayer = false)
	{
		PopupMessageInstance popupInstance = Instantiate(popupPrefab, newPosition, Quaternion.identity) as PopupMessageInstance;
		popupInstance.transform.SetParent(transform);
		popupInstance.transform.SetAsLastSibling();
		popupInstance.followPlayer = followPlayer;

		Image instanceImage = popupInstance.GetComponentInChildren<Image>();
		Text instanceText = popupInstance.GetComponentInChildren<Text>();

		if (newImage != null)
		{
			instanceImage.sprite = newImage;
			instanceImage.rectTransform.sizeDelta = newImage.bounds.size;

			if (newText == "")
			{
				instanceImage.rectTransform.pivot = new Vector2(0.5f, 0f);
			}
		}

		if (newText != "")
		{
			instanceText.text = newText;

			if (newImage == null)
			{
				instanceText.alignment = TextAnchor.LowerCenter;
			}
		}

		if (newImage != null && newText != "")
		{
			float imageXOffset = (instanceImage.rectTransform.sizeDelta.x + textBuffer + (instanceText.preferredWidth / 10f)) / 2f;
			float textXOffset = imageXOffset - textBuffer - instanceImage.rectTransform.sizeDelta.x;
			float textYOffset = instanceImage.rectTransform.sizeDelta.y / 2f;

			instanceImage.rectTransform.anchoredPosition = new Vector2(-imageXOffset, 0f);
			instanceText.rectTransform.anchoredPosition = new Vector2(-textXOffset, textYOffset);
		}
	}
}
