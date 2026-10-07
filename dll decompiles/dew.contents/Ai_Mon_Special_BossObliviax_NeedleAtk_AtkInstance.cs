using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossObliviax_NeedleAtk_AtkInstance : AbilityInstance
{
	public ScalingValue dmgFactor;

	public Knockback Knockback;

	public float delay;

	public float stunDuration;

	public GameObject fxHit;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		info.caster.Control.StartDaze(delay);
		yield return new SI.WaitForSeconds(delay);
		if ((Object)(object)NetworkedManagerBase<ActorManager>.instance == null)
		{
			yield break;
		}
		foreach (Hero allHero in NetworkedManagerBase<ActorManager>.instance.allHeroes)
		{
			if (!allHero.IsNullInactiveDeadOrKnockedOut() && tvDefaultHarmfulEffectTargets.Evaluate(allHero) && !allHero.Status.hasUncollidable && !allHero.Status.isUndetectableByNonAllies)
			{
				FxPlayNewNetworked(fxHit, allHero);
				float value = GetValue(dmgFactor);
				CreateDamage(DamageData.SourceType.Default, Mathf.Min(value, allHero.currentHealth * 0.5f)).SetOriginPosition(info.caster.agentPosition).Dispatch(allHero);
				Knockback.ApplyWithOrigin(info.caster.agentPosition, allHero);
				CreateBasicEffect(allHero, new StunEffect(), stunDuration, "needleatk_stun");
			}
		}
		info.caster.AI.Aggro(Dew.SelectRandomAliveHero(fallbackToDead: true, skipStealthed: true));
		yield return null;
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
