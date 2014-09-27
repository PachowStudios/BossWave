using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PopupMessage : MonoBehaviour
{
	public float messageTime = 3f;
	public float characterTypeDelay = 0.05f;

	private Queue<string> messages = new Queue<string>();
	private int currentLetter = 0;
	private float characterTimer = 0f;
	private float messageTimer = 0f;

	private Image image;
	private Text text;
	private Animator anim;

	void Awake()
	{
		image = GetComponent<Image>();
		text = GetComponentInChildren<Text>();
		anim = GetComponent<Animator>();
	}

	void OnGUI()
	{
		Color textColor = text.color;
		textColor.a = image.color.a;
		text.color = textColor;

		if (anim.GetCurrentAnimatorStateInfo(0).IsName("Hide"))
		{
			text.text = "";

			if (messages.Count > 0)
			{
				anim.SetTrigger("Show");
			}
		}

		if (anim.GetCurrentAnimatorStateInfo(0).IsName("Show"))
		{
			if (messages.Count > 0 && text.text.Length < messages.Peek().Length)
			{
				characterTimer += Time.deltaTime;

				if (characterTimer >= characterTypeDelay)
				{
					text.text += messages.Peek()[currentLetter];
					currentLetter++;
					characterTimer = 0f;
				}
			}
			else
			{
				messageTimer += Time.deltaTime;

				if (messageTimer >= messageTime)
				{
					text.text = "";
					messages.Dequeue();
					messageTimer = 0f;
					currentLetter = 0;
				}
			}

			if (messages.Count <= 0)
			{
				anim.SetTrigger("Hide");
			}
		}
	}

	public void AddMessage(string message)
	{
		messages.Enqueue(message);
	}
}
