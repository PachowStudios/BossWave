using UnityEngine;
using System.Collections;

public class Health : Powerup
{
	public float healthAmount;

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
		player.AddHealth(healthAmount);

		base.Pickup();
	}
}
