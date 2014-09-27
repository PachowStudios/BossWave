using UnityEngine;
using System.Collections;

public abstract class Powerup : MonoBehaviour 
{
	public bool autoDestroy = true;
	public float minLifetime = 10f;
	public float maxLifetime = 15f;

	protected PlayerControl player;

	private SpriteRenderer spriteRenderer;
	private ExplodeEffect explodeEffect;

	protected virtual void Awake()
	{
		player = GameObject.Find("Player").GetComponent<PlayerControl>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		explodeEffect = GetComponent<ExplodeEffect>();

		if (autoDestroy)
		{
			Invoke("Pickup", Random.Range(minLifetime, maxLifetime));
		}
	}

	protected virtual void OnTriggerEnter2D(Collider2D trigger)
	{
		if (trigger.tag == "Player")
		{
			Pickup();
		}
	}

	protected virtual void Pickup()
	{
		explodeEffect.Explode(Vector3.zero, spriteRenderer.sprite);
		Destroy(gameObject);
	}
}
