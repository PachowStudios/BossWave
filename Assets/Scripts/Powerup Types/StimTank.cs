using UnityEngine;
using System.Collections;

public class StimTank : Powerup
{
	public float speedMultiplier = 1.5f;
	public float duration = 5f;
	public Sprite popupImage;

	new void Awake()
	{
		base.Awake();
	}

	protected override void Pickup()
	{
		PlayerControl.instance.SpeedBoost(speedMultiplier, duration);
		PopupMessage.CreatePopup(PlayerControl.instance.PopupMessagePoint, ((int)duration).ToString(), popupImage);

		base.Pickup();
	}

	
}
