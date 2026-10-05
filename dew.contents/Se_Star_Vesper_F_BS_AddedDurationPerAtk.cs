using System;
using Mirror;
using UnityEngine;

public class Se_Star_Vesper_F_BS_AddedDurationPerAtk : StarEffect
{
	public float addedDuration = 0.2f;

	public override Type heroType => typeof(Hero_Vesper);

	public override Type skillType => typeof(St_R_BaptismOfSun);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.ActorEvent_OnAbilityInstanceCreated += new Action<EventInfoAbilityInstance>(Callback);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			victim.ActorEvent_OnAbilityInstanceCreated -= new Action<EventInfoAbilityInstance>(Callback);
		}
	}

	private void Callback(EventInfoAbilityInstance obj)
	{
		AbilityInstance instance = obj.instance;
		Se_R_BaptismOfSun_Buff se = instance as Se_R_BaptismOfSun_Buff;
		if (se == null)
		{
			return;
		}
		se.DoAttackEmpower((EventInfoAttackEffect eventInfoAttackEffect, int i) =>
		{
			if (eventInfoAttackEffect.type == AttackEffectType.BasicAttackMain && se.maxDuration.HasValue && se.remainingDuration.HasValue)
			{
				se.SetTimer(se.maxDuration.Value, se.remainingDuration.Value + addedDuration);
			}
		});
	}

	private void MirrorProcessed()
	{
	}
}
