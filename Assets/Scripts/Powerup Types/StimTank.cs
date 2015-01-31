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
		PlayerControl.Instance.SpeedBoost(speedMultiplier, duration);
		PopupMessage.Instance.CreatePopup(PlayerControl.Instance.PopupMessagePoint, ((int)duration).ToString(), popupImage);

		base.Pickup();
	}

	
}
