using UnityEngine;
using System.Collections;

public class SaveChildren : MonoBehaviour 
{
	private void OnDestroy()
	{
		transform.DetachChildren();
	}
}
