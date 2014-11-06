using UnityEngine;
using System.Collections;

public class Microchip : Powerup
{
	public int minValue = 1;
	public int maxValue = 10;
	private int microchipValue = 0;
	public Vector2 minScatter = new Vector2(5f, 10f);
	public Vector2 maxScatter = new Vector2(5f, 10f);
	private Vector2 scatterSpeed;
	public float minShake = 10f;
	public float maxShake = 100f;
	private float shakeAmount = 0f;
	public float shakeTime = 1f;
	public float magnetForce = 50f;
	public float magnetUplift = 10f;
	public float lookDamping = 3f;

	private BoxCollider2D pickupCollider;
	private CircleCollider2D magnetCollider;

	new void Awake()
	{
		base.Awake();

		pickupCollider = GetComponent<BoxCollider2D>();
		magnetCollider = GetComponent<CircleCollider2D>();

		microchipValue = Random.Range(minValue, maxValue);
		scatterSpeed = new Vector2(Random.Range(minScatter.x, maxScatter.x),
								   Random.Range(minScatter.y, maxScatter.y));
		shakeAmount = Extensions.ConvertRange(scatterSpeed.x, minScatter.x, maxScatter.x, minShake, maxShake);
		scatterSpeed.x *= Extensions.RandomSign();
		shakeAmount *= Mathf.Sign(scatterSpeed.x);
	}

	protected override void OnTriggerEnter2D(Collider2D trigger)
	{
		if (pickupCollider.bounds.Intersects(trigger.bounds))
		{
			base.OnTriggerEnter2D(trigger);
		}
	}

	void OnTriggerStay2D(Collider2D trigger)
	{
		if (trigger.tag == "Player")
		{
			rigidbody2D.AddExplosionForce(-magnetForce, PlayerControl.instance.transform.position, magnetCollider.radius * 2, magnetUplift);
			gameObject.RotateUpdate(new Vector3(0, 0, transform.LookAt2D(PlayerControl.instance.transform.position) - 90f), lookDamping);
		}

		OnTriggerEnter2D(trigger);
	}

	void OnTriggerExit2D(Collider2D trigger)
	{
		if (trigger.tag == "Player")
		{
			gameObject.RotateTo(new Vector3(0, 0, 0), lookDamping / 2f, 0f);
		}
	}

	protected override void Pickup()
	{
		PlayerControl.instance.AddMicrochips(microchipValue);
		PopupMessage.CreatePopup(PlayerControl.instance.popupMessagePoint.position, microchipValue.ToString(), spriteRenderer.sprite);

		base.Pickup();
	}

	public void Scatter()
	{
		rigidbody2D.AddForce(new Vector2(scatterSpeed.x, scatterSpeed.y),
							 ForceMode2D.Impulse);
		gameObject.PunchRotation(new Vector3(0f, 0f, shakeAmount), shakeTime, 0f);
	}
}
