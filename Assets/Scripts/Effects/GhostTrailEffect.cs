using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public class GhostTrailEffect : MonoBehaviour 
{
	public bool trailActive = false;
	public float ghostSpawnTime = 0.1f;
	public float ghostLifetime = 0.25f;
	public AnimationCurve fadeCurve;

	private float ghostSpawnTimer = 0f;
	private List<SpriteRenderer> spriteRenderers;

	void Awake()
	{
		spriteRenderers = GetComponentsInChildren<SpriteRenderer>().ToList<SpriteRenderer>();
	}

	void FixedUpdate()
	{
		if (trailActive)
		{
			ghostSpawnTimer += Time.deltaTime;

			if (ghostSpawnTimer >= ghostSpawnTime)
			{
				foreach (SpriteRenderer spriteRenderer in spriteRenderers)
				{
					SpriteRenderer currentGhost = Instantiate(spriteRenderer, spriteRenderer.transform.position, Quaternion.identity) as SpriteRenderer;
					currentGhost.gameObject.HideInHiearchy();
					currentGhost.sortingOrder -= 1;
					currentGhost.DOFade(0f, ghostLifetime)
						.SetEase(fadeCurve)
						.OnUpdate(() => 
						{
							if (this != null)
							{
								currentGhost.transform.localScale = transform.localScale;
							}
							else
							{
								currentGhost.DOKill(true);
							}
						})
						.OnComplete(() => Destroy(currentGhost.gameObject));
				}

				ghostSpawnTimer = 0f;
			}
		}
	}
}
