using UnityEngine;
using System.Collections;

public class CustomCursor : MonoBehaviour
{
	#region Fields
	private static CustomCursor instance;

	public Texture2D texture;
	public Vector2 hotpoint;
	public CursorMode mode;
	#endregion

	#region Public Properties
	public static CustomCursor Instance
	{
		get { return instance; }
	}
	#endregion

	#region MonoBehaviour
	private void Awake()
	{
		instance = this;

		Cursor.SetCursor(texture, hotpoint, mode);
	}
	#endregion
}
