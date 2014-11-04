using UnityEngine;
using System.Collections;

public class SpeedBoost : Powerup
{
	public float speedMultiplier = 1.5f;
	public float duration = 5f;
	public Sprite popupImage;

	new void Awake()
	{
		base.Awake();
	}

	new void OnTriggerEnter2D(Collider2D trigger)
	{
		base.OnTriggerEnter2D(trigger);
	}

	protected override void Pickup()
	{
		player.speedMultiplier = speedMultiplier;
		player.ResetSpeed(duration);

		PopupMessage.CreatePopup(player.popupMessagePoint.position, popupImage, ((int)duration).ToString());

		base.Pickup();
	}

	
}
