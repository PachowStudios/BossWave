using UnityEngine;
using System.Collections;

public class Jumping : Enemy 
{
	public float jumpHeight = 4f;
	public float jumpTime = 1f;

	private bool jump = false;
	private float jumpTimer = 0f;

	new void Awake()
	{
		base.Awake();

		right = true;
	}

	void FixedUpdate()
	{
		InitialUpdate();

		CheckFrontCollision();

		if (controller.isGrounded)
		{
			jumpTimer += Time.deltaTime;
		}

		if (jumpTimer >= jumpTime)
		{
			jump = true;
		}

		if (jump && controller.isGrounded)
		{
			velocity.y = Mathf.Sqrt(2f * jumpHeight * -gravity);
			jumpTimer = 0f;
			jump = false;
			anim.SetTrigger("Jump");
		}

		anim.SetBool("Grounded", controller.isGrounded);

		GetMovement();
		ApplyMovement();
	}
}
