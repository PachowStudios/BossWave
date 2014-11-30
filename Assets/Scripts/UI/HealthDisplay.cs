using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class HealthDisplay : MonoBehaviour
{
	public int scoreDigits = 12;
	public int microchipsDigits = 5;
	public float textDamping = 1f;
	public float healthDamping = 1f;
	public Sprite healthFull;
	public Sprite health75;
	public Sprite health50;
	public Sprite health25;

	private float healthPercent;
	private Vector3 healthVelocity = Vector3.zero;

	private float scoreValue = 0f;
	private float scoreVelocity = 0f;

	private float microchipsValue = 0f;
	private float microchipsVelocity = 0f;

	private Image face;
	private Image bar;
	private Text score;
	private Text microchips;

	void Awake()
	{
		face = transform.FindChild("Face").GetComponent<Image>();
		bar = transform.FindChild("Bar").GetComponent<Image>();
		score = transform.FindChild("Score").GetComponent<Text>();
		microchips = transform.FindChild("Microchips").GetComponent<Text>();
	}

	void OnGUI()
	{
		healthPercent = Mathf.Clamp(PlayerControl.instance.Health / PlayerControl.instance.maxHealth, 0f, 1f);

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

		bar.transform.localScale = Vector3.SmoothDamp(bar.transform.localScale, new Vector3(healthPercent, 1f, 1f), ref healthVelocity, healthDamping);
		scoreValue = Mathf.SmoothDamp(scoreValue, PlayerControl.instance.score, ref scoreVelocity, textDamping);
		microchipsValue = Mathf.SmoothDamp(microchipsValue, PlayerControl.instance.microchips, ref microchipsVelocity, textDamping);

		score.text = Mathf.RoundToInt(scoreValue).ToString().PadLeft(scoreDigits, '0');
		microchips.text = Mathf.RoundToInt(microchipsValue).ToString().PadLeft(microchipsDigits, '0');
	}
}
