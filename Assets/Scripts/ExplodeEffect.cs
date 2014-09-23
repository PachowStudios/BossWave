using UnityEngine;
using System.Collections;

public class ExplodeEffect : MonoBehaviour 
{
	public GameObject pixelPrefab;

	private Sprite sprite;
	private float scaleX;
	private Vector2 colliderSize;

	public void Explode(Vector3 velocity, Vector2 colliderSize)
	{
		sprite = GetComponentInChildren<SpriteRenderer>().sprite;
		scaleX = transform.FindChild("Body").localScale.x;

		for (int i = 0; i <= colliderSize.x * 10f; i++)
		{
			for (int j = 0; j <= colliderSize.y * 10f; j++)
			{
				Vector3 pixelPosition = transform.TransformPoint((i / 10f) - (colliderSize.x / 2f), (j / 10f) + 0.05f, 0);

				Color pixelColor;

				if (scaleX < 0f)
				{
					pixelColor = sprite.texture.GetPixel((int)sprite.rect.x + (int)sprite.rect.width - i,
														 (int)sprite.rect.y + j);
				}
				else
				{
					pixelColor = sprite.texture.GetPixel((int)sprite.rect.x + i, 
														 (int)sprite.rect.y + j);
				}
				
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
