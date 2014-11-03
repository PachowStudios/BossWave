using UnityEngine;
using System.Collections;

public class ComboMeter : MonoBehaviour 
{
	private Animator anim;
	private PlayerControl player;

	void Awake()
	{
		anim = GetComponent<Animator>();
		player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();
	}

	void OnGUI()
	{
		anim.SetInteger("Combo", (int)player.combo);
	}
}
