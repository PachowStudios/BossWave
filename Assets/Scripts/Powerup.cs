using UnityEngine;
using System.Collections;

public abstract class Powerup : MonoBehaviour 
{
	protected PlayerControl player;

	private SpriteRenderer spriteRenderer;
	private ExplodeEffect explodeEffect;

	protected virtual void Awake()
	{
		player = GameObject.Find("Player").GetComponent<PlayerControl>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		explodeEffect = GetComponent<ExplodeEffect>();
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
		spriteRenderer.enabled = false;
		Vector2 colliderSize = new Vector2(spriteRenderer.bounds.size.x,
										   spriteRenderer.bounds.size.y);

		collider2D.enabled = false;
		explodeEffect.Explode(Vector3.zero, colliderSize);
		Destroy(gameObject);
	}
}
