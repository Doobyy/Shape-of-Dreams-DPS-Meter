using System;
using Mirror;
using UnityEngine;

public class Mon_SnowMountain_SnowWolf : Monster, ISpawnableAsMiniBoss
{
	private Se_Mon_SnowMountain_SnowWolf_SlowApproach _slowApproach;

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if ((UnityEngine.Object)(object)_slowApproach == null)
		{
			_slowApproach = DewResources.GetByType<Se_Mon_SnowMountain_SnowWolf_SlowApproach>(default(ResourceLoadSettings));
		}
		if (!Status.HasStatusEffect<Se_Mon_SnowMountain_SnowWolf_SlowApproach>() && !((UnityEngine.Object)(object)context.targetEnemy == null))
		{
			float num = Vector3.Distance(context.targetEnemy.position, position);
			if (AI.Helper_CanBeCast<At_Mon_SnowMountain_SnowWolf_SlowApproach>() && AI.Helper_IsTargetInRange<At_Mon_SnowMountain_SnowWolf_SlowApproach>() && num >= _slowApproach.distanceRange.x && num <= _slowApproach.distanceRange.y)
			{
				AI.Helper_CastAbilityAuto<At_Mon_SnowMountain_SnowWolf_SlowApproach>();
			}
			else if (AI.Helper_CanBeCast<At_Mon_SnowMountain_SnowWolf_Pounce>() && AI.Helper_IsTargetInRange<At_Mon_SnowMountain_SnowWolf_Pounce>())
			{
				AI.Helper_CastAbilityAuto<At_Mon_SnowMountain_SnowWolf_Pounce>();
			}
			else
			{
				AI.Helper_ChaseTarget();
			}
		}
	}

	public void OnBeforeSpawnAsMiniBoss()
	{
	}

	public void OnCreateAsMiniBoss()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		ISpawnableAsMiniBoss.GiveGenericMiniBossBonus(this);
		At_Mon_SnowMountain_SnowWolf_Pounce ability = Ability.GetAbility<At_Mon_SnowMountain_SnowWolf_Pounce>();
		ability.configs[0].cooldownTime *= 0.35f;
		ability.configs[0].castMethod._length *= 2f;
		ActorEvent_OnAbilityInstanceBeforePrepare += (Action<EventInfoAbilityInstance>)((EventInfoAbilityInstance ins) =>
		{
			if (ins.instance is Ai_Mon_SnowMountain_SnowWolf_Pounce { dash: var dash })
			{
				dash.distance *= 1.5f;
			}
		});
	}

	private void MirrorProcessed()
	{
	}
}
