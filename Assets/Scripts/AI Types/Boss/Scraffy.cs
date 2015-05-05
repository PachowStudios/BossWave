using UnityEngine;
using System.Collections;

public sealed class Scraffy : Boss
{
	#region Fields

	#endregion

	#region Internal Helper Methods
	protected override void CheckDeath(bool showDrops = true)
	{
		
	}
	#endregion

	#region Public Methods
	public override void Spawn()
	{
		if (spawned)
			return;
	}

	public override void End()
	{

	}
	#endregion
}
