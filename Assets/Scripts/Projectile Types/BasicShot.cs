using UnityEngine;
using System.Collections;

public class BasicShot : Projectile
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
