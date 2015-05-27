using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BasicProjectile : Projectile
{
	#region Fields
	public bool useModifiers = false;

	private ProjectileMod[] modifiers;
	#endregion

	#region MonoBehaviour
	protected override void Awake()
	{
		base.Awake();

		if (useModifiers)
		{
			modifiers = GetComponents<ProjectileMod>().OrderBy(p => p.priority).ToArray();
			useModifiers = modifiers.Length > 0;
		}
	}

	private void Update()
	{
		if (useModifiers)
			foreach (var modifier in modifiers)
				modifier.ApplyModifier();
	}

	private void LateUpdate()
	{
		DoMovement();
	}
	#endregion
}
