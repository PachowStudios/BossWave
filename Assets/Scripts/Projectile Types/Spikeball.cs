using UnityEngine;
using System.Collections;

public class Spikeball : Projectile
{
	new void Awake()
	{
		base.Awake();
	}

	void FixedUpdate()
	{
		InitialUpdate();

		ApplyMovement();
	}
}
