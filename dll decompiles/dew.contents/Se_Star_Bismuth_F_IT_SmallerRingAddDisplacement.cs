using System;
using Mirror;
using UnityEngine;

public class Se_Star_Bismuth_F_IT_SmallerRingAddDisplacement : StarEffect
{
	public float increasedCooldown = 3f;

	public float dmgReduction = 0.45f;

	public int dashCount = 2;

	public float recastWindow = 999f;

	public override Type heroType => typeof(Hero_Bismuth);

	public override Type skillType => typeof(St_QR_InfernalTales);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		DoSkill((SkillTrigger s) =>
		{
			s.configs[0].cooldownTime += increasedCooldown;
			return () =>
			{
				s.configs[0].cooldownTime = DewResources.GetByType<St_QR_InfernalTales>(default(ResourceLoadSettings)).configs[0].cooldownTime;
			};
		});
		hero.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(OnBeforePrepare);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)hero != null)
		{
			hero.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(OnBeforePrepare);
		}
	}

	private void OnBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (!(obj.instance is Ai_QR_InfernalTales ai_QR_InfernalTales))
		{
			return;
		}
		ai_QR_InfernalTales.dealtDamageProcessor.Add(Processor);
		AbilityTrigger trigger = obj.instance.firstTrigger;
		if ((UnityEngine.Object)(object)trigger == null)
		{
			return;
		}
		int remaining = dashCount;
		AbilityTrigger.ChangedConfigHandle handle = null;
		ArmDash();
		ai_QR_InfernalTales.ClientActorEvent_OnDestroyed += (Action<Actor>)((Actor _) =>
		{
			if (handle != null && handle.isActive)
			{
				trigger.UndoChangeConfig();
			}
		});
		void ArmDash()
		{
			if (remaining > 0)
			{
				handle = trigger.ChangeConfigTimedOnce(1, recastWindow, (EventInfoAbilityInstance _) =>
				{
					remaining--;
					ArmDash();
				}, null, setFillAmount: false);
			}
		}
	}

	private void Processor(ref DamageData data, Actor actor, Entity target)
	{
		if (!data.IsAmountModifiedBy(this))
		{
			data.ApplyReduction(dmgReduction);
			data.SetAmountModifiedBy(this);
		}
	}

	private void MirrorProcessed()
	{
	}
}
