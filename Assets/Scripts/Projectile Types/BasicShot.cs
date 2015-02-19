using UnityEngine;
using System.Collections;

public class BasicShot : Projectile
{
	#region MonoBehaviour
	private void LateUpdate()
	{
		DoMovement();
	}
	#endregion
}
