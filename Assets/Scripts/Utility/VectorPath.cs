using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class VectorPath : MonoBehaviour
{
	#region Fields
	public string pathName;
	public Color pathColor = Color.cyan;
	public PathType pathType = PathType.Linear;
	public Vector3[] nodes = new Vector3[] { Vector3.zero, Vector3.zero };

	private static Dictionary<string, VectorPath> paths = new Dictionary<string, VectorPath>();
	#endregion

	#region MonoBehaviour
	private void OnEnable()
	{
		if (paths.ContainsKey(pathName.ToLower()))
		{
			paths.Remove(pathName.ToLower());
		}

		paths.Add(pathName.ToLower(), this);
	}

	private void OnDrawGizmosSelected()
	{
		if (enabled && nodes.Length >= 2)
		{
			Gizmos.color = pathColor;

			for (int i = 1; i < nodes.Length; i++)
			{
				Gizmos.DrawLine(nodes[i - 1], nodes[i]);
			}
		}
	}
	#endregion

	#region Public Methods
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

	public static PathType GetPathType(string requestedName)
	{
		requestedName = requestedName.ToLower();

		if (paths.ContainsKey(requestedName))
		{
			return paths[requestedName].pathType;
		}
		else
		{
			Debug.Log("No path with the name " + requestedName + " exists!");
			return PathType.Linear;
		}
	}
	#endregion
}
