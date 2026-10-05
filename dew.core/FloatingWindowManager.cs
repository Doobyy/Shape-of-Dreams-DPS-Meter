using System;
using UnityEngine;

public class FloatingWindowManager : ManagerBase<FloatingWindowManager>
{
	public int backButtonPriority = 15;

	public Action<MonoBehaviour> onTargetChanged;

	public float maxDistance = 4f;

	public MonoBehaviour currentTarget { get; private set; }

	public float lastSetTargetUnscaledTime { get; private set; }

	private void Start()
	{
		ManagerBase<GlobalUIManager>.instance.AddBackHandler(this, backButtonPriority, () =>
		{
			if (currentTarget == null)
			{
				return false;
			}
			if (ManagerBase<EditSkillManager>.instance.mode != EditSkillManager.ModeType.None)
			{
				ManagerBase<EditSkillManager>.instance.EndEdit();
			}
			ClearTarget();
			return true;
		});
		InGameUIManager inGameUIManager = InGameUIManager.instance;
		inGameUIManager.onWorldDisplayedChanged = (Action<WorldDisplayStatus>)Delegate.Combine(inGameUIManager.onWorldDisplayedChanged, (Action<WorldDisplayStatus>)((WorldDisplayStatus _) =>
		{
			ClearTarget();
		}));
		NetworkedManagerBase<ClientEventManager>.instance.OnTakeDamage += (Action<EventInfoDamage>)((EventInfoDamage dmg) =>
		{
			if ((UnityEngine.Object)(object)DewPlayer.local != null && (UnityEngine.Object)(object)dmg.victim == (UnityEngine.Object)(object)DewPlayer.local.hero && !dmg.damage.HasAttr(DamageAttribute.DamageOverTime))
			{
				ClearTarget();
			}
		});
	}

	public void SetTarget(MonoBehaviour newTarget)
	{
		if ((object)currentTarget != newTarget)
		{
			lastSetTargetUnscaledTime = Time.unscaledTime;
			currentTarget = newTarget;
			onTargetChanged?.Invoke(currentTarget);
		}
	}

	public void ClearTarget()
	{
		SetTarget(null);
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (currentTarget != null && DewInput.GetButtonDown(DewSave.profileMain.controls.interact, checkGameAreaForMouse: true))
		{
			ClearTarget();
		}
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if ((object)currentTarget != null)
		{
			if (NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition || DewPlayer.local.hero.IsNullInactiveDeadOrKnockedOut())
			{
				ClearTarget();
			}
			else if (currentTarget == null || (currentTarget is Actor a && a.IsNullOrInactive()) || (currentTarget is Shrine shrine && !shrine.CanInteract(ManagerBase<ControlManager>.instance.controllingEntity)))
			{
				ClearTarget();
			}
			else if (currentTarget != null && Vector3.Distance(currentTarget.transform.position, DewPlayer.local.hero.position) > maxDistance)
			{
				ClearTarget();
			}
		}
	}
}
