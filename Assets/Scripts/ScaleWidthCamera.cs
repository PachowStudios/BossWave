using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class ScaleWidthCamera : MonoBehaviour 
{
	public int editorFOV;
	public bool overrideSettings = false;
	public bool useWorldSpaceUI = false;
	public RectTransform worldSpaceUI;

	public static int FOV = 400;

	void OnPreRender()
	{
		if (overrideSettings || (!Application.isPlaying && Application.isEditor))
		{
			FOV = editorFOV;
		}

		camera.orthographicSize = FOV / 20f / camera.aspect;

		if (useWorldSpaceUI && worldSpaceUI != null)
		{
			worldSpaceUI.sizeDelta = new Vector2(FOV / 10f, FOV / 10f / camera.aspect);
		}
	}
}
