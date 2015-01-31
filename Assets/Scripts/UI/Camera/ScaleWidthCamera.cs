using UnityEngine;
using System.Collections;
using DG.Tweening;

[ExecuteInEditMode]
public class ScaleWidthCamera : MonoBehaviour 
{
	private static ScaleWidthCamera instance;

	public bool overrideSettings = false;
	public int overrideFOV;
	public bool useWorldSpaceUI = false;
	public RectTransform worldSpaceUI;

	public int FOV = 400;

	public static ScaleWidthCamera Instance
	{
		get { return instance; }
	}

	private void OnEnable()
	{
		instance = this;
	}

	private void OnPreRender()
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

	public void AnimateFOV(int newFOV, float time)
	{
		Instance.overrideSettings = true;
		Instance.overrideFOV = FOV;
		DOTween.To(() => Instance.overrideFOV, x => Instance.overrideFOV = x, newFOV, time)
			.SetEase(Ease.OutQuint);
	}
}
