using System;
using Mirror;
using UnityEngine;

public class Se_Star_Nachia_L_WaltzArmor : StarEffect
{
	public StarScalingValue cooldownPenalty;

	public StarScalingValue armorAmount;

	public float sizeAmp = 0.2f;

	public override Type heroType => typeof(Hero_Nachia);

	public override bool isMovementSkillType => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)skill != null)
			{
				DoSkillBonusAll(new SkillBonus
				{
					cooldownOffset = GetValue(cooldownPenalty),
					ignoreReceiveCooldownReductionFlag = true
				});
			}
			hero.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
			hero.ActorEvent_OnAbilityInstanceCreated += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceCreated);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)hero != null)
		{
			hero.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
			hero.ActorEvent_OnAbilityInstanceCreated -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceCreated);
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_M_DreamyWaltz_BuffExplosion ai_M_DreamyWaltz_BuffExplosion)
		{
			ai_M_DreamyWaltz_BuffExplosion.NetworksizeMultiplier = ai_M_DreamyWaltz_BuffExplosion.sizeMultiplier + sizeAmp;
		}
	}

	private void ActorEventOnAbilityInstanceCreated(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Se_M_DreamyWaltz_Buff se_M_DreamyWaltz_Buff)
		{
			se_M_DreamyWaltz_Buff.DoArmorBoost(se_M_DreamyWaltz_Buff.strengthMultiplier * GetValue(armorAmount));
		}
	}

	private void MirrorProcessed()
	{
	}
}
