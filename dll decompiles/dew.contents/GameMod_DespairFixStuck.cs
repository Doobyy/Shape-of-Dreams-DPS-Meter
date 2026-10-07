using Mirror;
using UnityEngine;

public class GameMod_DespairFixStuck : GameModifierBase
{
	private float _lastCheckTime;

	protected override void ActiveLogicUpdate(float dt)
	{
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || Time.time - _lastCheckTime < 1.5f)
		{
			return;
		}
		_lastCheckTime = Time.time;
		if (NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition || NetworkedManagerBase<ZoneManager>.instance.nodes.Count == 0 || !NetworkedManagerBase<ZoneManager>.instance.isCurrentNodeHunted || NetworkedManagerBase<ZoneManager>.instance.currentNode.type != WorldNodeType.Merchant || NetworkedManagerBase<ZoneManager>.instance.currentZone == null || NetworkedManagerBase<ZoneManager>.instance.currentZone.name != "Zone_Despair")
		{
			return;
		}
		foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
		{
			if (allEntity.Control.isDisplacing || !(allEntity is Monster))
			{
				continue;
			}
			Hero closestAliveHero = Dew.GetClosestAliveHero(allEntity.agentPosition);
			if (closestAliveHero.IsNullInactiveDeadOrKnockedOut())
			{
				break;
			}
			if (!closestAliveHero.Control.isDisplacing && (int)Dew.GetNavMeshPathStatus(allEntity.agentPosition, closestAliveHero.agentPosition) != 0)
			{
				allEntity.Control.Stop();
				allEntity.Control.CancelOngoingChannels();
				allEntity.Control.CancelOngoingDisplacement();
				if (!allEntity.Status.HasStatusEffect<Se_Shrine_Despair_Teleport>())
				{
					Vector3 validAgentDestination_LinearSweep = Dew.GetValidAgentDestination_LinearSweep(closestAliveHero.agentPosition, closestAliveHero.agentPosition + (Random.insideUnitSphere * 3f).Flattened());
					allEntity.CreateStatusEffect<Se_Shrine_Despair_Teleport>(allEntity, new CastInfo(allEntity, validAgentDestination_LinearSweep));
					Debug.Log("GameMod_DespairFixStuck: Teleporting stuck monster " + allEntity.GetActorReadableName() + " to nearest player");
				}
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
