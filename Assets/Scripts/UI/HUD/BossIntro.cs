using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BossIntro : MonoBehaviour 
{
	private static BossIntro instance;

	public Image bossImage;
	public Text bossName;
	public Text playerName;
	public Text bossDescription;
	public Text playerDescription;
	public float textTypeInterval = 0.05f;

	public string defaultPlayerName;
	public string defaultPlayerDescription;

	private string newBossDescription;

	private Animator anim;

	public static BossIntro Instance
	{
		get { return instance; }
	}

	private void Awake()
	{
		instance = this;

		anim = GetComponent<Animator>();
	}

	public void Show(string newBossName, string newBossDescription, Sprite newBossSprite)
	{
		bossName.text = newBossName;
		bossDescription.text = "";
		this.newBossDescription = newBossDescription;
		bossImage.sprite = newBossSprite;

		playerName.text = defaultPlayerName;
		playerDescription.text = "";

		anim.SetTrigger("Show");
	}

	public void AnimateText()
	{
		StartCoroutine(bossDescription.Animate(newBossDescription, textTypeInterval));
		StartCoroutine(playerDescription.Animate(defaultPlayerDescription, textTypeInterval, true));
	}
}
