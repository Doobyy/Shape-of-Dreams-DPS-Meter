using System;
using Mirror;
using UnityEngine;

public class Mon_Ink_DivineAnimal : Monster, ISpawnableAsMiniBoss
{
	public bool applyUnstoppable;

	public override void OnStartServer()
	{
		base.OnStartServer();
		if (applyUnstoppable)
		{
			CreateBasicEffect(this, new UnstoppableEffect(), float.PositiveInfinity);
			if (!Visual.skipSpawning)
			{
				CreateAbilityInstance<Ai_Mon_Ink_DivineAnimal_Stomp>(Dew.GetPositionOnGround(((Component)(object)this).transform.position), null, new CastInfo(this));
			}
		}
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if (!((UnityEngine.Object)(object)context.targetEnemy == null))
		{
			if (AI.Helper_CanBeCast<At_Mon_Ink_DivineAnimal_Bite>() && AI.Helper_IsTargetInRange<At_Mon_Ink_DivineAnimal_Bite>())
			{
				AI.Helper_CastAbilityAuto<At_Mon_Ink_DivineAnimal_Bite>();
			}
			if (AI.Helper_CanBeCast<At_Mon_Ink_DivineAnimal_Missile>() && AI.Helper_IsTargetInRange<At_Mon_Ink_DivineAnimal_Missile>())
			{
				AI.Helper_CastAbilityAuto<At_Mon_Ink_DivineAnimal_Missile>();
			}
			AI.Helper_ChaseTarget();
		}
	}

	public void OnBeforeSpawnAsMiniBoss()
	{
	}

	public void OnCreateAsMiniBoss()
	{
		TriggerConfig triggerConfig = Ability.GetAbility<At_Mon_Ink_DivineAnimal_Bite>().configs[0];
		triggerConfig.channel.duration *= 0.7f;
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		ISpawnableAsMiniBoss.GiveGenericMiniBossBonus(this);
		ActorEvent_OnAbilityInstanceBeforePrepare += (Action<EventInfoAbilityInstance>)((EventInfoAbilityInstance obj) =>
		{
			if (obj.instance is Ai_Mon_Ink_DivineAnimal_Missile ai_Mon_Ink_DivineAnimal_Missile)
			{
				ai_Mon_Ink_DivineAnimal_Missile.missileCount *= 3;
			}
		});
		triggerConfig.cooldownTime = 0f;
		Status.AddStatBonus(new StatBonus
		{
			abilityHasteFlat = 100f
		});
	}

	private void MirrorProcessed()
	{
	}
}
