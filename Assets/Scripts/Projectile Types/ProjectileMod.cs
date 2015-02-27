using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Projectile))]
public abstract class ProjectileMod : MonoBehaviour
{
	#region Fields
	public int priority = 0;

	protected Projectile thisProjectile;
	#endregion

	#region MonoBehaviour
	protected virtual void Awake()
	{
		thisProjectile = GetComponent<Projectile>();
	}
	#endregion

	#region Public Methods
	public abstract void ApplyModifier();
	#endregion
}
