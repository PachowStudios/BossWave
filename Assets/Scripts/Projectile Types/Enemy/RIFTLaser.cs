using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Vectrosity;

public class RIFTLaser : Projectile
{
	public List<Texture2D> textures;
	public List<Color> colors;
	public Material material;
	[Range(0.01f, 0.1f)]
	public float animationTime = 0.01f;
	[Range(2, 32)]
	public int subdivisions = 16;
	public float width = 1.5f;
	public float lengthOffset = 0f;
	public string sortingLayer = "Player";
	public int sortingOrder = 1;
	public LayerMask collisionLayer;
	[Range(0f, 10f)]
	public float tipExplosionsPerSec = 7f;

	[HideInInspector]
	public Vector3 firePoint;
	[HideInInspector]
	public Vector3 targetPoint;

	private bool charging = true;
	private int currentAnimationFrame = 0;
	private float animationTimer = 0f;
	private float adjustedWidth;
	private float cooldownTime;
	private float cooldownTimer;
	private float tipExplosionTime;
	private float tipExplosionTimer = 0f;
	private Vector3 previousTipPosition;
	private Vector3 tipVelocity;
	private List<Vector3> targets = new List<Vector3>();
	private List<Vector3> previousPoints = new List<Vector3>();
	private VectorLine vectorLine;

	private SpriteRenderer tip;
	private SpriteRenderer charge;

	public bool Charging
	{
		get { return charging; }
	}

	new void Awake()
	{
		base.Awake();

		tip = transform.FindChild("Tip").GetComponent<SpriteRenderer>();
		charge = transform.FindChild("Charge").GetComponent<SpriteRenderer>();

		adjustedWidth = Camera.main.WorldToScreenPoint(Camera.main.ViewportToWorldPoint(Vector3.zero) + new Vector3(width, 0f, 0f)).x;
		cooldownTime = 1f / shotSpeed;
		cooldownTimer = cooldownTime;

		tipExplosionTime = 1f / tipExplosionsPerSec;

		VectorLine.SetCanvasCamera(Camera.main);
		VectorLine.canvas.planeDistance = 9;
		VectorLine.canvas.sortingLayerName = sortingLayer;
		VectorLine.canvas.sortingOrder = sortingOrder;

		vectorLine = new VectorLine("R.I.F.T Laser",
									Enumerable.Repeat<Vector3>(transform.position, subdivisions).ToList<Vector3>(),
									material,
									adjustedWidth,
									LineType.Continuous,
									Joins.Weld);
		vectorLine.textureScale = 1f;
	}

	void FixedUpdate()
	{
		transform.position = firePoint;
		transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, firePoint.LookAt2D(targetPoint)));
		charge.transform.rotation = Quaternion.identity;

		UpdateMaterials();

		if (!charging)
		{
			previousTipPosition = tip.transform.position;

			targets[0] = firePoint;
			targets[1] = transform.TransformPoint(new Vector3(firePoint.DistanceFrom(targetPoint) + lengthOffset, 0f, 0f));

			previousPoints = new List<Vector3>(vectorLine.points3);
			vectorLine.MakeSpline(targets.ToArray());
			vectorLine.MakeSpline(LerpList(previousPoints, vectorLine.points3, 0.25f).ToArray());

			tip.transform.position = vectorLine.points3.Last();
			tip.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, vectorLine.points3[vectorLine.points3.Count - 2].LookAt2D(vectorLine.points3.Last())));
			tipVelocity = (tip.transform.position - previousTipPosition) / Time.deltaTime / 10f;

			vectorLine.Draw();

			cooldownTimer += Time.deltaTime;

			if (cooldownTimer >= cooldownTime)
			{
				if (Physics2D.Raycast(firePoint, (tip.transform.position - firePoint).normalized, 100f, collisionLayer).collider != null)
				{
					PlayerControl.instance.Health -= damage;
					cooldownTimer = 0f;
				}
			}

			tipExplosionTimer += Time.deltaTime;

			if (tipExplosionTimer >= tipExplosionTime && tip.enabled)
			{
				ExplodeEffect.Explode(tip.transform, tipVelocity, tip.sprite, tip.material);
				tipExplosionTimer = 0f;
			}			
		}
	}

	void OnDestroy()
	{
		VectorLine.Destroy(ref vectorLine);
	}

	public void Fire()
	{
		targets.Add(firePoint);
		targets.Add(firePoint);
		vectorLine.MakeSpline(targets.ToArray());
		tip.enabled = true;
		charging = false;
	}

	private void UpdateMaterials()
	{
		animationTimer += Time.deltaTime;

		if (animationTimer >= animationTime)
		{
			material.SetColor("_TintColor", colors[currentAnimationFrame]);
			material.mainTexture = textures[currentAnimationFrame];

			currentAnimationFrame = (currentAnimationFrame + 1 >= textures.Count) ? 0 : currentAnimationFrame + 1;

			vectorLine.material = material;
			tip.material = material;
			charge.material = material;

			animationTimer = 0f;
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
}
