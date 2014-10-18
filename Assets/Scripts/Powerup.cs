using UnityEngine;
using System.Collections;

public abstract class Powerup : MonoBehaviour 
{
	public bool autoDestroy = true;
	public float minLifetime = 10f;
	public float maxLifetime = 15f;

	protected PlayerControl player;
	protected PopupMessage popupMessage;

	private SpriteRenderer spriteRenderer;

	protected virtual void Awake()
	{
		player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();
		popupMessage = GameObject.Find("Popup Message").GetComponent<PopupMessage>();

		spriteRenderer = GetComponent<SpriteRenderer>();

		if (autoDestroy)
		{
			Invoke("AutoDestroy", Random.Range(minLifetime, maxLifetime));
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
		ExplodeEffect.Explode(transform, Vector3.zero, spriteRenderer.sprite);
		Destroy(gameObject);
	}

	private void AutoDestroy()
	{
		ExplodeEffect.Explode(transform, Vector3.zero, spriteRenderer.sprite);
		Destroy(gameObject);
	}
}
