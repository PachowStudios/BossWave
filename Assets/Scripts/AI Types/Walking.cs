using UnityEngine;
using System.Collections;

public class Walking : Enemy
{
	void Awake()
	{
		base.Awake();

		right = true;
	}

	void FixedUpdate()
	{
		InitialUpdate();

		CheckFrontCollision();

		GetMovement();
		ApplyMovement();
	}
}
