using UnityEngine;
using System.Collections;

public class ExplodeEffect : MonoBehaviour 
{
	static private ExplodeEffect instance;

	public SpriteExplosion explosionPrefab;
	public float pixelsPerUnit = 10f;

	private static SpriteExplosion explosionInstance;

	void Awake()
	{
		instance = this;
	}

	public static void Explode(Transform transform, Vector3 velocity, Sprite sprite, Material material = null)
	{
		explosionInstance = Instantiate(instance.explosionPrefab, transform.position, transform.rotation) as SpriteExplosion;
		explosionInstance.transform.localScale = transform.localScale;
		explosionInstance.pixelsPerUnit = instance.pixelsPerUnit;
		explosionInstance.material = material;
		explosionInstance.Explode(velocity, sprite);
	}
}
