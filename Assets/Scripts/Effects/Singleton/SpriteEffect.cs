using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class SpriteEffect : MonoBehaviour
{
	#region Fields
	[System.Serializable]
	public struct Effect
	{
		public string name;
		public SpriteRenderer prefab;
	}

	private static SpriteEffect instance;

	public List<Effect> effectsLibrary;

	private Dictionary<string, SpriteRenderer> effects = new Dictionary<string, SpriteRenderer>(StringComparer.OrdinalIgnoreCase);
	#endregion

	#region Public Properties
	public static SpriteEffect Instance
	{ get { return instance; } }
	#endregion

	#region MonoBehaviour
	private void Awake()
	{
		instance = this;

		foreach (Effect effect in effectsLibrary)
			effects[effect.name] = effect.prefab;
	}
	#endregion

	#region Public Methods
	public void SpawnEffect(string requestedName, Vector3 targetPosition, Transform parent = null)
	{
		requestedName = requestedName.ToLower();

		if (effects.ContainsKey(requestedName))
		{
			SpriteRenderer currentEffect = Instantiate(effects[requestedName], targetPosition, Quaternion.identity) as SpriteRenderer;
			currentEffect.transform.parent = parent ?? transform;
		}
		else
			Debug.Log("No effect with the name " + requestedName + " exists!");
	}
	#endregion
}
