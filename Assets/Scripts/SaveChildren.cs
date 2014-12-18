using UnityEngine;
using System.Collections;

public class SaveChildren : MonoBehaviour 
{
	void OnDestroy()
	{
		transform.DetachChildren();
	}
}
