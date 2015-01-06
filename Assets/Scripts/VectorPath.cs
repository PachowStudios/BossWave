using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VectorPath : MonoBehaviour
{
	public static Dictionary<string, VectorPath> paths = new Dictionary<string, VectorPath>();

	public string pathName;
	public Color pathColor = Color.cyan;
	public List<Vector3> nodes = new List<Vector3>() { Vector3.zero, Vector3.zero };

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
		if (enabled && nodes.Count >= 2)
		{
			Gizmos.color = pathColor;

			for (int i = 1; i < nodes.Count; i++)
			{
				Gizmos.DrawLine(nodes[i - 1], nodes[i]);
			}
		}
	}

	public static Vector3[] GetPath(string requestedName)
	{
		requestedName = requestedName.ToLower();

		if (paths.ContainsKey(requestedName))
		{
			return paths[requestedName].nodes.ToArray();
		}
		else
		{
			Debug.Log("No path with the name " + requestedName + " exists!");
			return null;
		}
	}
}
