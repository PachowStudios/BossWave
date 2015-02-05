using UnityEngine;
using System.Collections;
using DG.Tweening;

public class CameraShake : MonoBehaviour 
{
	private static CameraShake instance;

	public static CameraShake Instance
	{
		get { return instance; }
	}

	private void Awake()
	{
		instance = this;
	}

	public void Shake(float duration, Vector3 strength)
	{
		transform.DOShakePosition(duration, strength);
	}
}
