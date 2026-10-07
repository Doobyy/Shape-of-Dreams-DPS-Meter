using System;
using Mirror;
using UnityEngine;

public class Se_Star_Husk_F_DE_HighAmpMissReset : StarEffect
{
	public float newAmpGainPerHit = 0.02f;

	public override Type heroType => typeof(Hero_Husk);

	public override Type skillType => typeof(St_Q_DeathMark);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		victim.EntityEvent_OnCastCompleteBeforePrepare += new Action<EventInfoCast>(EntityEventOnCastCompleteBeforePrepare);
		DoSkill((St_Q_DeathMark s) =>
		{
			s.resetAmpOnZoneLoad = false;
			return () =>
			{
				s.resetAmpOnZoneLoad = true;
			};
		});
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
		if (obj.instance is Ai_Q_DeathMark_Projectile ai_Q_DeathMark_Projectile)
		{
			ai_Q_DeathMark_Projectile.damageAmpGainOnFirstHit = newAmpGainPerHit;
			ai_Q_DeathMark_Projectile.lostAmpRatioOnCompleteMiss = 1f;
		}
	}

	private void MirrorProcessed()
	{
	}
}
