using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_R_Tranquility : StatusEffect
{
	public DewAnimationClip endAnim;

	public float movementSpeedPercentage;

	public ScalingValue totalReducedSeconds;

	public int ticks;

	public float duration;

	[NonSerialized]
	public bool disableInvul;

	[NonSerialized]
	public bool enableEssenceReduction;

	private int _doneTicks;

	private float _baseDuration;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_baseDuration = duration;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		duration = _baseDuration;
		disableInvul = false;
		enableEssenceReduction = false;
		_doneTicks = 0;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			victim.Control.StartChannel(new Channel
			{
				blockedActions = (Channel.BlockedAction.Ability | Channel.BlockedAction.Attack),
				duration = duration
			});
			if (!disableInvul)
			{
				DoUncollidable();
				DoInvulnerable();
			}
			else
			{
				DoUnstoppable();
			}
			DoStatBonus(new StatBonus
			{
				movementSpeedPercentage = movementSpeedPercentage
			});
			SetTimer(duration);
			ShowOnScreenTimer();
			for (int i = 0; i < ticks; i++)
			{
				DoTick();
				yield return new SI.WaitForSeconds(duration / (float)ticks);
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if (!victim.IsNullInactiveDeadOrKnockedOut() && endAnim != null)
			{
				victim.Animation.PlayAbilityAnimation(endAnim);
			}
			int num = ticks - _doneTicks;
			for (int i = 0; i < num; i++)
			{
				DoTick();
			}
		}
	}

	private void DoTick()
	{
		if (_doneTicks > ticks)
		{
			return;
		}
		_doneTicks++;
		if ((UnityEngine.Object)(object)victim == null || !victim.isActive || !(victim is Hero hero))
		{
			return;
		}
		AbilityTrigger abilityTrigger = firstTrigger;
		foreach (KeyValuePair<int, AbilityTrigger> ability in hero.Ability.abilities)
		{
			if (!((UnityEngine.Object)(object)abilityTrigger == (UnityEngine.Object)(object)ability.Value) && ability.Value is SkillTrigger trigger)
			{
				ApplyCooldownReduction(trigger, GetValue(totalReducedSeconds) / (float)ticks);
			}
		}
		if (!enableEssenceReduction)
		{
			return;
		}
		foreach (KeyValuePair<GemLocation, Gem> gem in hero.Skill.gems)
		{
			gem.Value.ApplyCooldownReduction(GetValue(totalReducedSeconds) / (float)ticks);
		}
	}

	private void MirrorProcessed()
	{
	}
}
