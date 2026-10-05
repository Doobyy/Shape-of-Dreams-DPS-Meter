using System;
using Mirror;
using UnityEngine;

public class St_Q_MoonlightPact : SkillTrigger
{
	public float cooldownReductionPerFenrirAttack = 1f;

	private Sum_Q_MoonlightPact_Fenrir _fenrir;

	protected override void OnUnequip(Entity formerOwner)
	{
		base.OnUnequip(formerOwner);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		Dew.CallDelayed(() =>
		{
			if ((UnityEngine.Object)(object)owner == null && !_fenrir.IsNullInactiveDeadOrKnockedOut())
			{
				currentConfigIndex = 1;
				_fenrir.Kill();
				_fenrir = null;
				currentConfigIndex = 1;
				SetCharge(1, 0);
			}
		});
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || owner.IsNullInactiveDeadOrKnockedOut())
		{
			return;
		}
		if ((UnityEngine.Object)(object)_fenrir != null)
		{
			fillAmount = _fenrir.normalizedHealth;
		}
		else
		{
			fillAmount = 0f;
		}
		if (!((UnityEngine.Object)(object)_fenrir == null) || (currentConfigIndex != 0 && (currentConfigIndex != 1 || currentCharges[1] <= 0)) || NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition || ManagerBase<TransitionManager>.instance.state == TransitionManager.StateType.Loading)
		{
			return;
		}
		_fenrir = SpawnSummon<Sum_Q_MoonlightPact_Fenrir>(Dew.GetGoodRewardPosition(owner.position), Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f));
		_fenrir.EntityEvent_OnDeath += (Action<EventInfoKill>)((EventInfoKill kill) =>
		{
			if (!this.IsNullOrInactive())
			{
				_fenrir = null;
				currentConfigIndex = 1;
				SetCharge(1, 0);
			}
		});
		_fenrir.EntityEvent_OnAttackFired += (Action<EventInfoAttackFired>)((EventInfoAttackFired _) =>
		{
			ApplyCooldownReduction(this, cooldownReductionPerFenrirAttack);
		});
		currentConfigIndex = 0;
	}

	protected override void OnLevelChange(int oldLevel, int newLevel)
	{
		base.OnLevelChange(oldLevel, newLevel);
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)_fenrir != null)
		{
			_fenrir.Destroy();
			_fenrir = null;
		}
	}

	private void MirrorProcessed()
	{
	}
}
