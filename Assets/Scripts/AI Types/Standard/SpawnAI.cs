using UnityEngine;
using System.Collections;

[RequireComponent(typeof(StandardEnemy))]
public abstract class SpawnAI : MonoBehaviour
{
	#region Fields
	protected StandardEnemy thisEnemy;
	protected Animator anim;
	#endregion

	#region Initialization Methods
	public virtual void Initialize(StandardEnemy thisEnemy, Animator anim)
	{
		this.thisEnemy = thisEnemy;
		this.anim = anim;
	}
	#endregion
}
