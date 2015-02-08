using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public class BlackHole : Projectile
{
	public float damageRate = 5f;
	public float outerForce = 20f;
	public float innerForce = 40f;
	public float outerRotation = 0.5f;
	public float innerRotation = 1f;
	[Range(0.1f, 1f)]
	public float activationBuffer = 80f;
	public float activationSpeed = 2f;
	public Color color = Color.black;
	public ParticleSystem particleSystemPrefab;
	public string particlesSortingLayer = "Foreground";
	public int particlesSortingOrder = 1;
	public float particleDestroyDelay = 1f;
	public float generatedParticleLifetime = 0.5f;
	public float affectedParticleLifetime = 1f;

	private bool activated = false;
	private bool spawned = false;
	private float damageTime;
	private float damageTimer;
	private ParticleSystem particleSystemInstance;
	private List<ParticleSystem> targetParticleSystems;
	private List<Enemy> targetEnemies = new List<Enemy>();
	private List<Projectile> targetProjectiles = new List<Projectile>();
	private List<Powerup> targetPowerups = new List<Powerup>();

	private CircleCollider2D outerRadius;
	private CircleCollider2D innerRadius;

	protected override void Awake()
	{
		controller = GetComponent<CharacterController2D>();
		spriteRenderer = transform.FindChild("sprite").GetComponent<SpriteRenderer>();
		anim = spriteRenderer.GetComponent<Animator>();

		outerRadius = transform.FindChild("outerRadius").GetComponent<CircleCollider2D>();
		innerRadius = transform.FindChild("innerRadius").GetComponent<CircleCollider2D>();

		damageTime = 1f / damageRate;
		damageTimer = damageTime;

		if (autoDestroy)
		{
			StartCoroutine(FailsafeDestroy());
		}
	}

	private void Update()
	{
		if (!activated && CrossPlatformInputManager.GetButtonDown("SecondaryShoot"))
		{
			activated = true;
		}
	}

	private void FixedUpdate()
	{
		InitialUpdate();

		ApplyMovement();

		if (activated)
		{
			shotSpeed = Mathf.Lerp(shotSpeed, 0f, 0.1f);
			anim.SetBool("Active", true);

			if (!spawned && shotSpeed <= activationSpeed)
			{
				Spawn();
			}

			if (spawned)
			{
				particleSystemInstance.transform.position = transform.position;

				damageTimer += Time.deltaTime;

				SimulateParticles();
				SimulateEnemies();
				SimulateProjectiles();
				SimulatePowerups();
				SimulatePlayer();

				if (damageTimer >= damageTime)
				{
					damageTimer = 0f;
				}
			}
		}
	}

	protected override void OnTriggerEnter2D(Collider2D trigger)
	{
		if (trigger.gameObject.layer == LayerMask.NameToLayer("Collider"))
		{
			CheckDestroyWorld();
		}
		else if (trigger.gameObject.layer == LayerMask.NameToLayer("Enemies") && outerRadius.OverlapPoint(trigger.bounds.center))
		{
			Enemy currentEnemy = trigger.gameObject.GetComponent<Enemy>();

			if (!currentEnemy.ignoreProjectiles)
			{
				if (trigger.bounds.center.DistanceFrom(transform.position) <= outerRadius.radius * activationBuffer)
				{
					if (!activated)
					{
						activated = true;
					}
				}

				if (activated)
				{
					if (!targetEnemies.Contains(currentEnemy))
					{
						targetEnemies.Add(currentEnemy);
					}
				}
			}
		}
		else if ((trigger.gameObject.layer == LayerMask.NameToLayer("Projectiles") || trigger.gameObject.layer == LayerMask.NameToLayer("EnemyProjectiles")) && outerRadius.OverlapPoint(trigger.bounds.center))
		{
			if (activated)
			{
				Projectile currentProjectile = trigger.gameObject.GetComponent<Projectile>();

				if (!targetProjectiles.Contains(currentProjectile))
				{
					targetProjectiles.Add(currentProjectile);
				}
			}
		}
	}

	private void Spawn()
	{
		spawned = true;

		spriteRenderer.DOColor(color, 0.2f);

		particleSystemInstance = Instantiate(particleSystemPrefab, transform.position, Quaternion.identity) as ParticleSystem;
		particleSystemInstance.renderer.sortingLayerName = particlesSortingLayer;
		particleSystemInstance.renderer.sortingOrder = particlesSortingOrder;
		particleSystemInstance.startColor = color;
		particleSystemInstance.startLifetime = generatedParticleLifetime;

		if (autoDestroy)
		{
			StartCoroutine(DestroyEmitter());
		}
	}

	private void SimulateParticles()
	{
		targetParticleSystems = GameObject.FindObjectsOfType<ParticleSystem>().ToList<ParticleSystem>();

		foreach (ParticleSystem particleSystem in targetParticleSystems)
		{
			ParticleSystem.Particle[] currentParticles = new ParticleSystem.Particle[particleSystem.particleCount];
			particleSystem.GetParticles(currentParticles);

			for (int i = 0; i < currentParticles.Length; i++)
			{
				if (innerRadius.OverlapPoint(currentParticles[i].position))
				{
					currentParticles[i].velocity = currentParticles[i].position.CalculateBlackHoleForce(innerForce, transform.position, outerRadius.radius, innerRotation);
				}
				else if (outerRadius.OverlapPoint(currentParticles[i].position))
				{
					currentParticles[i].velocity = Vector3.Lerp(currentParticles[i].velocity, currentParticles[i].position.CalculateBlackHoleForce(outerForce, transform.position, outerRadius.radius, outerRotation, 1.5f), 0.1f);
					currentParticles[i].startLifetime = currentParticles[i].lifetime = affectedParticleLifetime;
				}
			}

			particleSystem.SetParticles(currentParticles, currentParticles.Length);
		}
	}

	private void SimulateEnemies()
	{
		if (targetEnemies.Count > 0)
		{
			foreach (Enemy currentEnemy in targetEnemies)
			{
				if (currentEnemy != null && !currentEnemy.ignoreProjectiles)
				{
					if (innerRadius.OverlapPoint(currentEnemy.collider2D.bounds.center))
					{
						currentEnemy.Kill();
					}
					else if (outerRadius.OverlapPoint(currentEnemy.collider2D.bounds.center))
					{
						if (!currentEnemy.immuneToInstantKill)
						{
							currentEnemy.Move(Vector3.Lerp(currentEnemy.velocity, currentEnemy.transform.position.CalculateBlackHoleForce(outerForce, transform.position, outerRadius.radius, outerRotation), 0.5f));
						}

						if (damageTimer >= damageTime)
						{
							ExplodeEffect.Instance.ExplodePartial(currentEnemy.transform, currentEnemy.velocity, currentEnemy.Sprite, 0.05f);
							currentEnemy.TakeDamage(gameObject);
						}
					}
				}
			}

			targetEnemies.RemoveAll(e => e == null);
		}
	}

	private void SimulateProjectiles()
	{
		if (targetProjectiles.Count > 0)
		{
			foreach (Projectile currentProjectile in targetProjectiles)
			{
				if (currentProjectile != null)
				{
					if (innerRadius.OverlapPoint(currentProjectile.collider2D.bounds.center))
					{
						currentProjectile.DoDestroy();
					}
					else if (outerRadius.OverlapPoint(currentProjectile.collider2D.bounds.center))
					{
						Vector3 force = currentProjectile.transform.position.CalculateBlackHoleForce(outerForce, transform.position, outerRadius.radius, outerRotation);
						currentProjectile.direction = Vector3.Lerp(currentProjectile.direction, force.normalized, 0.05f);
						currentProjectile.Move(force);
					}
				}
			}

			targetProjectiles.RemoveAll(p => p == null);
		}
	}

	private void SimulatePowerups()
	{
		foreach (Powerup currentPowerup in GameObject.FindObjectsOfType<Powerup>())
		{
			if (!targetPowerups.Contains(currentPowerup) && outerRadius.OverlapPoint(currentPowerup.collider2D.bounds.center))
			{
				targetPowerups.Add(currentPowerup);
			}
		}

		if (targetPowerups.Count > 0)
		{
			foreach (Powerup currentPowerup in targetPowerups)
			{
				if (currentPowerup != null)
				{
					if (innerRadius.OverlapPoint(currentPowerup.collider2D.bounds.center))
					{
						currentPowerup.Destroy();
					}
					else if (outerRadius.OverlapPoint(currentPowerup.collider2D.bounds.center))
					{
						currentPowerup.rigidbody2D.velocity = Vector3.Lerp(currentPowerup.rigidbody2D.velocity, currentPowerup.transform.position.CalculateBlackHoleForce(outerForce, transform.position, outerRadius.radius, outerRotation), 0.5f);
					}
				}
			}

			targetPowerups.RemoveAll(p => p == null);
		}
	}

	private void SimulatePlayer()
	{
		if (innerRadius.OverlapPoint(PlayerControl.Instance.collider2D.bounds.center))
		{
			PlayerControl.Instance.Health = 0f;
		}
		else if (outerRadius.OverlapPoint(PlayerControl.Instance.collider2D.bounds.center))
		{
			PlayerControl.Instance.Move(Vector3.Lerp(PlayerControl.Instance.Velocity, PlayerControl.Instance.transform.position.CalculateBlackHoleForce(outerForce, transform.position, outerRadius.radius, outerRotation), 0.15f));

			if (damageTimer >= damageTime)
			{
				foreach (SpriteRenderer sprite in PlayerControl.Instance.SpriteRenderers)
				{
					if (sprite.color != Color.clear)
					{
						Transform tempTransform = new GameObject().transform;
						sprite.transform.CopyTo(tempTransform);
						tempTransform.localScale = PlayerControl.Instance.transform.localScale;
						ExplodeEffect.Instance.ExplodePartial(tempTransform.transform, PlayerControl.Instance.Velocity, sprite.sprite, 0.1f / PlayerControl.Instance.SpriteRenderers.Count);
						Destroy(tempTransform.gameObject);
					}
				}

				PlayerControl.Instance.Health -= damage;
			}
		}
	}

	private IEnumerator DestroyEmitter()
	{
		yield return new WaitForSeconds(lifetime);

		ExplodeEffect.Instance.Explode(transform, Vector3.zero, Sprite);
		particleSystemInstance.enableEmission = false;
		Destroy(particleSystemInstance.gameObject, particleDestroyDelay);
		Destroy(gameObject);
	}

	new IEnumerator FailsafeDestroy()
	{
		yield return new WaitForSeconds(lifetime);

		if (!activated)
		{
			ExplodeEffect.Instance.Explode(transform, velocity, Sprite);
			Destroy(gameObject);
		}
	}
}
