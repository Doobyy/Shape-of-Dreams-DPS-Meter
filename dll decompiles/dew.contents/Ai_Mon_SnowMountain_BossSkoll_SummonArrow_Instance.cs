using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_SnowMountain_BossSkoll_SummonArrow_Instance : AbilityInstance
{
	public class Ad_SkollArrow
	{
		public float time;
	}

	public float delay;

	public float duration;

	public float tickDmgRange;

	public float tickInterval;

	public Knockback knockback;

	public DewCollider explosionRange;

	public ScalingValue dmgFactor;

	public ScalingValue tickDmgFactor;

	public GameObject fxTelegraph;

	public GameObject fxInstance;

	public GameObject fxHit;

	public GameObject fxTickHit;

	private bool _tickDmgEnable;

	private static int _activeInstances;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			_activeInstances++;
			DestroyOnDeath(info.caster);
			FxPlayNetworked(fxTelegraph, info.point, Quaternion.identity);
			yield return new SI.WaitForSeconds(delay);
			FxPlayNetworked(fxInstance, info.point, Quaternion.Euler(0f, Random.Range(0, 360), 0f));
			float seconds = 0.15f;
			yield return new SI.WaitForSeconds(seconds);
			List<Entity> entities = explosionRange.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
			for (int i = 0; i < entities.Count; i++)
			{
				Entity entity = entities[i];
				CreateDamage(DamageData.SourceType.Default, dmgFactor).SetOriginPosition(info.point).SetElemental(ElementalType.Cold).Dispatch(entity);
				entity.Visual.KnockUp(KnockUpStrength.Big, isFriendly: false);
				knockback.ApplyWithOrigin(info.point, entity);
				FxPlayNewNetworked(fxHit, entity);
			}
			handle.Return();
			yield return null;
			_tickDmgEnable = true;
			yield return new SI.WaitForSeconds(duration);
			_tickDmgEnable = false;
			FxStopNetworked(fxInstance);
			Destroy();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_tickDmgEnable = false;
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		FxStopNetworked(fxInstance);
		_activeInstances = Mathf.Max(0, _activeInstances - 1);
		if (_activeInstances != 0 || !((Object)(object)NetworkedManagerBase<ActorManager>.instance != null))
		{
			return;
		}
		foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
		{
			if (!(allEntity is Monster) && allEntity.TryGetData<Ad_SkollArrow>(out var data))
			{
				allEntity.RemoveData(data);
			}
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || !_tickDmgEnable)
		{
			return;
		}
		ListReturnHandle<Entity> handle;
		foreach (Entity item in DewPhysics.OverlapCircleAllEntities(out handle, info.point, tickDmgRange, tvDefaultHarmfulEffectTargets))
		{
			if (item.TryGetData<Ad_SkollArrow>(out var data))
			{
				if (Time.time - data.time < tickInterval)
				{
					continue;
				}
				data.time = Time.time;
			}
			else
			{
				item.AddData(new Ad_SkollArrow
				{
					time = Time.time
				});
			}
			FxPlayNewNetworked(fxTickHit, item);
			CreateDamage(DamageData.SourceType.Default, tickDmgFactor).SetElemental(ElementalType.Cold).SetOriginPosition(info.point).SetAttr(DamageAttribute.DamageOverTime)
				.Dispatch(item);
		}
		handle.Return();
	}

	private void MirrorProcessed()
	{
	}
}
