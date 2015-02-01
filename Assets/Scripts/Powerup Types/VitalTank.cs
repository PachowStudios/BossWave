using UnityEngine;
using System.Collections;

public class VitalTank : Powerup
{
	public Sprite popupSprite;
	public float minHealth = 5f;
	public float maxHealth = 100f;

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
		PlayerControl.Instance.Health += healthAmount;
		PopupMessage.Instance.CreatePopup(PlayerControl.Instance.PopupMessagePoint, ((int)healthAmount).ToString(), popupSprite);

		base.Pickup();
	}
}
