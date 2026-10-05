using System;
using Mirror;
using UnityEngine;

public class Se_Star_Bismuth_F_IN_LessCooldownLessMissiles : StarEffect
{
	public float cooldownReduction = 0.5f;

	public int reducedMissiles = 2;

	public override Type heroType => typeof(Hero_Bismuth);

	public override Type skillType => typeof(St_QR_Innocence);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoSkillBonusAll(new SkillBonus
			{
				cooldownMultiplier = 1f - cooldownReduction
			});
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
		if (obj.instance is Ai_QR_Innocence_Spawner ai_QR_Innocence_Spawner)
		{
			ai_QR_Innocence_Spawner.spawnCountOffset -= reducedMissiles;
		}
	}

	private void MirrorProcessed()
	{
	}
}
