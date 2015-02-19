using UnityEngine;
using System.Collections;

public class Destroy : MonoBehaviour
{
	#region Public Methods
	public void DoDestroy()
	{
		Destroy(this.gameObject);
	}
	#endregion
}
