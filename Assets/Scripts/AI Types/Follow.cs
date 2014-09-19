using UnityEngine;
using System.Collections;

public class Follow : Enemy
{
	public float followBuffer = 1.5f;

	private Transform player;

	new void Awake()
	{
		base.Awake();

		player = GameObject.Find("Player").transform;
	}

	void FixedUpdate()
	{
		InitialUpdate();

		anim.SetBool("Walking", right || left);

		if (player.position.x > transform.position.x + followBuffer)
		{
			right = true;
			left = !right;
		}
		else if (player.position.x < transform.position.x - followBuffer)
		{
			left = true;
			right = !left;
		}
		else
		{
			right = left = false;
		}

		GetMovement();
		ApplyMovement();
	}
}
