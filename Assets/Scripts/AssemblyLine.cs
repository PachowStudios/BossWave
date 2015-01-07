using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class AssemblyLine : MonoBehaviour 
{
	public GameObject prefab;
	public string pathName;                                                    
	public float speed = 10f;
	public float spawnDelay = 1f;

	private float spawnTimer;
	private Vector3[] path;
	
	void Awake()
	{
		spawnTimer = spawnDelay;
	}

	void Start()
	{
		path = VectorPath.GetPath(pathName);
	}

	void FixedUpdate()
	{
		spawnTimer += Time.deltaTime;

		if (spawnTimer >= spawnDelay)
		{
			GameObject currentObject = Instantiate(prefab, path[0] + transform.position, Quaternion.identity) as GameObject;
			currentObject.transform.parent = transform;

			currentObject.transform.DOLocalPath(path, speed, PathType.Linear, PathMode.Sidescroller2D)
				.SetSpeedBased()
				.SetEase(Ease.Linear)
				.OnComplete(() => Destroy(currentObject));


			spawnTimer = 0f;
		}
	}
}
