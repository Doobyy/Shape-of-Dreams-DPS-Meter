using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_MiniBoss_IceAura_AoE : AbilityInstance
{
	public class Ad_IceAura
	{
		public float lastDamageTime;
	}

	public float interval;

	public float duration;

	public float range;

	public ScalingValue dmgFactor;

	public GameObject fxAoE;

	public GameObject fxHit;

	public GameObject fxTelegraph;

	[NonSerialized]
	public float delay;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			FxPlayNetworked(fxTelegraph, info.point, null);
			yield return new SI.WaitForSeconds(delay);
			info.caster.EntityEvent_OnDeath += new Action<EventInfoKill>(OnCasterDeath);
			FxPlayNetworked(fxAoE, info.point, null);
			yield return new SI.WaitForSeconds(duration);
			Destroy();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)info.caster != null)
			{
				info.caster.EntityEvent_OnDeath -= new Action<EventInfoKill>(OnCasterDeath);
			}
			FxStopNetworked(fxAoE);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		delay = 0f;
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || Time.time - creationTime < delay)
		{
			return;
		}
		List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, info.point, range, tvDefaultHarmfulEffectTargets);
		for (int i = 0; i < list.Count; i++)
		{
			Entity entity = list[i];
			if (entity.TryGetData<Ad_IceAura>(out var data))
			{
				if (Time.time - data.lastDamageTime < interval)
				{
					continue;
				}
				data.lastDamageTime = Time.time;
			}
			else
			{
				entity.AddData(new Ad_IceAura
				{
					lastDamageTime = Time.time
				});
			}
			CreateDamage(DamageData.SourceType.Default, dmgFactor).SetActor(info.caster).SetOriginPosition(info.point).SetElemental(ElementalType.Cold)
				.Dispatch(entity);
			FxPlayNewNetworked(fxHit, entity);
			if (entity.Status.TryGetStatusEffect<Se_MiniBoss_IceAura_Slow>(out var effect))
			{
				effect.ResetTimer();
			}
			else
			{
				CreateStatusEffect<Se_MiniBoss_IceAura_Slow>(entity, new CastInfo(info.caster, entity));
			}
		}
		handle.Return();
	}

	private void OnCasterDeath(EventInfoKill obj)
	{
		foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
		{
			if (allEntity.TryGetData<Ad_IceAura>(out var data))
			{
				allEntity.RemoveData(data);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
