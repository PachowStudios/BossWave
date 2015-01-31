using UnityEngine;
using System.Collections;

public class LevelPortal : MonoBehaviour 
{
	public string LevelName;
	public GameMenu gameMenu;

	private bool portalSelected = false;

	void Update()
	{
		if (PlayerControl.Instance.Jumped)
		{
			if (portalSelected)
			{
				gameMenu.LoadLevel(LevelName);
			}
		}
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.tag == "Player")
		{
			portalSelected = true;
		}
	}

	void OnTriggerExit2D(Collider2D other)
	{
		if (other.tag == "Player")
		{
			portalSelected = false;
		}
	}
}
