using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GunCrate : Powerup
{
	public Gun.RarityLevel minRarity;
	public Gun.RarityLevel maxRarity;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Pickup()
	{
		List<Gun> possibleGuns = GetPossibleGuns();

		if (possibleGuns[0].gunName != PlayerControl.Instance.Gun.gunName)
		{
			int newGun = Mathf.RoundToInt(Random.Range(0f, possibleGuns.Count - 1f));

			if (PlayerControl.Instance.GunsFull)
			{
				PopupSwapGun.Instance.CreatePopup(PlayerControl.Instance.PopupMessagePoint,
												  possibleGuns[newGun]);
			}
			else
			{
				PopupMessage.Instance.CreatePopup(PlayerControl.Instance.PopupMessagePoint,
												  "",
												  possibleGuns[newGun].SpriteRenderer.sprite,
												  true);
				CurrentGunName.Instance.Show(possibleGuns[newGun].gunName);
				PlayerControl.Instance.AddGun(possibleGuns[newGun]);
			}
		}

		base.Pickup();
	}

	private List<Gun> GetPossibleGuns()
	{
		List<Gun> possibleGuns = new List<Gun>();

		foreach (Gun gun in PowerupSpawner.Instance.guns)
		{
			if (minRarity <= gun.rarity && gun.rarity <= maxRarity && !PlayerControl.Instance.Guns.Any(g => g.gunName == gun.gunName))
			{
				possibleGuns.Add(gun);
			}
		}

		if (possibleGuns.Count == 0)
		{
			possibleGuns.Add(PlayerControl.Instance.Gun);
		}

		return possibleGuns;
	}
}
