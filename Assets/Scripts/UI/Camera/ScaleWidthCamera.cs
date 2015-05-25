using UnityEngine;
using System.Collections;
using DG.Tweening;

[ExecuteInEditMode]
public class ScaleWidthCamera : MonoBehaviour
{
	#region Fields
	private static ScaleWidthCamera main;

	public bool isMain = false;
	public bool syncWithMain = false;
	public int defaultFOV = 500;
	public bool useWorldSpaceUI = false;
	public RectTransform worldSpaceUI;

	public int FOV;
	#endregion

	#region Public Properties
	public static ScaleWidthCamera Main
	{ get { return main; } }
	#endregion

	#region MonoBehaviour
	private void OnEnable()
	{
		if (isMain)
			main = this;

		FOV = defaultFOV;
	}

	private void Update()
	{
		if (syncWithMain && !isMain && Main != null)
		{
			defaultFOV = Main.defaultFOV;
			FOV = Main.FOV;
			transform.position = Main.transform.position;
		}

		camera.orthographicSize = FOV / 20f / camera.aspect;

		if (useWorldSpaceUI && worldSpaceUI != null)
			worldSpaceUI.sizeDelta = new Vector2(FOV / 10f, FOV / 10f / camera.aspect);
	}
	#endregion

	#region Public Methods
	public void AnimateFOV(int newFOV, float time)
	{
		DOTween.To(() => FOV, x => FOV = x, newFOV, time)
			.SetEase(Ease.OutQuint);
	}
	#endregion
}
