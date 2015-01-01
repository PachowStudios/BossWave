using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Extensions
{
	// Renderer
	public static bool IsVisibleFrom(this Renderer renderer, Camera camera)
	{
		Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
		return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
	}

	// Transform
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

	public static List<Transform> FindChildTransforms(this Transform parent)
	{
		List<Transform> results = new List<Transform>();

		foreach (Transform child in parent)
		{
			results.Add(child);
		}

		return results;
	}

	public static Vector3 TransformPointLocal(this Transform parent, Vector3 target)
	{
		return parent.TransformPoint(target) - parent.position;
	}

	public static void CorrectScaleForRotation(this Transform parent, Vector3 target, bool correctY = false)
	{
		bool flipY = target.z > 90f && target.z < 270f;

		target.y = correctY && flipY ? 180f : 0f;

		Vector3 newScale = parent.localScale;
		newScale.y = flipY ? -1f : 1f;
		parent.localScale = newScale;
		parent.rotation = Quaternion.Euler(target);
	}

	public static void CopyTo(this Transform parent, Transform target)
	{
		target.name = parent.name + " Temp Transform";
		target.parent = parent.parent;
		target.position = parent.position;
		target.rotation = parent.rotation;
		target.localPosition = parent.localPosition;
		target.localRotation = parent.localRotation;
		target.localScale = parent.localScale;
	}

	public static void LookAt2D(this Transform parent, Vector3 target, bool local = false)
	{
		Quaternion newRotation = parent.position.LookAt2D(target);

		if (local)
		{
			parent.localRotation = newRotation;
		}
		else
		{
			parent.rotation = newRotation;
		}
	}

	// Bounds
	public static Vector3 ScaleToSize(this Bounds parent, Vector3 targetSize)
	{
		return new Vector3(targetSize.x / parent.size.x, targetSize.y / parent.size.y, 1f);
	}

	// Vector3
	public static Quaternion LookAt2D(this Vector3 parent, Vector3 target)
	{
		Vector3 targetPosition = target - parent;
		float angle = Mathf.Atan2(targetPosition.y, targetPosition.x) * Mathf.Rad2Deg;

		return Quaternion.Euler(new Vector3(0f, 0f, Quaternion.AngleAxis(angle, Vector3.forward).eulerAngles.z));
	}

	public static Vector3 DirectionToRotation2D(this Vector3 parent)
	{
		float angle = Mathf.Atan2(parent.y, parent.x) * Mathf.Rad2Deg;

		return Quaternion.AngleAxis(angle, Vector3.forward).eulerAngles;
	}

	public static float DistanceFrom(this Vector3 parent, Vector3 target)
	{
		return Mathf.Sqrt(Mathf.Pow(parent.x - target.x, 2) + Mathf.Pow(parent.y - target.y, 2));
	}

	public static Vector3 CalculateBlackHoleForce(this Vector3 parent, float implosionForce, Vector3 implosionPosition, float implosionRadius, float twistVelocity, float wearoffOverride = 1f)
	{
		Vector3 dir = (parent - implosionPosition);
		float wearoff = wearoffOverride - (dir.magnitude / implosionRadius);
		Vector3 baseForce = dir.normalized * (-implosionForce) * wearoff;
		baseForce = new Vector3(baseForce.x - (baseForce.y * twistVelocity), baseForce.y + (baseForce.x * twistVelocity), 0f);

		return baseForce;
	}

	public static Vector3 OffsetPosition(this Vector3 parent, float wiggle)
	{
		Vector3 result;

		result.x = Random.Range(parent.x - wiggle, parent.x + wiggle);
		result.y = Random.Range(parent.y - wiggle, parent.y + wiggle);
		result.z = parent.z;

		return result;
	}

	// Rigidbody2D
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

	// Generic List
	public static void Shuffle<T>(this List<T> parent)
	{
		for (int i = 0; i < parent.Count; i++)
		{
			T temp = parent[i];
			int randomIndex = Random.Range(i, parent.Count);
			parent[i] = parent[randomIndex];
			parent[randomIndex] = temp;
		}
	}

	// Utility
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

	public static float GetDecimal(float num)
	{
		string result;

		if (num.ToString().Split('.').Length == 2)
		{
			result = "0." + num.ToString().Split('.')[1];
		}
		else
		{
			result = "0";
		}

		return float.Parse(result);
	}

	public static Vector3 SuperSmoothLerp(Vector3 followOld, Vector3 targetOld, Vector3 targetNew, float elapsedTime, float lerpAmount)
	{
		Vector3 f = followOld - targetOld + (targetNew - targetOld) / (lerpAmount * elapsedTime);
		return targetNew - (targetNew - targetOld) / (lerpAmount * elapsedTime) + f * Mathf.Exp(-lerpAmount * elapsedTime);
	}

	public static IEnumerator WaitForRealSeconds(float time)
	{
		float start = Time.realtimeSinceStartup;

		while (Time.realtimeSinceStartup < start + time)
		{
			yield return null;
		}
	}

	public static Vector3 Vector3Range(Vector3 min, Vector3 max)
	{
		return new Vector3(Random.Range(min.x, max.x),
						   Random.Range(min.y, max.y),
						   Random.Range(min.z, max.z));
	}
}
