using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_Star_Aurena_L_ShareHeal : StarEffect
{
	public StarScalingValue shareRatio;

	public GameObject fxHealHit;

	public override Type heroType => typeof(Hero_Aurena);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.EntityEvent_OnTakeHeal += new Action<EventInfoHeal>(EntityEventOnTakeHeal);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)hero != null)
		{
			hero.EntityEvent_OnTakeHeal -= new Action<EventInfoHeal>(EntityEventOnTakeHeal);
		}
	}

	private void EntityEventOnTakeHeal(EventInfoHeal obj)
	{
		if (obj.chain.DidReact(this))
		{
			return;
		}
		List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, hero.agentPosition, 7f, tvDefaultUsefulEffectTargets, new CollisionCheckSettings
		{
			includeUncollidable = true
		});
		Entity entity = null;
		float num = 1f;
		foreach (Entity item in list)
		{
			if (!item.IsNullInactiveDeadOrKnockedOut() && !((UnityEngine.Object)(object)item == (UnityEngine.Object)(object)hero) && !(item.normalizedHealth > num))
			{
				num = item.normalizedHealth;
				entity = item;
			}
		}
		handle.Return();
		if (!entity.IsNullInactiveDeadOrKnockedOut())
		{
			float value = GetValue(shareRatio);
			FxPlayNewNetworked(fxHealHit, entity);
			Heal((obj.amount + obj.discardedAmount) * value).Dispatch(entity, obj.chain.New(this));
		}
	}

	private void MirrorProcessed()
	{
	}
}
