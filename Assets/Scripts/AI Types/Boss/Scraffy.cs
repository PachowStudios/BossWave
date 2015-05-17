using UnityEngine;
using System.Collections;
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

	public BoxCollider2D bodyCollider;
	public BoxCollider2D eyeCollider;
	public BoxCollider2D leftGunCollider;
	public BoxCollider2D rightGunCollider;
	public SpriteRenderer leftGunSprite;
	public SpriteRenderer rightGunSprite;
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
