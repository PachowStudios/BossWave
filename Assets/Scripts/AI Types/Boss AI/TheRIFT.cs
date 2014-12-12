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
	public AnimationCurve swoopCurve;
	public float smashRange = 4f;
	public float smashTime = 2f;
	public float smashGravity = -50f;
	public float minLaserTime = 5f;
	public float maxLaserTime = 10f;
	public float laserLength = 3f;
	public AnimationCurve laserCurve;
	public RIFTLaser laserPrefab;

	private float defaultGravity;
	private float swoopTimer = 0f;
	private float swoopTime;
	private float smashTimer = 0f;
	private float laserTimer = 0f;
	private float laserTime;
	private bool swooping = false;
	private bool smashing = false;
	private bool firingLaser = false;
	private List<Vector3> swoopPath = new List<Vector3>();
	private List<Vector3> laserPath = new List<Vector3>();
	private Vector3[] swoopPathArray;
	private Vector3[] laserPathArray;
	private RIFTLaser laserInstance;

	private Transform firePoint;
	private Transform groundLevel;

	private float swoopPercentage
	{
		get
		{
			return swoopCurve.Evaluate(Mathf.Clamp((swoopTimer - swoopTime) / swoopLength, 0f, 1f));
		}
	}

	private float laserPercentage
	{
		get
		{
			return laserCurve.Evaluate(Mathf.Clamp((laserTimer - laserTime) / laserLength, 0f, 1f));
		}
	}

	private float newSwoopTime
	{
		get
		{
			return Random.Range(minSwoopTime, maxSwoopTime);
		}
	}

	private float newLaserTime
	{
		get
		{
			return Random.Range(minLaserTime, maxLaserTime);
		}
	}

	new void Awake()
	{
		base.Awake();

		firePoint = transform.FindChild("firePoint");
		groundLevel = GameObject.FindGameObjectWithTag("GroundLevel").transform;

		defaultGravity = gravity;

		minFloatHeight += groundLevel.transform.position.y;
		maxFloatHeight += groundLevel.transform.position.y;

		swoopTime = newSwoopTime;
		laserTime = newLaserTime;
	}

	void FixedUpdate()
	{
		InitialUpdate();

		invincible = !swooping;

		// Floating
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

		// Smashing
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

		// Swooping
		if (!smashing && smashTimer >= smashTime && !firingLaser)
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

					gameObject.PutOnPath(swoopPathArray, swoopPercentage);

					if (transform.localScale.x > 0 && prevPosition.x > transform.position.x && swoopPercentage > 0.25f)
					{
						Flip();
					}

					if (swoopPercentage == 1f)
					{
						swooping = false;
						swoopTimer = 0f;
						swoopTime = newSwoopTime;

						if (transform.localScale.x < 0)
						{
							Flip();
						}
					}
				}
			}
		}

		// Firing Laser
		if (!swooping)
		{
			laserTimer += Time.deltaTime;

			if (laserTimer >= laserTime)
			{
				if (!firingLaser)
				{
					firingLaser = true;
					UpdateLaserPath();
					laserInstance = Instantiate(laserPrefab, firePoint.position, Quaternion.identity) as RIFTLaser;
					laserInstance.firePoint = firePoint.position;
				}
				else if (laserInstance != null)
				{
					laserInstance.firePoint = firePoint.position;

					if (laserInstance.Charging)
					{
						laserTimer = laserTime;
					}
					else
					{
						laserInstance.targetPoint = iTween.PointOnPath(laserPathArray, laserPercentage);

						if (laserPercentage == 1f)
						{
							firingLaser = false;
							laserTimer = 0f;
							laserTime = newLaserTime;
							laserInstance.firePoint = laserInstance.targetPoint;

							Destroy(laserInstance.gameObject, 0.5f);
						}
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
									groundLevel.position.y - 1f, 
									startingPosition.z));
		swoopPath.Add(Camera.main.ViewportToWorldPoint(new Vector3(1.2f, 0.5f, 10f)));
		swoopPath.Add(new Vector3(PlayerControl.instance.transform.position.x,
									Camera.main.ViewportToWorldPoint(new Vector3(1f, 0.75f, 10f)).y,
									startingPosition.z));
		swoopPath.Add(startingPosition);
		swoopPathArray = swoopPath.ToArray();
	}

	private void UpdateLaserPath()
	{
		Vector3 startingPosition = transform.position;
		Vector3 screenRight = Camera.main.ViewportToWorldPoint(new Vector3(1f, 1f, 10f));

		laserPath.Clear();
		laserPath.Add(new Vector3(startingPosition.x,
								  groundLevel.position.y,
								  startingPosition.z));
		laserPath.Add(new Vector3(screenRight.x + 1f,
								  groundLevel.position.y,
								  startingPosition.z));
		laserPath.Add(new Vector3(screenRight.x + 1f,
								  screenRight.y + 1f,
								  startingPosition.z));
		laserPathArray = laserPath.ToArray();
	}
}
