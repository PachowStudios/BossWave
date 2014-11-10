using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Vectrosity;

public class SmartLaser : Projectile
{
	[Range(2, 32)]
	public int subDivisionsPerTarget = 16;
	public Material material;
	public float width = 2f;
	public string sortingLayer = "Player";
	public int sortingOrder = 1;

	private float cooldownTime;
	private float cooldownTimer;

	private List<Enemy> targetEnemies = new List<Enemy>();
	private List<Vector3> targets = new List<Vector3>();
	private VectorLine vectorLine;

	new void Awake()
	{
		base.Awake();

		cooldownTime = 1f / shotSpeed;
		cooldownTimer = cooldownTime;

		VectorLine.SetCanvasCamera(Camera.main);
		VectorLine.canvas.planeDistance = 9;
		VectorLine.canvas.sortingLayerName = sortingLayer;
		VectorLine.canvas.sortingOrder = sortingOrder;

		vectorLine = new VectorLine("Laser", new List<Vector3>(), material, width, LineType.Continuous, Joins.Weld);
		vectorLine.textureScale = 1.0f;
	}

	void FixedUpdate()
	{
		GetTargets();
		UpdatePath();

		vectorLine.material = material;
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

		if (targets.Count == 1)
		{
			targets.Add(PlayerControl.instance.gun.firePoint.TransformPoint(new Vector3(50f, 0f, 0f)));
		}
	}

	private void UpdatePath()
	{
		vectorLine.points3.Clear();

		Vector3[] targetsArray = targets.ToArray<Vector3>();
		int totalSubdivisions = (targets.Count - 1) * subDivisionsPerTarget;

		for (int i = 0; i < totalSubdivisions; i++)
		{
			Vector3 currentPoint = iTween.PointOnPath(targetsArray, i / (float)totalSubdivisions);
			vectorLine.points3.Add(currentPoint);
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
