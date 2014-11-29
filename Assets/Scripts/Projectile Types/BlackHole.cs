using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BlackHole : Projectile
{
	public float damageRate = 5f;
	public float outerForce = 20f;
	public float innerForce = 40f;
	public float outerRotation = 0.5f;
	public float innerRotation = 1f;
	[Range(0.1f, 1f)]
	public float activationBuffer = 80f;
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

	private CircleCollider2D outerRadius;
	private CircleCollider2D innerRadius;

	new void Awake()
	{
		controller = GetComponent<CharacterController2D>();
		spriteRenderer = transform.FindChild("sprite").GetComponent<SpriteRenderer>();

		outerRadius = transform.FindChild("outerRadius").GetComponent<CircleCollider2D>();
		innerRadius = transform.FindChild("innerRadius").GetComponent<CircleCollider2D>();

		damageTime = 1f / damageRate;
		damageTimer = damageTime;
	}

	void FixedUpdate()
	{
		InitialUpdate();

		ApplyMovement();

		if (activated)
		{
			shotSpeed = Mathf.Lerp(shotSpeed, 0f, 0.1f);

			if (!spawned && shotSpeed <= 2f)
			{
				Spawn();
			}

			if (spawned)
			{
				particleSystemInstance.transform.position = transform.position;

				SimulateParticles();
				SimulateEnemies();
				SimulateProjectiles();
			}
		}
	}

	new void OnTriggerEnter2D(Collider2D trigger)
	{
		if (trigger.gameObject.layer == LayerMask.NameToLayer("Collider"))
		{
			CheckDestroyWorld();
		}
		else if (trigger.gameObject.layer == LayerMask.NameToLayer("Enemies") && outerRadius.OverlapPoint(trigger.bounds.center))
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
				if (trigger.tag == "Enemy")
				{
					Enemy currentEnemy = trigger.gameObject.GetComponent<Enemy>();

					if (!targetEnemies.Contains(currentEnemy))
					{
						targetEnemies.Add(currentEnemy);
					}
				}
				else if (trigger.tag == "Projectile" || trigger.tag == "PlayerProjectile")
				{
					Projectile currentProjectile = trigger.gameObject.GetComponent<Projectile>();

					if (!targetProjectiles.Contains(currentProjectile))
					{
						currentProjectile.disableMovement = true;
						targetProjectiles.Add(currentProjectile);
					}
				}
			}
		}
	}

	new void OnTriggerStay2D(Collider2D trigger)
	{
		OnTriggerEnter2D(trigger);
	}

	private void Spawn()
	{
		spawned = true;

		spriteRenderer.gameObject.ColorTo(color, 0.2f, 0f);

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
					currentParticles[i].velocity = Vector3.Lerp(currentParticles[i].velocity, currentParticles[i].position.CalculateBlackHoleForce(outerForce, transform.position, outerRadius.radius, outerRotation), 0.1f);
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
			damageTimer += Time.deltaTime;

			foreach (Enemy currentEnemy in targetEnemies)
			{
				if (currentEnemy != null)
				{
					if (innerRadius.OverlapPoint(currentEnemy.collider2D.bounds.center))
					{
						currentEnemy.Move(currentEnemy.transform.position.CalculateBlackHoleForce(innerForce, transform.position, outerRadius.radius, innerRotation));

						if (damageTimer >= damageTime)
						{
							currentEnemy.Kill();
						}
					}
					else if (outerRadius.OverlapPoint(currentEnemy.collider2D.bounds.center))
					{
						currentEnemy.Move(Vector3.Lerp(currentEnemy.velocity, currentEnemy.transform.position.CalculateBlackHoleForce(outerForce, transform.position, outerRadius.radius, outerRotation), 0.5f));

						if (damageTimer >= damageTime)
						{
							ExplodeEffect.ExplodePartial(currentEnemy.transform, currentEnemy.velocity * 2f, currentEnemy.Sprite, 0.1f);
							currentEnemy.TakeDamage(gameObject);
						}
					}
				}
			}

			targetEnemies.RemoveAll(e => e == null);

			if (damageTimer >= damageTime)
			{
				damageTimer = 0f;
			}
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
						currentProjectile.Move(currentProjectile.transform.position.CalculateBlackHoleForce(innerForce, transform.position, outerRadius.radius, innerRotation));

						currentProjectile.DoDestroy();
					}
					else if (outerRadius.OverlapPoint(currentProjectile.collider2D.bounds.center))
					{
						currentProjectile.Move(Vector3.Lerp(currentProjectile.velocity, currentProjectile.transform.position.CalculateBlackHoleForce(outerForce, transform.position, outerRadius.radius, outerRotation), 0.1f));
					}
				}
			}

			targetProjectiles.RemoveAll(p => p == null);
		}
	}

	private IEnumerator DestroyEmitter()
	{
		yield return new WaitForSeconds(lifetime);

		foreach (Projectile currentProjectile in targetProjectiles)
		{
			currentProjectile.disableMovement = false;
		}

		ExplodeEffect.Explode(transform, Vector3.zero, spriteRenderer.sprite);
		particleSystemInstance.enableEmission = false;
		Destroy(particleSystemInstance.gameObject, particleDestroyDelay);
		Destroy(gameObject);
	}
}
