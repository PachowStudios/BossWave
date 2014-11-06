using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class HealthDisplay : MonoBehaviour
{
	public int scoreDigits = 12;
	public Sprite healthFull;
	public Sprite health75;
	public Sprite health50;
	public Sprite health25;

	private float healthPercent;
	private Vector3 healthVelocity = Vector3.zero;

	private Image face;
	private Image bar;
	private Text score;

	private PlayerControl player;

	void Awake()
	{
		face = transform.FindChild("Face").GetComponent<Image>();
		bar = transform.FindChild("Bar").GetComponent<Image>();
		score = transform.FindChild("Score").GetComponent<Text>();

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

		bar.transform.localScale = Vector3.SmoothDamp(bar.transform.localScale, new Vector3(healthPercent, 1, 1), ref healthVelocity, 0.5f);

		score.text = player.score.ToString().PadLeft(scoreDigits, '0');
	}
}
