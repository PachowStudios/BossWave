using UnityEngine;
using System.Collections;

public class VitalTank : Powerup
{
	public float minHealth = 5f;
	public float maxHealth = 100f;
	public Sprite popupImage;

	private float healthAmount;

	new void Awake()
	{
		base.Awake();

		healthAmount = Random.Range(minHealth, maxHealth) * Random.Range(1f, 10f);
		healthAmount /= 10;
		healthAmount = Mathf.RoundToInt(Mathf.Clamp(healthAmount, minHealth, maxHealth));
	}

	protected override void Pickup()
	{
		PlayerControl.instance.AddHealth(healthAmount);
		PopupMessage.CreatePopup(PlayerControl.instance.popupMessagePoint.position, ((int)healthAmount).ToString(), popupImage);

		base.Pickup();
	}
}
