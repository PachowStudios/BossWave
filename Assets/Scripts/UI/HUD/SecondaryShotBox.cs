using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SecondaryShotBox : MonoBehaviour 
{
	public float cooldownDamping = 0.5f;
	public Gradient barGradient;

	private float cooldownPercent;
	private Vector3 cooldownVelocity = Vector3.zero;
	private bool show = false;

	private Animator anim;
	private Image bar;
	private Image icon;

	void Awake()
	{
		anim = GetComponent<Animator>();
		bar = transform.FindChild("Bar").GetComponent<Image>();
		icon = transform.FindChild("Icon").GetComponent<Image>();
	}

	void OnGUI()
	{
		if (!show && PlayerControl.instance.Gun.secondaryShot)
		{
			show = true;
			anim.SetTrigger("Show");
		}
		else if (show && !PlayerControl.instance.Gun.secondaryShot)
		{
			show = false;
			anim.SetTrigger("Hide");
		}

		if (show)
		{
			cooldownPercent = Mathf.Clamp(PlayerControl.instance.Gun.secondaryTimer / PlayerControl.instance.Gun.secondaryCooldown, 0f, 1f);
			icon.sprite = PlayerControl.instance.Gun.secondaryIcon;
		}
		else
		{
			cooldownPercent = 0f;
		}

		bar.transform.localScale = Vector3.SmoothDamp(bar.transform.localScale, new Vector3(cooldownPercent, 1f, 1f), ref cooldownVelocity, cooldownDamping);
		bar.color = barGradient.Evaluate(bar.transform.localScale.x);
	}
}
