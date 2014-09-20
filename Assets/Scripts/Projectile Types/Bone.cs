using UnityEngine;
using System.Collections;

public class Bone : Projectile
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
