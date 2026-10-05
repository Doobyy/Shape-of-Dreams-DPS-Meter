using System;
using Mirror;
using UnityEngine;

public class Se_Star_Mist_F_UD_ExplosionDamage : StarEffect
{
	public float durationReduction = 0.5f;

	public float explosionAmp = 2f;

	public override Type heroType => typeof(Hero_Mist);

	public override Type skillType => typeof(St_R_UnbreakableDetermination);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.EntityEvent_OnCastCompleteBeforePrepare += new Action<EventInfoCast>(EntityEventOnCastCompleteBeforePrepare);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			victim.EntityEvent_OnCastCompleteBeforePrepare -= new Action<EventInfoCast>(EntityEventOnCastCompleteBeforePrepare);
		}
	}

	private void EntityEventOnCastCompleteBeforePrepare(EventInfoCast obj)
	{
		if (obj.instance is Se_R_UnbreakableDetermination se_R_UnbreakableDetermination)
		{
			se_R_UnbreakableDetermination.initDuration *= 1f - durationReduction;
			se_R_UnbreakableDetermination.damage.apFactor += se_R_UnbreakableDetermination.damage.apFactor * explosionAmp;
		}
	}

	private void MirrorProcessed()
	{
	}
}
