using UnityEngine;
using System.Collections;

public class Microchip : Powerup
{
	#region Fields
	public enum Size
	{
		Small,
		Medium,
		Large
	};

	public Sprite popupSprite;
	public int minValue = 1;
	public int maxValue = 10;
	#endregion

	#region Internal Helper Methods
	protected override void Pickup()
	{
		int microchipValue = Random.Range(minValue, maxValue);

		PlayerControl.Instance.AddMicrochips(microchipValue);
		PopupMessage.Instance.CreatePopup(PlayerControl.Instance.PopupMessagePoint, microchipValue.ToString(), popupSprite);

		base.Pickup();
	}
	#endregion
}
