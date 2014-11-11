using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Vectrosity;

public class SmartLaser : Projectile
{
	public int maxTargets = 5;
	[Range(2, 32)]
	public int subDivisionsPerTarget = 16;
	public Material material;
	public float width = 2f;
	public float length = 20f;
	public string sortingLayer = "Player";
	public int sortingOrder = 1;

	private float adjustedWidth;
	private float cooldownTime;
	private float cooldownTimer;

	private List<Enemy> targetEnemies = new List<Enemy>();
	private List<Vector3> targets = new List<Vector3>();
	private VectorLine vectorLine;

	new void Awake()
	{
		base.Awake();

		adjustedWidth = Camera.main.WorldToScreenPoint(Camera.main.ViewportToWorldPoint(Vector3.zero) + new Vector3(width, 0f, 0f)).x;
		cooldownTime = 1f / shotSpeed;
		cooldownTimer = cooldownTime;

		VectorLine.SetCanvasCamera(Camera.main);
		VectorLine.canvas.planeDistance = 9;
		VectorLine.canvas.sortingLayerName = sortingLayer;
		VectorLine.canvas.sortingOrder = sortingOrder;

		vectorLine = new VectorLine("Laser", new Vector3[maxTargets * subDivisionsPerTarget], material, adjustedWidth, LineType.Continuous, Joins.Weld);
		vectorLine.textureScale = 1f;
		vectorLine.maxWeldDistance = adjustedWidth * 2f;
	}

	void FixedUpdate()
	{
		GetTargets();

		vectorLine.material = material;

		if (targetEnemies.Count <= 1)
		{
			vectorLine.points3.Clear();
			vectorLine.points3.AddRange(targets);
		}
		else
		{
			vectorLine.MakeSpline(targets.ToArray());
		}

		vectorLine.Draw();

		cooldownTimer += Time.deltaTime;

		if (cooldownTimer >= cooldownTime)
		{
			DamageTargets();
			cooldownTimer = 0f;
		}
	}

	void OnDestroy()
	{
		VectorLine.Destroy(ref vectorLine);
	}

	private void GetTargets()
	{
		targetEnemies.Clear();
		targets.Clear();

		Vector3 origin = PlayerControl.instance.gun.firePoint.position;
		transform.position = origin;

		targets.Add(origin);

		foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
		{
			targetEnemies.Add(enemy.GetComponent<Enemy>());
			targets.Add(enemy.collider2D.bounds.center);
		}

		targets = targets.OrderBy(v => v.DistanceFrom(origin)).ToList<Vector3>();

		if (targetEnemies.Count == 0)
		{
			targets.Add(PlayerControl.instance.gun.firePoint.TransformPoint(new Vector3(length, 0f, 0f)));
		}
	}

	private void UpdatePath()
	{
		vectorLine.points3.Clear();

		if (targets.Count > 2)
		{
			Vector3[] targetsArray = targets.ToArray<Vector3>();
			int totalSubdivisions = (targets.Count - 1) * subDivisionsPerTarget;

			for (int i = 0; i < totalSubdivisions; i++)
			{
				Vector3 currentPoint = iTween.PointOnPath(targetsArray, i / (float)totalSubdivisions);
				vectorLine.points3.Add(currentPoint);
			}
		}
		else
		{
			vectorLine.points3.AddRange(targets);
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
