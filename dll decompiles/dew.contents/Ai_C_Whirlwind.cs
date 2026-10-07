using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_C_Whirlwind : DamageInstance
{
	public DewAnimationClip animSpin;

	public DewAnimationClip animSpinEnd;

	public Transform swordHandleTransform;

	public float totalDuration = 4f;

	private EntityTransformModifier _mod;

	private float _currentAngle;

	private float _lastDamageTime = float.NegativeInfinity;

	private AbilityLockHandle _handle;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		_currentAngle = 0f;
		_lastDamageTime = float.NegativeInfinity;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		_mod = info.caster.Visual.GetNewTransformModifier();
		if (((NetworkBehaviour)this).isServer)
		{
			_handle = info.caster.Ability.GetNewAbilityLockHandle();
			_handle.LockAllAbilitiesCast();
			_handle.UnlockMovementSkillCast();
			info.caster.Control.Rotate(Quaternion.identity, immediately: true, totalDuration);
			info.caster.Control.StartChannel(new Channel
			{
				duration = totalDuration,
				onCancel = DestroyIfActive,
				onComplete = DestroyIfActive
			});
			info.caster.Animation.PlayAbilityAnimation(animSpin);
			yield return new SI.WaitForSeconds(0.75f);
			firstTrigger.ChangeConfigTimedOnce(1, totalDuration - 0.75f);
		}
	}

	private float GetTickInterval()
	{
		return 0.48f / Mathf.Max(1f, info.caster.Status.attackSpeedMultiplier);
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		((Component)(object)this).transform.position = info.caster.agentPosition.WithY(info.caster.Visual.GetCenterPosition().y);
		_currentAngle += 1000f * Time.deltaTime * info.caster.Status.attackSpeedMultiplier;
		_mod.rotation = Quaternion.Euler(0f, _currentAngle, 0f);
		Vector3 forward = (swordHandleTransform.position - info.caster.position).Flattened();
		swordHandleTransform.rotation = Quaternion.Euler(0f, 30f, 0f) * Quaternion.LookRotation(forward);
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (Time.time - creationTime > totalDuration)
		{
			Destroy();
			return;
		}
		float tickInterval = GetTickInterval();
		if (!(Time.time - _lastDamageTime < tickInterval))
		{
			if (float.IsInfinity(_lastDamageTime))
			{
				_lastDamageTime = Time.time;
			}
			else
			{
				_lastDamageTime += tickInterval;
			}
			info.caster.Animation.PlayAbilityAnimation(animSpin);
			DoCollisionChecks();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (_mod != null)
		{
			_mod.Stop();
			_mod = null;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			if (_handle != null)
			{
				_handle.Stop();
				_handle = null;
			}
			info.caster.Control.StopOverrideRotation();
			info.caster.Animation.StopAbilityAnimation(animSpin);
			if (!info.caster.IsNullInactiveDeadOrKnockedOut())
			{
				info.caster.Animation.PlayAbilityAnimation(animSpinEnd);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
