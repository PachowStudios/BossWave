using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class ScaleWidthCamera : MonoBehaviour 
{
	public static ScaleWidthCamera instance;

	public bool overrideSettings = false;
	public int overrideFOV;
	public bool useWorldSpaceUI = false;
	public RectTransform worldSpaceUI;

	public static int FOV = 400;

	void Awake()
	{
		instance = this;
	}

	void OnPreRender()
	{
		if (overrideSettings)
		{
			FOV = overrideFOV;
		}

		camera.orthographicSize = FOV / 20f / camera.aspect;

		if (useWorldSpaceUI && worldSpaceUI != null)
		{
			worldSpaceUI.sizeDelta = new Vector2(FOV / 10f, FOV / 10f / camera.aspect);
		}
	}

	public static void AnimateFOV(int newFOV, float time)
	{
		instance.overrideSettings = true;

		iTween.ValueTo(instance.gameObject, iTween.Hash("from", FOV,
														"to", newFOV,
														"time", time,
														"easetype", iTween.EaseType.easeOutQuint,
														"onupdate", "UpdateFOV"));
	}

	private void UpdateFOV(float newValue)
	{
		overrideFOV = (int)newValue;
	}
}
