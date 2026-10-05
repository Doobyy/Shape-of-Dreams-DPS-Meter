using System;
using Mirror;
using UnityEngine;

public class Gem_R_Panic : Gem
{
	public override void OnEquipGem(Hero newOwner)
	{
		base.OnEquipGem(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			newOwner.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(EntityEventOnTakeDamage);
		}
	}

	public override void OnUnequipGem(Hero oldOwner)
	{
		base.OnUnequipGem(oldOwner);
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)oldOwner != null)
		{
			oldOwner.EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(EntityEventOnTakeDamage);
		}
	}

	private void EntityEventOnTakeDamage(EventInfoDamage obj)
	{
		if (isValid)
		{
			if (owner.Status.TryGetStatusEffect<Se_Gem_R_Panic>(out var effect))
			{
				effect.ResetTimer();
			}
			else
			{
				CreateStatusEffect<Se_Gem_R_Panic>(owner, default);
			}
			NotifyUse();
		}
	}

	private void MirrorProcessed()
	{
	}
}
