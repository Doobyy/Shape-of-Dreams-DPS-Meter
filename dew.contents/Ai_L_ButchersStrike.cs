using System;
using Mirror;
using UnityEngine;

public class Ai_L_ButchersStrike : InstantDamageInstance
{
	public float slowDuration = 2f;

	public float slowAmount = 30f;

	[NonSerialized]
	private ScalingValue _baseDmgFactor;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_baseDmgFactor = dmgFactor;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		dmgFactor = _baseDmgFactor;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			ResetCooldown(info.caster.Ability.attackAbility);
		}
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		AbilityTrigger ft = firstTrigger;
		Se_L_ButchersStrike_KillTimer se_L_ButchersStrike_KillTimer = entity.Status.FindStatusEffect((Se_L_ButchersStrike_KillTimer se) => (UnityEngine.Object)(object)se.firstTrigger == (UnityEngine.Object)(object)ft);
		if ((UnityEngine.Object)(object)se_L_ButchersStrike_KillTimer != null)
		{
			se_L_ButchersStrike_KillTimer.ResetTimer();
		}
		else if (entity.isDead)
		{
			CreateAbilityInstance<Ai_L_BloodStrike_Meat>(entity.agentPosition, null, new CastInfo(info.caster));
		}
		else
		{
			CreateStatusEffect<Se_L_ButchersStrike_KillTimer>(entity);
		}
		CreateBasicEffect(entity, new SlowEffect
		{
			strength = slowAmount
		}, slowDuration, "ButchersStrikeSlow", DuplicateEffectBehavior.UsePrevious);
	}

	private void MirrorProcessed()
	{
	}
}
