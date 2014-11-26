using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BlackHole : Projectile
{
	public float outerDamage = 0.5f;
	public float damageRate = 5f;
	public float outerForce = 20f;
	public float innerForce = 40f;
	public float outerRotation = 0.5f;
	public float innerRotation = 1f;
	[Range(0.1f, 1f)]
	public float activationBuffer = 80f;
	public ParticleSystem particleSystemPrefab;
	public string particlesSortingLayer = "Foreground";
	public int particlesSortingOrder = 1;
	public float lifetimeEmissionBuffer = 1f;

	private bool activated = false;
	private float cooldownTime;
	private float cooldownTimer;
	private float secondsActive;
	private ParticleSystem particleSystemInstance;
	private List<ParticleSystem> allParticleSystems;

	private CircleCollider2D outerRadius;
	private CircleCollider2D innerRadius;
	private BoxCollider2D hitBox;

	new void Awake()
	{
		controller = GetComponent<CharacterController2D>();
		spriteRenderer = GetComponent<SpriteRenderer>();

		outerRadius = transform.FindChild("outerRadius").GetComponent<CircleCollider2D>();
		innerRadius = transform.FindChild("innerRadius").GetComponent<CircleCollider2D>();
		hitBox = GetComponent<BoxCollider2D>();

		cooldownTime = 1f / damageRate;
		cooldownTimer = cooldownTime;
	}

	void FixedUpdate()
	{
		InitialUpdate();

		if (!activated)
		{
			ApplyMovement();
		}
		else
		{
			particleSystemInstance.transform.position = transform.position;

			SimulateBlackHole();
		}
	}

	new void OnTriggerEnter2D(Collider2D trigger)
	{
		if (trigger.gameObject.layer == LayerMask.NameToLayer("Collider") && hitBox.bounds.Intersects(trigger.bounds))
		{
			CheckDestroy();
		}
		else if (trigger.gameObject.layer == LayerMask.NameToLayer("Enemies") && outerRadius.bounds.Intersects(trigger.bounds))
		{
			if (trigger.bounds.center.DistanceFrom(transform.position) <= outerRadius.radius * activationBuffer)
			{
				Activate();
			}
		}
	}

	new void OnTriggerStay2D(Collider2D trigger)
	{
		OnTriggerEnter2D(trigger);
	}

	private void Activate()
	{
		if (!activated)
		{
			activated = true;

			particleSystemInstance = Instantiate(particleSystemPrefab, transform.position, Quaternion.identity) as ParticleSystem;
			particleSystemInstance.renderer.sortingLayerName = particlesSortingLayer;
			particleSystemInstance.renderer.sortingOrder = particlesSortingOrder;
			StartCoroutine(StopParticleEmission());

			secondsActive = 0f;

			if (autoDestroy)
			{
				StartCoroutine(DestroyEmitter());
			}
		}
	}

	private void SimulateBlackHole()
	{
		secondsActive += Time.deltaTime;
		particleSystemInstance.startLifetime = lifetime - secondsActive + 0.01f;

		allParticleSystems = GameObject.FindObjectsOfType<ParticleSystem>().ToList<ParticleSystem>();

		foreach (ParticleSystem particleSystem in allParticleSystems)
		{
			ParticleSystem.Particle[] currentParticles = new ParticleSystem.Particle[particleSystem.particleCount];
			particleSystem.GetParticles(currentParticles);

			for (int i = 0; i < currentParticles.Length; i++)
			{
				if (innerRadius.bounds.Contains(currentParticles[i].position))
				{
					currentParticles[i].velocity = currentParticles[i].position.CalculateBlackHoleForce(innerForce, transform.position, outerRadius.radius, innerRotation);
				}
				else if (outerRadius.bounds.Contains(currentParticles[i].position))
				{
					currentParticles[i].velocity = currentParticles[i].position.CalculateBlackHoleForce(outerForce, transform.position, outerRadius.radius + 0.5f, outerRotation);
				}
			}

			particleSystem.SetParticles(currentParticles, currentParticles.Length);
		}
	}

	private IEnumerator StopParticleEmission()
	{
		yield return new WaitForSeconds(lifetime - lifetimeEmissionBuffer);

		particleSystemInstance.enableEmission = false;
	}

	private IEnumerator DestroyEmitter()
	{
		yield return new WaitForSeconds(lifetime);

		ExplodeEffect.Explode(transform, Vector3.zero, spriteRenderer.sprite);
		Destroy(gameObject);
	}
}
