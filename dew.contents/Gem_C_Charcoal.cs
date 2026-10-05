using System.Collections;
using UnityEngine;

public class Gem_C_Charcoal : Gem
{
	public float delayMin = 0.15f;

	public float delayMax = 0.35f;

	public bool useProcChance;

	public float procChanceMultiplier = 2f;

	protected override void OnDealDamage(EventInfoDamage info)
	{
		base.OnDealDamage(info);
		float delay;
		if (!info.chain.DidReact(this) && owner.CheckEnemyOrNeutral(info.victim) && (!useProcChance || !(Random.value > info.damage.procCoefficient * procChanceMultiplier)))
		{
			delay = Random.Range(delayMin, delayMax);
			info.actor.LockDestroyFor(delay + 0.1f);
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(delay);
			if (IsReady() && !((Object)(object)owner == null) && !((Object)(object)info.victim == null))
			{
				CreateAbilityInstanceWithSource(info.actor, owner.position, Quaternion.identity, new CastInfo(owner, info.victim), (Ai_Gem_C_Charcoal_Projectile a) =>
				{
					a.chain = info.chain.New(this);
					if (!useProcChance)
					{
						a._strength = info.damage.procCoefficient;
					}
				});
				NotifyUse();
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
