using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpriteExplosion : MonoBehaviour 
{
	public float pixelsPerUnit = 10f;
	public float lifetime = 4f;
	public string sortingLayer = "Foreground";
	public int sortingOrder = 1;

	LevelManager levelManager;

	void Awake()
	{
		levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager>();
	}

	public void Explode(Vector3 velocity, Sprite sprite)
	{
		StartCoroutine(DoExplode(velocity, sprite));
	}

	private IEnumerator DoExplode(Vector3 velocity, Sprite sprite)
	{
		ParticleSystem partSystem = GetComponent<ParticleSystem>();
		List<ParticleSystem.Particle> particles = new List<ParticleSystem.Particle>();
		ParticleSystem.Particle currentParticle = new ParticleSystem.Particle();
		partSystem.renderer.sortingLayerName = sortingLayer;
		partSystem.renderer.sortingOrder = sortingOrder;
		currentParticle.size = 1f / pixelsPerUnit;
		Vector3 randomTranslate = new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));

		for (int i = 0; i < sprite.bounds.size.x * 10f; i++)
		{
			for (int j = 0; j < sprite.bounds.size.y * 10f; j++)
			{
				Vector2 positionOffset = new Vector2(sprite.bounds.extents.x - sprite.bounds.center.x - 0.05f,
													 sprite.bounds.extents.y - sprite.bounds.center.y - 0.05f);

				Vector3 particlePosition = transform.TransformPoint((i / 10f) - positionOffset.x,
																	(j / 10f) - positionOffset.y, 0);

				Color particleColor = sprite.texture.GetPixel((int)sprite.rect.x + i,
															  (int)sprite.rect.y + j);

				if (particleColor.a != 0f)
				{
					currentParticle.position = particlePosition;
					currentParticle.rotation = 0f;
					currentParticle.color = particleColor;
					currentParticle.startLifetime = currentParticle.lifetime = lifetime;
					currentParticle.velocity = new Vector2((levelManager.bossWavePlayerMoved ? -levelManager.bossWave.cameraMoveSpeed : velocity.x) + Random.Range(-1f, 1f),
														   velocity.y + Random.Range(-1f, 1f));
					currentParticle.velocity += randomTranslate;
					particles.Add(currentParticle);
				}
			}
		}

		partSystem.SetParticles(particles.ToArray(), particles.ToArray().Length);
		Destroy(gameObject, lifetime);
		yield return new WaitForSeconds(1f);
	}
}
