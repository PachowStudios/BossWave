using UnityEngine;
using System.Collections;

public class MeleeAI : FollowAI
{
	protected override void Attack(string triggerName)
	{
		base.Attack(triggerName);

		PlayerControl.instance.TakeDamage(gameObject);
	}
}
