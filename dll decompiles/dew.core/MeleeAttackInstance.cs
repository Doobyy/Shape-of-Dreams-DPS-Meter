using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class MeleeAttackInstance : AbilityInstance
{
	public DewCollider range;

	public bool scaleRangeWithTriggerRange = true;

	public float mainTargetAttackEffect = 1f;

	public float mainTargetDamage = 1f;

	public float subTargetAttackEffect = 0.5f;

	public float subTargetDamage = 0.5f;

	public GameObject fxHitMain;

	public GameObject fxHitSub;

	[NonSerialized]
	public bool isCrit;

	[NonSerialized]
	public int hitCount;

	[NonSerialized]
	private Vector3 _baseRangeScale = Vector3.one;

	[NonSerialized]
	private Vector3 _baseStartEffectScale = Vector3.one;

	[NonSerialized]
	private Vector3 _baseStartEffectNoStopScale = Vector3.one;

	[NonSerialized]
	private Vector3 _baseEndEffectScale = Vector3.one;

	public bool didHit => hitCount > 0;

	protected override void Awake()
	{
		base.Awake();
		if (range != null)
		{
			_baseRangeScale = range.transform.localScale;
		}
		if (startEffect != null)
		{
			_baseStartEffectScale = startEffect.transform.localScale;
		}
		if (startEffectNoStop != null)
		{
			_baseStartEffectNoStopScale = startEffectNoStop.transform.localScale;
		}
		if (endEffect != null)
		{
			_baseEndEffectScale = endEffect.transform.localScale;
		}
	}

	protected override void OnCreate()
	{
		Entity target = info.target;
		if ((UnityEngine.Object)(object)target == null)
		{
			((Component)(object)this).transform.rotation = info.rotation;
		}
		else
		{
			Vector3 forward = target.agentPosition - info.caster.agentPosition;
			if (forward.sqrMagnitude > 0.1f)
			{
				((Component)(object)this).transform.rotation = Quaternion.LookRotation(forward);
			}
		}
		AbilityTrigger abilityTrigger = firstTrigger;
		if (startEffect != null)
		{
			startEffect.transform.localScale = _baseStartEffectScale * abilityTrigger.ProcessRange(1f);
		}
		if (startEffectNoStop != null)
		{
			startEffectNoStop.transform.localScale = _baseStartEffectNoStopScale * abilityTrigger.ProcessRange(1f);
		}
		if (endEffect != null)
		{
			endEffect.transform.localScale = _baseEndEffectScale * abilityTrigger.ProcessRange(1f);
		}
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (scaleRangeWithTriggerRange)
		{
			range.transform.localScale = _baseRangeScale * abilityTrigger.configs[0].effectiveRange;
		}
		List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
		{
			sortComparer = CollisionCheckSettings.DistanceFromCenter
		});
		Entity entity = info.target;
		if (entity == null && entities.Count > 0)
		{
			entity = entities[0];
		}
		if ((UnityEngine.Object)(object)entity != null)
		{
			hitCount++;
		}
		foreach (Entity item in entities)
		{
			if (!((UnityEngine.Object)(object)item == (UnityEngine.Object)(object)entity))
			{
				hitCount++;
			}
		}
		if ((UnityEngine.Object)(object)entity != null)
		{
			try
			{
				OnHit(entity, isMain: true);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		foreach (Entity item2 in entities)
		{
			if (!((UnityEngine.Object)(object)item2 == (UnityEngine.Object)(object)entity))
			{
				try
				{
					OnHit(item2, isMain: false);
				}
				catch (Exception exception2)
				{
					Debug.LogException(exception2);
				}
			}
		}
		handle.Return();
		Destroy();
	}

	public virtual void OnHit(Entity entity, bool isMain)
	{
		if (isMain)
		{
			FxPlayNetworked(fxHitMain, entity);
			DoBasicAttackHit(info.caster, entity, isCrit, isMain: true, mainTargetDamage, mainTargetAttackEffect, delegate(ref DamageData dmg)
			{
				OnBeforeDispatchDamage(ref dmg, entity);
			});
		}
		else
		{
			FxPlayNetworked(fxHitSub, entity);
			DoBasicAttackHit(info.caster, entity, isCrit, isMain: false, subTargetDamage, subTargetAttackEffect, delegate(ref DamageData dmg)
			{
				OnBeforeDispatchDamage(ref dmg, entity);
			});
		}
	}

	public virtual void OnBeforeDispatchDamage(ref DamageData dmg, Entity target)
	{
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		hitCount = 0;
	}

	private void MirrorProcessed()
	{
	}
}
