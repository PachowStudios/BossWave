using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GunCrate : Powerup
{
	public Gun.RarityLevel minRarity;
	public Gun.RarityLevel maxRarity;

	new void Awake()
	{
		base.Awake();
	}

	protected override void Pickup()
	{
		int getPossibleGunTrys = 0;

		List<Gun> possibleGuns = GetPossibleGuns(GetNewRarity(), ref getPossibleGunTrys);

		if (possibleGuns[0].gunName != PlayerControl.instance.Gun.gunName)
		{
			int newGun = Mathf.RoundToInt(Random.Range(0f, possibleGuns.Count - 1f));

			PopupSwapGun.CreatePopup(PlayerControl.instance.PopupMessagePoint, possibleGuns[newGun]);
		}

		base.Pickup();
	}

	private int GetNewRarity()
	{
		return Random.Range((int)minRarity, (int)maxRarity + 1);
	}

	private List<Gun> GetPossibleGuns(int rarity, ref int trys)
	{
		trys++;

		List<Gun> possibleGuns = new List<Gun>();

		foreach (Gun gun in LevelManager.instance.guns)
		{
			if ((int)gun.rarity == rarity && PlayerControl.instance.Gun.gunName != gun.gunName)
			{
				possibleGuns.Add(gun);
			}
		}

		if (possibleGuns.Count == 0)
		{
			if (trys < 5)
			{
				possibleGuns = GetPossibleGuns(GetNewRarity(), ref trys);
			}
			else
			{
				possibleGuns.Add(PlayerControl.instance.Gun);
			}
		}

		return possibleGuns;
	}
}
