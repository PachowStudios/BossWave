using UnityEngine;
using System.Collections;

public class Fireball : Projectile
{
	new void Awake()
	{
		base.Awake();
	}

	void FixedUpdate()
	{
		InitialUpdate();

		GetMovement();
		ApplyMovement();
	}
}
