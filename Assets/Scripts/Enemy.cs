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

	public float gravity = -35f;
	public float walkSpeed = 5f;
	public float runSpeed = 8.5f;
	public bool canRun = false;
	public float groundDamping = 10f;
	public float inAirDamping = 5f;
	public float jumpHeight = 4f;
	public bool canJump = false;
	public float health = 10f;

	private bool run;
	private bool jump;

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
				normalizedHorizontalSpeed = -1;
				break;
			case direction.right:
				normalizedHorizontalSpeed = 1;
				break;
			case direction.random:
				normalizedHorizontalSpeed = Random.value >= 0.5 ? -1 : 1;
				break;
		}
	}

	void FixedUpdate()
	{
		velocity = controller.velocity;

		if (controller.isGrounded)
		{
			velocity.y = 0;
		}

		Collider2D[] frontHits = Physics2D.OverlapPointAll(frontCheck.position);

		foreach(Collider2D hit in frontHits)
		{
			if (hit.tag == "Obstacle" || hit.tag == "MainCamera")
			{
				Flip();
				break;
			}
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
	}
}
