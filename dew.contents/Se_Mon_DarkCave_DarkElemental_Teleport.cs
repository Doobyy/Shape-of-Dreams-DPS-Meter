using Mirror;
using UnityEngine;

public class Se_Mon_DarkCave_DarkElemental_Teleport : StatusEffect
{
	public float duration;

	public float minTeleportDistance = 2.5f;

	public Vector2 preferredDistance;

	public DewEase ease;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		SetTimer(duration);
		DoUnstoppable();
		victim.Control.StartDaze(duration);
		victim.Visual.DisableRenderers();
		Vector3 agentPosition = info.target.agentPosition;
		float num = float.NegativeInfinity;
		Vector3 destination = default;
		for (int i = 0; i < 10; i++)
		{
			Vector3 end = agentPosition + Random.onUnitSphere.Flattened().normalized * Random.Range(preferredDistance.x, preferredDistance.y);
			end = Dew.GetValidAgentDestination_LinearSweep(victim.agentPosition, end);
			float num2 = 0f;
			float num3 = Vector3.Distance(agentPosition, end);
			if (num3 < preferredDistance.x)
			{
				num2 -= preferredDistance.x - num3;
			}
			else
			{
				num2 = ((!(num3 > preferredDistance.y)) ? (num2 + 1f) : (num2 - (num3 - preferredDistance.y)));
			}
			float num4 = Vector3.Distance(end, victim.position);
			num2 = ((!(num4 < minTeleportDistance)) ? (num2 + 1f) : (num2 - (minTeleportDistance - num4) * 2f));
			if (num2 > num)
			{
				num = num2;
				destination = end;
			}
		}
		victim.Control.StartDisplacement(new DispByDestination
		{
			affectedByMovementSpeed = false,
			canGoOverTerrain = true,
			destination = destination,
			duration = duration,
			ease = ease,
			isCanceledByCC = false,
			isFriendly = true
		});
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (Object)(object)victim != null)
		{
			victim.Visual.EnableRenderers();
		}
	}

	private void MirrorProcessed()
	{
	}
}
