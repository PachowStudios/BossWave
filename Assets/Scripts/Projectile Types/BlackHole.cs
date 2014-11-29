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
	private List<ParticleSystem> allParticleSystems;
	private List<Enemy> allEnemies = new List<Enemy>();

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

			if (!spawned && shotSpeed <= 0.5f)
			{
				Spawn();
			}

			if (spawned)
			{
				particleSystemInstance.transform.position = transform.position;

				SimulateParticles();
				SimulateEnemies();
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
				Enemy currentEnemy = trigger.gameObject.GetComponent<Enemy>();

				if (!allEnemies.Contains(currentEnemy))
				{
					allEnemies.Add(currentEnemy);
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
		allParticleSystems = GameObject.FindObjectsOfType<ParticleSystem>().ToList<ParticleSystem>();

		foreach (ParticleSystem particleSystem in allParticleSystems)
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
		if (allEnemies.Count > 0)
		{
			damageTimer += Time.deltaTime;

			foreach (Enemy currentEnemy in allEnemies)
			{
				if (currentEnemy != null)
				{
					if (innerRadius.OverlapPoint(currentEnemy.transform.position))
					{
						currentEnemy.Move(currentEnemy.transform.position.CalculateBlackHoleForce(innerForce, transform.position, outerRadius.radius, innerRotation));

						if (damageTimer >= damageTime)
						{
							currentEnemy.Kill();
						}
					}
					else if (outerRadius.OverlapPoint(currentEnemy.transform.position))
					{
						currentEnemy.Move(Vector3.Lerp(currentEnemy.velocity, currentEnemy.transform.position.CalculateBlackHoleForce(outerForce, transform.position, outerRadius.radius, outerRotation), 0.1f));

						if (damageTimer >= damageTime)
						{
							ExplodeEffect.ExplodePartial(currentEnemy.transform, currentEnemy.velocity * 2f, currentEnemy.Sprite, 0.1f);
							currentEnemy.TakeDamage(gameObject);
						}
					}
				}
			}

			allEnemies.RemoveAll(e => e == null);

			if (damageTimer >= damageTime)
			{
				damageTimer = 0f;
			}
		}
	}

	private IEnumerator DestroyEmitter()
	{
		yield return new WaitForSeconds(lifetime);

		ExplodeEffect.Explode(transform, Vector3.zero, spriteRenderer.sprite);
		particleSystemInstance.enableEmission = false;
		Destroy(particleSystemInstance.gameObject, particleDestroyDelay);
		Destroy(gameObject);
	}
}
