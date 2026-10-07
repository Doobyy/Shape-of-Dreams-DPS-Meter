using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gem_R_Dusk : Gem
{
	public float empowerDuration = 4f;

	public int shootCount = 3;

	public float shootInterval = 0.15f;

	public float radius = 2f;

	protected override void OnCastComplete(EventInfoCast info)
	{
		base.OnCastComplete(info);
		AttackEmpowerEffect ae = new AttackEmpowerEffect();
		ae.onAttackEffect = (EventInfoAttackEffect effect, int i) =>
		{
			if (isValid && !effect.chain.DidReact(this))
			{
				((MonoBehaviour)(object)effect.actor).StartCoroutine(Routine());
			}
			IEnumerator Routine()
			{
				for (int j = 0; j < shootCount; j++)
				{
					if (!isValid)
					{
						break;
					}
					List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, effect.victim.position, radius, tvDefaultHarmfulEffectTargets);
					if (list.Count > 0)
					{
						CreateAbilityInstanceWithSource(ae.parent, owner.position, Quaternion.identity, new CastInfo(owner, list[Random.Range(0, list.Count)]), (Ai_Gem_R_Dusk_Projectile ai) =>
						{
							ai.chain = effect.chain.New(this);
							ai._strength = effect.strength;
						});
					}
					handle.Return();
					NotifyUse();
					yield return new WaitForSeconds(shootInterval);
				}
			}
		};
		CreateBasicEffectWithSource(info.instance, owner, ae, empowerDuration, "dusk_empower");
	}

	private void MirrorProcessed()
	{
	}
}
