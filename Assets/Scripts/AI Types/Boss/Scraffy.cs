using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public sealed class Scraffy : Boss
{
	#region Fields
	public Sprite warningPopup;
	public string spawnPathName;
	public float spawnPathTime;

	public float leftGunHealth = 200f;
	public float rightGunHealth = 200f;
	public float eyeMultiplier = 1.5f;

	public float bodyMovementRange = 18.8f;

	public float bodyGearRotationSpeed = 3f;
	public float sideGearRotationSpeed = 5f;

	public float sideFireStartTime;
	public Vector3 sideFireShakeIntensity;

	public Transform body;
	public BoxCollider2D bodyCollider;
	public BoxCollider2D eyeCollider;
	public BoxCollider2D leftGunCollider;
	public BoxCollider2D rightGunCollider;
	public SpriteRenderer leftGunSprite;
	public SpriteRenderer rightGunSprite;
	public List<Transform> bodyGears;
	public List<Transform> leftGears;
	public List<Transform> rightGears;
	public List<ParticleSystem> sideFires;

	private float bodyMovement = 1f;
	private float bodyVelocity = 0f;
	private bool rotateSideGears = false;

	private bool sideFireStarted = false;
	#endregion

	#region Internal Properties
	private float LeftGunHealth
	{
		get { return leftGunHealth;}
		set
		{
			leftGunHealth = Mathf.Max(value, 0f);

			if (flashOnHit)
				leftGunSprite.FlashColor(flashColor, flashLength);

			CheckSideGunDeath(leftGunHealth, leftGunSprite);
		}
	}

	private float RightGunHealth
	{
		get { return rightGunHealth; }
		set
		{
			rightGunHealth = Mathf.Max(value, 0f);

			if (flashOnHit)
				rightGunSprite.FlashColor(flashColor, flashLength);

			CheckSideGunDeath(rightGunHealth, rightGunSprite);
		}
	}
	#endregion

	#region MonoBehaviour
	private void Update()
	{
		CheckFireParticles();

		if (PlayerControl.Instance.transform.position.x > body.position.x + 1f)
			bodyMovement = 1f;
		else if (PlayerControl.Instance.transform.position.x < body.position.x - 1f)
			bodyMovement = -1f;
		else
			bodyMovement = 0f;
	}

	private void LateUpdate()
	{
		ApplyBodyMovement();
		ApplyGearRotation();
	}

	protected override void OnTriggerEnter2D(Collider2D other)
	{
		if (!ignoreProjectiles && other.tag == "PlayerProjectile")
		{
			if (Health > 0f)
			{
				if (other.bounds.Intersects(bodyCollider.bounds))
					TakeDamage(other.gameObject);
				else if (other.bounds.Intersects(eyeCollider.bounds))
					TakeDamage(other.gameObject, eyeMultiplier);
				else if (LeftGunHealth > 0f && other.bounds.Intersects(leftGunCollider.bounds))
					DamageSideGun(other.gameObject, true);
				else if (RightGunHealth > 0f && other.bounds.Intersects(rightGunCollider.bounds))
					DamageSideGun(other.gameObject, false);
				else
					other.GetComponent<Projectile>().CheckDestroyEnemy();
			}
		}
	}
	#endregion

	#region Internal Update Methods
	private void CheckFireParticles()
	{
		if (!sideFireStarted && LevelManager.Instance.MusicTime >= sideFireStartTime)
		{
			foreach (var fire in sideFires)
				fire.Play();

			CameraShake.Instance.Shake(0.5f, sideFireShakeIntensity);
			sideFireStarted = true;
		}
	}

	private void ApplyBodyMovement()
	{
		bodyVelocity = Mathf.Lerp(bodyVelocity,
								  bodyMovement * moveSpeed,
								  groundDamping * Time.deltaTime);

		body.Translate(new Vector3(bodyVelocity * Time.deltaTime, 0f), Space.Self);
		body.position = new Vector3(Mathf.Clamp(body.position.x, -bodyMovementRange, bodyMovementRange),
									body.position.y,
									body.position.z);

		if (Mathf.Abs(body.position.x) == bodyMovementRange)
			bodyMovement = 0f;
	}

	private void ApplyGearRotation()
	{
		if (bodyMovement != 0f)
			foreach (var gear in bodyGears)
				gear.Rotate(0f, 0f, -360f * bodyGearRotationSpeed * bodyMovement * Time.deltaTime);

		if (rotateSideGears)
		{
			foreach (var gear in leftGears)
				gear.Rotate(0f, 0f, 360f * sideGearRotationSpeed * Time.deltaTime);

			foreach (var gear in rightGears)
				gear.Rotate(0f, 0f, -360f * sideGearRotationSpeed * Time.deltaTime);
		}
	}
	#endregion

	#region Internal Helper Methods
	protected override void HandleDeath()
	{

	}

	private void DamageSideGun(GameObject enemy, bool leftSide)
	{
		Projectile enemyProjectile = enemy.GetComponent<Projectile>();
		float damage = enemyProjectile.damage;
		enemyProjectile.CheckDestroyEnemy();

		if (!invincible && damage != 0f)
		{
			if (leftSide)
				LeftGunHealth -= damage;
			else
				RightGunHealth -= damage;
		}
	}

	private void CheckSideGunDeath(float sideGunHealth, SpriteRenderer sideGunSprite)
	{
		if (sideGunHealth <= 0f)
		{

		}
	}
	#endregion

	#region Public Methods
	public override void Spawn()
	{
		if (spawned)
			return;

		Sequence spawnSequence = DOTween.Sequence();

		spawnSequence
			.AppendCallback(() =>
				{
					CameraShake.Instance.Shake(1f, new Vector3(0f, 2f, 0f));
					PopupMessage.Instance.CreatePopup(PlayerControl.Instance.PopupMessagePoint, "", warningPopup, true);
					PlayerControl.Instance.DisableInput();
				})
			.AppendInterval(2f)
			.AppendCallback(() =>
				{
					Cutscene.Instance.StartCutscene();
					CameraFollow.Instance.FollowObject(transform, newUsePlayerY: false, newYOffset: 0f, newSmoothing: 20f, newLockX: true);
					BossIntro.Instance.Show(introName, introDescription, introSprite);
					PlayerControl.Instance.GoToPoint(LevelManager.Instance.BossWaveWaitPoint, false, false);

					foreach (var gear in leftGears)
						gear.DORotate(new Vector3(0f, 0f, 360f * 5f), spawnPathTime, RotateMode.LocalAxisAdd)
							.SetEase(Ease.InExpo);

					foreach (var gear in rightGears)
						gear.DORotate(new Vector3(0f, 0f, -360f * 5f), spawnPathTime, RotateMode.LocalAxisAdd)
							.SetEase(Ease.InExpo);
				})
			.Append(transform.DOPath(VectorPath.GetPath(spawnPathName), spawnPathTime, VectorPath.GetPathType(spawnPathName), PathMode.Sidescroller2D)
				.SetEase(Ease.InExpo))
			.AppendCallback(() =>
				{
					CameraFollow.Instance.FollowObject(GameObject.FindGameObjectWithTag("CameraWrapper").transform, newUsePlayerY: false, newYOffset: 0f);
					spawned = true;
					LevelManager.Instance.StartBossWave();
					rotateSideGears = true;
				});
	}

	public override void End()
	{

	}
	#endregion
}
