using System;
using Mirror;
using UnityEngine;

public class Mon_DarkCave_Oppressor : Monster, ISpawnableAsMiniBoss
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
		base.AIUpdate(ref context);
		if (!((UnityEngine.Object)(object)context.targetEnemy == null))
		{
			if (AI.Helper_CanBeCast<At_Mon_DarkCave_Oppressor_Hypnotize>() && AI.Helper_IsTargetInRange<At_Mon_DarkCave_Oppressor_Hypnotize>())
			{
				AI.Helper_CastAbilityAuto<At_Mon_DarkCave_Oppressor_Hypnotize>();
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
		float hypnotizeScale = 1.25f;
		Ability.GetAbility<At_Mon_DarkCave_Oppressor_Hypnotize>().configs[0].effectOnCast.transform.localScale *= hypnotizeScale;
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		ISpawnableAsMiniBoss.GiveGenericMiniBossBonus(this);
		ActorEvent_OnAbilityInstanceBeforePrepare += (Action<EventInfoAbilityInstance>)((EventInfoAbilityInstance ai) =>
		{
			if (ai.instance is Ai_Mon_DarkCave_Oppressor_Hypnotize ai_Mon_DarkCave_Oppressor_Hypnotize)
			{
				ai_Mon_DarkCave_Oppressor_Hypnotize.NetworkscaleMultiplier = hypnotizeScale;
			}
		});
	}

	private void MirrorProcessed()
	{
	}
}
