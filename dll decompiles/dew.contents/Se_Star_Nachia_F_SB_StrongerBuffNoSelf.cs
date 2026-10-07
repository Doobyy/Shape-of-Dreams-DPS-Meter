using System;
using Mirror;
using UnityEngine;

public class Se_Star_Nachia_F_SB_StrongerBuffNoSelf : StarEffect
{
	public float effectAmp = 0.5f;

	public override Type heroType => typeof(Hero_Nachia);

	public override Type skillType => typeof(St_R_SerpentineBlessing);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)hero != null)
		{
			hero.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_R_SerpentineBlessing ai_R_SerpentineBlessing)
		{
			ai_R_SerpentineBlessing.dontDoSecondTarget = true;
		}
		if (obj.instance is Se_R_SerpentineBlessing_Buff se_R_SerpentineBlessing_Buff)
		{
			se_R_SerpentineBlessing_Buff.hasteAmount *= 1f + effectAmp;
		}
	}

	private void MirrorProcessed()
	{
	}
}
