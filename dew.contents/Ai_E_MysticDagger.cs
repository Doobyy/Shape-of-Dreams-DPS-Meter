using UnityEngine;

public class Ai_E_MysticDagger : StandardProjectile
{
	public GameObject fxReduceCooldownOnCaster;

	public ScalingValue damage;

	public ScalingValue cooldownReduction;

	public float reducedAmount => Mathf.Clamp(GetValue(cooldownReduction), 0f, 999f);

	public override bool reuseInRoom => true;

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		Damage(damage).SetDirection(info.forward).DoAttackEffect(AttackEffectType.Others).Dispatch(hit.entity);
		float amount = reducedAmount;
		if (info.caster is Hero hero)
		{
			if ((Object)(object)hero.Skill.Q != null)
			{
				ApplyCooldownReduction(hero.Skill.Q, amount);
			}
			if ((Object)(object)hero.Skill.W != null)
			{
				ApplyCooldownReduction(hero.Skill.W, amount);
			}
			if ((Object)(object)hero.Skill.E != null)
			{
				ApplyCooldownReduction(hero.Skill.E, amount);
			}
			if ((Object)(object)hero.Skill.R != null)
			{
				ApplyCooldownReduction(hero.Skill.R, amount);
			}
			if ((Object)(object)hero.Skill.Identity != null)
			{
				ApplyCooldownReduction(hero.Skill.Identity, amount);
			}
			FxPlayNetworked(fxReduceCooldownOnCaster, info.caster);
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
