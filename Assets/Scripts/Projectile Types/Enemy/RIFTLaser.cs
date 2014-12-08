using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Vectrosity;

public class RIFTLaser : Projectile
{
	public List<Texture2D> laserTextures;
	public List<Sprite> tipSprites;
	public List<Color> colors;
	public Material material;
	[Range(0.01f, 0.1f)]
	public float animationTime = 0.01f;
	public int subdivisions = 16;
	public float width = 1.5f;
	public float wiggle = 0.5f;
	public string sortingLayer = "Player";
	public int sortingOrder = 1;
	public LayerMask collisionLayer;
	[Range(0f, 10f)]
	public float tipExplosionsPerSec = 7f;

	[HideInInspector]
	public Vector3 firePoint;

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

	private List<Vector3> targets = new List<Vector3>();
	private VectorLine vectorLine;
	private SpriteRenderer tip;

	new void Awake()
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

		vectorLine = new VectorLine("R.I.F.T Laser",
									Enumerable.Repeat<Vector3>(transform.position, subdivisions).ToList<Vector3>(),
									material,
									adjustedWidth,
									LineType.Continuous,
									Joins.Fill);

		vectorLine.textureScale = 1f;
	}

	void FixedUpdate()
	{
		previousTipEnabled = tip.enabled;
		previousTipPosition = tip.transform.position;

		UpdateMaterials();
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

	private void GetTargets()
	{
		targets.Clear();

		transform.position = firePoint;
		targets.Add(firePoint);
		targets.Add(PlayerControl.instance.collider2D.bounds.center);
	}
}
