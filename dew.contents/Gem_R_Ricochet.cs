using System;
using System.Collections.Generic;
using UnityEngine;

public class Gem_R_Ricochet : Gem
{
	public float searchRange = 5f;

	public float ricochetChance => 1f - 1f / (1f + (float)quality * 0.01f);

	public override void OnEquipSkill(SkillTrigger newSkill)
	{
		base.OnEquipSkill(newSkill);
		newSkill.ActorEvent_OnDealDamage += new Action<EventInfoDamage>(ActorEventOnDealDamage);
		newSkill.ActorEvent_OnDoHeal += new Action<EventInfoHeal>(ActorEventOnDoHeal);
	}

	public override void OnUnequipSkill(SkillTrigger oldSkill)
	{
		base.OnUnequipSkill(oldSkill);
		if ((UnityEngine.Object)(object)oldSkill != null)
		{
			oldSkill.ActorEvent_OnDealDamage -= new Action<EventInfoDamage>(ActorEventOnDealDamage);
			oldSkill.ActorEvent_OnDoHeal -= new Action<EventInfoHeal>(ActorEventOnDoHeal);
		}
	}

	private void ActorEventOnDealDamage(EventInfoDamage obj)
	{
		if (obj.chain.DidReact(this) || UnityEngine.Random.value > ricochetChance)
		{
			return;
		}
		List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, obj.victim.agentPosition, searchRange, tvDefaultHarmfulEffectTargets);
		List<Entity> list2 = DewPool.GetList(out ListReturnHandle<Entity> handle2);
		foreach (Entity item in list)
		{
			if ((UnityEngine.Object)(object)obj.victim != (UnityEngine.Object)(object)item)
			{
				list2.Add(item);
			}
		}
		if (list2.Count > 0)
		{
			Entity target = list2[UnityEngine.Random.Range(0, list2.Count)];
			Vector3 start = obj.victim.Visual.GetCenterPosition();
			CreateAbilityInstance(start, null, new CastInfo(owner, target), (Ai_Gem_R_Ricochet ai) =>
			{
				ai.SetCustomStartPosition(start);
				ai.isHeal = false;
				ai.originDamage = obj.damage;
				ai.chain = obj.chain.New(this);
			});
			NotifyUse();
		}
		handle.Return();
		handle2.Return();
	}

	private void ActorEventOnDoHeal(EventInfoHeal obj)
	{
		if (obj.chain.DidReact(this) || UnityEngine.Random.value > ricochetChance)
		{
			return;
		}
		List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, obj.target.agentPosition, searchRange, tvDefaultUsefulEffectTargets);
		List<Entity> list2 = DewPool.GetList(out ListReturnHandle<Entity> handle2);
		foreach (Entity item in list)
		{
			if ((UnityEngine.Object)(object)obj.target != (UnityEngine.Object)(object)item)
			{
				list2.Add(item);
			}
		}
		if (list2.Count > 0)
		{
			Entity target = Dew.SelectRandomWeightedInList(list2, (Entity e) => 1f - e.normalizedHealth + 0.01f, null);
			Vector3 start = obj.target.Visual.GetCenterPosition();
			CreateAbilityInstance(start, null, new CastInfo(owner, target), (Ai_Gem_R_Ricochet ai) =>
			{
				ai.SetCustomStartPosition(start);
				ai.isHeal = true;
				ai.originHeal = obj.heal;
				ai.chain = obj.chain.New(this);
			});
			NotifyUse();
		}
		handle.Return();
		handle2.Return();
	}

	private void MirrorProcessed()
	{
	}
}
