using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class Mon_Despair_AzurakRollPillar : Monster
{
	public NavMeshObstacle obstacle;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			CreateBasicEffect(this, new UnstoppableEffect(), float.PositiveInfinity, "PillarUnstoppable");
			((Behaviour)(object)obstacle).enabled = true;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		((Behaviour)(object)obstacle).enabled = false;
	}

	private void MirrorProcessed()
	{
	}
}
