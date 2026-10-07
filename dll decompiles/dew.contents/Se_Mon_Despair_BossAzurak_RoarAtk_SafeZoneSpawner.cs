using Mirror;
using UnityEngine;

public class Se_Mon_Despair_BossAzurak_RoarAtk_SafeZoneSpawner : StatusEffect
{
	public DewCollider range;

	protected override void OnCreate()
	{
		((Component)(object)this).transform.rotation = Quaternion.LookRotation(victim.agentPosition - info.caster.agentPosition).Flattened();
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(victim);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		ListReturnHandle<Entity> handle;
		foreach (Entity entity in range.GetEntities(out handle, tvDefaultHarmfulEffectTargets))
		{
			if (!entity.IsNullInactiveDeadOrKnockedOut() && !(entity is Mon_Despair_AzurakRollPillar))
			{
				if (entity.Status.TryGetStatusEffect<Se_Mon_Despair_BossAzurak_RoarAtk_SafeZone>(out var effect))
				{
					effect.ResetTimer();
				}
				else
				{
					CreateStatusEffect<Se_Mon_Despair_BossAzurak_RoarAtk_SafeZone>(entity, new CastInfo(victim, entity));
				}
			}
		}
		handle.Return();
	}

	private void MirrorProcessed()
	{
	}
}
