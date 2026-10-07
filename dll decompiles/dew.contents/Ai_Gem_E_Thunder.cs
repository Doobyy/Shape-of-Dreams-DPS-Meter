using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Gem_E_Thunder : AbilityInstance, ACH_THEYRE_JUST_BIG_CATS.ILightingActor
{
	public float delay;

	public GameObject thunderEffect;

	public GameObject hitEffect;

	public DewCollider range;

	public AbilityTargetValidator hittable;

	public ScalingValue damage;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			yield return new SI.WaitForSeconds(delay);
			FxPlayNetworked(thunderEffect);
			List<Entity> entities = range.GetEntities(out var handle, hittable, info.caster);
			for (int i = 0; i < entities.Count; i++)
			{
				Damage(damage).SetElemental(ElementalType.Light).Dispatch(entities[i]);
				FxPlayNewNetworked(hitEffect, entities[i]);
			}
			handle.Return();
			Destroy();
		}
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if ((Object)(object)info.target != null)
		{
			((Component)(object)this).transform.position = info.target.Visual.GetBasePosition();
		}
	}

	private void MirrorProcessed()
	{
	}
}
