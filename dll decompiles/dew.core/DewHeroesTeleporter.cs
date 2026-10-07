using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class DewHeroesTeleporter : MonoBehaviour, IPlayerPathablePoint
{
	public Transform destination;

	public float spacing = 2f;

	public Vector3 pathablePosition => Dew.GetPositionOnGround(destination.position);

	public void TeleportAllHeroes()
	{
		if (!NetworkServer.active)
		{
			throw new InvalidOperationException();
		}
		Vector3 positionOnGround = Dew.GetPositionOnGround(destination.position);
		Vector3 normalized = destination.right.Flattened().normalized;
		List<Hero> allHeroes = NetworkedManagerBase<ActorManager>.instance.allHeroes;
		Vector3 end = positionOnGround - normalized * (spacing * (float)(allHeroes.Count - 1)) / 2f;
		foreach (Hero item in allHeroes)
		{
			if (item.IsNullInactiveDeadOrKnockedOut())
			{
				continue;
			}
			item.Control.CancelOngoingChannels();
			item.Control.CancelOngoingDisplacement();
			Vector3 validAgentDestination_LinearSweep = Dew.GetValidAgentDestination_LinearSweep(positionOnGround, end);
			item.Control.Teleport(validAgentDestination_LinearSweep);
			foreach (Summon summon in item.summons)
			{
				summon.Control.Teleport(validAgentDestination_LinearSweep + UnityEngine.Random.onUnitSphere.Flattened());
			}
			end += spacing * normalized;
		}
	}
}
