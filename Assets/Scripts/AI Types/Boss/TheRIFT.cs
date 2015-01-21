using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using DG.Tweening;

public class TheRIFT : Boss
{
	public enum Attacks
	{
		Swoop,
		Laser
	};

	[System.Serializable]
	public struct Attack
	{
		public float time;
		public float preAttackTime;
		public List<Attacks> possibleAttacks;
	};

	public Color spawnColor = new Color(0.133f, 0.137f, 0.153f, 0f);
	public string spawnPathName;
	public string silhouetteTubesName;
	public string silhouetteBodyName;
	public float spawnFadeTime = 3f;
	public float spawnPathTime = 5f;
	public float returnSpeed = 30f;
	public float minFloatHeight = 3f;
	public float maxFloatHeight = 5f;
	public float swoopLength = 5f;
	public float smashRange = 4f;
	public float smashTime = 2f;
	public float smashGravity = -50f;
	public float laserLength = 3f;
	public AnimationCurve swoopCurve;
	public AnimationCurve laserCurve;
	public RIFTLaser laserPrefab;
	public List<Attack> attacks;

	private float defaultGravity;
	private float startingX;
	private Vector3 prevPosition;
	private int currentAttack = 0;
	private float attackTimer = 0f;
	private float smashTimer = 0f;
	private bool attacking = false;
	private bool preAttacking = false;
	private bool floating = true;
	private bool applyMovement = true;
	private List<Vector3> swoopPath = new List<Vector3>();
	private List<Vector3> laserPath = new List<Vector3>();
	private RIFTLaser laserInstance;

	private List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();
	private Transform firePoint;
	private Transform groundLevel;
	private SpriteRenderer silhouetteTubes;
	private GameObject silhouetteBody;

	protected override void Awake()
	{
		base.Awake();

		spriteRenderers = GetComponentsInChildren<SpriteRenderer>().ToList<SpriteRenderer>();
		firePoint = transform.FindChild("firePoint");
		groundLevel = GameObject.FindGameObjectWithTag("GroundLevel").transform;
		silhouetteTubes = GameObject.Find(silhouetteTubesName).GetComponent<SpriteRenderer>();
		silhouetteBody = GameObject.Find(silhouetteBodyName);

		defaultGravity = gravity;

		minFloatHeight += groundLevel.transform.position.y;
		maxFloatHeight += groundLevel.transform.position.y;

		foreach (SpriteRenderer sprite in spriteRenderers)
		{
			sprite.color = spawnColor;
		}
	}

	void FixedUpdate()
	{
		if (spawned)
		{
			MainAI();
		}
	}

	public override void TakeDamage(GameObject enemy)
	{
		if (!invincible)
		{
			anim.SetTrigger("Hit");
		}

		base.TakeDamage(enemy);
	}

	public override void Spawn()
	{
		if (!spawned)
		{
			CameraFollow.instance.FollowObject(transform, false, 2f, true);

			Sequence spawnSequence = DOTween.Sequence();

			spawnSequence.AppendInterval(0.5f);

			foreach (SpriteRenderer sprite in spriteRenderers)
			{
				spawnSequence.Insert(1, sprite.DOColor(Color.white, spawnFadeTime));
			}

			spawnSequence
				.AppendCallback(() => ExplodeEffect.Explode(silhouetteTubes.transform, Vector3.zero, silhouetteTubes.sprite))
				.AppendCallback(() => Destroy(silhouetteTubes.gameObject))
				.AppendCallback(() => Destroy(silhouetteBody))
				.Append(transform.DOPath(VectorPath.GetPath(spawnPathName), spawnPathTime, PathType.CatmullRom, PathMode.Sidescroller2D)
					.SetEase(Ease.InCubic))
				.AppendCallback(FinishSpawn);
		}
	}

	private void FinishSpawn()
	{
		CameraFollow.instance.FollowObject(GameObject.FindGameObjectWithTag("CameraWrapper").transform, true);
		spawned = true;
		startingX = transform.position.x;
		prevPosition = transform.position;
	}

	private void MainAI()
	{
		InitialUpdate();

		invincible = !attacking;
		anim.SetBool("Eye Shield", invincible);

		attackTimer += Time.deltaTime;

		if (currentAttack < attacks.Count && attackTimer >= attacks[currentAttack].time - attacks[currentAttack].preAttackTime)
		{
			if (!attacking && !preAttacking)
			{
				StartCoroutine(DoAttack(attacks[currentAttack]));
				currentAttack++;
				preAttacking = true;
			}
		}

		if (applyMovement)
		{
			if (floating)
			{
				Float();
			}

			ReturnPosition();

			GetMovement();
			ApplyMovement();
		}
	}

	private IEnumerator DoAttack(Attack attack)
	{
		int attackToUse = UnityEngine.Random.Range(0, attacks[currentAttack].possibleAttacks.Count);
		Action attackFunction = null;

		switch (attack.possibleAttacks[attackToUse])
		{
			case Attacks.Laser:
				anim.SetTrigger("PreAttack Laser");
				attackFunction = () => StartCoroutine(FireLaser());
				break;
			case Attacks.Swoop:
				anim.SetTrigger("PreAttack Swoop");
				attackFunction = () => Swoop();
				break;
		}

		if (attackFunction != null)
		{
			yield return new WaitForSeconds(attack.preAttackTime);

			attacking = true;
			preAttacking = false;
			attackFunction.Invoke();
		}
	}

	private IEnumerator FireLaser()
	{
		laserInstance = Instantiate(laserPrefab, firePoint.position, Quaternion.identity) as RIFTLaser;

		GameObject laserTarget = new GameObject();
		laserTarget.transform.parent = transform;

		Tween chargeTween = DOTween.To(() => laserTarget.transform.position, x => laserTarget.transform.position = x, Vector3.zero, laserInstance.chargeTime)
			.OnUpdate(() => laserInstance.firePoint = firePoint.position);

		yield return chargeTween.WaitForCompletion();

		GenerateLaserPath();
		laserTarget.transform.position = laserPath[0];
		laserTarget.transform.DOPath(laserPath.ToArray(), laserLength, PathType.CatmullRom, PathMode.Sidescroller2D)
			.SetEase(laserCurve)
			.OnUpdate(() =>
			{
				laserInstance.firePoint = firePoint.position;
				laserInstance.targetPoint = laserTarget.transform.position;
			})
			.OnComplete(() =>
			{
				laserInstance.Stop();
				laserInstance = null;
				Destroy(laserTarget);
				attacking = false;
			});
	}

	private void Swoop()
	{
		applyMovement = false;

		GenerateSwoopPath();
		transform.DOPath(swoopPath.ToArray(), swoopLength, PathType.CatmullRom, PathMode.Sidescroller2D)
			.SetEase(swoopCurve)
			.OnComplete(() =>
			{
				applyMovement = true;
				attacking = false;
			});
	}

	private void Float()
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

	private void ReturnPosition()
	{
		if (transform.position.x > startingX + 1f)
		{
			velocity.x = -returnSpeed;
		}

		if (transform.localScale.x > 0f && 
			prevPosition.x > transform.position.x && 
			transform.position.x > startingX)
		{
			Flip();
		}
		else if (transform.localScale.x < 0f && 
				 transform.position.x <= startingX)
		{
			Flip();
		}

		prevPosition = transform.position;
	}

	private void GenerateLaserPath()
	{
		Vector3 startingPosition = transform.position;
		Vector3 screenRight = Camera.main.ViewportToWorldPoint(new Vector3(1f, 1f, 10f));

		laserPath.Clear();
		laserPath.Add(new Vector3(startingX,
								  groundLevel.position.y,
								  startingPosition.z));
		laserPath.Add(new Vector3(screenRight.x + 1f,
								  groundLevel.position.y,
								  startingPosition.z));
		laserPath.Add(laserPath[0]);
	}

	private void GenerateSwoopPath()
	{
		Vector3 startingPosition = transform.position;

		swoopPath.Clear();
		swoopPath.Add(startingPosition);
		swoopPath.Add(new Vector3(PlayerControl.instance.transform.position.x,
									groundLevel.position.y - 1f,
									startingPosition.z));
		swoopPath.Add(Camera.main.ViewportToWorldPoint(new Vector3(1.2f, 0.5f, 10f)));
	}

	//private void MainAIOld()
	//{
	//	InitialUpdate();

	//	invincible = !swooping && !firingLaser && !preAttacking;
	//	anim.SetBool("Eye Shield", invincible);

	//	if (((!swooping && !firingLaser) || preAttacking) && !smashing)
	//	{
	//		anim.SetBool("Attacking", false);
	//	}

	//	// Floating
	//	if ((!swooping && !smashing) || preAttacking)
	//	{
	//		if (transform.position.y >= maxFloatHeight)
	//		{
	//			gravity = defaultGravity;

	//			if (velocity.y > 0)
	//			{
	//				velocity.y -= -defaultGravity * Time.deltaTime;
	//			}
	//		}
	//		else if (transform.position.y <= minFloatHeight)
	//		{
	//			gravity = -defaultGravity;

	//			if (velocity.y < 0)
	//			{
	//				velocity.y += -defaultGravity * Time.deltaTime;
	//			}
	//		}
	//	}

	//	// Smashing
	//	if (!swooping)
	//	{
	//		smashTimer += Time.deltaTime;

	//		if (smashTimer >= smashTime)
	//		{
	//			if (!smashing)
	//			{
	//				if (Mathf.Abs(transform.position.x - PlayerControl.instance.transform.position.x) < smashRange)
	//				{
	//					smashing = true;
	//					anim.SetBool("Attacking", true);
	//					smashTimer = smashTime;
	//				}
	//			}
	//			else
	//			{
	//				if (transform.position.y > groundLevel.position.y)
	//				{
	//					gravity = Mathf.Lerp(gravity, smashGravity, 0.75f);
	//				}
	//				else
	//				{
	//					smashing = false;
	//					smashTimer = 0f;
	//				}
	//			}
	//		}
	//	}

	//	// Swooping
	//	if (!smashing && smashTimer >= smashTime && !firingLaser)
	//	{
	//		swoopTimer += Time.deltaTime;

	//		if (swoopTimer >= swoopTime)
	//		{
	//			if (!swooping)
	//			{
	//				if (velocity.y < 0f && transform.position.y < (minFloatHeight + maxFloatHeight) / 2f)
	//				{
	//					swooping = true;
	//					preAttacking = true;
	//					anim.SetTrigger("Blink");
	//				}
	//			}
	//			else if (!preAttacking)
	//			{
	//				Vector3 prevPosition = transform.position;

	//				gameObject.PutOnPath(swoopPathArray, swoopPercentage);

	//				if (transform.localScale.x > 0 && prevPosition.x > transform.position.x && swoopPercentage > 0.25f)
	//				{
	//					Flip();
	//				}

	//				if (swoopPercentage == 1f)
	//				{
	//					swooping = false;
	//					swoopTimer = 0f;
	//					swoopTime = newSwoopTime;

	//					if (transform.localScale.x < 0)
	//					{
	//						Flip();
	//					}
	//				}
	//			}
	//		}
	//	}

	//	// Firing Laser
	//	if (!swooping)
	//	{
	//		laserTimer += Time.deltaTime;

	//		if (laserTimer >= laserTime)
	//		{
	//			if (!firingLaser)
	//			{
	//				firingLaser = true;
	//				preAttacking = true;
	//				anim.SetTrigger("PreAttack");
	//			}
	//			else if (!preAttacking && laserInstance != null)
	//			{
	//				laserInstance.firePoint = firePoint.position;

	//				if (laserInstance.Charging)
	//				{
	//					laserTimer = laserTime;
	//				}
	//				else
	//				{
	//					laserInstance.targetPoint = iTween.PointOnPath(laserPathArray, laserPercentage);

	//					if (laserPercentage == 1f)
	//					{
	//						firingLaser = false;
	//						laserTimer = 0f;
	//						laserTime = newLaserTime;
	//						laserInstance.Stop();
	//						laserInstance = null;
	//					}
	//				}
	//			}
	//		}
	//	}

	//	if (!swooping || preAttacking)
	//	{
	//		GetMovement();
	//		ApplyMovement();
	//	}
	//}

	//private void UpdateSwoopPath()
	//{
	//	Vector3 startingPosition = transform.position;

	//	swoopPath.Clear();
	//	swoopPath.Add(startingPosition);
	//	swoopPath.Add(new Vector3(PlayerControl.instance.transform.position.x, 
	//								groundLevel.position.y - 1f, 
	//								startingPosition.z));
	//	swoopPath.Add(Camera.main.ViewportToWorldPoint(new Vector3(1.2f, 0.5f, 10f)));
	//	swoopPath.Add(new Vector3(PlayerControl.instance.transform.position.x,
	//								Camera.main.ViewportToWorldPoint(new Vector3(1f, 0.75f, 10f)).y,
	//								startingPosition.z));
	//	swoopPath.Add(startingPosition);
	//	swoopPathArray = swoopPath.ToArray();
	//}

	//private void UpdateLaserPath()
	//{
	//	Vector3 startingPosition = transform.position;
	//	Vector3 screenRight = Camera.main.ViewportToWorldPoint(new Vector3(1f, 1f, 10f));

	//	laserPath.Clear();
	//	laserPath.Add(new Vector3(startingPosition.x,
	//							  groundLevel.position.y,
	//							  startingPosition.z));
	//	laserPath.Add(new Vector3(screenRight.x + 1f,
	//							  groundLevel.position.y,
	//							  startingPosition.z));
	//	laserPath.Add(laserPath[0]);
	//	laserPathArray = laserPath.ToArray();
	//}

	//private void EndPreAttack()
	//{
	//	StartCoroutine(DoEndPreAttack());
	//}

	//private IEnumerator DoEndPreAttack()
	//{
	//	yield return new WaitForSeconds(newPreAttackTime);

	//	if (preAttacking)
	//	{
	//		preAttacking = false;
	//		anim.SetBool("Attacking", true);

	//		if (swooping)
	//		{
	//			UpdateSwoopPath();
	//			swoopTimer = swoopTime;
	//		}
	//		else if (firingLaser)
	//		{
	//			UpdateLaserPath();
	//			laserInstance = Instantiate(laserPrefab, firePoint.position, Quaternion.identity) as RIFTLaser;
	//			laserInstance.firePoint = firePoint.position;
	//		}
	//	}
	//}
}
