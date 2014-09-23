using UnityEngine;
using System.Collections;

public class FadeEffect : MonoBehaviour 
{
	public float lifetime = 4f;

	private float fadeTimer = 0f;

	private SpriteRenderer spriteRenderer;

	void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
	}

	void FixedUpdate()
	{
		fadeTimer += Time.deltaTime;

		if (fadeTimer >= lifetime - 1f)
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
