using UnityEngine;
using System.Collections;

public class BasicShot : Projectile
{
	private void FixedUpdate()
	{
		InitialUpdate();

		ApplyMovement();
	}
}
