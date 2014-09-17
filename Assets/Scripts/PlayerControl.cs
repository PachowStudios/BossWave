using UnityEngine;
using System.Collections;


public class PlayerControl : MonoBehaviour
{
	public float gravity = -25f;
	public float turningSpeed = 1f;
	public float walkSpeed = 5f;
	public float runSpeed = 8.5f;
	public float runFullSpeed = 10f;
	public float runFullTime = 1.5f;
	public float groundDamping = 20f;
	public float inAirDamping = 5f;
	public float jumpHeight = 5f;

	[HideInInspector]
	private float normalizedHorizontalSpeed = 0;

	private CharacterController2D controller;
	private Animator anim;
	private BoxCollider2D boxCollider;
	private RaycastHit2D lastControllerColliderHit;
	private Vector3 velocity;

	private bool left;
	private bool right;
	private bool jump;
	private bool run;
	private bool crouch;

	private bool runFull = false;
	private float runFullTimer = 0f;

	private float originalColliderHeight;
	private float originalColliderOffset;
	private float crouchingColliderHeight;
	private float crouchingColliderOffset;

	void Awake()
	{
		anim = GetComponent<Animator>();
		controller = GetComponent<CharacterController2D>();
		boxCollider = GetComponent<BoxCollider2D>();

		originalColliderHeight = boxCollider.size.y;
		crouchingColliderHeight = originalColliderHeight / 2;
		originalColliderOffset = boxCollider.center.y;
		crouchingColliderOffset = originalColliderOffset - (crouchingColliderHeight / 2);
	}

	void Update()
	{
		right = Input.GetButton("Right");
		left = Input.GetButton("Left");
		run = Input.GetButton("Run");
		jump = Input.GetButton("Jump");
		crouch = Input.GetButton("Crouch");

		run = run && (right || left);

		anim.SetBool("Walking", right || left);
		anim.SetBool("Running", run);
		anim.SetBool("Crouching", crouch);
		
		if (jump)
		{
			anim.SetTrigger("Jump");
		}
	}

	void FixedUpdate()
	{
		velocity = controller.velocity;

		anim.SetBool("Grounded", controller.isGrounded);
		anim.SetBool("Falling", velocity.y < 0f);

		if (controller.isGrounded)
		{
			velocity.y = 0;
		}

		if (crouch)
		{
			normalizedHorizontalSpeed = 0;
			boxCollider.size = new Vector2(boxCollider.size.x, crouchingColliderHeight);
			boxCollider.center = new Vector2(boxCollider.center.x, crouchingColliderOffset);
		}
		else
		{
			boxCollider.size = new Vector2(boxCollider.size.x, originalColliderHeight);
			boxCollider.center = new Vector2(boxCollider.center.x, originalColliderOffset);
		}

		if (!crouch || (crouch && !controller.isGrounded))
		{
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

		if (jump && controller.isGrounded)
		{
			velocity.y = Mathf.Sqrt(2f * jumpHeight * -gravity);
		}

		if (run)
		{
			runFullTimer += Time.deltaTime;

			if (runFullTimer >= runFullTime)
			{
				runFull = true;
				anim.SetBool("Running_Full", runFull);
			}
		}
		else if (runFullTimer > 0f)
		{
			runFullTimer = 0f;
			runFull = false;
			anim.SetBool("Running_Full", runFull);
		}

		float smoothedMovementFactor = controller.isGrounded ? groundDamping : inAirDamping;

		velocity.x = Mathf.Lerp(velocity.x, normalizedHorizontalSpeed * (run ? (runFull ? runFullSpeed : runSpeed) : walkSpeed), Time.fixedDeltaTime * smoothedMovementFactor);

		velocity.y += gravity * Time.fixedDeltaTime;

		controller.move(velocity * Time.fixedDeltaTime);
	}

	void Flip()
	{
		transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
		
		if (runFullTimer > 0f)
		{
			runFullTimer = 0f;
			runFull = false;
			anim.SetBool("Running_Full", runFull);
		}

		if (Mathf.Abs(velocity.x) > turningSpeed && controller.isGrounded)
		{
			anim.SetTrigger("Turn");
		}
	}
}
