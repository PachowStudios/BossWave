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

	public float bodyGearRotationSpeed = 3f;
	public float sideGearRotationSpeed = 5f;

	public float sideFireStartTime;
	public Vector3 sideFireShakeIntensity;

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

	private float bodyMovement = 0f;
	private bool rotateBodyGears = false;
	private bool rotateSideGears = true;

	private bool sideFireStarted = false;
	#endregion

	#region Internal Properties
	private float LeftGunHealth
	{
		get { return leftGunHealth;}
		set
		{
			leftGunHealth = Mathf.Max(value, 0f);
			CheckSideGunDeath(leftGunHealth, leftGunSprite);
		}
	}

	private float RightGunHealth
	{
		get { return rightGunHealth; }
		set
		{
			rightGunHealth = Mathf.Max(value, 0f);
			CheckSideGunDeath(rightGunHealth, rightGunSprite);
		}
	}
	#endregion

	#region MonoBehaviour
	private void Update()
	{
		RotateGears();
		CheckFireParticles();
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
	private void RotateGears()
	{
		if (rotateBodyGears)
		{
			foreach (var gear in bodyGears)
				gear.Rotate(0f, 0f, -360f * bodyGearRotationSpeed * bodyMovement * Time.deltaTime);
		}

		if (rotateSideGears)
		{
			foreach (var gear in leftGears)
				gear.Rotate(0f, 0f, 360f * sideGearRotationSpeed * Time.deltaTime);

			foreach (var gear in rightGears)
				gear.Rotate(0f, 0f, -360f * sideGearRotationSpeed * Time.deltaTime);
		}
	}

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
	#endregion

	#region Internal Helper Methods
	protected override void CheckDeath(bool showDrops = true)
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
				leftGunHealth -= damage;
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
					CameraFollow.Instance.FollowObject(transform, false, 0.5f, true);
					BossIntro.Instance.Show(introName, introDescription, introSprite);
					PlayerControl.Instance.GoToPoint(LevelManager.Instance.BossWaveWaitPoint, false, false);
				})
			.Append(transform.DOPath(VectorPath.GetPath(spawnPathName), spawnPathTime, VectorPath.GetPathType(spawnPathName), PathMode.Sidescroller2D)
				.SetEase(Ease.InOutSine))
			.AppendCallback(() =>
				{
					CameraFollow.Instance.FollowObject(GameObject.FindGameObjectWithTag("CameraWrapper").transform, false, newYOffset: 0f);
					spawned = true;
					LevelManager.Instance.StartBossWave();
				});
	}

	public override void End()
	{

	}
	#endregion
}
