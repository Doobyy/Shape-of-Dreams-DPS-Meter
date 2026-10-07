using System;
using Mirror;
using UnityEngine;

public class Ai_Star_Cetus_F_BI_SelfDamageAoeCold_AOE : TickDamageInstance
{
	public float selfTickDamageMaxHpRatio = 0.015f;

	public float dealtTickDamageRatio = 0.1f;

	[NonSerialized]
	public Ai_Q_BigBorealChunk_Spawner spawner;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		spawner = null;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			dmgFactor = new ScalingValue
			{
				baseValue = spawner.GetValue(DewResources.GetByType<Ai_Q_BigBorealChunk_Explosion>(default(ResourceLoadSettings)).dmgFactor) * dealtTickDamageRatio
			};
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer)
		{
			((Component)(object)this).transform.position = info.caster.position;
		}
	}

	protected override void OnHit(Entity entity)
	{
		if (info.caster.CheckEnemyOrNeutral(entity))
		{
			base.OnHit(entity);
		}
		else if (!((UnityEngine.Object)(object)entity != (UnityEngine.Object)(object)info.caster))
		{
			PureDamage(selfTickDamageMaxHpRatio * info.caster.maxHealth).SetElemental(ElementalType.Cold).Dispatch(entity);
			if ((bool)hitEffect)
			{
				FxPlayNewNetworked(hitEffect, entity, info.caster.position, null);
			}
		}
	}

	protected override void OnBeforeDispatchDamage(ref DamageData dmg, Entity target)
	{
		base.OnBeforeDispatchDamage(ref dmg, target);
		dmg.ApplyAmplification(spawner.GetCurrentAmp());
	}

	private void MirrorProcessed()
	{
	}
}
