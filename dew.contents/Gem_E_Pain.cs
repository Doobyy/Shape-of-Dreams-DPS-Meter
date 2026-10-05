using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Gem_E_Pain : Gem
{
	[Serializable]
	public class ElementalEffect
	{
		public GameObject hitEffect;

		public GameObject skillEffect;
	}

	public AbilityTargetValidator validator;

	public ScalingValue damageRatio;

	public DewCollider range;

	public float procCoefficientMultiplier = 0.5f;

	[SerializeField]
	private ElementalEffect _fireEffects;

	[SerializeField]
	private ElementalEffect _coldEffects;

	[SerializeField]
	private ElementalEffect _lightEffects;

	[SerializeField]
	private ElementalEffect _darkEffects;

	[SerializeField]
	private ElementalEffect _normalEffects;

	private GameObject _hit;

	private GameObject _effect;

	public override void OnEquipSkill(SkillTrigger newSkill)
	{
		base.OnEquipSkill(newSkill);
		if (((NetworkBehaviour)this).isServer)
		{
			newSkill.ActorEvent_OnDealDamage += new Action<EventInfoDamage>(ActorEventOnDealDamage);
		}
	}

	public override void OnUnequipSkill(SkillTrigger oldSkill)
	{
		base.OnUnequipSkill(oldSkill);
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)oldSkill != null)
		{
			oldSkill.ActorEvent_OnDealDamage -= new Action<EventInfoDamage>(ActorEventOnDealDamage);
		}
	}

	private void ActorEventOnDealDamage(EventInfoDamage obj)
	{
		if ((UnityEngine.Object)(object)owner == null || (UnityEngine.Object)(object)skill == null || (UnityEngine.Object)(object)obj.actor == null || !owner.CheckEnemyOrNeutral(obj.victim) || obj.chain.DidReact(this))
		{
			return;
		}
		ElementalEffect elementalEffect = obj.damage.elemental switch
		{
			ElementalType.Fire => _fireEffects, 
			ElementalType.Cold => _coldEffects, 
			ElementalType.Light => _lightEffects, 
			ElementalType.Dark => _darkEffects, 
			null => _normalEffects, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		_hit = elementalEffect.hitEffect;
		_effect = elementalEffect.skillEffect;
		FxPlayNewNetworked(_effect, obj.victim.position, Quaternion.identity);
		range.transform.position = obj.victim.position;
		List<Entity> entities = range.GetEntities(out var handle, validator, owner);
		float value = GetValue(damageRatio);
		foreach (Entity item in entities)
		{
			if (!((UnityEngine.Object)(object)item == (UnityEngine.Object)(object)obj.victim))
			{
				DamageData damageData = obj.actor.DefaultDamage((obj.damage.amount + obj.damage.discardedAmount) * value, obj.damage.procCoefficient * procCoefficientMultiplier).SetAmountOrigin(obj.damage).SetOriginPosition(obj.victim.position)
					.SetSourceType(obj.damage.type);
				if (obj.damage.elemental.HasValue)
				{
					damageData.SetElemental(obj.damage.elemental.Value);
				}
				damageData.Dispatch(item, obj.chain.New(this));
				FxPlayNewNetworked(_hit, item);
			}
		}
		handle.Return();
		NotifyUse();
	}

	private void MirrorProcessed()
	{
	}
}
