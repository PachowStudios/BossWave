using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GiveGun : Powerup
{
	public List<Gun> guns;

	new void Awake()
	{
		base.Awake();
	}

	protected override void Pickup()
	{
		int newRarity = Mathf.RoundToInt(Random.Range((int)PlayerControl.instance.gun.rarity - 1f,
													  (int)PlayerControl.instance.gun.rarity + 2f));
		newRarity = Mathf.Clamp(newRarity, newRarity,
							    (int)Gun.RarityLevel.NUM_TYPES);

		List<Gun> possibleGuns = new List<Gun>();

		foreach (Gun gun in guns)
		{
			if ((int)gun.rarity == newRarity && PlayerControl.instance.gun.gunName != gun.gunName)
			{
				possibleGuns.Add(gun);
			}
		}

		if (possibleGuns.Count > 0)
		{
			int newGun = Mathf.RoundToInt(Random.Range(0f, possibleGuns.Count - 1f));

			PopupSwapGun.CreatePopup(PlayerControl.instance.popupMessagePoint.position, possibleGuns[newGun]);
		}

		base.Pickup();
	}
}
