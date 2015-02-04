using UnityEngine;
using System.Collections;

public abstract class Boss : Enemy
{
	public abstract void Spawn();
	public abstract void End();
}
