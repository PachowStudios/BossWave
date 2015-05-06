using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class VectorPath : MonoBehaviour
{
	#region Fields
	private static Dictionary<string, VectorPath> paths = new Dictionary<string, VectorPath>();

	public string pathName;
	public Color pathColor = Color.cyan;
	public PathType pathType = PathType.Linear;
	public bool isLocal = false;
	public Vector3[] nodes = new Vector3[] { Vector3.zero, Vector3.zero };
	#endregion

	#region Public Properties
	public Vector3[] Nodes
	{ get { return isLocal ? LocalizePath() : nodes; } }
	#endregion

	#region MonoBehaviour
	private void OnEnable()
	{
		if (paths.ContainsKey(pathName.ToLower()))
			paths.Remove(pathName.ToLower());

		paths.Add(pathName.ToLower(), this);
	}

	private void OnDrawGizmosSelected()
	{
		if (enabled && nodes.Length >= 2)
		{
			Gizmos.color = pathColor;

			Vector3[] currentNodes = Nodes;

			for (int i = 1; i < currentNodes.Length; i++)
				Gizmos.DrawLine(currentNodes[i - 1], currentNodes[i]);
		}
	}
	#endregion

	#region Internal Helper Methods
	private Vector3[] LocalizePath()
	{
		Vector3[] localPath = new Vector3[nodes.Length];

		for (int i = 0; i < nodes.Length; i++)
			localPath[i] = nodes[i] + transform.position;

		return localPath;
	}
	#endregion

	#region Public Methods
	public static Vector3[] GetPath(string requestedName)
	{
		requestedName = requestedName.ToLower();

		if (paths.ContainsKey(requestedName))
		{
			return paths[requestedName].Nodes;
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
