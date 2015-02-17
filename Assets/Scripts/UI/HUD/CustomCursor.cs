using UnityEngine;
using System.Collections;

public class CustomCursor : MonoBehaviour
{
	private static CustomCursor instance;

	public Texture2D texture;
	public Vector2 hotpoint;
	public CursorMode mode;

	public static CustomCursor Instance
	{
		get { return instance; }
	}

	private void Awake()
	{
		instance = this;

		Cursor.SetCursor(texture, hotpoint, mode);
	}
}
