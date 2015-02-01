using UnityEngine;
using System.Collections;
using DG.Tweening;

public class FadeEffect : MonoBehaviour 
{
	public float fadeTime = 0.5f;

	private SpriteRenderer spriteRenderer;

	private void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
	}

	private void Start()
	{
		spriteRenderer.DOFade(0f, fadeTime)
			.From();
	}
}
