using UnityEngine;
using System.Collections;

public class Scatter : MonoBehaviour 
{
	public Vector2 minScatter = new Vector2(5f, 10f);
	public Vector2 maxScatter = new Vector2(5f, 10f);
	private Vector2 scatterSpeed;
	public float minShake = 10f;
	public float maxShake = 100f;
	private float shakeAmount = 0f;
	public float shakeTime = 1f;

	void Awake()
	{
		scatterSpeed = new Vector2(Random.Range(minScatter.x, maxScatter.x),
								   Random.Range(minScatter.y, maxScatter.y));
		shakeAmount = Extensions.ConvertRange(scatterSpeed.x, minScatter.x, maxScatter.x, minShake, maxShake);
		scatterSpeed.x *= Extensions.RandomSign();
		shakeAmount *= Mathf.Sign(scatterSpeed.x);
	}

	public void DoScatter()
	{
		rigidbody2D.AddForce(new Vector2(scatterSpeed.x, scatterSpeed.y), ForceMode2D.Impulse);
		gameObject.PunchRotation(new Vector3(0f, 0f, shakeAmount), shakeTime, 0f);
	}
}
