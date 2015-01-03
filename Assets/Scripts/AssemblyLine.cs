using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
		path = iTweenPath.GetPath(pathName);
	}

	void FixedUpdate()
	{
		spawnTimer += Time.deltaTime;

		if (spawnTimer >= spawnDelay)
		{
			GameObject currentObject = Instantiate(prefab, path[0], Quaternion.identity) as GameObject;
			currentObject.transform.parent = transform;

			iTween.MoveTo(currentObject, iTween.Hash("path", path,
													 "speed", speed,
													 "easetype", iTween.EaseType.linear,
													 "oncomplete", "DestroyObject",
													 "oncompleteparams", currentObject,
													 "oncompletetarget", gameObject));

			spawnTimer = 0f;
		}
	}

	private void DestroyObject(GameObject target)
	{
		Destroy(target);
	}
}
