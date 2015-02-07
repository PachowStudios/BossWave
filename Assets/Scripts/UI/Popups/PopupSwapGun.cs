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

		newPosition.z = transform.position.z;

		PopupSwapGunInstance popupInstance = Instantiate(popupPrefab, newPosition, Quaternion.identity) as PopupSwapGunInstance;
		currentPopup = popupInstance;
		popupInstance.transform.SetParent(transform);
		popupInstance.transform.SetAsFirstSibling();
		popupInstance.transform.localScale = popupPrefab.transform.localScale;
		popupInstance.newGunPrefab = newGunPrefab;
	}

	public void ClearPopup()
	{
		if (currentPopup != null)
		{
			currentPopup.DisappearNoSelection();
		}
	}
}
