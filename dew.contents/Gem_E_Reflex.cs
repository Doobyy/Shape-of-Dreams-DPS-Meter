using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Gem_E_Reflex : Gem
{
	public float internalCooldownTime = 0.4f;

	public float range;

	private float _lastHitTime;

	public override void OnEquipGem(Hero newOwner)
	{
		base.OnEquipGem(newOwner);
		if (((NetworkBehaviour)this).isServer && !newOwner.IsNullOrInactive())
		{
			newOwner.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(OnTakeDamage);
			_lastHitTime = 0f;
		}
	}

	public override void OnUnequipGem(Hero oldOwner)
	{
		base.OnUnequipGem(oldOwner);
		if (((NetworkBehaviour)this).isServer && !oldOwner.IsNullOrInactive())
		{
			oldOwner.EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(OnTakeDamage);
		}
	}

	private void OnTakeDamage(EventInfoDamage obj)
	{
		if (Time.time - _lastHitTime <= internalCooldownTime)
		{
			return;
		}
		if (owner.Status.TryGetStatusEffect<Se_Gem_E_Reflex_ArmorBonus>(out var effect))
		{
			effect.ResetTimer();
		}
		else
		{
			CreateStatusEffect<Se_Gem_E_Reflex_ArmorBonus>(owner, new CastInfo(owner));
		}
		List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, owner.agentPosition, range, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
		{
			sortComparer = CollisionCheckSettings.DistanceFromCenter
		});
		if (list.Count <= 0)
		{
			return;
		}
		foreach (Entity item in list)
		{
			if (!item.IsNullInactiveDeadOrKnockedOut())
			{
				if (!isValid)
				{
					return;
				}
				CreateAbilityInstance<Ai_Gem_E_Reflex_IceJet>(owner.position, Quaternion.LookRotation(item.agentPosition - owner.agentPosition, Vector3.up), new CastInfo(owner));
				NotifyUse();
				break;
			}
		}
		handle.Return();
		_lastHitTime = Time.time;
	}

	private void MirrorProcessed()
	{
	}
}
