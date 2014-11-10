using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(LineRenderer))]
public class SmartLaser : Projectile
{
	[Range(2, 32)]
	public int subDivisionsPerTarget = 16;

	private float cooldownTime;
	private float cooldownTimer;

	private List<Enemy> targetEnemies = new List<Enemy>();
	private List<Vector3> targets = new List<Vector3>();

	private LineRenderer lineRenderer;

	new void Awake()
	{
		base.Awake();

		cooldownTime = 1f / shotSpeed;
		cooldownTimer = cooldownTime;

		lineRenderer = GetComponent<LineRenderer>();
	}

	void FixedUpdate()
	{
		GetTargets();
		UpdatePath();

		cooldownTimer += Time.deltaTime;

		if (cooldownTimer >= cooldownTime)
		{
			DamageTargets();
			cooldownTimer = 0f;
		}
	}

	private void GetTargets()
	{
		targetEnemies.Clear();
		targets.Clear();

		Vector3 origin = PlayerControl.instance.gun.firePoint.position + new Vector3(0f, 0f, -1f);

		targets.Add(origin);
		//targets.Add(PlayerControl.instance.gun.firePoint.TransformPoint(new Vector3(2f, 0f, 0f)) + new Vector3(0f, 0f, -1f));

		foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
		{
			targetEnemies.Add(enemy.GetComponent<Enemy>());
			targets.Add(enemy.transform.position + new Vector3(0f, 0f, -1f));
		}

		targets = targets.OrderBy(v => v.DistanceFrom(origin)).ToList<Vector3>();
	}

	private void UpdatePath()
	{
		Vector3[] targetsArray = targets.ToArray<Vector3>();
		int totalSubdivisions = (targets.Count - 1) * subDivisionsPerTarget;

		lineRenderer.SetVertexCount(totalSubdivisions);

		for (int i = 0; i < totalSubdivisions; i++)
		{
			Vector3 currentPoint = iTween.PointOnPath(targetsArray, i / (float)totalSubdivisions);
			lineRenderer.SetPosition(i, currentPoint);
		}
	}

	private void DamageTargets()
	{
		foreach (Enemy enemy in targetEnemies)
		{
			enemy.TakeDamage(gameObject);
		}
	}
}
