using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using DG.Tweening;

public sealed class TheRIFT : Boss
{
	#region Fields
	public enum AttackType
	{
		Swoop,
		Laser,
		Cannon
	};

	public enum AttackPattern
	{
		Sweep,
		SweepAndBack,
		SwoopPlayer,
		SwoopPlayerFar,
		PlayerProximity,
	};

	[System.Serializable]
	public struct Attack
	{
		public AttackType type;
		public float length;
		public int modifier;
		public AttackPattern pattern;
		public AnimationCurve curve;
	}

	[System.Serializable]
	public struct AttackWave
	{
		public float time;
		public float preAttackTime;
		public List<Attack> possibleAttacks;
	};

	public string introName;
	public string introDescription;
	public Sprite introSprite;
	public Sprite warningPopup;
	public int fightFOV = 630;
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
	public string cannonFireEffect;
	public List<AttackWave> attacks;

	private float defaultGravity;
	private float startingX;
	private Vector3 prevPosition;
	private int currentAttack = 0;
	private bool attacking = false;
	private bool preAttacking = false;
	private bool floating = true;
	private bool applyMovement = true;
	private bool dead = false;
	private RIFTLaser laserInstance;

	private List<SpriteRenderer> spriteRenderers;
	private Transform firePoint;
	private GhostTrailEffect ghostTrail;

	private SpriteRenderer silhouetteTubes;
	private GameObject silhouetteBody;
	private SpriteRenderer wall;
	private Transform door;
	#endregion

	#region MonoBehaviour
	protected override void Awake()
	{
		base.Awake();

		spriteRenderers = GetComponentsInChildren<SpriteRenderer>().ToList<SpriteRenderer>();
		firePoint = transform.FindChild("firePoint");
		ghostTrail = GetComponent<GhostTrailEffect>();

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

	private void Update()
	{
		if (spawned && !dead)
		{
			invincible = !attacking;
			anim.SetBool("Eye Shield", invincible);
			anim.SetBool("Attacking", attacking);

			if (currentAttack < attacks.Count && LevelManager.Instance.MusicTime >= attacks[currentAttack].time - attacks[currentAttack].preAttackTime &&
				!PlayerControl.Instance.Dead)
			{
				if (!attacking && !preAttacking)
				{
					StartCoroutine(DoAttack(attacks[currentAttack]));
					currentAttack++;
					preAttacking = true;
				}
			}
			else if (PlayerControl.Instance.Dead)
			{
				End();
			}
		}
	}

	private void LateUpdate()
	{
		if (spawned)
		{
			if (!dead)
			{
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
			else
			{
				FallToGround();

				GetMovement();
				ApplyMovement();
			}
		}
	}
	#endregion

	#region Internal Helper Methods
	private IEnumerator DoAttack(AttackWave attackWave)
	{
		int attackToUse = UnityEngine.Random.Range(0, attackWave.possibleAttacks.Count);
		Action<Attack> attackFunction = null;

		switch (attackWave.possibleAttacks[attackToUse].type)
		{
			case AttackType.Laser:
				anim.SetTrigger("PreAttack Laser");
				attackFunction = (x) => FireLaser(x);
				attackWave.preAttackTime = Mathf.Max(0f, attackWave.preAttackTime - RIFTLaser.ChargeLength);
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
			yield return new WaitForSeconds(attackWave.preAttackTime);

			preAttacking = false;
			attackFunction.Invoke(attackWave.possibleAttacks[attackToUse]);
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

		DOTween.Sequence()
			.SetId("RIFT Attack")
			.Append(DOTween.To(x => { }, 0f, 0f, RIFTLaser.ChargeLength)
				.OnUpdate(() =>
				{
					laserInstance.firePoint = firePoint.position;
					laserTarget.localPosition = laserPath[0];
				}))
			.Append(laserTarget.DOLocalPath(laserPath, length, pathType, PathMode.Sidescroller2D)
				.SetEase(curve)
				.OnUpdate(() =>
				{
					laserInstance.firePoint = firePoint.position;
					laserInstance.targetPoint = laserTarget.position;
				})
				.OnKill(() =>
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
		ghostTrail.trailActive = true;

		transform.DOPath(swoopPath, length, PathType.CatmullRom, PathMode.Sidescroller2D)
			.SetId("RIFT Swoop")
			.SetEase(curve)
			.OnComplete(() =>
			{
				applyMovement = true;
				ghostTrail.trailActive = false;
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
		cannonSequence
			.SetId("RIFT Attack")
			.AppendInterval(0.25f);

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
						SpriteEffect.Instance.SpawnEffect(cannonFireEffect, firePoint.position, firePoint);
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
		if (transform.position.x > startingX + 3f)
		{
			velocity.x = Mathf.Lerp(velocity.x, -returnSpeed, 15f * Time.deltaTime);
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

	private void FallToGround()
	{
		if (transform.position.y > LevelManager.Instance.GroundLevel.y - 0.7f)
		{
			gravity = defaultGravity;
			anim.SetTrigger("Hit");
		}
		else
		{
			if (gravity != 0f)
			{
				SpriteEffect.Instance.SpawnEffect("Big Dust Poof", transform.position);
				CameraShake.Instance.Shake(1f, new Vector3(0f, 2f, 0f));
				anim.SetBool("Dead", true);
				LevelManager.Instance.CompleteLevel();
			}

			gravity = 0f;
			velocity = Vector3.zero;
			enabled = false;
		}
	}

	private Vector3[] GeneratePath(AttackPattern pattern)
	{
		List<Vector3> path = new List<Vector3>();
		Vector3 screenRight = Camera.main.ViewportToWorldPoint(new Vector3(1f, 1f, 10f));

		switch (pattern)
		{ 
			case AttackPattern.Sweep:
				path.Add(new Vector3(startingX,
									 LevelManager.Instance.GroundLevel.y,
									 transform.position.z));
				path.Add(new Vector3(screenRight.x + 1f,
									 LevelManager.Instance.GroundLevel.y,
									 transform.position.z));
				break;
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
									 LevelManager.Instance.GroundLevel.y - 3f,
									 transform.position.z));
				path.Add(Camera.main.ViewportToWorldPoint(new Vector3(1.2f, 0.5f, 10f)));
				break;
			case AttackPattern.SwoopPlayerFar:
				path.Add(transform.position);
				path.Add(new Vector3(PlayerControl.Instance.transform.position.x,
									 LevelManager.Instance.GroundLevel.y - 3f,
									 transform.position.z));
				path.Add(Camera.main.ViewportToWorldPoint(new Vector3(1.2f, 0.5f, 10f)));
				path.Add(Camera.main.ViewportToWorldPoint(new Vector3(1.7f, 0.5f, 10f)));
				break;
			case AttackPattern.PlayerProximity:
				path.Add(new Vector3(PlayerControl.Instance.transform.position.x - 4f,
									 LevelManager.Instance.GroundLevel.y,
									 transform.position.z));
				path.Add(new Vector3(PlayerControl.Instance.transform.position.x + 4f,
									 LevelManager.Instance.GroundLevel.y,
									 transform.position.z));
				break;
		}

		return path.ToArray();
	}

	protected override void CheckDeath(bool showDrops = true)
	{
		if (Health <= 0f && !dead)
		{
			dead = true;
			ghostTrail.trailActive = false;

			PlayerControl.Instance.AddPointsFromEnemy(maxHealth, damage);
			DOTween.Kill("RIFT Attack");
			DOTween.Kill("RIFT Swoop");
			DOTween.Kill("Boss Wave Timer");
		}
	}
	#endregion

	#region Public Methods
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
			Sequence spawnSequence = DOTween.Sequence();

			foreach (SpriteRenderer sprite in spriteRenderers)
			{
				spawnSequence.Insert(0, sprite.DOColor(Color.white, spawnFadeTime)
					.SetEase(Ease.InExpo));
			}

			spawnSequence
				.AppendCallback(() =>
				{
					ExplodeEffect.Instance.Explode(silhouetteTubes.transform, Vector3.zero, silhouetteTubes.sprite);
					Destroy(silhouetteTubes.gameObject);
					Destroy(silhouetteBody);
					CameraShake.Instance.Shake(2f, new Vector3(0f, 2f, 0f));
					LevelManager.Instance.KillAllEnemies();
					AssemblyLine.StopAll();
					StartCoroutine(PlayerControl.Instance.JumpToFloor());
					PopupMessage.Instance.CreatePopup(PlayerControl.Instance.PopupMessagePoint, "", warningPopup, true);
				})
				.AppendInterval(2f)
				.AppendCallback(() =>
				{
					Cutscene.Instance.Show();
					ScaleWidthCamera.Instance.AnimateFOV(fightFOV, 1f);
					CameraFollow.Instance.FollowObject(transform, false, 3.9f, true);
					BossIntro.Instance.Show(introName, introDescription, introSprite);
					PlayerControl.Instance.GoToPoint(LevelManager.Instance.bossWave.playerWaitPoint.position, false, false);
				})
				.Append(transform.DOPath(VectorPath.GetPath(spawnPathName), spawnPathTime, VectorPath.GetPathType(spawnPathName), PathMode.Sidescroller2D)
					.SetEase(Ease.InQuart)
					.OnComplete(() =>
					{
						door.DOLocalPath(VectorPath.GetPath(doorPathName), doorOpenTime, VectorPath.GetPathType(doorPathName), PathMode.Sidescroller2D)
							.SetEase(Ease.InOutCubic);
					}))
				.AppendCallback(() =>
				{
					FireLaser(spawnLaserPathTime, 0, laserIntroCurve, VectorPath.GetPath(spawnLaserPathName), VectorPath.GetPathType(spawnLaserPathName), GameObject.Find("Foregrounds").transform);
					CameraFollow.Instance.FollowObject(GameObject.FindGameObjectWithTag("CameraWrapper").transform, true);
					spawned = true;
					startingX = transform.position.x;
					prevPosition = transform.position;
				})
				.AppendInterval(spawnLaserPathTime + 0.25f)
				.AppendCallback(() =>
				{
					ExplodeEffect.Instance.Explode(wall.transform, Vector3.zero, wall.sprite);
					Destroy(wall.gameObject);
				});
		}
	}

	public override void End()
	{
		DOTween.Kill("RIFT Attack");
		maxFloatHeight = 1000f;
		minFloatHeight = 990f;
	}
	#endregion
}
