using UnityEngine;
using System.Collections;

public class ExplodeEffect : MonoBehaviour 
{
	public GameObject pixelPrefab;
	public float effectLength = 3f;

	private Sprite sprite;
	private Vector2 adjustedBounds;

	void Awake()
	{
		sprite = GetComponentInChildren<SpriteRenderer>().sprite;
	}

	public void Explode(Vector3 velocity)
	{
		adjustedBounds = new Vector2(collider2D.bounds.size.x * 10,
									 collider2D.bounds.size.y * 10);

		for (int i = 1; i <= adjustedBounds.x; i++)
		{
			for (int j = 1; j <= adjustedBounds.y; j++)
			{
				Vector3 pixelPosition = transform.TransformPoint((i / 10f) - 1, (j / 10f), 0);
				Color pixelColor = sprite.texture.GetPixel((int)sprite.rect.x + i, (int)sprite.rect.y + j);

				if (pixelColor != Color.clear)
				{
					GameObject pixelInstance = Instantiate(pixelPrefab, pixelPosition, Quaternion.identity) as GameObject;
					pixelInstance.GetComponent<SpriteRenderer>().color = pixelColor;
					pixelInstance.rigidbody2D.AddForce(new Vector2(((velocity.x * 50f) + Random.Range(-250, 250)), 
																   ((velocity.y * 50f) + Random.Range(-250, 250))));
					Destroy(pixelInstance, effectLength);
				}
			}
		}
	}
}
