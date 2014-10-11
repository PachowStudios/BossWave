using UnityEngine;
using System.Collections;

public class BossBoo : Enemy
{
	public float minFlyHeight = 3f;
	public float maxFlyHeight = 5f;
	public float flyHeightBuffer = 1f;
	public float minSwoopTime = 3f;
	public float maxSwoopTime = 5f;

	private float defaultGravity;
	private float flyHeight;
	private float swoopTimer = 0f;
	private float currentSwoopTime = 0f;
	private bool swooping = false;

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

	void FixedUpdate()
	{
		InitialUpdate();

		CheckFrontCollision();

		if (true)
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
		}

		swoopTimer += Time.deltaTime;

		if (swoopTimer >= currentSwoopTime)
		{
			swooping = true;
			anim.SetBool("Attacking", swooping);
		}

		GetMovement();
		ApplyMovement();
	}
}
