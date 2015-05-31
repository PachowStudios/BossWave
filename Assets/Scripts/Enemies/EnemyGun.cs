using UnityEngine;
using System.Collections;

public class EnemyGun : MonoBehaviour
{
	#region Fields
	public Projectile projectile;
	public Vector2 shotDirection;
	public bool aimAtPlayer = false;
	public string shootEffect;
	#endregion

	#region Public Methods
	public void Fire()
	{
		Vector2 currentShotDirection;

		if (aimAtPlayer)
		{
			transform.LookAt2D(PlayerControl.Instance.collider2D.bounds.center, true);
			currentShotDirection = transform.localRotation * Vector3.right;
		}
		else
		{
			currentShotDirection = transform.lossyScale.Dot(shotDirection);
		}

		if (!shootEffect.IsNullOrEmpty())
			SpriteEffect.Instance.SpawnEffect(shootEffect, transform.position, parent: transform);

		Projectile projectileInstance = Instantiate(projectile, transform.position, Quaternion.identity) as Projectile;
		projectileInstance.Initialize(currentShotDirection);
	}
	#endregion
}
