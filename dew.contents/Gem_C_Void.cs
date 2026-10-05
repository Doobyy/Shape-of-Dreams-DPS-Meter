using UnityEngine;

public class Gem_C_Void : Gem
{
	public float maxDuration = 4f;

	protected override void OnCastComplete(EventInfoCast info)
	{
		base.OnCastComplete(info);
		AttackEmpowerEffect ae = new AttackEmpowerEffect();
		ae.maxTriggerCount = 1;
		ae.onAttackEffect = (EventInfoAttackEffect effect, int i) =>
		{
			if (effect.type != AttackEffectType.BasicAttackSub && !effect.chain.DidReact(this))
			{
				CreateAbilityInstanceWithSource(ae.parent, effect.victim.position, Quaternion.identity, new CastInfo(owner), (Ai_Gem_C_Void_Explosion ai) =>
				{
					ai.chain = effect.chain.New(this);
					ai.strength = effect.strength;
				});
				ae.parent.Destroy();
			}
		};
		CreateBasicEffectWithSource(info.instance, owner, ae, maxDuration, "void_empowerattack");
		ResetCooldown(owner.Ability.attackAbility);
		NotifyUse();
	}

	private void MirrorProcessed()
	{
	}
}
