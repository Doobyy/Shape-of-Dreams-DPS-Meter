using System;
using Mirror;
using UnityEngine;

public class Se_Star_Lacerta_F_HC_NoDelayDoubleCast : StarEffect
{
	public float damageReduction = 0.25f;

	public int addedCharges = 1;

	public GameObject fxReload;

	public override Type heroType => typeof(Hero_Lacerta);

	public override Type skillType => typeof(St_Q_HandCannon);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		DoSkillBonusAll(new SkillBonus
		{
			addedCharge = addedCharges
		});
		DoSkill((SkillTrigger s) =>
		{
			float prevChannelDuration = s.configs[0].channel.duration;
			int prevAddedCharges = s.configs[0].addedCharges;
			s.configs[0].channel.duration = 0f;
			s.configs[0].addedCharges = 999;
			s.dealtDamageProcessor.Add(ReduceDamage);
			s.configs[0].endAnim = null;
			victim.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(OnBeforePrepare);
			victim.ActorEvent_OnAbilityInstanceCreated += new Action<EventInfoAbilityInstance>(OnCreated);
			return () =>
			{
				if (!s.IsNullOrInactive())
				{
					s.configs[0].channel.duration = prevChannelDuration;
					s.configs[0].addedCharges = prevAddedCharges;
					s.dealtDamageProcessor.Remove(ReduceDamage);
					s.configs[0].endAnim = DewResources.GetByType<St_Q_HandCannon>(default(ResourceLoadSettings)).configs[0].endAnim;
				}
				if ((UnityEngine.Object)(object)victim != null)
				{
					victim.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(OnBeforePrepare);
					victim.ActorEvent_OnAbilityInstanceCreated -= new Action<EventInfoAbilityInstance>(OnCreated);
				}
			};
		});
	}

	private void OnCreated(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_Q_HandCannon ai_Q_HandCannon)
		{
			FxPlayNewNetworked(fxReload, ai_Q_HandCannon.position, ai_Q_HandCannon.rotation);
		}
	}

	private void OnBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_Q_HandCannon ai_Q_HandCannon)
		{
			ai_Q_HandCannon.enableRecoil = false;
		}
	}

	private void ReduceDamage(ref DamageData data, Actor actor, Entity target)
	{
		if (actor is Ai_Q_HandCannon && !data.IsAmountModifiedBy(this))
		{
			data.ApplyReduction(damageReduction);
			data.SetAmountModifiedBy(this);
		}
	}

	private void MirrorProcessed()
	{
	}
}
