using UnityEngine;
using System.Collections;

public class ExplodeEffect : MonoBehaviour 
{
	public GameObject pixelPrefab;

	private Sprite sprite;
	private Vector2 colliderSize;

	public void Explode(Vector3 velocity, Vector2 colliderSize)
	{
		sprite = GetComponentInChildren<SpriteRenderer>().sprite;

		for (int i = 1; i <= colliderSize.x; i++)
		{
			for (int j = 1; j <= colliderSize.y; j++)
			{
				Vector3 pixelPosition = transform.TransformPoint((i / 10f) - 0.8f, (j / 10f) - 0.1f, 0);
				Color pixelColor = sprite.texture.GetPixel((int)sprite.rect.x + i - 1, (int)sprite.rect.y + j - 1);
				
				if (pixelColor.a != 0f)
				{
					GameObject pixelInstance = Instantiate(pixelPrefab, pixelPosition, Quaternion.identity) as GameObject;
					pixelInstance.GetComponent<SpriteRenderer>().color = pixelColor;
					pixelInstance.rigidbody2D.AddForce(new Vector2(((velocity.x * 50f) + Random.Range(-250, 250)), 
																   ((velocity.y * 50f) + Random.Range(-250, 250))));
				}
			}
		}
	}
}
