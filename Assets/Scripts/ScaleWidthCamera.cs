using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class ScaleWidthCamera : MonoBehaviour 
{
	public float targetWidth = 1920;
	public float FOV = 42f;

	void OnGUI()
	{
		float height = (targetWidth / Screen.width) * Screen.height;

		camera.orthographicSize = (height / FOV) / 2f;
	}
}
