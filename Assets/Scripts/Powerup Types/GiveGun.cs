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

	new void OnTriggerEnter2D(Collider2D trigger)
	{
		base.OnTriggerEnter2D(trigger);
	}

	protected override void Pickup()
	{
		int newRarity = Random.Range((int)player.gun.rarity - 1,
									 (int)player.gun.rarity + 2);
		newRarity = (int)Mathf.Clamp(newRarity, newRarity,
									 (int)Gun.RarityLevel.NUM_TYPES);

		List<Gun> possibleGuns = new List<Gun>();

		foreach (Gun gun in guns)
		{
			if ((int)gun.rarity == newRarity && player.gun.gunName != gun.gunName)
			{
				possibleGuns.Add(gun);
			}
		}

		if (possibleGuns.Count > 0)
		{
			Gun newGun = possibleGuns[(int)Random.Range(0, possibleGuns.Count - 1)];

			player.SwapGun(newGun);
		}

		base.Pickup();
	}
}
