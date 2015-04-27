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
		if (portalSelected && PlayerControl.Instance.Jumped)
				gameMenu.LoadLevel(LevelName);
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.tag == "Player")
			portalSelected = true;
	}

	private void OnTriggerStay2D(Collider2D other)
	{
		OnTriggerEnter2D(other);
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (other.tag == "Player")
			portalSelected = false;
	}
	#endregion
}
