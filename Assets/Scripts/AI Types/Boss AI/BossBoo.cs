using UnityEngine;
using System.Collections;

public class BossBoo : Enemy
{
	new void Awake()
	{
		base.Awake();
	}

	void FixedUpdate()
	{
		InitialUpdate();

		CheckFrontCollision();

		GetMovement();
		ApplyMovement();
	}
}
