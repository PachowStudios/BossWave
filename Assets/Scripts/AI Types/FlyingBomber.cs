﻿using UnityEngine;
using System.Collections;

public class FlyingBomber : Enemy
{
	public Projectile projectile;
	public float minFlyHeigt = 5f;
	public float maxFlyHeight = 7.5f;
	public float minDetectionRange = 5f;
	public float maxDetectionRange = 8f;
	public float minBombTime = 1.5f;
	public float maxBombTime = 4f;

	private float defaultGravity;
	private float flyHeight;
	private float detectionRange;
	private float bombTimer = 0f;
	private float currentBombTime = 0f;

	private Transform player;
	private Transform gun;
	private Transform groundLevel;

	new void Awake()
	{
		base.Awake();

		right = true;

		player = GameObject.Find("Player").transform;
		gun = transform.FindChild("Gun");
		groundLevel = GameObject.FindGameObjectWithTag("GroundLevel").transform;

		defaultGravity = gravity;
		flyHeight = Random.Range(minFlyHeigt, maxFlyHeight) + groundLevel.position.y;
		detectionRange = Random.Range(minDetectionRange, maxDetectionRange);
		currentBombTime = Random.Range(minBombTime, maxBombTime);
	}

	void FixedUpdate()
	{
		InitialUpdate();

		CheckFrontCollision();

		if (transform.position.y >= flyHeight)
		{
			gravity = defaultGravity;
		}
		else
		{
			gravity = 0;
			velocity.y = 0;
		}

		bombTimer += Time.deltaTime;

		if (bombTimer >= currentBombTime && Mathf.Abs(player.position.x - transform.position.x) <= detectionRange)
		{
			anim.SetTrigger("Shoot");

			Vector3 gunLookPosition = player.collider2D.bounds.center;
			gunLookPosition -= gun.transform.position;
			gunLookPosition.y += (Mathf.Abs(projectile.gravity) / 3.5f);
			float gunAngle = Mathf.Atan2(gunLookPosition.y, gunLookPosition.x) * Mathf.Rad2Deg;
			gun.transform.rotation = Quaternion.AngleAxis(gunAngle, Vector3.forward);

			float bulletRotation = transform.localScale.x > 0 ? 0f : 180f;

			Projectile projectileInstance = Instantiate(projectile, gun.transform.position, Quaternion.Euler(new Vector3(0, 0, bulletRotation))) as Projectile;
			projectileInstance.direction = gun.transform.right;

			bombTimer = 0f;
		}

		if (player.position.x > transform.position.x + detectionRange)
		{
			right = true;
			left = !right;
		}
		else if (player.position.x < transform.position.x - detectionRange)
		{
			left = true;
			right = !left;
		}
		else
		{
			right = left = false;
		}

		GetMovement();
		ApplyMovement();
	}
}
