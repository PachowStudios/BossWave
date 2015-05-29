using UnityEngine;
using System.Collections;

public class SaveChildren : MonoBehaviour
{
	#region MonoBehaviour
	private void OnDisable()
	{
		transform.DetachChildren();
	}
	#endregion
}
