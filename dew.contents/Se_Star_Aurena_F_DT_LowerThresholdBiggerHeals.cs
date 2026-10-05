using System;
using Mirror;

public class Se_Star_Aurena_F_DT_LowerThresholdBiggerHeals : StarEffect
{
	public float newThreshold = 0.4f;

	public float healAmp = 0.4f;

	public override Type heroType => typeof(Hero_Aurena);

	public override Type skillType => typeof(St_R_DangerousTheory);

	protected override void OnCreate()
	{
		base.OnCreate();
		DoSkill((SkillTrigger s) =>
		{
			St_R_DangerousTheory dt = s as St_R_DangerousTheory;
			if (dt == null)
			{
				return (Action)null;
			}
			dt.healHpThreshold = newThreshold;
			return () =>
			{
				dt.healHpThreshold = DewResources.GetByType<St_R_DangerousTheory>(ResourceLoadSettings.Light).healHpThreshold;
			};
		});
		if (((NetworkBehaviour)this).isServer)
		{
			hero.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Se_R_DangerousTheory se_R_DangerousTheory)
		{
			se_R_DangerousTheory.healAmount *= 1f + healAmp;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	private void MirrorProcessed()
	{
	}
}
