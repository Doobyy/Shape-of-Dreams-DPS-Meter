using UnityEngine;

public class Gem_E_Fever : Gem
{
	protected override void OnDealDamage(EventInfoDamage info)
	{
		base.OnDealDamage(info);
		if (owner.CheckEnemyOrNeutral(info.victim) && info.damage.elemental == ElementalType.Fire && !info.chain.DidReact(this) && !(Object)(object)info.victim.Status.FindStatusEffect((Se_Gem_E_Fever_LivingBomb b) => (Object)(object)b.info.caster == (Object)(object)owner))
		{
			CreateStatusEffectWithSource(info.actor, info.victim, new CastInfo(owner), (Se_Gem_E_Fever_LivingBomb se) =>
			{
				se.chain = info.chain.New(this);
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
