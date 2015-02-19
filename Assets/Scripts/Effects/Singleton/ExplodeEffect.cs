using UnityEngine;
using System.Collections;

public class ExplodeEffect : MonoBehaviour
{
	#region Fields
	private static ExplodeEffect instance;

	public SpriteExplosion explosionPrefab;
	public float pixelsPerUnit = 10f;
	#endregion

	#region Public Properties
	public static ExplodeEffect Instance
	{
		get { return instance; }
	}
	#endregion

	#region MonoBehaviour
	private void Awake()
	{
		instance = this;
	}
	#endregion

	#region Public Methods
	public void Explode(Transform target, Vector3 velocity, Sprite sprite, Material material = null)
	{
		SpriteExplosion explosionInstance = Instantiate(explosionPrefab, target.position, target.rotation) as SpriteExplosion;
		explosionInstance.transform.parent = transform;
		explosionInstance.transform.localScale = target.localScale;
		explosionInstance.pixelsPerUnit = pixelsPerUnit;
		explosionInstance.material = material;
		explosionInstance.Explode(velocity, sprite);
	}

	public void ExplodePartial(Transform target, Vector3 velocity, Sprite sprite, float percentage, Material material = null)
	{
		SpriteExplosion explosionInstance = Instantiate(explosionPrefab, target.position, target.rotation) as SpriteExplosion;
		explosionInstance.transform.parent = transform;
		explosionInstance.transform.localScale = target.localScale;
		explosionInstance.pixelsPerUnit = pixelsPerUnit;
		explosionInstance.material = material;
		explosionInstance.ExplodePartial(velocity, sprite, percentage);
	}
	#endregion
}
