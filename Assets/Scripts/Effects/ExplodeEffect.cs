using UnityEngine;
using System.Collections;

public class ExplodeEffect : MonoBehaviour 
{
	public SpriteExplosion explosionPrefab;
	public float pixelsPerUnit = 10f;

	private SpriteExplosion explosionInstance;

	public void Explode(Vector3 velocity, Sprite sprite)
	{
		explosionInstance = Instantiate(explosionPrefab, transform.position, transform.rotation) as SpriteExplosion;
		explosionInstance.transform.localScale = transform.localScale;
		explosionInstance.pixelsPerUnit = pixelsPerUnit;
		explosionInstance.Explode(velocity, sprite);
	}
}
