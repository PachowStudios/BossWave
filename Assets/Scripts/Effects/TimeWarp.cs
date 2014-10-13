using UnityEngine;
using System.Collections;

public static class TimeWarp
{
	private static bool isWarping = false;

	public static IEnumerator Warp(float timeScale, float length)
	{
		if (!isWarping)
		{
			isWarping = true;
			float startTime = Time.realtimeSinceStartup;

			while (Time.timeScale > timeScale + 0.01f)
			{
				Time.timeScale = Mathf.Lerp(Time.timeScale, timeScale, 0.2f);
				yield return new WaitForSeconds(0.01f * Time.timeScale);
			}

			Time.timeScale = timeScale;

			yield return new WaitForSeconds((length - ((Time.realtimeSinceStartup - startTime) * 2f)) * Time.timeScale);

			while (Time.timeScale < 0.99f)
			{
				Time.timeScale = Mathf.Lerp(Time.timeScale, 1f, 0.2f);
				yield return new WaitForSeconds(0.01f * Time.timeScale);
			}

			Time.timeScale = 1f;
			isWarping = false;
		}
		else
		{
			Debug.LogError("Tried to timewarp during a timewarp!");
		}
	}
}
