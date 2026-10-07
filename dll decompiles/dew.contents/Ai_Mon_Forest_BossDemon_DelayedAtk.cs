using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Forest_BossDemon_DelayedAtk : AbilityInstance
{
	public ChannelData channel;

	public float animBaseDuration = 0.8f;

	public DewAnimationClip startAnim;

	public DewAnimationClip endAnim;

	public GameObject chargeEffect;

	public GameObject fxTelegraphOnTarget;

	public float postDelay = 0.85f;

	public DewCollider delayedAtkRange;

	public float delayedAtkDashDis = 5f;

	private float _animSelectValue;

	protected override IEnumerator OnCreateSequenced()
	{
		yield return base.OnCreateSequenced();
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		FxPlayNetworked(fxTelegraphOnTarget, info.target);
		info.caster.Control.RotateTowards(info.target, immediately: false);
		Reveal(info.target, 2f);
		for (int i = 0; i < startAnim.entries.Length; i++)
		{
			startAnim.entries[i].duration = animBaseDuration + channel.duration;
		}
		_animSelectValue = Random.value;
		info.caster.Animation.PlayAbilityAnimation(startAnim, 1f, _animSelectValue);
		FxPlayNetworked(chargeEffect, info.caster);
		channel.Get().AddOnComplete(() =>
		{
			info.caster.Control.RotateTowards(info.target, immediately: true);
			info.caster.Animation.PlayAbilityAnimation(endAnim, 1f, _animSelectValue);
			CastInfo castInfo = new CastInfo(info.caster)
			{
				angle = CastInfo.GetAngle(info.target.GetAIPosition(info.caster) - info.caster.position)
			};
			CreateAbilityInstance(info.caster.position, castInfo.rotation, castInfo, (Ai_Mon_Forest_BossDemon_Atk p) =>
			{
				p.range.points = delayedAtkRange.points;
				p.dash.distance = delayedAtkDashDis;
			});
			info.caster.Control.StartDaze(postDelay);
			Destroy();
		}).AddOnCancel(() =>
		{
			info.caster.Animation.StopAbilityAnimation(startAnim);
			DestroyIfActive();
		})
			.Dispatch(info.caster);
		DestroyOnDeath(info.caster);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(chargeEffect);
			FxStopNetworked(fxTelegraphOnTarget);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer)
		{
			info.caster.Control.RotateTowards(info.target, immediately: false);
		}
	}

	private void MirrorProcessed()
	{
	}
}
