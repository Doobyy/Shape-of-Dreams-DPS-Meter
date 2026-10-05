using Mirror;
using UnityEngine;

public class Se_Gem_R_Hedgehog_Aura : StatusEffect
{
	public float empoweredAmp = 0.5f;

	public float tickInterval;

	public float tickCount;

	public float radius;

	public float procCoefficient;

	public ScalingValue dmgFactor;

	public GameObject fxHit;

	private Gem_R_Hedgehog _hedgehog;

	private float _timer;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			_timer = float.NegativeInfinity;
			_hedgehog = gem as Gem_R_Hedgehog;
			SetTimer(tickCount * tickInterval);
			ShowOnScreenTimer("Gem_R_Hedgehog");
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || Time.time - _timer < tickInterval)
		{
			return;
		}
		_timer = Time.time;
		ListReturnHandle<Entity> handle;
		foreach (Entity item in DewPhysics.OverlapCircleAllEntities(out handle, victim.agentPosition, radius, tvDefaultHarmfulEffectTargets))
		{
			DamageData damageData = CreateDamage(DamageData.SourceType.Magic, dmgFactor, procCoefficient);
			if ((Object)(object)_hedgehog != null && _hedgehog.IsEmpowered() && !damageData.IsAmountModifiedBy(this))
			{
				damageData.SetAttr(DamageAttribute.IsCrit);
				damageData.ApplyAmplification(empoweredAmp);
				damageData.SetAmountModifiedBy(this);
			}
			damageData.SetOriginPosition(victim.agentPosition);
			damageData.Dispatch(item);
			FxPlayNewNetworked(fxHit, item);
		}
		handle.Return();
	}

	private void MirrorProcessed()
	{
	}
}
