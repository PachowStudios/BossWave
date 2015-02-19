using UnityEngine;
using System.Collections;

public abstract class Boss : Enemy
{
	#region Public Methods
	public abstract void Spawn();
	public abstract void End();
	#endregion
}
