using System;
using Mirror;
using UnityEngine;

public class Se_Star_Husk_L_DodgeChargesAndRangeWithPenalty : StarEffect
{
	public int addedCharges = 1;

	public StarScalingValue cooldownPenalty;

	public StarScalingValue moveAmp;

	public GameObject fxActivate;

	public override Type heroType => typeof(Hero_Husk);

	public override bool isMovementSkillType => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if ((UnityEngine.Object)(object)skill != null)
		{
			DoSkillBonusAll(new SkillBonus
			{
				addedCharge = addedCharges,
				cooldownOffset = GetValue(cooldownPenalty),
				ignoreReceiveCooldownReductionFlag = true
			});
			DoSkill((SkillTrigger s) =>
			{
				s.configs[0].castMethod._range *= 1f + GetValue(moveAmp);
				s.SyncCastMethodChanges(0);
				return () =>
				{
					s.configs[0].castMethod._range /= 1f + GetValue(moveAmp);
					if (!s.IsNullOrInactive())
					{
						s.SyncCastMethodChanges(0);
					}
				};
			});
		}
		hero.ActorEvent_OnAbilityInstanceCreated += new Action<EventInfoAbilityInstance>(PlayEffect);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)hero != null)
		{
			hero.ActorEvent_OnAbilityInstanceCreated -= new Action<EventInfoAbilityInstance>(PlayEffect);
		}
	}

	private void PlayEffect(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_M_FlashStep)
		{
			FxPlayNetworked(fxActivate, hero, hero.agentPosition, obj.instance.info.rotation);
		}
	}

	private void MirrorProcessed()
	{
	}
}
