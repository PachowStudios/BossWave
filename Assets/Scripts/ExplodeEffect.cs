using UnityEngine;
using System.Collections;

public class ExplodeEffect : MonoBehaviour 
{
	public GameObject pixelPrefab;

	public void Explode(Vector3 velocity, Sprite sprite, bool enableColliders = false)
	{
		for (int i = 0; i <= sprite.bounds.size.x * 10f; i++)
		{
			for (int j = 0; j <= sprite.bounds.size.y * 10f; j++)
			{
				Vector2 positionOffest = new Vector2(sprite.bounds.extents.x - sprite.bounds.center.x,
													 sprite.bounds.extents.y - sprite.bounds.center.y - 0.05f);

				Vector3 pixelPosition = transform.TransformPoint((i / 10f) - positionOffest.x, 
																 (j / 10f) - positionOffest.y, 0);

				Color pixelColor = sprite.texture.GetPixel((int)sprite.rect.x + i, 
													       (int)sprite.rect.y + j);
				
				if (pixelColor.a != 0f)
				{
					GameObject pixelInstance = Instantiate(pixelPrefab, pixelPosition, Quaternion.identity) as GameObject;

					if (enableColliders)
					{
						pixelInstance.GetComponent<FadeEffect>().lifetime = 4f;
						pixelInstance.GetComponent<FadeEffect>().startTime = 3f;
					}

					pixelInstance.GetComponent<SpriteRenderer>().color = pixelColor;
					pixelInstance.GetComponent<Collider2D>().enabled = enableColliders;
					float velocityMultiplier = enableColliders ? 1f : 2.5f;
					pixelInstance.rigidbody2D.AddForce(new Vector2(((velocity.x * 10f) + Random.Range(-250, 250)) * velocityMultiplier, 
																   ((velocity.y * 10f) + Random.Range(-250, 300)) * velocityMultiplier));
				}
			}
		}
	}
}
