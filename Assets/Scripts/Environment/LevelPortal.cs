using UnityEngine;
using System.Collections;

public class LevelPortal : MonoBehaviour 
{
	public string LevelName;
	public GameMenu gameMenu;

	private bool portalSelected = false;

	void Update()
	{
		#if MOBILE_INPUT
		if (CrossPlatformInputManager.GetAxis("Vertical") > 0.6f)
		#else
		if (CrossPlatformInputManager.GetButton("Jump"))
		#endif
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
