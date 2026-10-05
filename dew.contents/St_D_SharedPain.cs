using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class St_D_SharedPain : SkillTrigger
{
	public ScalingValue countRaw;

	public float checkRadius = 10f;

	public int maxCount = 8;

	public int clampedCount => Mathf.Min(Mathf.RoundToInt(GetValue(countRaw)), maxCount);

	protected override void OnEquip(Entity newOwner)
	{
		base.OnEquip(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			((Hero)newOwner).EntityEvent_OnCastCompleteBeforePrepare += new Action<EventInfoCast>(EntityEventOnCastCompleteBeforePrepare);
		}
	}

	protected override void OnUnequip(Entity formerOwner)
	{
		base.OnUnequip(formerOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			((Hero)formerOwner).EntityEvent_OnCastCompleteBeforePrepare -= new Action<EventInfoCast>(EntityEventOnCastCompleteBeforePrepare);
		}
	}

	private void EntityEventOnCastCompleteBeforePrepare(EventInfoCast obj)
	{
		if (obj.trigger is SkillTrigger && !(obj.trigger is St_D_SharedPain) && !(obj.trigger is SkillTrigger { skillType: HeroSkillLocation.Movement }))
		{
			obj.instance.ActorEvent_OnDealDamage += new Action<EventInfoDamage>(ActorEventOnDealDamage);
		}
		void ActorEventOnDealDamage(EventInfoDamage eventInfoDamage)
		{
			if (owner.CheckEnemyOrNeutral(eventInfoDamage.victim))
			{
				Vector3 centerPosition = eventInfoDamage.victim.Visual.GetCenterPosition();
				List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, eventInfoDamage.victim.position, checkRadius, new CollisionCheckSettings
				{
					sortComparer = CollisionCheckSettings.Random
				});
				Entity entity = null;
				foreach (Entity item in list)
				{
					if (!((owner.GetRelation(item) == EntityRelation.Ally) | (owner.GetRelation(item) == EntityRelation.Neutral)) && !((UnityEngine.Object)(object)item == (UnityEngine.Object)(object)eventInfoDamage.victim) && !((UnityEngine.Object)(object)item == (UnityEngine.Object)(object)owner) && !item.IsNullInactiveDeadOrKnockedOut())
					{
						entity = item;
						break;
					}
				}
				handle.Return();
				if ((UnityEngine.Object)(object)entity == null && Vector3.Distance(owner.agentPosition, eventInfoDamage.victim.agentPosition) <= checkRadius && !owner.IsNullInactiveDeadOrKnockedOut())
				{
					entity = owner;
				}
				if ((UnityEngine.Object)(object)entity != null)
				{
					CreateAbilityInstance(centerPosition, null, new CastInfo(owner, entity), (Ai_D_SharedPain_Instance p) =>
					{
						p.maxSharedCount = clampedCount;
						p.checkRadius = checkRadius;
						p.originalDamage = eventInfoDamage.damage;
						p.lastEntity = eventInfoDamage.victim;
					});
				}
				obj.instance.ActorEvent_OnDealDamage -= new Action<EventInfoDamage>(ActorEventOnDealDamage);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
