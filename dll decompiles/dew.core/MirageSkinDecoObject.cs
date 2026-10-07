using System.Collections.Generic;
using UnityEngine;

public class MirageSkinDecoObject : MonoBehaviour
{
	public AssetRef<MirageSkinEffect> targetEffect;

	private void Awake()
	{
		MirageSkinEffect lightAsset = targetEffect.lightAsset;
		if ((Object)(object)lightAsset == null)
		{
			return;
		}
		EntityVisual componentInParent = GetComponentInParent<EntityVisual>(includeInactive: true);
		if ((Object)(object)componentInParent == null)
		{
			gameObject.SetActive(value: false);
			return;
		}
		string key = ((object)lightAsset).GetType().Name;
		if (!componentInParent.mirageSkinObjects.TryGetValue(key, out var value))
		{
			value = new List<GameObject>(1);
			componentInParent.mirageSkinObjects.Add(key, value);
		}
		value.Add(gameObject);
		gameObject.SetActive(value: false);
	}
}
