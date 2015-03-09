using UnityEngine;
using System.Collections;

[RequireComponent(typeof(StandardEnemy))]
public abstract class AttackAI : MonoBehaviour 
{
	#region Fields
	protected StandardEnemy thisEnemy;
	protected Animator anim;
	#endregion

	#region MonoBehaviour
	protected virtual void Awake()
	{
		thisEnemy = GetComponent<StandardEnemy>();
		anim = GetComponent<Animator>();
	}
	#endregion

	#region Internal Update Methods
	public abstract void CheckAttack();
	#endregion

	#region Internal Helper Methods
	protected bool IsPlayerInRange(float min, float max)
	{
		int direction = thisEnemy.FacingRight ? 1 : -1;
		Vector3 startPoint = new Vector3(transform.position.x + (min * direction), collider2D.bounds.center.y, 0f);
		Vector3 endPoint = startPoint + new Vector3((max - min) * direction, 0f, 0f);
		RaycastHit2D linecast = Physics2D.Linecast(startPoint, endPoint, LayerMask.GetMask("Player"));

		return linecast.collider != null;
	}

	protected bool IsPlayerVisible(float range = Mathf.Infinity)
	{
		RaycastHit2D linecast = Physics2D.Linecast(collider2D.bounds.center,
												   PlayerControl.Instance.collider2D.bounds.center,
												   LayerMask.GetMask("Collider"));

		return linecast.collider == null && linecast.distance <= range;
	}
	#endregion
}
