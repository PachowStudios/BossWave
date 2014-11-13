using UnityEngine;
using System.Collections;

public class TargetPractice : Enemy
{
	new void Awake()
	{
		base.Awake();
	}

	void FixedUpdate()
	{
		InitialUpdate();
	}
}
