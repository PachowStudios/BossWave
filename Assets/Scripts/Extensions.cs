using UnityEngine;
using System.Collections;

public static class Extensions
{
	public static bool IsVisibleFrom(this Renderer renderer, Camera camera)
	{
		Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
		return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
	}

	public static Transform FindSubChild(this Transform parent, string name, bool confirmEnabled = true)
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
				if (confirmEnabled)
				{
					if (result.gameObject.activeInHierarchy)
					{
						return result;
					}
				}
				else
				{
					return result;
				}
			}
		}

		return null;
	}

	public static float LookAt2D(this Transform parent, Vector3 target)
	{
		Vector3 targetPosition = target - parent.position;
		float angle = Mathf.Atan2(targetPosition.y, targetPosition.x) * Mathf.Rad2Deg;

		return Quaternion.AngleAxis(angle, Vector3.forward).eulerAngles.z;
	}

	public static Vector3 TransformPointLocal(this Transform parent, Vector3 target)
	{
		return parent.TransformPoint(target) - parent.position;
	}

	public static float DistanceFrom(this Vector3 parent, Vector3 target)
	{
		return Mathf.Sqrt(Mathf.Pow(parent.x - target.x, 2) + Mathf.Pow(parent.y - target.y, 2));
	}

	public static void AddExplosionForce(this Rigidbody2D parent, float explosionForce, Vector3 explosionPosition, float explosionRadius, float upliftModifier = 0f)
	{
		Vector3 dir = (parent.transform.position - explosionPosition);
		float wearoff = 1 - (dir.magnitude / explosionRadius);
		Vector3 baseForce = dir.normalized * explosionForce * wearoff;
		parent.AddForce(baseForce);

		if (upliftModifier != 0f)
		{
			float upliftWearoff = 1 - upliftModifier / explosionRadius;
			Vector3 upliftForce = Vector2.up * explosionForce * upliftWearoff;
			parent.AddForce(upliftForce);
		}
	}

	public static int RandomSign()
	{
		return (Random.value < 0.5) ? -1 : 1;
	}

	public static float ConvertRange(float num, float oldMin, float oldMax, float newMin, float newMax)
	{
		float oldRange = oldMax - oldMin;
		float newRange = newMax - newMin;

		return (((num - oldMin) * newRange) / oldRange) + newMin;
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
