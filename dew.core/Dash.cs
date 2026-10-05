using System;
using UnityEngine;

[Serializable]
public class Dash
{
	public float distance = 2.25f;

	public float duration = 0.25f;

	public DewEase ease;

	public bool rotateForward = true;

	public bool affectedByMovementSpeed;

	public bool useLinearSweep = true;

	public bool canGoOverTerrain;

	public DispByDestination ApplyByDestination(Entity ent, Vector3 dest, Action<DispByDestination> beforeApply = null)
	{
		return ApplyByDirection(ent, dest - ent.agentPosition, beforeApply);
	}

	public DispByDestination ApplyByDirection(Entity ent, Vector3 dir, Action<DispByDestination> beforeApply = null)
	{
		Vector3 destination = (useLinearSweep ? Dew.GetValidAgentDestination_LinearSweep(ent.agentPosition, ent.agentPosition + dir.normalized * distance) : Dew.GetValidAgentDestination_Closest(ent.agentPosition, ent.agentPosition + dir.normalized * distance));
		DispByDestination dispByDestination = new DispByDestination
		{
			canGoOverTerrain = canGoOverTerrain,
			destination = destination,
			duration = duration,
			ease = ease,
			isCanceledByCC = true,
			isFriendly = true,
			rotateForward = rotateForward,
			affectedByMovementSpeed = affectedByMovementSpeed
		};
		beforeApply?.Invoke(dispByDestination);
		ent.Control.StartDisplacement(dispByDestination);
		return dispByDestination;
	}
}
