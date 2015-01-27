using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PopupSwapGun : MonoBehaviour 
{
	private static PopupSwapGun instance;

	public PopupSwapGunInstance popupPrefab;

	private static PopupSwapGunInstance currentPopup = null;

	void Awake()
	{
		instance = this;
	}

	public static void CreatePopup(Vector3 newPosition, Gun newGunPrefab)
	{
		if (currentPopup != null)
		{
			currentPopup.DisappearNoSelection();
		}

		newPosition.z = instance.transform.position.z;

		PopupSwapGunInstance popupInstance = Instantiate(instance.popupPrefab, newPosition, Quaternion.identity) as PopupSwapGunInstance;
		currentPopup = popupInstance;
		popupInstance.transform.SetParent(instance.transform);
		popupInstance.transform.SetAsFirstSibling();
		popupInstance.transform.localScale = instance.popupPrefab.transform.localScale;
		popupInstance.newGunPrefab = newGunPrefab;

		Image oldGun = popupInstance.transform.FindSubChild("Old Gun").GetComponent<Image>();
		Image newGun = popupInstance.transform.FindSubChild("New Gun").GetComponent<Image>();
		Text oldStats = popupInstance.transform.FindSubChild("Old Stats").GetComponent<Text>();
		Text newStats = popupInstance.transform.FindSubChild("New Stats").GetComponent<Text>();

		oldGun.sprite = PlayerControl.instance.Gun.Sprite;
		newGun.sprite = newGunPrefab.Sprite;

		oldStats.text = PlayerControl.instance.Gun.projectile.damage + "\n" +
						PlayerControl.instance.Gun.FireRate + "\n" +
						PlayerControl.instance.Gun.projectile.knockback.x;

		newStats.text = newGunPrefab.projectile.damage + "\n" +
						newGunPrefab.FireRate + "\n" +
						newGunPrefab.projectile.knockback.x;
	}
}
