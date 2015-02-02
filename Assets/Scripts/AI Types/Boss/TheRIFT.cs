using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using DG.Tweening;

public sealed class TheRIFT : Boss
{
	public enum AttackType
	{
		Swoop,
		Laser,
		Cannon
	};

	public enum AttackPattern
	{
		SweepAndBack,
		SwoopPlayer,
		PlayerProximity
	};

	[System.Serializable]
	public struct Attack
	{
		public float time;
		public float preAttackTime;
		public float length;
		public int modifier;
		public AttackPattern pattern;
		public AnimationCurve curve;
		public List<AttackType> possibleAttacks;
	};

	public string introName;
	public string introDescription;
	public Sprite introSprite;
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
	public AnimationCurve laserIntroCurve;
	public RIFTLaser laserPrefab;
	public Projectile cannonPrefab;
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
	private SpriteRenderer silhouetteTubes;
	private GameObject silhouetteBody;
	private SpriteRenderer wall;
	private Transform door;

	protected override void Awake()
	{
		base.Awake();

		spriteRenderers = GetComponentsInChildren<SpriteRenderer>().ToList<SpriteRenderer>();
		firePoint = transform.FindChild("firePoint");
		silhouetteTubes = GameObject.Find(silhouetteTubesName).GetComponent<SpriteRenderer>();
		silhouetteBody = GameObject.Find(silhouetteBodyName);
		wall = GameObject.Find(wallName).GetComponent<SpriteRenderer>();
		door = GameObject.Find(doorName).transform;

		defaultGravity = gravity;

		minFloatHeight += LevelManager.Instance.GroundLevel.y;
		maxFloatHeight += LevelManager.Instance.GroundLevel.y;

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
			CameraFollow.Instance.FollowObject(transform, false, 2f, true);

			Sequence spawnSequence = DOTween.Sequence();

			spawnSequence.AppendCallback(() => BossIntro.Instance.Show(introName, introDescription, introSprite));
			spawnSequence.AppendInterval(0.5f);

			foreach (SpriteRenderer sprite in spriteRenderers)
			{
				spawnSequence.Insert(1, sprite.DOColor(Color.white, spawnFadeTime));
			}

			spawnSequence
				.AppendCallback(() => ExplodeEffect.Instance.Explode(silhouetteTubes.transform, Vector3.zero, silhouetteTubes.sprite))
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
				.AppendCallback(() => FireLaser(spawnLaserPathTime, 0, laserIntroCurve, VectorPath.GetPath(spawnLaserPathName), VectorPath.GetPathType(spawnLaserPathName), GameObject.Find("Foregrounds").transform))
				.AppendInterval(spawnLaserPathTime + 0.25f)
				.AppendCallback(() =>
				{
					ExplodeEffect.Instance.Explode(wall.transform, Vector3.zero, wall.sprite);
					Destroy(wall.gameObject);
				});
		}
	}

	private void FinishSpawn()
	{
		CameraFollow.Instance.FollowObject(GameObject.FindGameObjectWithTag("CameraWrapper").transform, true);
		spawned = true;
		startingX = transform.position.x;
		prevPosition = transform.position;
	}

	private void MainAI()
	{
		InitialUpdate();

		invincible = !attacking;
		anim.SetBool("Eye Shield", invincible);
		anim.SetBool("Attacking", attacking);

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
		int attackToUse = UnityEngine.Random.Range(0, attack.possibleAttacks.Count);
		Action<Attack> attackFunction = null;

		switch (attack.possibleAttacks[attackToUse])
		{
			case AttackType.Laser:
				anim.SetTrigger("PreAttack Laser");
				attackFunction = (x) => FireLaser(x);
				break;
			case AttackType.Swoop:
				anim.SetTrigger("PreAttack Swoop");
				attackFunction = (x) => Swoop(x);
				break;
			case AttackType.Cannon:
				anim.SetTrigger("PreAttack Laser");
				attackFunction = (x) => FireCannon(x);
				break;
		}

		if (attackFunction != null)
		{
			yield return new WaitForSeconds(attack.preAttackTime);

			preAttacking = false;
			attackFunction.Invoke(attack);
		}
	}

	private void FireLaser(float length, AttackPattern pattern, AnimationCurve curve, Vector3[] laserPath = null, PathType pathType = PathType.CatmullRom, Transform targetParent = null)
	{
		attacking = true;
		anim.SetTrigger("Attack Flash");

		laserPath = (laserPath == null) ? GeneratePath(pattern) : laserPath;

		laserInstance = Instantiate(laserPrefab, firePoint.position, Quaternion.identity) as RIFTLaser;

		Transform laserTarget = new GameObject().transform;
		laserTarget.gameObject.HideInHiearchy();
		laserTarget.parent = (targetParent == null) ? laserTarget : targetParent;

		Sequence laserSequence = DOTween.Sequence();
		laserSequence
			.Append(DOTween.To(() => laserTarget.transform.position, x => laserTarget.transform.position = x, laserPath[0], laserInstance.chargeTime)
				.OnUpdate(() => laserInstance.firePoint = firePoint.position))
			.Append(laserTarget.DOLocalPath(laserPath, length, pathType, PathMode.Sidescroller2D)
				.SetEase(curve)
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

	private void FireLaser(Attack attack)
	{
		FireLaser(attack.length, attack.pattern, attack.curve);
	}

	private void Swoop(float length, AttackPattern pattern, AnimationCurve curve, Vector3[] swoopPath = null)
	{
		attacking = true;
		anim.SetTrigger("Attack Flash");

		swoopPath = (swoopPath == null) ? GeneratePath(pattern) : swoopPath;
		applyMovement = false;

		transform.DOPath(swoopPath, length, PathType.CatmullRom, PathMode.Sidescroller2D)
			.SetEase(curve)
			.OnComplete(() =>
			{
				applyMovement = true;
				attacking = false;
			});
	}

	private void Swoop(Attack attack)
	{
		Swoop(attack.length, attack.pattern, attack.curve);
	}

	private void FireCannon(float length, AttackPattern pattern, AnimationCurve curve, int shots, Vector3[] cannonPath = null)
	{
		attacking = true;
		anim.SetTrigger("Attack Cannon");

		cannonPath = (cannonPath == null) ? GeneratePath(pattern) : cannonPath;

		Transform cannonTarget = new GameObject().transform;
		cannonTarget.gameObject.HideInHiearchy();
		cannonTarget.transform.position = cannonPath[0];

		Sequence cannonSequence = DOTween.Sequence();
		cannonSequence.AppendInterval(0.25f);

		for (int i = 1; i <= shots; i++)
		{
			cannonSequence
				.AppendInterval(curve.EvaluateInterval(i, shots) * length)
				.AppendCallback(() =>
				{
					if (cannonTarget != null)
					{
						Projectile currentProjectile = Instantiate(cannonPrefab, firePoint.position, Quaternion.identity) as Projectile;
						currentProjectile.Initialize(firePoint.position.LookAt2D(cannonTarget.position) * Vector3.right);
						SpriteEffect.Instance.SpawnEffect("Small Dust Explosion", firePoint.position, firePoint);
						anim.SetTrigger("Cannon Fire");
					}
				});
		}

		cannonTarget.DOPath(cannonPath, length, PathType.Linear, PathMode.Sidescroller2D)
			.SetDelay(0.25f)
			.SetEase(Ease.Linear)
			.OnComplete(() =>
			{
				attacking = false;
				cannonSequence.Kill();
				Destroy(cannonTarget.gameObject);
			});
	}

	private void FireCannon(Attack attack)
	{
		FireCannon(attack.length, attack.pattern, attack.curve, attack.modifier);
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

	private Vector3[] GeneratePath(AttackPattern pattern)
	{
		List<Vector3> path = new List<Vector3>();
		Vector3 screenRight = Camera.main.ViewportToWorldPoint(new Vector3(1f, 1f, 10f));

		switch (pattern)
		{ 
			case AttackPattern.SweepAndBack:
				path.Add(new Vector3(startingX,
									 LevelManager.Instance.GroundLevel.y,
									 transform.position.z));
				path.Add(new Vector3(screenRight.x + 1f,
									 LevelManager.Instance.GroundLevel.y,
									 transform.position.z));
				path.Add(path[0]);
				break;
			case AttackPattern.SwoopPlayer:
				path.Add(transform.position);
				path.Add(new Vector3(PlayerControl.Instance.transform.position.x,
									 LevelManager.Instance.GroundLevel.y - 1f,
									 transform.position.z));
				path.Add(Camera.main.ViewportToWorldPoint(new Vector3(1.2f, 0.5f, 10f)));
				break;
			case AttackPattern.PlayerProximity:
				path.Add(new Vector3(PlayerControl.Instance.transform.position.x - 3f,
									 LevelManager.Instance.GroundLevel.y,
									 transform.position.z));
				path.Add(new Vector3(PlayerControl.Instance.transform.position.x + 3f,
									 LevelManager.Instance.GroundLevel.y,
									 transform.position.z));
				break;
		}

		return path.ToArray();
	}
}
