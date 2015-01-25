using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using DG.Tweening;

public sealed class TheRIFT : Boss
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
	public string spawnLaserPathName;
	public string doorPathName;
	public string silhouetteTubesName;
	public string silhouetteBodyName;
	public string wallName;
	public string doorName;
	public float spawnFadeTime = 3f;
	public float spawnPathTime = 5f;
	public float spawnLaserPathTime = 2f;
	public float doorOpenTime = 1f;
	public float returnSpeed = 30f;
	public float minFloatHeight = 3f;
	public float maxFloatHeight = 5f;
	public float swoopLength = 5f;
	public float laserLength = 3f;
	public AnimationCurve swoopCurve;
	public AnimationCurve laserCurve;
	public AnimationCurve laserIntroCurve;
	public RIFTLaser laserPrefab;
	public List<Attack> attacks;

	private float defaultGravity;
	private float startingX;
	private Vector3 prevPosition;
	private int currentAttack = 0;
	private float attackTimer = 0f;
	private bool attacking = false;
	private bool preAttacking = false;
	private bool floating = true;
	private bool applyMovement = true;
	private RIFTLaser laserInstance;

	private List<SpriteRenderer> spriteRenderers;
	private Transform firePoint;
	private Transform groundLevel;
	private SpriteRenderer silhouetteTubes;
	private GameObject silhouetteBody;
	private SpriteRenderer wall;
	private Transform door;

	protected override void Awake()
	{
		base.Awake();

		spriteRenderers = GetComponentsInChildren<SpriteRenderer>().ToList<SpriteRenderer>();
		firePoint = transform.FindChild("firePoint");
		groundLevel = GameObject.FindGameObjectWithTag("GroundLevel").transform;
		silhouetteTubes = GameObject.Find(silhouetteTubesName).GetComponent<SpriteRenderer>();
		silhouetteBody = GameObject.Find(silhouetteBodyName);
		wall = GameObject.Find(wallName).GetComponent<SpriteRenderer>();
		door = GameObject.Find(doorName).transform;

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
				.Append(transform.DOPath(VectorPath.GetPath(spawnPathName), spawnPathTime, VectorPath.GetPathType(spawnPathName), PathMode.Sidescroller2D)
					.SetEase(Ease.InCubic)
					.OnComplete(() =>
					{
						door.DOLocalPath(VectorPath.GetPath(doorPathName), doorOpenTime, VectorPath.GetPathType(doorPathName), PathMode.Sidescroller2D)
							.SetEase(Ease.InOutCubic);
					}))
				.AppendCallback(FinishSpawn)
				.AppendCallback(() => FireLaser(VectorPath.GetPath(spawnLaserPathName), VectorPath.GetPathType(spawnLaserPathName), spawnLaserPathTime, laserIntroCurve, GameObject.Find("Foregrounds").transform))
				.AppendInterval(spawnLaserPathTime + 0.25f)
				.AppendCallback(() =>
				{
					ExplodeEffect.Explode(wall.transform, Vector3.zero, wall.sprite);
					Destroy(wall.gameObject);
				});
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
				attackFunction = () => FireLaser();
				break;
			case Attacks.Swoop:
				anim.SetTrigger("PreAttack Swoop");
				attackFunction = () => Swoop();
				break;
		}

		if (attackFunction != null)
		{
			yield return new WaitForSeconds(attack.preAttackTime);

			preAttacking = false;
			attackFunction.Invoke();
		}
	}

	private void FireLaser(Vector3[] laserPath = null, PathType pathType = PathType.CatmullRom, float length = -1f, AnimationCurve easeCurve = null, Transform targetParent = null)
	{
		attacking = true;

		laserPath = (laserPath == null) ? GenerateLaserPath() : laserPath;
		length = (length == -1f) ? laserLength : length;
		easeCurve = (easeCurve == null) ? laserCurve : easeCurve;

		laserInstance = Instantiate(laserPrefab, firePoint.position, Quaternion.identity) as RIFTLaser;

		Transform laserTarget = new GameObject().transform;
		laserTarget.name = "Laser Target";
		laserTarget.parent = (targetParent == null) ? laserTarget : targetParent;

		Sequence laserSequence = DOTween.Sequence();

		laserSequence
			.Append(DOTween.To(() => laserTarget.transform.position, x => laserTarget.transform.position = x, laserPath[0], laserInstance.chargeTime)
				.OnUpdate(() => laserInstance.firePoint = firePoint.position))
			.Append(laserTarget.DOLocalPath(laserPath, length, pathType, PathMode.Sidescroller2D)
				.SetEase(easeCurve)
				.OnUpdate(() =>
				{
					laserInstance.firePoint = firePoint.position;
					laserInstance.targetPoint = laserTarget.transform.position;
				})
				.OnComplete(() =>
				{
					laserInstance.Stop();
					laserInstance = null;
					attacking = false;
					Destroy(laserTarget.gameObject);
				}));
	}

	private void Swoop(Vector3[] swoopPath = null)
	{
		attacking = true;

		swoopPath = (swoopPath == null) ? GenerateSwoopPath() : swoopPath;
		applyMovement = false;

		transform.DOPath(swoopPath, swoopLength, PathType.CatmullRom, PathMode.Sidescroller2D)
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
			velocity.x = Mathf.Lerp(velocity.x, -returnSpeed, 0.25f);
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

	private Vector3[] GenerateLaserPath()
	{
		List<Vector3> laserPath = new List<Vector3>();
		Vector3 screenRight = Camera.main.ViewportToWorldPoint(new Vector3(1f, 1f, 10f));

		laserPath.Add(new Vector3(startingX,
								  groundLevel.position.y,
								  transform.position.z));
		laserPath.Add(new Vector3(screenRight.x + 1f,
								  groundLevel.position.y,
								  transform.position.z));
		laserPath.Add(laserPath[0]);

		return laserPath.ToArray();
	}

	private Vector3[] GenerateSwoopPath()
	{
		List<Vector3> swoopPath = new List<Vector3>();

		swoopPath.Add(transform.position);
		swoopPath.Add(new Vector3(PlayerControl.instance.transform.position.x,
									groundLevel.position.y - 1f,
									transform.position.z));
		swoopPath.Add(Camera.main.ViewportToWorldPoint(new Vector3(1.2f, 0.5f, 10f)));

		return swoopPath.ToArray();
	}
}
