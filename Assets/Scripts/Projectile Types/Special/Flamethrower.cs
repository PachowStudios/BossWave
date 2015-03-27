using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Flamethrower : Projectile
{
	#region Fields
	public float range = 7f;
	public Vector2 width = new Vector2(0.5f, 2f);
	public float damageDelay = 0.25f;
	public float maxParticleLifetime = 1.75f;
	public LayerMask collisionLayer;

	private float cooldownTime;
	private float cooldownTimer;

	private List<Enemy> targetEnemies = new List<Enemy>();
	private ParticleSystem partSystem;
	private PolygonCollider2D detectionCollider;
	#endregion

	#region MonoBehaviour
	protected override void Awake()
	{
		base.Awake();

		cooldownTime = 1f / shotSpeed;
		cooldownTimer = -damageDelay;

		partSystem = GetComponentInChildren<ParticleSystem>();
		detectionCollider = gameObject.AddComponent<PolygonCollider2D>();
		detectionCollider.isTrigger = true;
		UpdateRotation();
	}

	private void Update()
	{
		UpdateCollider();
		GetTargets();

		cooldownTimer += Time.deltaTime;

		if (cooldownTimer >= cooldownTime)
		{
			DamageTargets();
			cooldownTimer = 0f;
		}
	}

	private void LateUpdate()
	{
		UpdateRotation();
	}

	private void OnDisable()
	{
		partSystem.transform.parent = null;
		partSystem.enableEmission = false;
		Destroy(partSystem.gameObject, maxParticleLifetime);
	}
	#endregion

	#region Internal Update Methods
	private void UpdateCollider()
	{
		detectionCollider.SetPath(0, new Vector2[] { PlayerControl.Instance.Gun.firePoint.TransformPointLocal(new Vector2(0f, width.x / 2f)),
													 PlayerControl.Instance.Gun.firePoint.TransformPointLocal(new Vector2(0f, -(width.x / 2f))),
													 PlayerControl.Instance.Gun.firePoint.TransformPointLocal(new Vector2(range, -(width.y / 2f))),
													 PlayerControl.Instance.Gun.firePoint.TransformPointLocal(new Vector2(range, width.y / 2f)) } );
	}

	private void GetTargets()
	{
		targetEnemies.Clear();

		Vector3 origin = PlayerControl.Instance.Gun.firePoint.position;
		transform.position = origin;

		foreach (Enemy enemy in GameObject.FindObjectsOfType<Enemy>())
		{
			if (enemy.collider2D != null && enemy.spawned)
			{
				if (detectionCollider.OverlapPoint(enemy.collider2D.bounds.center))
				{
					RaycastHit2D linecast = Physics2D.Linecast(origin, enemy.collider2D.bounds.center, collisionLayer);
					
					if (linecast.collider == null)
					{
						targetEnemies.Add(enemy);
					}
				}
			}
		}
	}

	private void DamageTargets()
	{
		foreach (Enemy enemy in targetEnemies)
		{
			enemy.TakeDamage(gameObject);
		}
	}

	private void UpdateRotation()
	{
		partSystem.transform.parent.rotation = Quaternion.Euler(PlayerControl.Instance.Gun.ShotDirection.DirectionToRotation2D());
	}
	#endregion
}
