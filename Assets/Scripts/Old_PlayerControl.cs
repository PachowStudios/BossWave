using UnityEngine;
using System.Collections;

public class Old_PlayerControl : MonoBehaviour 
{
	[HideInInspector]
	public bool facingRight = true;
	[HideInInspector]
	public bool jump = false;
	[HideInInspector]
	public bool running = false;
	[HideInInspector]
	public bool runningFull = false;
	[HideInInspector]
	public float horizontal = 0f;

	public float moveForce = 30f;
	public float turningSpeed = 1f;
	public float walkingSpeed = 5f;
	public float runningSpeed = 8.5f;
	public float runningFullSpeed = 10f;
	public float runningFullTime = 1.25f;
	public float jumpForce = 350f;

	private float runningFullTimer = 0f;

	private Transform groundCheck;
	private bool grounded;

	private Animator anim;

	void Awake()
	{
		groundCheck = transform.Find("groundCheck");
		anim = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update() 
	{
		grounded = Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Ground"));
		anim.SetBool("Grounded", grounded);

		horizontal = Input.GetAxisRaw("Horizontal");

		if (Input.GetButtonDown("Jump") && grounded)
		{
			jump = true;
		}

		if (Input.GetButton("Run") && Mathf.Abs(horizontal) >= 0.1f)
		{
			running = true;
			anim.SetBool("Running", true);
		}
		else
		{
			running = false;
			anim.SetBool("Running", false);
		}
	}

	void FixedUpdate()
	{
		anim.SetFloat("Speed", Mathf.Abs(horizontal));
		anim.SetFloat("Velocity", Mathf.Abs(rigidbody2D.velocity.x));
		anim.SetFloat("Vert_Velocity", rigidbody2D.velocity.y);

		if ((horizontal * rigidbody2D.velocity.x < walkingSpeed) || 
			(running && horizontal * rigidbody2D.velocity.x < runningSpeed) || 
			(runningFull && horizontal * rigidbody2D.velocity.x < runningFullSpeed))
		{
			rigidbody2D.AddForce(Vector2.right * horizontal * moveForce);
		}

		if (horizontal > 0 && !facingRight)
		{
			Flip();
		}
		else if (horizontal < 0 && facingRight)
		{
			Flip();
		}

		if (jump)
		{
			Jump();
		}

		if (running)
		{
			runningFullTimer += Time.deltaTime;

			if (runningFullTimer >= runningFullTime)
			{
				runningFull = true;
				anim.SetBool("Running_Full", runningFull);
			}
		}
		else if (runningFullTimer >= 0f)
		{
			runningFullTimer = 0f;
			runningFull = false;
			anim.SetBool("Running_Full", runningFull);
		}
	}

	void Flip()
	{
		facingRight = !facingRight;

		Vector3 localScale = transform.localScale;
		localScale.x *= -1;
		transform.localScale = localScale;

		if (Mathf.Abs(rigidbody2D.velocity.x) > turningSpeed)
		{
			anim.SetTrigger("Turn");
		}
	}

	void Jump()
	{
		anim.SetTrigger("Jump");

		rigidbody2D.AddForce(new Vector2(0f, jumpForce));
		jump = false;
	}
}
