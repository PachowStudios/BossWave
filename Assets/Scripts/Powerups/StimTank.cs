using UnityEngine;
using System.Collections;

public class StimTank : Powerup
{
	#region Fields
	public Sprite popupSprite;
	public float speedMultiplier = 1.5f;
	public float duration = 5f;
	#endregion

	#region Internal Helper Methods
	protected override void Pickup()
	{
		PlayerControl.Instance.SpeedBoost(speedMultiplier, duration);
		PopupMessage.Instance.CreatePopup(PlayerControl.Instance.PopupMessagePoint, ((int)duration).ToString(), popupSprite);

		base.Pickup();
	}
	#endregion
}
