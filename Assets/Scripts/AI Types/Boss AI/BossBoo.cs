using UnityEngine;
using System.Collections;

public class BossBoo : Enemy
{
	public float minFlyHeight = 3f;
	public float maxFlyHeight = 5f;
	public float flyHeightBuffer = 1f;
	public float minSwoopTime = 3f;
	public float maxSwoopTime = 5f;
	public float minSwoopDistance = 10f;
	public float maxSwoopDistance = 12f;
	public float swoopSpeedMultiplier = 3f;
	public float smashRange = 4f;
	public float smashTime = 2f;
	public float smashGravity = -50f;

	private float defaultGravity;
	private float flyHeight;
	private float swoopTimer = 0f;
	private float smashTimer = 0f;
	private float currentSwoopTime;
	private float currentSwoopDistance;
	private float startingY;
	private float startingX;
	private float targetX;
	private float startingHeight;
	private float swoopGravity;
	private bool swooping = false;
	private bool swoopReturn = false;
	private bool smashing = false;

	private Transform player;
	private Transform groundLevel;

	new void Awake()
	{
		base.Awake();

		player = playerControl.transform;
		groundLevel = GameObject.FindGameObjectWithTag("GroundLevel").transform;

		defaultGravity = gravity;
		flyHeightBuffer /= 5f;
		flyHeight = Random.Range(minFlyHeight, maxFlyHeight) + groundLevel.position.y;
		currentSwoopTime = Random.Range(minSwoopTime, maxSwoopTime);
	}

	void Start()
	{
		startingX = transform.position.x;
	}

	void FixedUpdate()
	{
		InitialUpdate();

		invincible = !swooping || swoopReturn;

		if (!swooping && !smashing)
		{
			if (transform.position.y >= flyHeight + flyHeightBuffer)
			{
				gravity = defaultGravity;

				if (velocity.y > 0)
				{
					velocity.y -= -defaultGravity * Time.fixedDeltaTime;
				}
			}
			else if (transform.position.y <= flyHeight - flyHeightBuffer)
			{
				gravity = -defaultGravity;

				if (velocity.y < 0)
				{
					velocity.y += -defaultGravity * Time.fixedDeltaTime;
				}
			}

			if (transform.position.x < startingX)
			{
				right = true;
				left = !right;
			}
			else
			{
				right = left = false;
			}
		}

		smashTimer += Time.deltaTime;

		if (!swooping && Mathf.Abs(transform.position.x - player.position.x) < smashRange)
		{
			if (smashTimer >= smashTime)
			{
				smashing = true;
				anim.SetBool("Attacking", true);
			}
		}

		if (smashing)
		{
			if (transform.position.y > groundLevel.position.y)
			{
				gravity = Mathf.Lerp(gravity, smashGravity, 0.5f);
			}
			else
			{
				smashing = false;
				anim.SetBool("Attacking", false);
				smashTimer = 0f;
			}
		}

		if (!smashing)
		{
			swoopTimer += Time.deltaTime;

			if (swoopTimer >= currentSwoopTime)
			{
				if (!swooping && velocity.y < 0f)
				{
					swooping = true;
					anim.SetBool("Attacking", true);
					currentSwoopDistance = Random.Range(minSwoopDistance, maxSwoopDistance);
					targetX = startingX + currentSwoopDistance;
					moveSpeed *= swoopSpeedMultiplier;
					startingY = transform.position.y;
					startingHeight = Mathf.Abs(startingY - groundLevel.position.y);
					float secondsToTarget = currentSwoopDistance / (moveSpeed * swoopSpeedMultiplier);
					swoopGravity = -(startingHeight / secondsToTarget);
				}

				if (swooping)
				{
					if (transform.position.x < targetX && !swoopReturn)
					{
						right = true;
						left = !right;
						gravity = Mathf.Lerp(gravity, swoopGravity, 0.5f);
					}
					else if (transform.position.x < targetX + currentSwoopDistance && !swoopReturn)
					{
						right = true;
						left = !right;
						gravity = Mathf.Lerp(gravity, -swoopGravity, 0.5f);
					}
					else if (transform.position.x > targetX + currentSwoopDistance && !swoopReturn)
					{
						swoopReturn = true;
						anim.SetBool("Attacking", false);
					}
					else if (transform.position.x > startingX && swoopReturn)
					{
						left = true;
						right = !left;

						if (transform.position.y < startingY + startingHeight)
						{
							gravity = Mathf.Lerp(gravity, -swoopGravity, 0.5f);
						}
						else
						{
							gravity = Mathf.Lerp(gravity, swoopGravity * 2f, 0.5f);
						}
					}
					else if (transform.position.x < startingX && swoopReturn)
					{
						right = left = false;
						swoopReturn = false;
						swooping = false;
						swoopTimer = 0f;
						moveSpeed /= swoopSpeedMultiplier;
						currentSwoopTime = Random.Range(minSwoopTime, maxSwoopTime) * 2f;
					}
				}
			}
		}

		GetMovement();
		ApplyMovement();
	}
}
