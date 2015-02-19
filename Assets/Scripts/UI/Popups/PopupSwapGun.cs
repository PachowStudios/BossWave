using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PopupSwapGun : MonoBehaviour
{
	#region Fields
	private static PopupSwapGun instance;

	public PopupSwapGunInstance popupPrefab;

	private PopupSwapGunInstance currentPopup = null;
	#endregion

	#region Public Properties
	public static PopupSwapGun Instance
	{
		get { return instance; }
	}

	public bool ShowingPopup
	{
		get { return currentPopup != null; }
	}
	#endregion

	#region MonoBehaviour
	private void Awake()
	{
		instance = this;
	}
	#endregion

	#region Public Methods
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
	#endregion
}
