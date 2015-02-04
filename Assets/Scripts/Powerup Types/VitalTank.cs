using UnityEngine;
using System.Collections;

public class VitalTank : Powerup
{
	public Sprite popupSprite;
	public float minHealth = 5f;
	public float maxHealth = 100f;
	public AnimationCurve healthCurve;

	private float healthAmount;

	new void Awake()
	{
		base.Awake();

		healthAmount = Extensions.ConvertRange(healthCurve.Evaluate(Random.value),
											   0f, 1f, minHealth, maxHealth);
	}

	protected override void Pickup()
	{
		PlayerControl.Instance.Health += healthAmount;
		PopupMessage.Instance.CreatePopup(PlayerControl.Instance.PopupMessagePoint, ((int)healthAmount).ToString(), popupSprite);

		base.Pickup();
	}
}
