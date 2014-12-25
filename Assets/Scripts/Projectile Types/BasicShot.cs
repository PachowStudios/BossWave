using UnityEngine;
using System.Collections;

public class BasicShot : Projectile
{
	void FixedUpdate()
	{
		InitialUpdate();

		ApplyMovement();
	}
}
