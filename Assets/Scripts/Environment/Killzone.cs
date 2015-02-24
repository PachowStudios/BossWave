using UnityEngine;
using System.Collections;

public class Killzone : MonoBehaviour
{
	#region Fields
	public float damage = 50f;

	private Transform respawnPoint;
	#endregion

	#region MonoBehaviour
	private void Awake()
	{
		respawnPoint = transform.FindChild("Respawn Point");
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.tag == "Player")
		{
			if (PlayerControl.Instance.Health > 0f)
			{
				if (!LevelManager.Instance.bossWavePlayerMoved)
				{
					PlayerControl.Instance.TakeDamage(gameObject, damage);
					PlayerControl.Instance.transform.position = respawnPoint.position;
				}
				else
				{
					PlayerControl.Instance.Health = 0f;
				}
			}
		}
		else if (other.tag == "Enemy")
		{
			other.GetComponent<Enemy>().Kill();
		}
	}

	private void OnTriggerStay2D(Collider2D other)
	{
		OnTriggerEnter2D(other);
	}
	#endregion
}
