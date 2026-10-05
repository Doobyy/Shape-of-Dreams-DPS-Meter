using System;
using Mirror;
using UnityEngine;

public class Se_Star_Aurena_F_DT_TwiceWithPenalty : StarEffect
{
	public int newEmpowerCount = 2;

	public float cooldownPenalty = 2f;

	public override Type heroType => typeof(Hero_Aurena);

	public override Type skillType => typeof(St_R_DangerousTheory);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		hero.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		DoSkill((SkillTrigger s) =>
		{
			St_R_DangerousTheory dt = s as St_R_DangerousTheory;
			if (dt == null)
			{
				return (Action)null;
			}
			dt.NetworkpreventCastWhenNotEmpowered = true;
			return () =>
			{
				dt.NetworkpreventCastWhenNotEmpowered = false;
			};
		});
		DoSkillBonusAll(new SkillBonus
		{
			cooldownOffset = cooldownPenalty
		});
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
		if (obj.instance is Se_R_DangerousTheory se_R_DangerousTheory)
		{
			se_R_DangerousTheory.empowerCount = newEmpowerCount;
		}
	}

	private void MirrorProcessed()
	{
	}
}
