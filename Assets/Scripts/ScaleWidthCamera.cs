using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class ScaleWidthCamera : MonoBehaviour 
{
	public int targetWidth = 1920;
	public float pixelsToUnits = 10f;

	void Update()
	{
		int height = Mathf.RoundToInt(targetWidth / (float)Screen.width * Screen.height);

		camera.orthographicSize = height / pixelsToUnits / 2f;
	}
}
