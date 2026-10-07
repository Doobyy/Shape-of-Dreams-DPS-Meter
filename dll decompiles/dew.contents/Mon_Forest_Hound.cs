using System;
using Mirror;
using UnityEngine;

public class Mon_Forest_Hound : Monster, ISpawnableAsMiniBoss
{
	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if (!((UnityEngine.Object)(object)context.targetEnemy == null))
		{
			if (AI.Helper_CanBeCast<At_Mon_Forest_Hound_Charge>() && AI.Helper_IsTargetInRange<At_Mon_Forest_Hound_Charge>())
			{
				AI.Helper_CastAbilityAuto<At_Mon_Forest_Hound_Charge>();
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
		At_Mon_Forest_Hound_Charge ability = Ability.GetAbility<At_Mon_Forest_Hound_Charge>();
		TriggerConfig[] configs = ability.configs;
		foreach (TriggerConfig obj in configs)
		{
			obj.maxCharges = 3;
			obj.addedCharges = 3;
		}
		ability.TriggerEvent_OnCastCompleteBeforePrepare += (Action<EventInfoCast>)((EventInfoCast cast) =>
		{
			if (cast.instance is Ai_Mon_Forest_Hound_Charge ai_Mon_Forest_Hound_Charge)
			{
				ai_Mon_Forest_Hound_Charge.duration *= 0.7f;
			}
		});
	}

	private void MirrorProcessed()
	{
	}
}
