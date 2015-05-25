using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Enemy))]
public class DeathShakeEffect : MonoBehaviour
{
	#region Fields
	public float duration = 0.5f;
	public Vector3 intensity = new Vector3(0f, 0.5f, 0f);

	private Enemy thisEnemy;
	#endregion

	#region MonoBehaviour
	private void Awake()
	{
		thisEnemy = GetComponent<Enemy>();
	}

	private void OnEnable()
	{
		if (thisEnemy != null)
			thisEnemy.OnDeath += DoShake;
	}

	private void OnDisable()
	{
		if (thisEnemy != null)
			thisEnemy.OnDeath -= DoShake;
	}
	#endregion

	#region Internal Helper Methods
	private void DoShake()
	{
		CameraShake.Instance.Shake(duration, intensity);
	}
	#endregion
}
