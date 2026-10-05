using System;
using Mirror;
using UnityEngine;

public class Se_Star_Vesper_F_SE_PointCast : StarEffect
{
	public float reductionRatioOnAlly = 0.1f;

	public float reductionRatioOnEnemy = 0.03f;

	public override Type heroType => typeof(Hero_Vesper);

	public override Type skillType => typeof(St_R_SanctuaryOfEl);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		DoSkill((SkillTrigger s) =>
		{
			skill.configs[0].castMethod.type = CastMethodType.Point;
			skill.configs[0].castMethod._range = 13f;
			skill.configs[1].castMethod.type = CastMethodType.Point;
			skill.configs[1].castMethod._range = 13f;
			skill.SyncCastMethodChanges(0);
			skill.SyncCastMethodChanges(1);
			return () =>
			{
				skill.configs[0].castMethod.type = CastMethodType.None;
				skill.configs[0].castMethod._range = 0f;
				skill.configs[1].castMethod.type = CastMethodType.None;
				skill.configs[1].castMethod._range = 0f;
				skill.SyncCastMethodChanges(0);
				skill.SyncCastMethodChanges(1);
			};
		});
		victim.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_R_SanctuaryOfEl_Ground ai_R_SanctuaryOfEl_Ground)
		{
			ai_R_SanctuaryOfEl_Ground.cooldownReductionRatioByExplosionAlly = reductionRatioOnAlly;
			ai_R_SanctuaryOfEl_Ground.cooldownReductionRatioByExplosionEnemy = reductionRatioOnEnemy;
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
