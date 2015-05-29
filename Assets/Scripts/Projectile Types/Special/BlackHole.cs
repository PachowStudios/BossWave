using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public class BlackHole : Projectile
{
	#region Fields
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
	private ParticleSystem[] targetParticleSystems;
	private List<Enemy> targetEnemies = new List<Enemy>();
	private List<Projectile> targetProjectiles = new List<Projectile>();
	private Powerup[] targetPowerups;

	private CircleCollider2D outerRadius;
	private CircleCollider2D innerRadius;
	#endregion

	#region MonoBehaviour
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
			StartCoroutine(FailsafeDestroy());
	}

	private void Update()
	{
		GetInput();

		if (activated)
		{
			shotSpeed = Mathf.Lerp(shotSpeed, 0f, 0.1f);
			anim.SetBool("Active", true);

			if (!spawned && shotSpeed <= activationSpeed)
				Spawn();

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
					damageTimer = 0f;
			}
		}
	}

	private void LateUpdate()
	{
		DoMovement();
	}
	#endregion

	#region Internal Update Methods
	private void GetInput()
	{
		if (!activated && CrossPlatformInputManager.GetButtonDown("SecondaryShoot"))
			activated = true;
	}

	private void SimulateParticles()
	{
		targetParticleSystems = GameObject.FindObjectsOfType<ParticleSystem>();

		if (targetParticleSystems.Length == 0)
			return;

		foreach (var particleSystem in targetParticleSystems)
		{
			var currentParticles = new ParticleSystem.Particle[particleSystem.particleCount];
			particleSystem.GetParticles(currentParticles);

			for (int i = 0; i < currentParticles.Length; i++)
			{
				if (innerRadius.OverlapPoint(currentParticles[i].position))
					currentParticles[i].velocity = currentParticles[i].position.CalculateBlackHoleForce(innerForce, 
																										transform.position, 
																										outerRadius.radius, 
																										innerRotation);
				else if (outerRadius.OverlapPoint(currentParticles[i].position))
					currentParticles[i].velocity = Vector3.Lerp(currentParticles[i].velocity, 
																currentParticles[i].position.CalculateBlackHoleForce(outerForce, 
																													 transform.position, 
																													 outerRadius.radius, 
																													 outerRotation, 
																													 1.5f), 
																0.1f);
			}

			particleSystem.SetParticles(currentParticles, currentParticles.Length);
		}
	}

	private void SimulateEnemies()
	{
		if (targetEnemies.Count == 0)
			return;

		for (int i = 0; i < targetEnemies.Count; i++)
		{
			if (targetEnemies[i] == null || targetEnemies[i].ignoreProjectiles)
				continue;

			if (innerRadius.OverlapPoint(targetEnemies[i].collider2D.bounds.center))
				targetEnemies[i].Kill();
			else if (outerRadius.OverlapPoint(targetEnemies[i].collider2D.bounds.center))
			{
				if (!targetEnemies[i].immuneToInstantKill)
				{
					targetEnemies[i].Move(Vector3.Lerp(targetEnemies[i].Velocity,
													   targetEnemies[i].transform.position.CalculateBlackHoleForce(outerForce, 
																												   transform.position, 
																												   outerRadius.radius, 
																												   outerRotation), 
													   30f * Time.deltaTime));
				}

				if (damageTimer >= damageTime)
				{
					ExplodeEffect.Instance.ExplodePartial(targetEnemies[i].transform, 
														  targetEnemies[i].Velocity, 
														  targetEnemies[i].Sprite, 
														  0.05f);
					targetEnemies[i].TakeDamage(this);
				}
			}
		}

		targetEnemies.RemoveAll(e => e == null);
	}

	private void SimulateProjectiles()
	{
		if (targetProjectiles.Count == 0)
			return;

		for (int i = 0; i < targetProjectiles.Count; i++)
		{
			if (targetProjectiles[i] == null)
				continue;

			if (innerRadius.OverlapPoint(targetProjectiles[i].collider2D.bounds.center))
			{
				targetProjectiles[i].destroyShake = false;
				targetProjectiles[i].DoDestroy();
			}
			else if (outerRadius.OverlapPoint(targetProjectiles[i].collider2D.bounds.center))
			{
				var force = targetProjectiles[i].transform.position.CalculateBlackHoleForce(outerForce, 
																							transform.position, 
																							outerRadius.radius, 
																							outerRotation * 3f);
				targetProjectiles[i].direction = Vector3.Lerp(targetProjectiles[i].direction, 
															  force.normalized, 
															  3f * Time.deltaTime);
				targetProjectiles[i].Move(force);
			}
		}

		targetProjectiles.RemoveAll(p => p == null);
	}

	private void SimulatePowerups()
	{
		targetPowerups = GameObject.FindObjectsOfType<Powerup>();

		if (targetPowerups.Length == 0)
			return;

		foreach (var currentPowerup in targetPowerups)
		{
			if (currentPowerup == null)
				continue;

			if (innerRadius.OverlapPoint(currentPowerup.collider2D.bounds.center))
				currentPowerup.Destroy();
			else if (outerRadius.OverlapPoint(currentPowerup.collider2D.bounds.center))
				currentPowerup.rigidbody2D.velocity = Vector3.Lerp(currentPowerup.rigidbody2D.velocity, 
																	currentPowerup.transform.position.CalculateBlackHoleForce(outerForce, 
																															  transform.position, 
																															  outerRadius.radius, 
																															  outerRotation), 
																	30f * Time.deltaTime);
		}
	}

	private void SimulatePlayer()
	{
		if (innerRadius.OverlapPoint(PlayerControl.Instance.collider2D.bounds.center))
			PlayerControl.Instance.Health = 0f;
		else if (outerRadius.OverlapPoint(PlayerControl.Instance.collider2D.bounds.center))
		{
			PlayerControl.Instance.Move(Vector3.Lerp(PlayerControl.Instance.Velocity, 
													 PlayerControl.Instance.transform.position.CalculateBlackHoleForce(outerForce, 
																													   transform.position, 
																													   outerRadius.radius, 
																													   outerRotation), 
													 5f * Time.deltaTime));

			if (damageTimer >= damageTime)
			{
				var playerSprites = PlayerControl.Instance.SpriteRenderers;

				for (int i = 0; i < playerSprites.Count; i++)
				{
					if (playerSprites[i].color == Color.clear)
						continue;

					Transform tempTransform = new GameObject().transform;
					playerSprites[i].transform.CopyTo(tempTransform);
					tempTransform.localScale = PlayerControl.Instance.transform.localScale;
					ExplodeEffect.Instance.ExplodePartial(tempTransform.transform,
														  PlayerControl.Instance.Velocity,
														  playerSprites[i].sprite,
														  0.1f / playerSprites.Count);
					Destroy(tempTransform.gameObject);
				}

				PlayerControl.Instance.Health -= damage;
			}
		}
	}
	#endregion

	#region Internal Helper Methods
	protected override void HandleTrigger(Collider2D other)
	{
		base.HandleTrigger(other);

		if (other.gameObject.layer == LayerMask.NameToLayer("Enemies") && 
			outerRadius.OverlapPoint(other.bounds.center))
		{
			var currentEnemy = other.gameObject.GetComponent<Enemy>();

			if (currentEnemy != null && !currentEnemy.ignoreProjectiles)
			{
				if (!activated && other.bounds.center.DistanceFrom(transform.position) <= outerRadius.radius * activationBuffer)
					activated = true;

				if (activated && !targetEnemies.Contains(currentEnemy))
					targetEnemies.Add(currentEnemy);
			}
		}
		else if (other.gameObject.layer == LayerMask.NameToLayer("Projectiles") && 
				 outerRadius.OverlapPoint(other.bounds.center))
		{
			if (activated)
			{
				var currentProjectile = other.gameObject.GetComponent<BasicProjectile>();

				if (currentProjectile != null && !targetProjectiles.Contains(currentProjectile))
				{
					currentProjectile.useModifiers = false;
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
			StartCoroutine(DestroyEmitter());
	}

	private IEnumerator DestroyEmitter()
	{
		yield return new WaitForSeconds(lifetime);

		CameraShake.Instance.Shake(0.5f, new Vector3(0f, 2f, 0f));
		ExplodeEffect.Instance.Explode(transform, Vector3.zero, Sprite);
		particleSystemInstance.enableEmission = false;
		Destroy(particleSystemInstance.gameObject, particleDestroyDelay);
		Destroy(gameObject);
	}
	#endregion

	#region Public Methods
	public override void DoDestroy()
	{
		if (!activated)
			base.DoDestroy();
	}
	#endregion
}
