using UnityEngine;
using System.Collections;

public class LevelPortal : MonoBehaviour
{
	#region Fields
	public string LevelName;
	public GameMenu gameMenu;

	private bool portalSelected = false;
	#endregion

	#region MonoBehaviour
	private void Update()
	{
		if (PlayerControl.Instance.Jumped)
		{
			if (portalSelected)
			{
				gameMenu.LoadLevel(LevelName);
			}
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.tag == "Player")
		{
			portalSelected = true;
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (other.tag == "Player")
		{
			portalSelected = false;
		}
	}
	#endregion
}
