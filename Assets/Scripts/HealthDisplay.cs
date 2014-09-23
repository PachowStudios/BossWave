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
	private Text score;

	private float healthPercent;

	void Awake()
	{
		face = transform.FindChild("Face").GetComponent<Image>();
		bar = transform.FindChild("Bar").GetComponent<Image>();
		score = transform.FindChild("Score").GetComponent<Text>();
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

		score.text = player.score.ToString();
	}
}
