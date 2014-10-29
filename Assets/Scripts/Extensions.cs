using UnityEngine;
using System.Collections;

public static class Extensions
{
	public static bool IsVisibleFrom(this Renderer renderer, Camera camera)
	{
		Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
		return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
	}

	public static Transform FindSubChild(this Transform parent, string name)
	{
		if (parent.name.Equals(name))
		{
			return parent;
		}

		foreach (Transform child in parent)
		{
			Transform result = FindSubChild(child, name);

			if (result != null)
			{
				return result;
			}
		}

		return null;
	}

	public static IEnumerator WaitForRealSeconds(float time)
	{
		float start = Time.realtimeSinceStartup;

		while (Time.realtimeSinceStartup < start + time)
		{
			yield return null;
		}
	}
}
