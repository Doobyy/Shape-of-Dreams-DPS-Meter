using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Sum_R_SummonLittleBaam_Baam : Summon
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			CreateBasicEffect(this, new UnstoppableEffect(), float.PositiveInfinity);
		}
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		float effectiveRange = Ability.attackAbility.configs[0].effectiveRange;
		if ((UnityEngine.Object)(object)context.targetEnemy != null && (context.targetEnemy.agentPosition - agentPosition).sqrMagnitude > effectiveRange * effectiveRange)
		{
			List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, agentPosition, effectiveRange, tvDefaultHarmfulEffectTargets);
			if (list.Count > 0)
			{
				Entity target = Dew.SelectBestWithScore((IList<Entity>)list, (Func<Entity, int, float>)((Entity entity, int i) => 0f - (entity.agentPosition - agentPosition).sqrMagnitude), 0f, (DewRandom)null);
				AI.Aggro(target);
			}
			handle.Return();
		}
		base.AIUpdate(ref context);
	}

	private void MirrorProcessed()
	{
	}
}
