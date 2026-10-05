using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossObliviax_ChargeSequence_Ready : AbilityInstance
{
	public float chargeDuration;

	public float postDelay;

	public float shieldRatio;

	public GameObject fxComplete;

	public GameObject fxCancel;

	public DewAnimationClip castingAnimClip;

	public DewAnimationClip completeAnimClip;

	public DewAnimationClip endAnimClip;

	private float _damageTakenTime = -1f;

	private bool _isChargeComplete;

	private float _time;

	private int _takenCount;

	private bool _isShieldBroken;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			GiveShield(info.caster, info.caster.Status.maxHealth * shieldRatio, chargeDuration).shield.onDamageNegated += new Action<EventInfoDamageNegatedByShield>(OnDamageNegatedByShield);
			info.caster.Animation.PlayAbilityAnimation(castingAnimClip);
			info.caster.Control.StartChannel(new Channel
			{
				blockedActions = Channel.BlockedAction.Everything,
				duration = chargeDuration,
				onCancel = () =>
				{
					_isChargeComplete = true;
					info.caster.Animation.PlayAbilityAnimation(endAnimClip);
					_isShieldBroken = true;
					info.caster.Control.StartDaze(postDelay);
					FxStopNetworked(startEffect);
					FxPlayNetworked(fxCancel, info.caster);
				},
				onComplete = () =>
				{
					_isChargeComplete = true;
					info.caster.Animation.PlayAbilityAnimation(completeAnimClip);
					FxStopNetworked(startEffect);
					FxPlayNetworked(fxComplete, info.caster);
					CreateAbilityInstance<Ai_Mon_Special_BossObliviax_ChargeSequence_Spawner>(info.caster.position, null, new CastInfo(info.caster));
				}
			});
			yield return new SI.WaitForCondition(() => _isChargeComplete);
			yield return new SI.WaitForSeconds(postDelay);
			Destroy();
		}
	}

	private void OnDamageNegatedByShield(EventInfoDamageNegatedByShield obj)
	{
		if (obj.shield.amount < 0.001f)
		{
			info.caster.Control.CancelOngoingChannels();
			DestroyIfActive();
		}
	}

	private void MirrorProcessed()
	{
	}
}
