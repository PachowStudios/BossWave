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

	public List<Effect> allEffects;

	private static Dictionary<string, SpriteRenderer> effects = new Dictionary<string, SpriteRenderer>();

	void Awake()
	{
		instance = this;

		foreach (Effect effect in allEffects)
		{
			effects.Add(effect.name.ToLower(), effect.prefab);
		}
	}

	public static void SpawnEffect(string requestedName, Vector3 targetPosition, Transform parent = null)
	{
		requestedName = requestedName.ToLower();

		if (effects.ContainsKey(requestedName))
		{
			SpriteRenderer currentEffect = Instantiate(effects[requestedName], targetPosition, Quaternion.identity) as SpriteRenderer;
			currentEffect.transform.parent = (parent == null) ? instance.transform : parent;
		}
		else
		{
			Debug.Log("No effect with the name " + requestedName + " exists!");
		}
	}
}
