using UnityEngine;
using System.Collections;

public class SaveChildren : MonoBehaviour
{
	#region MonoBehaviour
	private void OnDestroy()
	{
		transform.DetachChildren();
	}
	#endregion
}
