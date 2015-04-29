using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public sealed class ElevatorHealthMeter : MonoBehaviour
{
	#region Fields
	private static ElevatorHealthMeter instance;

	public Mask healthMask;
	public float healthDamping = 0.5f;

	private float originalHealthHeight;
	private Vector2 healthVelocity = Vector2.zero;
	#endregion

	#region Public Properties
	public static ElevatorHealthMeter Instance
	{ get { return instance; } }
	#endregion

	#region MonoBehaviour
	private void Awake()
	{
		instance = this;

		originalHealthHeight = healthMask.rectTransform.sizeDelta.y;
	}

	private void Update()
	{
		healthMask.rectTransform.sizeDelta = Vector2.SmoothDamp(healthMask.rectTransform.sizeDelta,
																new Vector2(healthMask.rectTransform.sizeDelta.x,
																			originalHealthHeight * Level2.Instance.elevator.HealthPercent),
																ref healthVelocity,
																healthDamping);
	}
	#endregion
}
