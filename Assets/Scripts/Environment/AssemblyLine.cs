using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class AssemblyLine : MonoBehaviour
{
	#region Fields
	public GameObject prefab;
	public string pathName;                                                    
	public float speed = 10f;
	public float spawnDelay = 1f;

	private float spawnTimer;
	private Vector3[] path;

	private static bool stop = false;
	#endregion

	#region MonoBehaviour
	private void Awake()
	{
		spawnTimer = spawnDelay;
	}

	private void Start()
	{
		path = VectorPath.GetPath(pathName);
	}

	private void Update()
	{
		if (!stop)
		{
			spawnTimer += Time.deltaTime;

			if (spawnTimer >= spawnDelay)
			{
				Spawn();
				spawnTimer = 0f;
			}
		}
	}
	#endregion

	#region Internal Helper Methods
	private Tween Spawn()
	{
		GameObject currentObject = Instantiate(prefab, path[0] + transform.position, Quaternion.identity) as GameObject;
		currentObject.transform.parent = transform;

		Tween currentTween = currentObject.transform.DOLocalPath(path, speed, PathType.Linear, PathMode.Sidescroller2D)
			.SetId("Assembly Line")
			.SetSpeedBased()
			.SetEase(Ease.Linear)
			.OnComplete(() => Destroy(currentObject));

		return currentTween;
	}
	#endregion

	#region Public Methods
	public static void StopAll()
	{
		DOTween.Pause("Assembly Line");
		stop = true;
	}
	#endregion
}
