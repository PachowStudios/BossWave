using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Vectrosity;

public class SmartLaser : Projectile
{
	public List<Texture2D> laserTextures;
	public List<Sprite> tipSprites;
	public List<Color> colors;
	public Material material;
	[Range(0.01f, 0.1f)]
	public float animationTime = 0.01f;
	public int maxJumps = 5;
	public float jumpRange = 5f;
	public float detectionRange = 2f;
	[Range(2, 32)]
	public int subDivisionsPerTarget = 16;
	public float width = 0.5f;
	public float length = 10f;
	public float wiggle = 0.5f;
	public string sortingLayer = "Player";
	public int sortingOrder = 1;
	public LayerMask collisionLayer;
	[Range(0f, 10f)]
	public float tipExplosionsPerSec = 1f;

	private int currentAnimationFrame = 0;
	private float animationTimer = 0f;
	private float adjustedWidth;
	private float cooldownTime;
	private float cooldownTimer;
	private float tipExplosionTime;
	private float tipExplosionTimer;
	private bool previousTipEnabled = false;
	private Vector3 previousTipPosition;
	private Vector3 tipVelocity;

	private List<Enemy> allEnemies = new List<Enemy>();
	private List<Enemy> targetEnemies = new List<Enemy>();
	private List<Vector3> targets = new List<Vector3>();
	private List<Vector3> previousPoints = new List<Vector3>();
	private VectorLine vectorLine;
	private PolygonCollider2D detectionCollider;
	private SpriteRenderer tip;

	protected override void Awake()
	{
		base.Awake();

		tip = transform.FindChild("Tip").GetComponent<SpriteRenderer>();

		adjustedWidth = Camera.main.WorldToScreenPoint(Camera.main.ViewportToWorldPoint(Vector3.zero) + new Vector3(width, 0f, 0f)).x;
		cooldownTime = 1f / shotSpeed;
		cooldownTimer = cooldownTime;

		tipExplosionTime = 1f / tipExplosionsPerSec;
		tipExplosionTimer = tipExplosionTime;

		VectorLine.SetCanvasCamera(Camera.main);
		VectorLine.canvas.planeDistance = 9;
		VectorLine.canvas.sortingLayerName = sortingLayer;
		VectorLine.canvas.sortingOrder = sortingOrder;

		int totalSubdivisions = (GameObject.FindGameObjectsWithTag("Enemy").ToList<GameObject>().Count + 1) * subDivisionsPerTarget;

		vectorLine = new VectorLine("Laser", 
									Enumerable.Repeat<Vector3>(PlayerControl.Instance.Gun.firePoint.position, totalSubdivisions).ToList<Vector3>(), 
									material, 
									adjustedWidth, 
									LineType.Continuous,
									Joins.Fill);
		vectorLine.textureScale = 1f;

		detectionCollider = gameObject.AddComponent<PolygonCollider2D>();
		detectionCollider.isTrigger = true;
	}

	private void FixedUpdate()
	{
		previousTipEnabled = tip.enabled;
		previousTipPosition = tip.transform.position;

		UpdateMaterials();
		UpdateCollider();
		GetTargets();

		previousPoints = new List<Vector3>(vectorLine.points3);
		vectorLine.MakeSpline(targets.ToArray());
		vectorLine.MakeSpline(LerpList(previousPoints, vectorLine.points3, 0.25f).ToArray());

		tip.transform.position = vectorLine.points3.Last();
		tipVelocity = (tip.transform.position - previousTipPosition) / Time.deltaTime / 10f;

		vectorLine.Draw();

		cooldownTimer += Time.deltaTime;

		if (cooldownTimer >= cooldownTime)
		{
			DamageTargets();
			cooldownTimer = 0f;
		}

		tipExplosionTimer += Time.deltaTime;

		if (!previousTipEnabled && tip.enabled)
		{
			tipExplosionTimer = tipExplosionTime;
		}

		if (tipExplosionTimer >= tipExplosionTime && tip.enabled)
		{
			if (targetEnemies.Count > 0)
			{
				foreach (Enemy enemy in targetEnemies)
				{
					ExplodeEffect.Instance.Explode(enemy.transform, Vector3.zero, tip.sprite, tip.material);
				}
			}
			else
			{
				ExplodeEffect.Instance.Explode(tip.transform, tipVelocity, tip.sprite, tip.material);
			}

			tipExplosionTimer = 0f;
		}
	}

	private void OnDestroy()
	{
		VectorLine.Destroy(ref vectorLine);
	}

	private void UpdateMaterials()
	{
		animationTimer += Time.deltaTime;

		if (animationTimer >= animationTime)
		{
			material.SetColor("_TintColor", colors[currentAnimationFrame]);

			material.mainTexture = laserTextures[currentAnimationFrame];
			tip.sprite = tipSprites[currentAnimationFrame];

			animationTimer = 0f;
			currentAnimationFrame = (currentAnimationFrame + 1 >= laserTextures.Count) ? 0 : currentAnimationFrame + 1;

			vectorLine.material = material;
			tip.material = material;
		}
	}

	private void UpdateCollider()
	{
		detectionCollider.SetPath(0, new Vector2[] { PlayerControl.Instance.Gun.firePoint.TransformPointLocal(new Vector2(0f, detectionRange / 4f)),
													 PlayerControl.Instance.Gun.firePoint.TransformPointLocal(new Vector2(0f, -(detectionRange / 4f))),
													 PlayerControl.Instance.Gun.firePoint.TransformPointLocal(new Vector2(length, -detectionRange)),
													 PlayerControl.Instance.Gun.firePoint.TransformPointLocal(new Vector2(length, detectionRange)) } );
	}

	private void GetTargets()
	{
		allEnemies.Clear();
		targetEnemies.Clear();
		targets.Clear();

		Vector3 origin = PlayerControl.Instance.Gun.firePoint.position;
		transform.position = origin;
		targets.Add(origin);

		foreach (Enemy enemy in GameObject.FindObjectsOfType<Enemy>())
		{
			if (enemy.collider2D != null && enemy.spawned)
			{
				allEnemies.Add(enemy);
			}
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

			allEnemies.RemoveAll(e => directTargets.Contains(e));

			foreach (Enemy enemy in directTargets)
			{
				int jumps = 0;
				Enemy currentEnemy = enemy;

				targetEnemies.Add(enemy);
				targets.Add(enemy.collider2D.bounds.center.OffsetPosition(wiggle));

				do
				{
					currentEnemy = GetClosestEnemy(currentEnemy);

					if (currentEnemy != null)
					{
						targetEnemies.Add(currentEnemy);
						targets.Add(currentEnemy.collider2D.bounds.center.OffsetPosition(wiggle));

						jumps++;
					}
				} while (currentEnemy != null && jumps < maxJumps);
			}
		}
		
		if (targetEnemies.Count == 0)
		{
			Vector3 endPoint = PlayerControl.Instance.Gun.firePoint.TransformPoint(new Vector3(length, 0f, 0f));
			RaycastHit2D raycast = Physics2D.Linecast(origin, endPoint, collisionLayer);

			if (raycast.collider != null)
			{
				endPoint = raycast.point;
				tip.enabled = true;
			}
			else
			{
				tip.enabled = false;
			}

			targets.Add(endPoint.OffsetPosition(wiggle));
		}
		else
		{
			tip.enabled = true;
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
}
