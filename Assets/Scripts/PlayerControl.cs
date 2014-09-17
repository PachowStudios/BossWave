using UnityEngine;
using System.Collections;


public class PlayerControl : MonoBehaviour
{
	public float gravity = -25f;
	public float walkSpeed = 5f;
	public float runSpeed = 8.5f;
	public float runFullSpeed = 10f;
	public float groundDamping = 20f;
	public float inAirDamping = 5f;
	public float jumpHeight = 5f;

	[HideInInspector]
	private float normalizedHorizontalSpeed = 0;

	private CharacterController2D controller;
	private Animator anim;
	private RaycastHit2D lastControllerColliderHit;
	private Vector3 velocity;

	private bool left;
	private bool right;
	private bool jump;
	private bool run;

	void Awake()
	{
		anim = GetComponent<Animator>();
		controller = GetComponent<CharacterController2D>();
	}

	void Update()
	{
		right = Input.GetButton("Right");
		left = Input.GetButton("Left");
		jump = Input.GetButton("Jump");
		run = Input.GetButton("Run");
	}

	void FixedUpdate()
	{
		velocity = controller.velocity;

		if (controller.isGrounded)
		{
			velocity.y = 0;
		}

		if (right)
		{
			normalizedHorizontalSpeed = 1;

			if (transform.localScale.x < 0f)
			{
				transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
			}

			if (controller.isGrounded)
			{
				anim.Play("Walking");
			}
		}
		else if (left)
		{
			normalizedHorizontalSpeed = -1;

			if (transform.localScale.x > 0f)
			{
				transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
			}

			if (controller.isGrounded)
			{
				anim.Play("Walking");
			}
		}
		else
		{
			normalizedHorizontalSpeed = 0;

			if (controller.isGrounded)
			{
				anim.Play("Idle");
			}
		}

		if (jump && controller.isGrounded)
		{
			velocity.y = Mathf.Sqrt(2f * jumpHeight * -gravity);
			anim.Play("Jumping");
		}

		float smoothedMovementFactor = controller.isGrounded ? groundDamping : inAirDamping;

		velocity.x = Mathf.Lerp(velocity.x, normalizedHorizontalSpeed * (run ? runSpeed : walkSpeed), Time.fixedDeltaTime * smoothedMovementFactor);

		velocity.y += gravity * Time.fixedDeltaTime;

		controller.move(velocity * Time.fixedDeltaTime);
	}
}
