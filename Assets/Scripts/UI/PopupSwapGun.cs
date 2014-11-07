using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PopupSwapGun : MonoBehaviour 
{
	private static PopupSwapGun instance;

	public PopupSwapGunInstance popupPrefab;

	void Awake()
	{
		instance = this;
	}

	public static void CreatePopup(Vector3 newPosition, Gun newGunPrefab)
	{
		newPosition.z = instance.transform.position.z;

		PopupSwapGunInstance popupInstance = Instantiate(instance.popupPrefab, newPosition, Quaternion.identity) as PopupSwapGunInstance;
		popupInstance.transform.parent = instance.transform;
		popupInstance.transform.localScale = instance.popupPrefab.transform.localScale;

		Image oldGun = popupInstance.transform.FindChild("Old Gun").GetComponent<Image>();
		Image newGun = popupInstance.transform.FindChild("New Gun").GetComponent<Image>();
		Text oldStats = popupInstance.transform.FindChild("Old Stats").GetComponent<Text>();
		Text newStats = popupInstance.transform.FindChild("New Stats").GetComponent<Text>();

		oldGun.sprite = PlayerControl.instance.gun.GetComponent<SpriteRenderer>().sprite;
		newGun.sprite = newGunPrefab.GetComponent<SpriteRenderer>().sprite;

		oldStats.text = PlayerControl.instance.gun.projectile.damage + "\n" +
						PlayerControl.instance.gun.projectile.shotSpeed + "\n" +
						PlayerControl.instance.gun.projectile.knockback;

		newStats.text = newGunPrefab.projectile.damage + "\n" +
						newGunPrefab.projectile.shotSpeed + "\n" +
						newGunPrefab.projectile.knockback;
	}
}
