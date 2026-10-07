using System;
using Mirror;
using UnityEngine;

public class Ai_D_ParryMaster_Parry : AbilityInstance
{
	public float duration = 1f;

	public DewAnimationClip anim;

	public float animSpeedMultiplier = 1f;

	public GameObject fxSuccess;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (anim != null && anim.entries != null && anim.entries.Length != 0 && !Ai_GenericDodge.ShouldSkipAnimation(info.caster))
		{
			int num = 0;
			if (firstTrigger is St_M_ParryMaster st_M_ParryMaster)
			{
				num = st_M_ParryMaster.nextParryAnimIndex % anim.entries.Length;
				st_M_ParryMaster.nextParryAnimIndex = (num + 1) % anim.entries.Length;
			}
			float clipSelectValue = ((float)num + 0.5f) / (float)anim.entries.Length;
			info.caster.Animation.PlayAbilityAnimation(anim, animSpeedMultiplier * anim.entries[num].duration / duration, clipSelectValue);
		}
		float hitboxMultiplier = 1.4f;
		info.caster.Control.outerRadius = Mathf.Round(info.caster.Control.outerRadius * hitboxMultiplier * 100f) / 100f;
		Channel c = info.caster.Control.StartChannel(new Channel
		{
			blockedActions = Channel.BlockedAction.Everything,
			duration = duration,
			onComplete = DestroyIfActive
		});
		CreateBasicEffect(info.caster, new InvulnerableEffect(), duration, "parry_inv");
		info.caster.EntityEvent_OnDamageNegatedByImmunity += new Action<EventInfoDamageNegatedByImmunity>(EntityEventOnDamageNegatedByImmunity);
		ClientActorEvent_OnDestroyed += (Action<Actor>)((Actor _) =>
		{
			info.caster.EntityEvent_OnDamageNegatedByImmunity -= new Action<EventInfoDamageNegatedByImmunity>(EntityEventOnDamageNegatedByImmunity);
			info.caster.Control.outerRadius = Mathf.Round(info.caster.Control.outerRadius / hitboxMultiplier * 100f) / 100f;
		});
		void EntityEventOnDamageNegatedByImmunity(EventInfoDamageNegatedByImmunity obj)
		{
			c.Complete();
			FxPlayNewNetworked(fxSuccess, info.caster);
			firstTrigger.currentCharges[0]++;
			if (info.caster.Status.TryGetStatusEffect<Se_D_ParryMaster_Buff>(out var effect))
			{
				effect.StackAndReset();
			}
			else
			{
				CreateStatusEffect<Se_D_ParryMaster_Buff>(info.caster);
			}
			DestroyIfActive();
		}
	}

	private void MirrorProcessed()
	{
	}
}
