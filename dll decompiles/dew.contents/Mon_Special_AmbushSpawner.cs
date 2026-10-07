using System.Collections;
using Mirror;
using UnityEngine;

public class Mon_Special_AmbushSpawner : Monster, IForceHeroicHealthbar
{
	public float range;

	public float rotSpeed;

	private EntityTransformModifier _entTransform;

	private float _angle;

	protected override void OnCreate()
	{
		base.OnCreate();
		_entTransform = Visual.GetNewTransformModifier();
		FxPlay(((Component)(object)this).gameObject, this);
		if (((NetworkBehaviour)this).isServer)
		{
			CreateBasicEffect(this, new UnstoppableEffect(), float.PositiveInfinity);
		}
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		base.AIUpdate(ref context);
		Hero closestAliveHero = Dew.GetClosestAliveHero(agentPosition, fallbackToDead: false, this);
		if (!closestAliveHero.IsNullOrInactive() && (int)Dew.GetNavMeshPathStatus(agentPosition, closestAliveHero.GetAIAgentPosition(this)) == 0 && AI.Helper_CanBeCast<At_Mon_Special_AmbushSpawner_Spawn>() && !(NetworkedManagerBase<GameManager>.instance.spawnedPopulation > NetworkedManagerBase<GameManager>.instance.maxSpawnedPopulation * 1.5f))
		{
			AI.Helper_CastAbilityAuto<At_Mon_Special_AmbushSpawner_Spawn>();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (_entTransform != null)
		{
			_entTransform.Stop();
			_entTransform = null;
		}
	}

	protected override void OnDeath(EventInfoKill info)
	{
		base.OnDeath(info);
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkedManagerBase<FlavourManager>.instance.FxPlayNewNetworked(NetworkedManagerBase<FlavourManager>.instance.hitStopDealDamage, this);
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			Vector3 pos = agentPosition;
			yield return new WaitForSeconds(1.5f);
			SingletonDewNetworkBehaviour<Room>.instance.rewards.DropChaosReward(pos, isHighQuality: false);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (_entTransform != null)
		{
			_angle += rotSpeed * dt;
			_entTransform.rotation = Quaternion.Euler(0f, _angle, 0f);
		}
	}

	private void MirrorProcessed()
	{
	}
}
