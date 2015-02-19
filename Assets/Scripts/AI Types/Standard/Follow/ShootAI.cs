using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public sealed class ShootAI : FollowAI
{
	#region Fields
	public float maxRangeOffset = 1f;
	public bool horizontalShot = true;
	public bool useRandomGun = false;
	public bool burstShot = false;
	public int shotsPerBurst = 1;
	public float delayBetweenShots = 0.1f;
	public float attackBuffer = 0f;
	public Projectile projectile;

	private List<Transform> guns;
	private int currentGun = 0;
	#endregion

	#region MonoBehaviour
	protected override void Awake()
	{
		base.Awake();

		guns = transform.FindChild("FirePoints").FindChildTransforms();

		float rangeOffset = Random.Range(-maxRangeOffset, maxRangeOffset);
		followRange += rangeOffset;
		attackRange += rangeOffset;
	}
	#endregion

	#region Internal Helper Methods
	protected override void Attack()
	{
		base.Attack();

		if (useRandomGun)
		{
			guns.Shuffle();
		}

		if (burstShot)
		{
			for (int i = 1; i <= shotsPerBurst; i++)
			{
				StartCoroutine(Fire(delayBetweenShots * i + attackBuffer));
			}

			if (attackBuffer != 0f)
			{
				DOTween.Sequence()
					.AppendInterval(delayBetweenShots * shotsPerBurst + attackBuffer)
					.AppendCallback(() => 
					{
						if (this != null)
						{
							anim.SetTrigger("End Attack");
						}
					});
			}
		}
		else
		{
			StartCoroutine(Fire(attackBuffer));
		}
	}

	private IEnumerator Fire(float delay = 0f)
	{
		if (delay > 0f)
		{
			yield return new WaitForSeconds(delay);
		}

		guns[currentGun].LookAt2D(PlayerControl.Instance.collider2D.bounds.center, true);

		Vector3 shotDirection = Vector3.zero;

		if (horizontalShot)
		{
			if (FacingRight)
			{
				shotDirection = new Vector3(1f, 0f, 0f);
			}
			else
			{
				shotDirection = new Vector3(-1f, 0f, 0f);
			}
		}
		else
		{
			shotDirection = guns[currentGun].localRotation * Vector3.right;
		}

		Projectile projectileInstance = Instantiate(projectile, guns[currentGun].position, Quaternion.identity) as Projectile;
		projectileInstance.Initialize(shotDirection);

		currentGun = (currentGun + 1 == guns.Count) ? 0 : currentGun + 1;
		attackCooldownTimer = 0f;

		yield return null;
	}
	#endregion
}
