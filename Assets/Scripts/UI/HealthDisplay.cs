using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthDisplay : MonoBehaviour
{
	public Sprite healthFull;
	public Sprite health75;
	public Sprite health50;
	public Sprite health25;

	private float healthPercent;

	private Image face;
	private Image bar;

	private PlayerControl player;

	void Awake()
	{
		face = transform.FindChild("Face").GetComponent<Image>();
		bar = transform.FindChild("Bar").GetComponent<Image>();

		player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();
	}

	void OnGUI()
	{
		healthPercent = Mathf.Clamp(player.health / player.maxHealth, 0f, 1f);

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
