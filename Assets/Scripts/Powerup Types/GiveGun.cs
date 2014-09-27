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
		int newRarity = Mathf.RoundToInt(Random.Range((int)player.gun.rarity - 1f,
													  (int)player.gun.rarity + 2f));
		newRarity = Mathf.Clamp(newRarity, newRarity,
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
			int newGun = Mathf.RoundToInt(Random.Range(0f, possibleGuns.Count - 1f));

			player.SwapGun(possibleGuns[newGun]);

			string rarityName = "";

			switch (possibleGuns[newGun].rarity)
			{
				case Gun.RarityLevel.Common:
					rarityName = "Common";
					break;
				case Gun.RarityLevel.Uncommon:
					rarityName = "Uncommon";
					break;
				case Gun.RarityLevel.Rare:
					rarityName = "Rare";
					break;
			}

			popupMessage.AddMessage("You got a " + rarityName + " " + possibleGuns[newGun].gunName + "!");
		}

		base.Pickup();
	}
}
