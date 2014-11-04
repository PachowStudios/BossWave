using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class ScaleWidthCamera : MonoBehaviour 
{
	public float targetWidth = 1920;

	public static float FOV = 42f;

	void OnPreRender()
	{
		float height = (targetWidth / Screen.width) * Screen.height;

		camera.orthographicSize = (height / FOV) / 2f;
	}
}
