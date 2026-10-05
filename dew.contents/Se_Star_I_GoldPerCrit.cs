using System;
using Mirror;
using UnityEngine;

public class Se_Star_I_GoldPerCrit : StarEffect
{
	public StarScalingValue goldAmount;

	public GameObject fxGold;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.EntityEvent_OnAttackHit += new Action<EventInfoAttackHit>(EntityEventOnAttackHit);
		}
	}

	private void EntityEventOnAttackHit(EventInfoAttackHit obj)
	{
		if (!obj.victim.Status.hasDamageImmunity && obj.isCrit)
		{
			string dataKey = Dew.GetDataKey("Se_Star_I_GoldPerCrit", "didActivate", ((NetworkBehaviour)this).netId.ToString());
			if (!obj.victim.persistentData.GetDataOrDefault(dataKey, defaultValue: false))
			{
				NetworkedManagerBase<PickupManager>.instance.DropGold(isKillGold: false, isGivenByOtherPlayer: false, DewMath.RandomRoundToInt(GetValue(goldAmount)), obj.victim.agentPosition, hero);
				FxPlayNewNetworked(fxGold, obj.victim);
				obj.victim.persistentData.SetData(dataKey, value: true);
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			victim.EntityEvent_OnAttackHit -= new Action<EventInfoAttackHit>(EntityEventOnAttackHit);
		}
	}

	private void MirrorProcessed()
	{
	}
}
