using Mirror;
using UnityEngine;

public class Gem_R_Abyss : Gem
{
	public float atkEffectPower;

	public int enableAtkEffectStackCount;

	public GameObject fxHit;

	public GameObject fxAtkEffectActivate;

	public float atkEffectChance => 1f - 1f / (1f + (float)quality * 0.0075f);

	public override void OnEquipSkill(SkillTrigger newSkill)
	{
		base.OnEquipSkill(newSkill);
		if (((NetworkBehaviour)this).isServer)
		{
			newSkill.dealtDamageProcessor.Add(SetDarkness, -2000);
		}
	}

	public override void OnUnequipSkill(SkillTrigger oldSkill)
	{
		base.OnUnequipSkill(oldSkill);
		if (((NetworkBehaviour)this).isServer && !oldSkill.IsNullOrInactive())
		{
			oldSkill.dealtDamageProcessor.Remove(SetDarkness);
		}
	}

	private void SetDarkness(ref DamageData data, Actor actor, Entity target)
	{
		if (owner.CheckEnemyOrNeutral(target))
		{
			data.SetElemental(ElementalType.Dark);
			FxPlayNewNetworked(fxHit, target);
		}
	}

	protected override void OnDealDamage(EventInfoDamage info)
	{
		base.OnDealDamage(info);
		if (!((Object)(object)info.victim == (Object)(object)owner) && !info.chain.DidReact(this) && !(Random.value > atkEffectChance) && info.victim.Status.GetElementalStack(ElementalType.Dark) >= enableAtkEffectStackCount)
		{
			FxPlayNewNetworked(fxAtkEffectActivate, info.victim);
			info.actor.TriggerAttackEffects(owner, info.victim, atkEffectPower * info.damage.procCoefficient, AttackEffectType.Others, info.chain.New(this));
			NotifyUse();
		}
	}

	private void MirrorProcessed()
	{
	}
}
