using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_R_PillarOfFlame : AbilityInstance
{
	public GameObject pillarEffect;

	public GameObject hitEffect;

	public float delay = 0.75f;

	public int ticks;

	public float tickInterval;

	public DewCollider range;

	public ScalingValue initDamage;

	public ScalingValue totalDamage;

	public float initKnockupStrength = 0.7f;

	public float procCoefficient = 0.5f;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		position = info.point;
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		yield return new SI.WaitForSeconds(delay);
		FxPlayNetworked(pillarEffect);
		for (int i = 0; i < ticks; i++)
		{
			List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
			for (int j = 0; j < entities.Count; j++)
			{
				Entity entity = entities[j];
				if (i == 0)
				{
					entity.Visual.KnockUp(initKnockupStrength, isFriendly: false);
					Damage(initDamage).SetElemental(ElementalType.Fire).SetDirection(Vector3.up).SetAttr(DamageAttribute.AreaOfEffect)
						.Dispatch(entity);
				}
				else
				{
					Damage(totalDamage, procCoefficient).ApplyRawMultiplier(1f / (float)(ticks - 1)).SetElemental(ElementalType.Fire).SetDirection(Vector3.up)
						.SetAttr(DamageAttribute.DamageOverTime)
						.SetAttr(DamageAttribute.AreaOfEffect)
						.Dispatch(entity);
				}
				FxPlayNewNetworked(hitEffect, entity);
			}
			handle.Return();
			if (i < ticks - 1)
			{
				yield return new SI.WaitForSeconds(tickInterval);
			}
		}
		FxStopNetworked(pillarEffect);
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
