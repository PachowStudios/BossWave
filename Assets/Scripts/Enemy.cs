using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour 
{
	public enum direction
	{
		left,
		right,
		random
	};

	public enum aiType
	{
		walking,
		jumping
	};

	public float gravity = -35f;
	public float walkSpeed = 5f;
	public float runSpeed = 8.5f;
	public float groundDamping = 10f;
	public float inAirDamping = 5f;
	public float jumpHeight = 4f;
	public float health = 10f;
	public float jumpTime = 2f;
	public aiType AI = aiType.walking;

	private bool right = false;
	private bool left = false;
	private bool run = false;
	private bool jump = false;

	private float jumpTimer = 0f;

	[HideInInspector]
	private float normalizedHorizontalSpeed = 0;
	[HideInInspector]
	public direction startingDirection = direction.right;

	private CharacterController2D controller;
	private Animator anim;
	private RaycastHit2D lastControllerColliderHit;
	private Vector3 velocity;
	private Transform frontCheck;

	void Awake()
	{
		anim = GetComponent<Animator>();
		controller = GetComponent<CharacterController2D>();
		frontCheck = transform.Find("frontCheck").transform;

		switch(startingDirection)
		{
			case direction.left:
				left = true;
				break;
			case direction.right:
				right = true;
				break;
			case direction.random:
				left = Random.value >= 0.5;
				right = !left;
				break;
		}
	}

	void FixedUpdate()
	{
		velocity = controller.velocity;

		anim.SetBool("Grounded", controller.isGrounded);

		if (controller.isGrounded)
		{
			velocity.y = 0;
		}

		if (AI == aiType.walking || AI == aiType.jumping)
		{
			Collider2D[] frontHits = Physics2D.OverlapPointAll(frontCheck.position);

			foreach (Collider2D hit in frontHits)
			{
				if (hit.tag == "Obstacle" || hit.tag == "MainCamera")
				{
					Flip();
					break;
				}
			}

			if (right)
			{
				normalizedHorizontalSpeed = 1;

				if (transform.localScale.x < 0f)
				{
					Flip();
				}
			}
			else if (left)
			{
				normalizedHorizontalSpeed = -1;

				if (transform.localScale.x > 0f)
				{
					Flip();
				}
			}
			else
			{
				normalizedHorizontalSpeed = 0;
			}
		}

		if (AI == aiType.jumping)
		{
			jumpTimer += Time.deltaTime;

			if (jumpTimer >= jumpTime)
			{
				jump = true;
			}
		}

		if (jump && controller.isGrounded)
		{
			velocity.y = Mathf.Sqrt(2f * jumpHeight * -gravity);
			jumpTimer = 0f;
			jump = false;
			anim.SetTrigger("Jump");
		}

		float smoothedMovementFactor = controller.isGrounded ? groundDamping : inAirDamping;

		velocity.x = Mathf.Lerp(velocity.x, normalizedHorizontalSpeed * (run ? runSpeed : walkSpeed), Time.fixedDeltaTime * smoothedMovementFactor);
		velocity.y += gravity * Time.fixedDeltaTime;


		controller.move(velocity * Time.fixedDeltaTime);
	}

	void Flip()
	{
		transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
		normalizedHorizontalSpeed *= -1;
		right = !right;
		left = !right;
	}
}
