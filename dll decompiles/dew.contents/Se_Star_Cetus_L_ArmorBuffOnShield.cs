using System;
using Mirror;
using UnityEngine;

public class Se_Star_Cetus_L_ArmorBuffOnShield : StarEffect
{
	public float damagePenaltyReduction = 0.05f;

	public StarScalingValue armorAmount;

	public override Type heroType => typeof(Hero_Cetus);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.dealtDamageProcessor.Add(Process);
			hero.ActorEvent_OnGiveShield += new Action<EventInfoShield>(ActorEventOnGiveShield);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (bool)(UnityEngine.Object)(object)hero)
		{
			hero.dealtDamageProcessor.Remove(Process);
			hero.ActorEvent_OnGiveShield -= new Action<EventInfoShield>(ActorEventOnGiveShield);
		}
	}

	private void Process(ref DamageData data, Actor from, Entity to)
	{
		if (!data.IsAmountModifiedBy(this))
		{
			data.ApplyReduction(damagePenaltyReduction);
			data.SetAmountModifiedBy(this);
		}
	}

	private void ActorEventOnGiveShield(EventInfoShield obj)
	{
		Se_Star_Cetus_L_ArmorBuffOnShield_Buff se_Star_Cetus_L_ArmorBuffOnShield_Buff = obj.target.Status.FindStatusEffect((Se_Star_Cetus_L_ArmorBuffOnShield_Buff se) => (UnityEngine.Object)(object)se.info.caster == (UnityEngine.Object)(object)hero);
		if (!se_Star_Cetus_L_ArmorBuffOnShield_Buff.IsNullOrInactive())
		{
			se_Star_Cetus_L_ArmorBuffOnShield_Buff.ResetTimer();
			return;
		}
		CreateStatusEffect(obj.target, new CastInfo(hero), (Se_Star_Cetus_L_ArmorBuffOnShield_Buff se) =>
		{
			se.armorAmount = GetValueInt(armorAmount);
		});
	}

	private void MirrorProcessed()
	{
	}
}
