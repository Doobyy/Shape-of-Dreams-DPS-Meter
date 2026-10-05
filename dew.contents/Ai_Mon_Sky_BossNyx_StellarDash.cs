using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Sky_BossNyx_StellarDash : AbilityInstance
{
	public DewAnimationClip animToCancel;

	public DewAnimationClip endAnim;

	public float colRadius;

	public ScalingValue damage;

	public float slowAmount;

	public float slowDuration;

	public bool isSlowDecay;

	public GameObject hitEffect;

	public float speed;

	public DewEase ease;

	public bool resetSwipe = true;

	private Vector3 _lastPos;

	private List<Entity> _hitEnts = new List<Entity>();

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			Vector3 validAgentDestination_LinearSweep = Dew.GetValidAgentDestination_LinearSweep(info.caster.agentPosition, info.point);
			info.caster.Control.StartDisplacement(new DispByDestination
			{
				affectedByMovementSpeed = true,
				canGoOverTerrain = false,
				destination = validAgentDestination_LinearSweep,
				duration = Vector2.Distance(info.caster.agentPosition.ToXY(), validAgentDestination_LinearSweep.ToXY()) / speed,
				ease = ease,
				isCanceledByCC = true,
				isFriendly = true,
				onCancel = DestroyIfActive,
				onFinish = DestroyIfActive,
				rotateForward = true
			});
			_lastPos = info.caster.position;
		}
		yield break;
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		Vector3 lastPos = _lastPos;
		Vector3 vector = (_lastPos = info.caster.position);
		ListReturnHandle<Entity> handle;
		foreach (Entity item in DewPhysics.SphereCastAllEntities(out handle, lastPos, colRadius, vector - lastPos, (vector - lastPos).ToXY().magnitude, tvDefaultHarmfulEffectTargets))
		{
			if (!_hitEnts.Contains(item))
			{
				_hitEnts.Add(item);
				if (GetValue(damage) > 0f)
				{
					Damage(damage).SetDirection(vector - lastPos).Dispatch(item);
				}
				FxPlayNewNetworked(hitEffect, item);
				CreateBasicEffect(item, new SlowEffect
				{
					decay = isSlowDecay,
					strength = slowAmount
				}, slowDuration, "stellar_slow");
			}
		}
		handle.Return();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !info.caster.IsNullInactiveDeadOrKnockedOut())
		{
			if (endAnim != null)
			{
				info.caster.Animation.PlayAbilityAnimation(endAnim);
			}
			else if (animToCancel != null)
			{
				info.caster.Animation.StopAbilityAnimation(animToCancel);
			}
			if (resetSwipe && info.caster.Ability.TryGetAbility<At_Mon_Sky_BossNyx_Swipe>(out var trigger))
			{
				ResetCooldown(trigger);
			}
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_hitEnts.Clear();
	}

	private void MirrorProcessed()
	{
	}
}
