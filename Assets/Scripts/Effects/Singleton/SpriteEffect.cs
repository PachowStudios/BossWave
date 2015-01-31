using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class SpriteEffect : MonoBehaviour 
{
	[System.Serializable]
	public struct Effect
	{
		public string name;
		public SpriteRenderer prefab;
	}

	private static SpriteEffect instance;

	public List<Effect> effectsLibrary;

	private Dictionary<string, SpriteRenderer> effects = new Dictionary<string, SpriteRenderer>();

	public static SpriteEffect Instance
	{
		get { return instance; }
	}

	private void Awake()
	{
		instance = this;

		foreach (Effect effect in effectsLibrary)
		{
			if (effects.ContainsKey(effect.name.ToLower()))
			{
				effects.Remove(effect.name.ToLower());
			}

			effects.Add(effect.name.ToLower(), effect.prefab);
		}
	}

	public void SpawnEffect(string requestedName, Vector3 targetPosition, Transform parent = null)
	{
		requestedName = requestedName.ToLower();

		if (effects.ContainsKey(requestedName))
		{
			SpriteRenderer currentEffect = Instantiate(effects[requestedName], targetPosition, Quaternion.identity) as SpriteRenderer;
			currentEffect.transform.parent = (parent == null) ? transform : parent;
		}
		else
		{
			Debug.Log("No effect with the name " + requestedName + " exists!");
		}
	}
}
