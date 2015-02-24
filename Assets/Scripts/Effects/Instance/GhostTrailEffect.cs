using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DG.Tweening;

public class GhostTrailEffect : MonoBehaviour
{
	#region Fields
	public bool trailActive = false;
	public float ghostSpawnTime = 0.075f;
	public float ghostLifetime = 0.25f;
	public AnimationCurve fadeCurve;
	public string sortingLayer;

	private float ghostSpawnTimer = 0f;
	private List<SpriteRenderer> spriteRenderers;
	#endregion

	#region Internal Properties
	private ReadOnlyCollection<SpriteRenderer> SpriteRenderers
	{
		get
		{
			if (gameObject.tag == "Player")
			{
				return PlayerControl.Instance.SpriteRenderers;
			}
			else
			{
				return spriteRenderers.AsReadOnly();
			}
		}
	}
	#endregion

	#region MonoBehaviour
	private void Awake()
	{
		spriteRenderers = GetComponentsInChildren<SpriteRenderer>().ToList();
	}

	private void LateUpdate()
	{
		if (trailActive)
		{
			ghostSpawnTimer += Time.deltaTime;

			if (ghostSpawnTimer >= ghostSpawnTime)
			{
				GameObject currentParent = new GameObject();
				transform.CopyTo(currentParent.transform);
				currentParent.name = gameObject.name + " Ghost";

				foreach (SpriteRenderer spriteRenderer in SpriteRenderers)
				{
					GameObject currentGhost = new GameObject();
					spriteRenderer.transform.CopyTo(currentGhost.transform);
					currentGhost.name = spriteRenderer.name + " Ghost";
					currentGhost.transform.parent = currentParent.transform;

					SpriteRenderer ghostSpriteRenderer = currentGhost.AddComponent<SpriteRenderer>();
					ghostSpriteRenderer.sprite = spriteRenderer.sprite;
					ghostSpriteRenderer.color = spriteRenderer.color;
					ghostSpriteRenderer.enabled = spriteRenderer.enabled;
					ghostSpriteRenderer.sortingLayerName = sortingLayer;
					ghostSpriteRenderer.sortingOrder = -20 + spriteRenderer.sortingOrder;

					ghostSpriteRenderer.DOFade(0f, ghostLifetime)
						.SetEase(fadeCurve)
						.OnUpdate(() =>
						{
							if (this == null)
							{
								ghostSpriteRenderer.DOKill(true);
							}
						})
						.OnComplete(() =>
						{
							if (currentParent != null)
							{
								Destroy(currentParent);
							}
						});
				}

				ghostSpawnTimer = 0f;
			}
		}
	}
	#endregion
}
