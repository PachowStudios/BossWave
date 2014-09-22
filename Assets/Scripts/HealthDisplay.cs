using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthDisplay : MonoBehaviour
{
	public PlayerControl player;
	public Sprite healthFull;
	public Sprite health75;
	public Sprite health50;
	public Sprite health25;

	private Image face;
	private Image bar;

	private float healthPercent;

	void Awake()
	{
		face = transform.FindChild("Face").GetComponent<Image>();
		bar = transform.FindChild("Bar").GetComponent<Image>();
	}

	void OnGUI()
	{
		healthPercent = player.health / player.maxHealth;

		if (healthPercent > 0.75f)
		{
			face.sprite = healthFull;
		}
		else if (healthPercent > 0.5f)
		{
			face.sprite = health75;
		}
		else if (healthPercent > 0.25f)
		{
			face.sprite = health50;
		}
		else
		{
			face.sprite = health25;
		}

		bar.transform.localScale = new Vector3(healthPercent, 1, 1);
	}
}
