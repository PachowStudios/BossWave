using UnityEngine;
using System.Collections;

public class PopupSwapGunInstance : MonoBehaviour 
{
	void FixedUpdate()
	{
		transform.position = PlayerControl.instance.popupMessagePoint.position;
	}
}
