using System;
using Mirror;
using UnityEngine;

public class Se_Star_Lacerta_F_PS_SlowerStronger : StarEffect
{
	public float speedPenaltyRatio;

	public float damageAmp;

	public float widthAmp;

	public override Type heroType => typeof(Hero_Lacerta);

	public override Type skillType => typeof(St_R_PrecisionShot);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_R_PrecisionShot { channel: var channel } ai_R_PrecisionShot)
		{
			channel.chargeFullDuration /= 1f - speedPenaltyRatio;
			ai_R_PrecisionShot.channel.castMethod._width *= 1f + widthAmp;
		}
		if (obj.instance is Ai_R_PrecisionShot_Projectile ai_R_PrecisionShot_Projectile)
		{
			ai_R_PrecisionShot_Projectile.isBiggerShot = true;
			ai_R_PrecisionShot_Projectile.collisionRadius *= 1f + widthAmp;
			ai_R_PrecisionShot_Projectile.damageMin.apFactor += ai_R_PrecisionShot_Projectile.damageMin.apFactor * damageAmp;
			ai_R_PrecisionShot_Projectile.damageMax.apFactor += ai_R_PrecisionShot_Projectile.damageMax.apFactor * damageAmp;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			victim.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	private void MirrorProcessed()
	{
	}
}
