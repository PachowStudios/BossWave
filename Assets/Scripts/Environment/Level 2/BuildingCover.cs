using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public sealed class BuildingCover : MonoBehaviour
{
	#region Fields
	public static bool allowHiding = true;

	public float hiddenOpacity = 0f;
	public float fadeTime = 0.5f;

	public SpriteRenderer leftGlass;
	public SpriteRenderer rightGlass;

	private bool hidden = false;

	private List<SpriteRenderer> fadeSpriteRenderers = new List<SpriteRenderer>();
	#endregion

	#region MonoBehaviour
	private void Awake()
	{
		fadeSpriteRenderers.Add(GetComponent<SpriteRenderer>());
		fadeSpriteRenderers.Add(leftGlass);
		fadeSpriteRenderers.Add(rightGlass);
	}

	private void Update()
	{
		bool overlapping = PlayerControl.Instance.collider2D.bounds.Intersects(collider2D.bounds);
		overlapping = allowHiding && overlapping;

		if (overlapping && !hidden)
			Hide();
		else if (!overlapping && hidden)
			Show();
	}
	#endregion

	#region Internal Helper Methods
	private void Show()
	{
		foreach (SpriteRenderer spriteRenderer in fadeSpriteRenderers)
		{
			if (spriteRenderer != null)
			{
				spriteRenderer.DOKill();
				spriteRenderer.DOFade(1f, fadeTime);
			}
		}

		hidden = false;
	}

	private void Hide()
	{
		foreach (SpriteRenderer spriteRenderer in fadeSpriteRenderers)
		{
			if (spriteRenderer != null)
			{
				spriteRenderer.DOKill();
				spriteRenderer.DOFade(hiddenOpacity, fadeTime);
			}
		}

		hidden = true;
	}
	#endregion
}
