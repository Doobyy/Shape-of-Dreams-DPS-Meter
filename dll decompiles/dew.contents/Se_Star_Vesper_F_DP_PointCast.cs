using System;
using Mirror;
using UnityEngine;

public class Se_Star_Vesper_F_DP_PointCast : StarEffect
{
	public float radiusAmp = 0.2f;

	public override Type heroType => typeof(Hero_Vesper);

	public override Type skillType => typeof(St_Q_Discipline);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		DoSkill((SkillTrigger s) =>
		{
			s.configs[0].castMethod.type = CastMethodType.Point;
			s.configs[0].castMethod._isClamping = true;
			s.configs[0].castMethod._radius *= 1f + radiusAmp;
			s.SyncCastMethodChanges(0);
			return () =>
			{
				s.configs[0].castMethod.type = CastMethodType.Target;
				s.configs[0].castMethod._isClamping = false;
				s.configs[0].castMethod._radius /= 1f + radiusAmp;
				if (!s.IsNullOrInactive())
				{
					s.SyncCastMethodChanges(0);
				}
			};
		});
		victim.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			victim.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_Q_Discipline_Stomp ai_Q_Discipline_Stomp)
		{
			ai_Q_Discipline_Stomp.NetworkradiusMultiplier = ai_Q_Discipline_Stomp.radiusMultiplier * (1f + radiusAmp);
		}
	}

	private void MirrorProcessed()
	{
	}
}
