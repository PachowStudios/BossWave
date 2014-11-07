using UnityEngine;
using System.Collections;

public class ComboMeter : MonoBehaviour 
{
	private Animator anim;

	void Awake()
	{
		anim = GetComponent<Animator>();
	}

	void OnGUI()
	{
		anim.SetInteger("Combo", (int)PlayerControl.instance.combo);
	}
}
