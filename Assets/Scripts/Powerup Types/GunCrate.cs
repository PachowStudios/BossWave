using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GunCrate : Powerup
{
	#region Fields
	public Gun.RarityLevel minRarity;
	public Gun.RarityLevel maxRarity;
	#endregion

	#region Internal Helper Methods
	protected override void Pickup()
	{
		List<Gun> possibleGuns = GetPossibleGuns();

		if (possibleGuns[0].gunName != PlayerControl.Instance.Gun.gunName)
		{
			Gun newGun = possibleGuns[Random.Range(0, possibleGuns.Count)];

			if (PlayerControl.Instance.GunsFull)
			{
				PopupSwapGun.Instance.CreatePopup(PlayerControl.Instance.PopupMessagePoint,
												  newGun);
			}
			else
			{
				PopupMessage.Instance.CreatePopup(PlayerControl.Instance.PopupMessagePoint,
												  "",
												  newGun.SpriteRenderer.sprite,
												  true);
				CurrentGunName.Instance.Show(newGun.gunName, newGun.Color);
				PlayerControl.Instance.AddGun(newGun);
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
	#endregion
}
