using UnityEngine;
using System.Collections;
using DG.Tweening;

[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public sealed class TransparentForeground : MonoBehaviour
{
	#region Fields
	public static bool allowHiding = true;

	public float hiddenOpacity = 0f;
	public float fadeTime = 0.5f;

	private bool hidden = false;

	private SpriteRenderer spriteRenderer;
	#endregion

	#region MonoBehaviour
	private void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
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
		spriteRenderer.DOKill();
		spriteRenderer.DOFade(1f, fadeTime);
		hidden = false;
	}

	private void Hide()
	{
		spriteRenderer.DOKill();
		spriteRenderer.DOFade(hiddenOpacity, fadeTime);
		hidden = true;
	}
	#endregion
}
