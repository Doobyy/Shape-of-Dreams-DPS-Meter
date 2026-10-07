using System;
using Mirror;
using UnityEngine;

public class Mon_Despair_WretchedArtillery : Monster, ISpawnableAsMiniBoss
{
	public bool isUnstoppable = true;

	[NonSerialized]
	public Transform[] partTransforms;

	private Vector3[] _localScales;

	public override void OnModelLoaded()
	{
		base.OnModelLoaded();
		partTransforms = Visual.model.GetCustomMappingArray<Transform>("partTransforms");
		_localScales = new Vector3[partTransforms.Length];
		for (int i = 0; i < partTransforms.Length; i++)
		{
			_localScales[i] = partTransforms[i].localScale;
		}
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		if (isUnstoppable)
		{
			CreateBasicEffect(this, new UnstoppableEffect(), float.PositiveInfinity);
		}
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if (!((UnityEngine.Object)(object)context.targetEnemy == null))
		{
			if (AI.Helper_CanBeCast<At_Mon_Despair_WretchedArtillery_Melee>() && AI.Helper_IsTargetInRange<At_Mon_Despair_WretchedArtillery_Melee>())
			{
				AI.Helper_CastAbilityAuto<At_Mon_Despair_WretchedArtillery_Melee>();
			}
			else
			{
				AI.Helper_ChaseTarget();
			}
		}
	}

	private void LateUpdate()
	{
		if (partTransforms == null)
		{
			return;
		}
		for (int i = 0; i < partTransforms.Length; i++)
		{
			if ((bool)partTransforms[i])
			{
				partTransforms[i].localScale = _localScales[i];
			}
		}
	}

	public void OnBeforeSpawnAsMiniBoss()
	{
	}

	public void OnCreateAsMiniBoss()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			ISpawnableAsMiniBoss.GiveGenericMiniBossBonus(this, 0.35f);
			Status.AddStatBonus(new StatBonus
			{
				attackSpeedPercentage = 70f,
				abilityHasteFlat = 80f
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
