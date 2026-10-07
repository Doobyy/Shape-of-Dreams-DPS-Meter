using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class EntityModel : MonoBehaviour
{
	public Renderer[] bodyRenderers;

	public Transform healthBarPosition;

	[FormerlySerializedAs("conversationPosition")]
	public Transform conversationPivotPosition;

	public bool hasGoldDissolve;

	public EntityVisual.EntityDeathBehavior deathBehavior = EntityVisual.EntityDeathBehavior.Dissolve;

	public float dissolveDelay = 0.2f;

	public float dissolveDuration = 1f;

	public GameObject fxLoop;

	public GameObject fxDeath;

	public GameObject fxTakeDamage;

	public AnimationClipWithSpeed idle = AnimationClipWithSpeed.Default;

	public EntityAnimation.LocomotionType locomotion;

	public float walkAnimationSpeed = 1f;

	public AnimationClip runForwardClip;

	public AnimationClip runForwardRightClip;

	public AnimationClip runRightClip;

	public AnimationClip runBackwardRightClip;

	public AnimationClip runBackwardClip;

	public AnimationClip runBackwardLeftClip;

	public AnimationClip runLeftClip;

	public AnimationClip runForwardLeftClip;

	public AnimationClipWithSpeed stagger = AnimationClipWithSpeed.Default;

	public AnimationClipWithSpeed death = AnimationClipWithSpeed.Default;

	public List<AbilityAnimationReplacementPair> abilityAnimationReplacements;

	public AnimationClipWithSpeed lobby = AnimationClipWithSpeed.Default;

	public Transform weapon;

	public Transform holsteredWeapon;

	public List<EntityModelCustomMapping> customMappings = new List<EntityModelCustomMapping>();

	private Dictionary<(string, Type), object> _customMappingsCached = new Dictionary<(string, Type), object>();

	public bool isInitialized { get; internal set; }

	public bool support4Directions => locomotion != EntityAnimation.LocomotionType.Simple;

	public bool support8Directions => locomotion == EntityAnimation.LocomotionType.EightDirections;

	public bool TryGetCustomMapping<T>(string key, out T target) where T : class
	{
		target = GetCustomMapping<T>(key);
		return target != null;
	}

	public bool ContainsCustomMappingKey(string key)
	{
		return GetCustomMapping<GameObject>(key) != null;
	}

	public T GetCustomMapping<T>(string key) where T : class
	{
		(string, Type) key2 = (key, typeof(T));
		if (_customMappingsCached.TryGetValue(key2, out var value))
		{
			return value as T;
		}
		GameObject gameObject = null;
		for (int i = 0; i < customMappings.Count; i++)
		{
			if (customMappings[i].id == key)
			{
				gameObject = customMappings[i].target;
				break;
			}
		}
		T component;
		if (gameObject == null)
		{
			component = null;
		}
		else if (typeof(T) == typeof(GameObject))
		{
			component = gameObject as T;
		}
		else
		{
			gameObject.TryGetComponent<T>(out component);
		}
		_customMappingsCached[key2] = component;
		return component;
	}

	public T[] GetCustomMappingArray<T>(string key) where T : class
	{
		List<T> list = DewPool.GetList(out ListReturnHandle<T> handle);
		int num = 0;
		T target;
		while (TryGetCustomMapping<T>(key + num, out target))
		{
			list.Add(target);
			num++;
		}
		T[] result = list.ToArray();
		handle.Return();
		return result;
	}
}
