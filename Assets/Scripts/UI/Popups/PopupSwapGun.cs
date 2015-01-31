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

	private void Awake()
	{
		instance = this;
	}

	public void CreatePopup(Vector3 newPosition, Gun newGunPrefab)
	{
		if (currentPopup != null)
		{
			currentPopup.DisappearNoSelection();
		}

		newPosition.z = transform.position.z;

		PopupSwapGunInstance popupInstance = Instantiate(popupPrefab, newPosition, Quaternion.identity) as PopupSwapGunInstance;
		currentPopup = popupInstance;
		popupInstance.transform.SetParent(transform);
		popupInstance.transform.SetAsFirstSibling();
		popupInstance.transform.localScale = popupPrefab.transform.localScale;
		popupInstance.newGunPrefab = newGunPrefab;

		Image oldGun = popupInstance.transform.FindSubChild("Old Gun").GetComponent<Image>();
		Image newGun = popupInstance.transform.FindSubChild("New Gun").GetComponent<Image>();
		Text oldStats = popupInstance.transform.FindSubChild("Old Stats").GetComponent<Text>();
		Text newStats = popupInstance.transform.FindSubChild("New Stats").GetComponent<Text>();

		oldGun.sprite = PlayerControl.Instance.Gun.Sprite;
		newGun.sprite = newGunPrefab.Sprite;

		oldStats.text = PlayerControl.Instance.Gun.projectile.damage + "\n" +
						PlayerControl.Instance.Gun.FireRate + "\n" +
						PlayerControl.Instance.Gun.projectile.knockback.x;

		newStats.text = newGunPrefab.projectile.damage + "\n" +
						newGunPrefab.FireRate + "\n" +
						newGunPrefab.projectile.knockback.x;
	}
}
