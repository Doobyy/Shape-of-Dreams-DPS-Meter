using System;
using Mirror;
using UnityEngine;

public class Se_Star_Vesper_F_BS_MovSpd : StarEffect
{
	public float movSpdRatio = 0.5f;

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
		if (obj.instance is Se_R_BaptismOfSun_Buff se_R_BaptismOfSun_Buff)
		{
			se_R_BaptismOfSun_Buff.DoSpeed(se_R_BaptismOfSun_Buff.GetValue(se_R_BaptismOfSun_Buff.hasteAmount) * movSpdRatio);
		}
	}

	private void MirrorProcessed()
	{
	}
}
