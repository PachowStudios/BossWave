using UnityEngine;
using System.Collections;
using DG.Tweening;

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
		instance.overrideFOV = FOV;
		DOTween.To(() => instance.overrideFOV, x => instance.overrideFOV = x, newFOV, time)
			.SetEase(Ease.OutQuint);
	}
}
