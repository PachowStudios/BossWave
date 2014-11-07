using UnityEngine;
using System.Collections;

public class Follow : Enemy
{
	public float followBuffer = 5f;

	new void Awake()
	{
		base.Awake();
	}

	void FixedUpdate()
	{
		InitialUpdate();

		anim.SetBool("Walking", right || left);

		if (PlayerControl.instance.transform.position.x > transform.position.x + followBuffer)
		{
			right = true;
			left = !right;
		}
		else if (PlayerControl.instance.transform.position.x < transform.position.x - followBuffer)
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
