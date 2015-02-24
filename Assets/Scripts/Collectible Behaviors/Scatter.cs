using UnityEngine;
using System.Collections;
using DG.Tweening;

public class Scatter : MonoBehaviour
{
	#region Fields
	public Vector2 minScatter = new Vector2(5f, 10f);
	public Vector2 maxScatter = new Vector2(5f, 10f);
	public float minShake = 10f;
	public float maxShake = 100f;
	public float shakeTime = 1f;

	private Vector2 scatterVelocity;
	private float shakeAmount = 0f;
	#endregion

	#region MonoBehaviour
	private void Awake()
	{
		scatterVelocity = new Vector2(Random.Range(minScatter.x, maxScatter.x),
								   Random.Range(minScatter.y, maxScatter.y));
		shakeAmount = Extensions.ConvertRange(scatterVelocity.x, minScatter.x, maxScatter.x, minShake, maxShake);
		scatterVelocity.x *= Extensions.RandomSign();
		shakeAmount *= Mathf.Sign(-scatterVelocity.x);
	}
	#endregion

	#region Public Methods
	public void DoScatter()
	{
		rigidbody2D.AddForce(scatterVelocity, ForceMode2D.Impulse);
		transform.DOPunchRotation(new Vector3(0f, 0f, shakeAmount), shakeTime, 0, 0f);
	}
	#endregion
}
