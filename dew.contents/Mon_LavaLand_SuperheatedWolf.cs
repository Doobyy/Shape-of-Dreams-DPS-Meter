using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Mon_LavaLand_SuperheatedWolf : Monster, ISpawnableAsMiniBoss
{
	public float selfDestructChance = 0.5f;

	public float selfDestructHpRatioThreshold = 0.6f;

	public float selfDestructPropagateRadius = 5f;

	public Vector2 selfDestructPropagateDelay;

	[NonSerialized]
	public bool didSelfDestruct;

	private bool _wantsToSelfDestructImmediately;

	private bool _didPropagate;

	public override void OnStartServer()
	{
		base.OnStartServer();
		AddData(new LavaLand_Lava.Ad_Lava
		{
			isImmune = true
		});
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_wantsToSelfDestructImmediately = false;
			_didPropagate = false;
			didSelfDestruct = false;
		}
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if ((UnityEngine.Object)(object)context.targetEnemy == null)
		{
			return;
		}
		if ((_wantsToSelfDestructImmediately || (UnityEngine.Random.value < selfDestructChance && normalizedHealth < selfDestructHpRatioThreshold && AI.Helper_IsTargetInRange<At_Mon_LavaLand_SuperheatedWolf_SelfDestruct>())) && AI.Helper_CanBeCast<At_Mon_LavaLand_SuperheatedWolf_SelfDestruct>())
		{
			AI.Helper_CastAbilityAuto<At_Mon_LavaLand_SuperheatedWolf_SelfDestruct>();
			if (!_didPropagate)
			{
				_didPropagate = true;
				((MonoBehaviour)(object)this).StartCoroutine(Routine());
			}
		}
		AI.Helper_ChaseTarget();
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(UnityEngine.Random.Range(selfDestructPropagateDelay.x, selfDestructPropagateDelay.y));
			List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, agentPosition, selfDestructPropagateRadius);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] is Mon_LavaLand_SuperheatedWolf mon_LavaLand_SuperheatedWolf)
				{
					mon_LavaLand_SuperheatedWolf._wantsToSelfDestructImmediately = true;
				}
			}
			handle.Return();
		}
	}

	public void OnBeforeSpawnAsMiniBoss()
	{
	}

	public void OnCreateAsMiniBoss()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			ISpawnableAsMiniBoss.GiveGenericMiniBossBonus(this);
			Status.AddStatBonus(new StatBonus
			{
				movementSpeedPercentage = 30f,
				attackSpeedPercentage = 30f
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
