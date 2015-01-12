﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VectorPath : MonoBehaviour
{
	private static Dictionary<string, VectorPath> paths = new Dictionary<string, VectorPath>();

	public string pathName;
	public Color pathColor = Color.cyan;
	public bool linear = true;
	public Vector3[] nodes = new Vector3[] { Vector3.zero, Vector3.zero };

	void OnEnable()
	{
		if (paths.ContainsKey(pathName.ToLower()))
		{
			paths.Remove(pathName.ToLower());
		}

		paths.Add(pathName.ToLower(), this);
	}

	void OnDrawGizmosSelected()
	{
		if (enabled && nodes.Length >= 2)
		{
			if (linear)
			{
				Gizmos.color = pathColor;

				for (int i = 1; i < nodes.Length; i++)
				{
					Gizmos.DrawLine(nodes[i - 1], nodes[i]);
				}
			}
			else
			{
				iTween.DrawPath(nodes, pathColor);
			}
		}
	}

	public static Vector3[] GetPath(string requestedName)
	{
		requestedName = requestedName.ToLower();

		if (paths.ContainsKey(requestedName))
		{
			return paths[requestedName].nodes;
		}
		else
		{
			Debug.Log("No path with the name " + requestedName + " exists!");
			return null;
		}
	}
}
