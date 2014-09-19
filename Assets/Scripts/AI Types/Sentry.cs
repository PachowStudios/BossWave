using UnityEngine;
using System.Collections;

public class Sentry : Enemy
{
	public float detectionRange = 5f;
	public float minWaitTime = 0.5f;
	public float maxWaitTime = 3f;
	public float minMoveTime = 0.5f;
	public float maxMoveTime = 3f;
	public float minFireTime = 1f;
	public float maxFireTime = 3f;
	public float cooldownTime = 3f;

	private float waitTimer = 0f;
	private float currentWaitTime = 0f;
	private float moveTimer = 0f;
	private float currentMoveTime = 0f;
	private float fireTimer = 0f;
	private float currentFireTime = 0f;
	private float cooldownTimer = 0f;
	private bool fire = false;
	private bool fireUp = false;

	private Transform player;
	private Collider2D sideFireCollider;
	private Collider2D upFireCollider;

	new void Awake()
	{
		base.Awake();

		currentWaitTime = Random.Range(minWaitTime, maxWaitTime);
		currentMoveTime = Random.Range(minMoveTime, maxMoveTime);
		currentFireTime = Random.Range(minFireTime, maxFireTime);

		player = GameObject.Find("Player").transform;
		sideFireCollider = transform.FindChild("sideFire").collider2D;
		upFireCollider = transform.FindChild("upFire").collider2D;
	}

	void FixedUpdate()
	{
		InitialUpdate();

		CheckFrontCollision();

		anim.SetBool("Walking", right || left);
		anim.SetBool("Fire", fire);
		anim.SetBool("Fire_Up", fireUp);

		if (!right && !left && !fire)
		{
			waitTimer += Time.deltaTime;

			if (waitTimer >= currentWaitTime)
			{
				right = Random.value >= 0.5;
				left = !right;

				waitTimer = 0f;
				currentWaitTime = Random.Range(minWaitTime, maxWaitTime);
			}
		}
		
		if ((right || left) && !fire)
		{
			moveTimer += Time.deltaTime;

			if (moveTimer >= currentMoveTime)
			{
				right = false;
				left = false;

				moveTimer = 0f;
				currentMoveTime = Random.Range(minMoveTime, maxMoveTime);
			}
		}

		if (fire)
		{
			fireTimer += Time.deltaTime;

			if (anim.GetCurrentAnimatorStateInfo(0).IsName("Fire_Side"))
			{
				sideFireCollider.enabled = true;
			}
			else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Fire_Up"))
			{
				upFireCollider.enabled = true;
			}

			if (fireTimer >= currentFireTime)
			{
				fire = false;

				fireTimer = 0f;
				currentFireTime = Random.Range(minFireTime, maxFireTime);

				sideFireCollider.enabled = false;
				upFireCollider.enabled = false;
			}
		}
		else
		{
			cooldownTimer += Time.deltaTime;

			if (cooldownTimer >= cooldownTime)
			{
				if (Mathf.Abs(player.position.x - transform.position.x) <= detectionRange)
				{
					if ((player.position.x < transform.position.x && transform.localScale.x > 0f) ||
						(player.position.x > transform.position.x && transform.localScale.x < 0f))
					{
						Flip();
					}

					fire = true;
					right = left = false;

					cooldownTimer = 0f;

					fireUp = player.position.y >= transform.position.y + 1;

				}
			}
		}

		GetMovement();
		ApplyMovement();
	}
}
