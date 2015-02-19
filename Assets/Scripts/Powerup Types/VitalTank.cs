using UnityEngine;
using System.Collections;

public class VitalTank : Powerup
{
	#region Fields
	public Sprite popupSprite;
	public float minHealth = 5f;
	public float maxHealth = 100f;
	public AnimationCurve healthCurve;
	#endregion

	#region Internal Helper Methods
	protected override void Pickup()
	{
		float healthAmount = Extensions.ConvertRange(healthCurve.Evaluate(Random.value), 
													 0f, 1f, minHealth, maxHealth);

		PlayerControl.Instance.Health += healthAmount;
		PopupMessage.Instance.CreatePopup(PlayerControl.Instance.PopupMessagePoint, ((int)healthAmount).ToString(), popupSprite);

		base.Pickup();
	}
	#endregion
}
