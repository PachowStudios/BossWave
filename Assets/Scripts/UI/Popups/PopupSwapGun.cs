using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PopupSwapGun : MonoBehaviour 
{
	private static PopupSwapGun instance;

	public PopupSwapGunInstance popupPrefab;

	private PopupSwapGunInstance currentPopup = null;

	public static PopupSwapGun Instance
	{
		get { return instance; }
	}

	public bool ShowingPopup
	{
		get { return currentPopup != null; }
	}

	private void Awake()
	{
		instance = this;
	}

	public void CreatePopup(Vector3 newPosition, Gun newGunPrefab)
	{
		ClearPopup();

		PopupSwapGunInstance popupInstance = Instantiate(popupPrefab, newPosition, Quaternion.identity) as PopupSwapGunInstance;
		currentPopup = popupInstance;
		popupInstance.transform.SetParent(transform);
		popupInstance.transform.SetAsLastSibling();

		popupInstance.newGunPrefab = newGunPrefab;
		popupInstance.newImage.sprite = newGunPrefab.SpriteRenderer.sprite;
		popupInstance.newImage.rectTransform.sizeDelta = newGunPrefab.SpriteRenderer.sprite.bounds.size;
	}

	public void ClearPopup()
	{
		if (currentPopup != null)
		{
			currentPopup.DisappearNoSelection();
		}
	}
}
