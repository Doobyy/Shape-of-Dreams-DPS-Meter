using System;
using Mirror;
using UnityEngine;

public class Se_Star_Yubar_F_SN_Artillery : StarEffect
{
	public float rangeAmp;

	public float radiusAmp;

	public int upgradesPerMaxCharge = 3;

	public override Type heroType => typeof(Hero_Yubar);

	public override Type skillType => typeof(St_Q_SuperNova);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		DoSkill((SkillTrigger s) =>
		{
			SkillBonus bonus = s.AddSkillBonus(new SkillBonus
			{
				addedCharge = (s.level - 1) / upgradesPerMaxCharge
			});
			s.ClientSkillEvent_OnLevelChange += new Action<int, int>(ClientSkillEventOnLevelChange);
			s.configs[0].castMethod._range *= 1f + rangeAmp;
			s.configs[0].castMethod._radius *= 1f + radiusAmp;
			s.SyncCastMethodChanges(0);
			return () =>
			{
				bonus.Stop();
				s.configs[0].castMethod._range /= 1f + rangeAmp;
				s.configs[0].castMethod._radius /= 1f + radiusAmp;
				s.ClientSkillEvent_OnLevelChange -= new Action<int, int>(ClientSkillEventOnLevelChange);
				s.SyncCastMethodChanges(0);
			};
			void ClientSkillEventOnLevelChange(int arg1, int arg2)
			{
				bonus.addedCharge = (s.level - 1) / upgradesPerMaxCharge;
			}
		});
		victim.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_Q_SuperNova ai_Q_SuperNova)
		{
			ai_Q_SuperNova.NetworkscaleMultiplier = ai_Q_SuperNova.scaleMultiplier * (1f + radiusAmp);
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
