using System;
using Mirror;
using UnityEngine;

public class St_L_SmallMoltenCore : SkillTrigger
{
	public GameObject fxEmpowerOnDragon;

	private Sum_L_SmallMoltenCore_Dragon _dragon;

	protected override void OnUnequip(Entity formerOwner)
	{
		base.OnUnequip(formerOwner);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		Dew.CallDelayed(() =>
		{
			if ((UnityEngine.Object)(object)owner == null && !_dragon.IsNullInactiveDeadOrKnockedOut())
			{
				_dragon.Kill();
				_dragon = null;
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
		fillAmount = (((UnityEngine.Object)(object)_dragon != null) ? _dragon.normalizedHealth : 0f);
		if (!((UnityEngine.Object)(object)_dragon == null) || (currentConfigIndex != 0 && (currentConfigIndex != 1 || currentCharges[1] <= 0)) || NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition || ManagerBase<TransitionManager>.instance.state == TransitionManager.StateType.Loading)
		{
			return;
		}
		_dragon = SpawnSummon<Sum_L_SmallMoltenCore_Dragon>(Dew.GetGoodRewardPosition(owner.position), Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f));
		_dragon.EntityEvent_OnDeath += (Action<EventInfoKill>)((EventInfoKill _) =>
		{
			if (!this.IsNullOrInactive())
			{
				_dragon = null;
				currentConfigIndex = 1;
				SetCharge(1, 0);
			}
		});
		currentConfigIndex = 0;
	}

	public override AbilityInstance OnCastComplete(int configIndex, CastInfo info)
	{
		AbilityInstance result = base.OnCastComplete(configIndex, info);
		if (((NetworkBehaviour)this).isServer && !_dragon.IsNullInactiveDeadOrKnockedOut())
		{
			FxPlayNetworked(fxEmpowerOnDragon, _dragon);
			_dragon.Empower();
			if (owner.Status.TryGetStatusEffect<Se_L_SmallMoltenCore_EmpowerAtk>(out var effect))
			{
				effect.Destroy();
			}
			CreateStatusEffect(owner, new CastInfo(owner), (Se_L_SmallMoltenCore_EmpowerAtk b) =>
			{
				b.dragon = _dragon;
				b.ClientActorEvent_OnDestroyed += (Action<Actor>)((Actor _) =>
				{
					FxStopNetworked(fxEmpowerOnDragon);
				});
			});
		}
		return result;
	}

	protected override void OnLevelChange(int oldLevel, int newLevel)
	{
		base.OnLevelChange(oldLevel, newLevel);
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)_dragon != null)
		{
			_dragon.Destroy();
			_dragon = null;
		}
	}

	private void MirrorProcessed()
	{
	}
}
