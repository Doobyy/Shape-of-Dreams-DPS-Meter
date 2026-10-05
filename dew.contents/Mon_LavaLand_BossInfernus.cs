using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Mon_LavaLand_BossInfernus : BossMonster, IPrewarmMonsterContributor
{
	public float widenShoulderAmount = 0.15f;

	public float headLiftAngle = 30f;

	private Transform _spine;

	private Transform _leftShoulder;

	private Transform _rightShoulder;

	private Transform _head;

	public void ContributeMonsterPrewarm(Dictionary<Monster, int> counts, int instanceCount)
	{
		Mon_LavaLand_InfernusPillar byType = DewResources.GetByType<Mon_LavaLand_InfernusPillar>(default(ResourceLoadSettings));
		if (!((UnityEngine.Object)(object)byType == null))
		{
			counts.TryGetValue(byType, out var value);
			counts[byType] = value + 15 * instanceCount;
		}
	}

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
			CreateBasicEffect(this, new UnstoppableEffect(), float.PositiveInfinity);
		}
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if (!((UnityEngine.Object)(object)context.targetEnemy == null))
		{
			AI.Helper_ChaseTarget();
		}
	}

	private void LateUpdate()
	{
		if (!((UnityEngine.Object)(object)Animation.animator == null) && ((Behaviour)(object)Animation.animator).enabled)
		{
			if (_spine == null)
			{
				_spine = Animation.animator.GetBoneTransform((HumanBodyBones)7);
				_leftShoulder = Animation.animator.GetBoneTransform((HumanBodyBones)11);
				_rightShoulder = Animation.animator.GetBoneTransform((HumanBodyBones)12);
				_head = Animation.animator.GetBoneTransform((HumanBodyBones)10);
			}
			if (!(_spine == null))
			{
				Quaternion quaternion = _spine.rotation;
				_leftShoulder.position += quaternion * new Vector3(0f, 0f, 0f - widenShoulderAmount);
				_rightShoulder.position += quaternion * new Vector3(0f, 0f, widenShoulderAmount);
				_head.rotation *= Quaternion.Euler(0f, 0f, 0f - headLiftAngle);
			}
		}
	}

	public override Type GetUniqueReward()
	{
		return typeof(Gem_U_EternalFlame);
	}

	private void MirrorProcessed()
	{
	}
}
