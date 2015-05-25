using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class SpriteEffect : MonoBehaviour
{
	#region Types
	[Serializable]
	public struct Effect
	{
		public string name;
		public Animator prefab;
		public int variations;
	}
	#endregion

	#region Fields
	private static SpriteEffect instance;

	public List<Effect> effectsLibrary;

	private Dictionary<string, Effect> effects = new Dictionary<string, Effect>(StringComparer.OrdinalIgnoreCase);
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
			effects[effect.name] = effect;
	}
	#endregion

	#region Internal Helper Methods
	private IEnumerator DoSpawnEffect(string requestedName, Vector3 targetPosition, Transform parent = null, float? delay = null)
	{
		yield return new WaitForSeconds(delay ?? 0f);

		Effect currentEffect;

		if (effects.TryGetValue(requestedName, out currentEffect))
		{
			var effectInstance = Instantiate(currentEffect.prefab, targetPosition, Quaternion.identity) as Animator;
			effectInstance.transform.parent = parent ?? transform;

			if (currentEffect.variations > 1)
				effectInstance.SetTrigger(UnityEngine.Random.Range(1, currentEffect.variations + 1).ToString());
		}
		else
			Debug.Log("No effect with the name " + requestedName + " exists!");
	}
	#endregion

	#region Public Methods
	public void SpawnEffect(string requestedName, Vector3 targetPosition, Transform parent = null, float? delay = null)
	{
		StartCoroutine(DoSpawnEffect(requestedName, targetPosition, parent, delay));
	}
	#endregion
}
