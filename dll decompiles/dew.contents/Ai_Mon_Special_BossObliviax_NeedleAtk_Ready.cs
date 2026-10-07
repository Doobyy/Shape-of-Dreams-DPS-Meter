using System;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossObliviax_NeedleAtk_Ready : AbilityInstance
{
	public DewAnimationClip castAnimClip;

	public DewAnimationClip endAnimClip;

	public float counterDuration;

	public float takenDamageGraceTime;

	public float startGraceTime;

	public int maxDamageTakeCount;

	public GameObject fxOnStackFull;

	private float _damageTakenTime = -1f;

	private int _takenCount;

	private Channel _channel;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			info.caster.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(OnTakeDamage);
			maxDamageTakeCount += (Dew.GetAliveHeroCount() - 1) * 2;
			info.caster.Visual.genericStackIndicatorMax = maxDamageTakeCount;
			CreateBasicEffect(info.caster, new ArmorBoostEffect
			{
				strength = 200f
			}, 10f, "NeedleAtkShielding").DestroyOnDestroy(this);
			CreateBasicEffect(info.caster, new UnstoppableEffect(), 10f, "ObliviaxUnstoppable").DestroyOnDestroy(this);
			_channel = info.caster.Control.StartChannel(new Channel
			{
				blockedActions = Channel.BlockedAction.Everything,
				duration = counterDuration,
				onCancel = () =>
				{
					info.caster.Animation.PlayAbilityAnimation(endAnimClip);
				},
				onComplete = () =>
				{
					info.caster.Visual.genericStackIndicatorValue = 0;
					info.caster.Control.StartDaze(1f);
					DestroyIfActive();
				}
			});
			info.caster.Animation.PlayAbilityAnimation(castAnimClip);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			info.caster.Visual.genericStackIndicatorValue = 0;
			info.caster.EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(OnTakeDamage);
		}
	}

	private void OnTakeDamage(EventInfoDamage eventInfo)
	{
		if (!(eventInfo.actor is ElementalStatusEffect) && !(Time.time - creationTime < startGraceTime) && !(Time.time - _damageTakenTime < takenDamageGraceTime))
		{
			_damageTakenTime = Time.time;
			_takenCount++;
			info.caster.Visual.genericStackIndicatorValue = _takenCount;
			if (_takenCount >= maxDamageTakeCount)
			{
				_channel.Cancel();
				CreateAbilityInstance<Ai_Mon_Special_BossObliviax_NeedleAtk>(info.caster.position, null, info);
				DestroyIfActive();
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
