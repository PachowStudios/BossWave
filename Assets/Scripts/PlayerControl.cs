using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour 
{
	[HideInInspector]
	public bool facingRight = true;
	[HideInInspector]
	public bool jump = false;
	[HideInInspector]
	public bool running = false;

	public float moveForce = 365f;
	public float walkingSpeed = 5f;
	public float runningSpeed = 10f;
	public float jumpForce = 1000f;

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

		if (Input.GetButtonDown("Jump") && grounded)
		{
			jump = true;
		}

		if (Input.GetButton("Run"))
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
		float horizontal = Input.GetAxis("Horizontal");

		anim.SetFloat("Speed", Mathf.Abs(horizontal));

		if (horizontal * rigidbody2D.velocity.x < walkingSpeed || (running && horizontal * rigidbody2D.velocity.x < runningSpeed))
		{
			rigidbody2D.AddForce(Vector2.right * horizontal * moveForce);
		}

		if ((!running && Mathf.Abs(rigidbody2D.velocity.x) > walkingSpeed) || Mathf.Abs(rigidbody2D.velocity.x) > runningSpeed)
		{
			rigidbody2D.velocity = new Vector2(Mathf.Sign(rigidbody2D.velocity.x) * walkingSpeed, rigidbody2D.velocity.y);
		}

		if (horizontal > 0 && !facingRight)
		{
			Flip();
		}
		else if (horizontal < 0 && facingRight)
		{
			Flip();
		}
	}

	void Flip()
	{
		facingRight = !facingRight;

		Vector3 localScale = transform.localScale;
		localScale.x *= -1;
		transform.localScale = localScale;
	}
}
