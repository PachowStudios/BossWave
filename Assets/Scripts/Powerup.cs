﻿using UnityEngine;
using System.Collections;

public abstract class Powerup : MonoBehaviour 
{
	public bool autoDestroy = true;
	public float minLifetime = 10f;
	public float maxLifetime = 15f;

	protected PlayerControl player;
	protected PopupMessage popupMessage;

	private ExplodeEffect explodeEffect;
	private SpriteRenderer spriteRenderer;

	protected virtual void Awake()
	{
		player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();
		popupMessage = GameObject.Find("Popup Message").GetComponent<PopupMessage>();

		explodeEffect = GetComponent<ExplodeEffect>();
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
		explodeEffect.Explode(Vector3.zero, spriteRenderer.sprite);
		Destroy(gameObject);
	}

	private void AutoDestroy()
	{
		explodeEffect.Explode(Vector3.zero, spriteRenderer.sprite);
		Destroy(gameObject);
	}
}
