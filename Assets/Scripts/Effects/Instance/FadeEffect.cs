using UnityEngine;
using System.Collections;

public class FadeEffect : MonoBehaviour 
{
	public float lifetime = 4f;
	public float startTime = 1f;
	public bool fadeIn = false;

	private float fadeTimer = 0f;

	private SpriteRenderer spriteRenderer;

	void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();

		if (fadeIn)
		{
			Color newColor = spriteRenderer.color;
			newColor.a = 0f;
			spriteRenderer.color = newColor;
		}
	}

	void FixedUpdate()
	{
		if (fadeIn)
		{
			if (spriteRenderer.color.a < 1f)
			{
				Color newColor = spriteRenderer.color;
				newColor.a = Mathf.Lerp(newColor.a, 1f, 0.025f);
				spriteRenderer.color = newColor;
			}
		}
		else
		{
			fadeTimer += Time.deltaTime;

			if (fadeTimer >= startTime)
			{
				Color newColor = spriteRenderer.color;
				newColor.a = Mathf.Lerp(newColor.a, 0f, 0.025f);
				spriteRenderer.color = newColor;

				if (fadeTimer >= lifetime)
				{
					Destroy(gameObject);
				}
			}
		}
	}
}
