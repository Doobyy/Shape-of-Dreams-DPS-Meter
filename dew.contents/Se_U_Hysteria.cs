using Mirror;
using UnityEngine;

public class Se_U_Hysteria : StatusEffect
{
	public float duration;

	public float clawInterval;

	public bool isUnstoppable = true;

	private float _lastClawTime;

	private bool _isRight;

	private AbilityLockHandle _handle;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		_isRight = false;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			if (isUnstoppable)
			{
				DoUnstoppable();
			}
			DoSpeed(-50f);
			SetTimer(duration);
			ShowOnScreenTimer();
			_handle = info.caster.Ability.GetNewAbilityLockHandle();
			_handle.LockAllAbilitiesCast();
			_handle.UnlockMovementSkillCast();
			_lastClawTime = Time.time;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && _handle != null)
		{
			_handle.Stop();
			_handle = null;
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer)
		{
			info.caster.Control.MoveToDestination(info.caster.owner.cursorWorldPos, immediately: false);
		}
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (victim.IsNullInactiveDeadOrKnockedOut() || NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition)
		{
			Destroy();
		}
		else
		{
			if (victim.Control.isDisplacing)
			{
				return;
			}
			float num = clawInterval / Mathf.Max(victim.Status.attackSpeedMultiplier, 1f);
			if (Time.time - _lastClawTime > num)
			{
				_isRight = !_isRight;
				_lastClawTime = Time.time;
				float angle = CastInfo.GetAngle(victim.owner.cursorWorldPos - victim.position);
				CreateAbilityInstance(victim.position, Quaternion.Euler(0f, angle, 0f), new CastInfo(victim, angle), (Ai_U_Hysteria_Claw claw) =>
				{
					claw.isRight = _isRight;
				});
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
