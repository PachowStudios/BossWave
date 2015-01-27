using UnityEngine;
using System.Collections;

public class Microchip : Powerup
{
	public enum Size
	{
		Small,
		Medium,
		Large
	};

	public int minValue = 1;
	public int maxValue = 10;
	private int microchipValue = 0;

	new void Awake()
	{
		base.Awake();

		microchipValue = Random.Range(minValue, maxValue);
	}

	protected override void Pickup()
	{
		PlayerControl.instance.AddMicrochips(microchipValue);
		PopupMessage.CreatePopup(PlayerControl.instance.PopupMessagePoint, microchipValue.ToString(), spriteRenderer.sprite);

		base.Pickup();
	}
}
