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
	public float swoopSpeedMultiplier = 1.5f;

	private float defaultGravity;
	private float flyHeight;
	private float swoopTimer = 0f;
	private float currentSwoopTime;
	private float currentSwoopDistance;
	private float startingY;
	private float startingX;
	private float targetX;
	private float startingHeight;
	private float swoopGravity;
	private bool swooping = false;
	private bool swoopReturn = false;

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

		if (!swooping)
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

		GetMovement();
		ApplyMovement();
	}
}
