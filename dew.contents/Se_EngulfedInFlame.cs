using Mirror;
using UnityEngine;

public class Se_EngulfedInFlame : StatusEffect
{
	public float immolationInterval;

	public float immolationRadius;

	public GameObject fxImmolationHit;

	public ScalingValue immolationDamage;

	public float minDelay;

	private float _lastImmolationTime;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.dealtDamageProcessor.Add(VictimOndealtDamageProcessor, -2000);
			_lastImmolationTime = Time.time + minDelay;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !((Object)(object)victim == null))
		{
			victim.dealtDamageProcessor.Remove(VictimOndealtDamageProcessor);
			if (victim.Status.TryGetStatusEffect<Se_Elm_Fire>(out var effect))
			{
				effect.Destroy();
			}
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (victim.Status.fireStack <= 0)
		{
			Dew.GetClosestAliveHero(victim.position, fallbackToDead: true, victim).ApplyElemental(ElementalType.Fire, victim);
		}
		if (Time.time - _lastImmolationTime < immolationInterval || victim.isSleeping || victim.Visual.isSpawning)
		{
			return;
		}
		_lastImmolationTime = Time.time;
		ListReturnHandle<Entity> handle;
		foreach (Entity item in DewPhysics.OverlapCircleAllEntities(out handle, victim.position, immolationRadius, tvDefaultHarmfulEffectTargets))
		{
			if (!item.Control.isDisplacing)
			{
				Damage(immolationDamage, 0.66f).SetElemental(ElementalType.Fire).SetOriginPosition(victim.position).SetAttr(DamageAttribute.DamageOverTime)
					.Dispatch(item);
				FxPlayNewNetworked(fxImmolationHit, item);
			}
		}
		handle.Return();
	}

	private void VictimOndealtDamageProcessor(ref DamageData data, Actor actor, Entity target)
	{
		if (!(actor is ElementalStatusEffect) && !((Object)(object)victim == (Object)(object)target))
		{
			data.SetElemental(ElementalType.Fire);
		}
	}

	private void MirrorProcessed()
	{
	}
}
