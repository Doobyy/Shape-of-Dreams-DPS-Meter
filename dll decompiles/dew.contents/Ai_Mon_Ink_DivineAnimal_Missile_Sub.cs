using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_DivineAnimal_Missile_Sub : AbilityInstance
{
	public GameObject skillEffect;

	public GameObject hitEffect;

	public DewCollider range;

	public AbilityTargetValidator hittable;

	public ScalingValue dmgFactor;

	public float startDelay;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			yield return new SI.WaitForSeconds(startDelay);
			FxPlayNetworked(skillEffect);
			List<Entity> entities = range.GetEntities(out var handle, hittable, info.caster);
			for (int i = 0; i < entities.Count; i++)
			{
				Entity entity = entities[i];
				CreateDamage(DamageData.SourceType.Default, dmgFactor).SetElemental(ElementalType.Fire).Dispatch(entity);
				FxPlayNetworked(hitEffect, entity);
			}
			handle.Return();
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
