using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TheRIFT : Enemy
{
	public float minFloatHeight = 3f;
	public float maxFloatHeight = 5f;
	public float minSwoopTime = 3f;
	public float maxSwoopTime = 5f;
	public float swoopLength = 5f;
	public float smashRange = 4f;
	public float smashTime = 2f;
	public float smashGravity = -50f;
	public AnimationCurve swoopCurve;

	private float defaultGravity;
	private float swoopTimer = 0f;
	private float smashTimer = 0f;
	private float swoopTime;
	private bool swooping = false;
	private bool smashing = false;
	private List<Vector3> swoopPath = new List<Vector3>();

	private Transform groundLevel;

	private float swoopPercentage
	{
		get
		{
			return swoopCurve.Evaluate(Mathf.Clamp((swoopTimer - swoopTime) / swoopLength, 0f, 1f));
		}
	}

	private float newSwoopTime
	{
		get
		{
			return Random.Range(minSwoopTime, maxSwoopTime);
		}
	}

	new void Awake()
	{
		base.Awake();

		groundLevel = GameObject.FindGameObjectWithTag("GroundLevel").transform;

		defaultGravity = gravity;

		minFloatHeight += groundLevel.transform.position.y;
		maxFloatHeight += groundLevel.transform.position.y;

		swoopTime = newSwoopTime;
	}

	void FixedUpdate()
	{
		InitialUpdate();

		invincible = !swooping;

		if (!swooping && !smashing)
		{
			if (transform.position.y >= maxFloatHeight)
			{
				gravity = defaultGravity;

				if (velocity.y > 0)
				{
					velocity.y -= -defaultGravity * Time.deltaTime;
				}
			}
			else if (transform.position.y <= minFloatHeight)
			{
				gravity = -defaultGravity;

				if (velocity.y < 0)
				{
					velocity.y += -defaultGravity * Time.deltaTime;
				}
			}
		}

		if (!swooping)
		{
			smashTimer += Time.deltaTime;

			if (smashTimer >= smashTime)
			{
				if (!smashing)
				{
					if (Mathf.Abs(transform.position.x - PlayerControl.instance.transform.position.x) < smashRange)
					{
						smashing = true;
						smashTimer = smashTime;
					}
				}
				else
				{
					if (transform.position.y > groundLevel.position.y)
					{
						gravity = Mathf.Lerp(gravity, smashGravity, 0.75f);
					}
					else
					{
						smashing = false;
						smashTimer = 0f;
					}
				}
			}
		}

		if (!smashing)
		{
			swoopTimer += Time.deltaTime;

			if (swoopTimer >= swoopTime)
			{
				if (!swooping)
				{
					if (velocity.y < 0f && transform.position.y < (minFloatHeight + maxFloatHeight) / 2f)
					{
						swooping = true;
						swoopTimer = swoopTime;
						UpdateSwoopPath();
					}
				}
				else
				{
					Vector3 prevPosition = transform.position;

					gameObject.PutOnPath(swoopPath.ToArray(), swoopPercentage);

					if (transform.localScale.x > 0 && prevPosition.x > transform.position.x && swoopPercentage > 0.25f)
					{
						Flip();
					}

					if (swoopPercentage == 1f)
					{
						swooping = false;
						swoopTimer = 0f;
						swoopTime = newSwoopTime;
						Flip();
					}
				}
			}
		}

		if (!swooping)
		{
			GetMovement();
			ApplyMovement();
		}
	}

	private void UpdateSwoopPath()
	{
		Vector3 startingPosition = transform.position;

		swoopPath.Clear();
		swoopPath.Add(startingPosition);
		swoopPath.Add(new Vector3(PlayerControl.instance.transform.position.x, 
									groundLevel.position.y - 0.2f, 
									startingPosition.z));
		swoopPath.Add(Camera.main.ViewportToWorldPoint(new Vector3(1.2f, 0.5f, 10f)));
		swoopPath.Add(new Vector3(PlayerControl.instance.transform.position.x,
									Camera.main.ViewportToWorldPoint(new Vector3(1f, 0.75f, 10f)).y,
									startingPosition.z));
		swoopPath.Add(startingPosition);
	}
}
