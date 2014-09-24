using UnityEngine;
using System.Collections;

public class GiveGun : Powerup
{
	public Gun newGun;

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
		player.SwapWeapon(newGun);

		base.Pickup();
	}
}
