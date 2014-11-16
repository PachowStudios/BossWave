using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Vectrosity;

public class SmartLaser : Projectile
{
	public int maxJumps = 5;
	public float jumpRange = 5f;
	public float detectionRange = 2f;
	[Range(2, 32)]
	public int subDivisionsPerTarget = 16;
	public Material material;
	public float width = 0.5f;
	public float length = 10f;
	public float wiggle = 0.5f;
	public string sortingLayer = "Player";
	public int sortingOrder = 1;

	private float adjustedWidth;
	private float cooldownTime;
	private float cooldownTimer;

	private List<Enemy> allEnemies = new List<Enemy>();
	private List<Enemy> targetEnemies = new List<Enemy>();
	private List<Vector3> targets = new List<Vector3>();
	private List<Vector3> previousPoints = new List<Vector3>();
	private VectorLine vectorLine;
	private PolygonCollider2D detectionCollider;

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

		int totalSubdivisions = (GameObject.FindGameObjectsWithTag("Enemy").ToList<GameObject>().Count + 1) * subDivisionsPerTarget;

		vectorLine = new VectorLine("Laser", 
									Enumerable.Repeat<Vector3>(PlayerControl.instance.gun.firePoint.position, totalSubdivisions).ToList<Vector3>(), 
									material, 
									adjustedWidth, 
									LineType.Continuous, 
									Joins.Weld);
		vectorLine.textureScale = 1f;

		detectionCollider = gameObject.AddComponent<PolygonCollider2D>();
		detectionCollider.isTrigger = true;
	}

	void OnDrawGizmos()
	{
		foreach (Enemy target in targetEnemies)
		{
			if (target != null)
			{
				Gizmos.color = new Color(Color.magenta.r, Color.magenta.g, Color.magenta.b, 0.15f);
				Gizmos.DrawSphere(target.transform.position, jumpRange);
			}
		}
	}

	void FixedUpdate()
	{
		UpdateCollider();
		GetTargets();

		vectorLine.material = material;

		previousPoints = new List<Vector3>(vectorLine.points3);
		vectorLine.MakeSpline(targets.ToArray());
		vectorLine.MakeSpline(LerpList(previousPoints, vectorLine.points3, 0.25f).ToArray());

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

	private void UpdateCollider()
	{
		detectionCollider.SetPath(0, new Vector2[] { PlayerControl.instance.gun.firePoint.TransformPointLocal(new Vector2(0f, detectionRange)),
													 PlayerControl.instance.gun.firePoint.TransformPointLocal(new Vector2(0f, -detectionRange)),
													 PlayerControl.instance.gun.firePoint.TransformPointLocal(new Vector2(length, -detectionRange)),
													 PlayerControl.instance.gun.firePoint.TransformPointLocal(new Vector2(length, detectionRange)) } );
	}

	private void GetTargets()
	{
		allEnemies.Clear();
		targetEnemies.Clear();
		targets.Clear();

		Vector3 origin = PlayerControl.instance.gun.firePoint.position;
		transform.position = origin;
		targets.Add(origin);

		foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
		{
			allEnemies.Add(enemy.GetComponent<Enemy>());
		}

		if (allEnemies.Count > 0)
		{
			allEnemies = allEnemies.OrderBy(e => e.collider2D.bounds.center.DistanceFrom(origin)).ToList<Enemy>();

			List<Enemy> directTargets = new List<Enemy>();

			foreach (Enemy enemy in allEnemies)
			{
				if (detectionCollider.OverlapPoint(enemy.transform.position))
				{
					directTargets.Add(enemy);
				}
			}

			allEnemies.RemoveAll(directTargets.Contains);

			foreach (Enemy enemy in directTargets)
			{
				int jumps = 0;
				Enemy currentEnemy = enemy;

				targetEnemies.Add(enemy);
				targets.Add(OffsetPosition(enemy.collider2D.bounds.center));

				do
				{
					currentEnemy = GetClosestEnemy(currentEnemy);

					if (currentEnemy != null)
					{
						targetEnemies.Add(currentEnemy);
						targets.Add(OffsetPosition(currentEnemy.collider2D.bounds.center));

						jumps++;
					}
				} while (currentEnemy != null && jumps < maxJumps);
			}
		}
		
		if (targetEnemies.Count == 0)
		{
			targets.Add(OffsetPosition(PlayerControl.instance.gun.firePoint.TransformPoint(new Vector3(length, 0f, 0f))));
		}
	}

	private void DamageTargets()
	{
		foreach (Enemy enemy in targetEnemies)
		{
			enemy.TakeDamage(gameObject);
		}
	}

	private List<Vector3> LerpList(List<Vector3> oldList, List<Vector3> newList, float defaultLerpPoint)
	{
		float currentLerpPoint = defaultLerpPoint;

		if (oldList.Count == newList.Count)
		{
			List<Vector3> result = new List<Vector3>();

			for (int i = 0; i < newList.Count; i++)
			{
				if (newList[i].DistanceFrom(targets[0]) < targets[1].DistanceFrom(targets[0]))
				{
					currentLerpPoint = Extensions.ConvertRange(1f - newList[i].DistanceFrom(targets[0]) / targets[1].DistanceFrom(targets[0]), 0f, 1f, defaultLerpPoint, 1f);
				}
				else
				{
					currentLerpPoint = defaultLerpPoint;
				}

				result.Add(new Vector3(Mathf.Lerp(oldList[i].x, newList[i].x, currentLerpPoint),
									   Mathf.Lerp(oldList[i].y, newList[i].y, currentLerpPoint),
									   newList[i].z));
			}

			return result;
		}
		else
		{
			return newList;
		}
	}

	private Enemy GetClosestEnemy(Enemy currentEnemy)
	{
		Enemy closestEnemy = null;
		float closestDistance = jumpRange;

		allEnemies.Remove(currentEnemy);

		foreach (Enemy enemy in allEnemies)
		{
			float currentDistance = currentEnemy.collider2D.bounds.center.DistanceFrom(enemy.collider2D.bounds.center);

			if (currentDistance <= closestDistance)
			{
				closestEnemy = enemy;
				closestDistance = currentDistance;
			}
		}

		return closestEnemy;
	}

	private Vector3 OffsetPosition(Vector3 currentPosition)
	{
		Vector3 result;

		result.x = Random.Range(currentPosition.x - wiggle, currentPosition.x + wiggle);
		result.y = Random.Range(currentPosition.y - wiggle, currentPosition.y + wiggle);
		result.z = currentPosition.z;
		
		return result;
	}
}
