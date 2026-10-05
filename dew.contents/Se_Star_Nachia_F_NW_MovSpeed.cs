using System;
using Mirror;
using UnityEngine;

public class Se_Star_Nachia_F_NW_MovSpeed : StarEffect
{
	public float speedMultiplier = 0.8f;

	public override Type heroType => typeof(Hero_Nachia);

	public override Type skillType => typeof(St_R_NaturesWhisper);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.ActorEvent_OnAbilityInstanceCreated += new Action<EventInfoAbilityInstance>(AbilityInstanceCreated);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)hero != null)
		{
			hero.ActorEvent_OnAbilityInstanceCreated -= new Action<EventInfoAbilityInstance>(AbilityInstanceCreated);
		}
	}

	private void AbilityInstanceCreated(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Se_R_NaturesWhisper_Buff se_R_NaturesWhisper_Buff)
		{
			se_R_NaturesWhisper_Buff.DoSpeed(se_R_NaturesWhisper_Buff.GetValue(se_R_NaturesWhisper_Buff.hasteAmount) * speedMultiplier);
		}
	}

	private void MirrorProcessed()
	{
	}
}
