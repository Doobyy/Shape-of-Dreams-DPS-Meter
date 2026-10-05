using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Gem_E_Blossom : Gem
{
	public GameObject healEffect;

	public ScalingValue conversionRatio;

	public float damageReduction;

	public override void OnEquipSkill(SkillTrigger newSkill)
	{
		base.OnEquipSkill(newSkill);
		if (((NetworkBehaviour)this).isServer)
		{
			newSkill.dealtDamageProcessor.Add(Processor);
			newSkill.ActorEvent_OnDealDamage += new Action<EventInfoDamage>(ConvertToHeal);
		}
	}

	public override void OnUnequipSkill(SkillTrigger oldSkill)
	{
		base.OnUnequipSkill(oldSkill);
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)oldSkill != null)
		{
			oldSkill.dealtDamageProcessor.Remove(Processor);
			oldSkill.ActorEvent_OnDealDamage -= new Action<EventInfoDamage>(ConvertToHeal);
		}
	}

	private void Processor(ref DamageData data, Actor actor, Entity target)
	{
		if (isValid && owner.CheckEnemyOrNeutral(target))
		{
			data.ApplyReduction(damageReduction);
		}
	}

	private void ConvertToHeal(EventInfoDamage obj)
	{
		if (!isValid || obj.chain.DidReact(this) || !owner.CheckEnemyOrNeutral(obj.victim))
		{
			return;
		}
		HealData healData = Heal(obj.damage.amount * GetValue(conversionRatio)).SetAmountOrigin(obj.damage);
		List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, owner.position, 20f, tvDefaultUsefulEffectTargets, new CollisionCheckSettings
		{
			includeUncollidable = true
		});
		if (list.Count > 0)
		{
			Entity entity = Dew.SelectRandomWeightedInList(list, (Entity e) => (!e.IsNullInactiveDeadOrKnockedOut()) ? Mathf.Clamp(1f - e.currentHealth / e.maxHealth, 0.001f, 1f) : 0f, null);
			healData.Dispatch(entity, obj.chain.New(this));
			FxPlayNewNetworked(healEffect, entity);
			NotifyUse();
		}
		handle.Return();
	}

	private void MirrorProcessed()
	{
	}
}
