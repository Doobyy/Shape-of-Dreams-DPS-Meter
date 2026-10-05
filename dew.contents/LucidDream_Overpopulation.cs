using Mirror;
using UnityEngine;

public class LucidDream_Overpopulation : LucidDream
{
	public float popMultiplier = 1.5f;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkedManagerBase<GameManager>.instance.maxAndSpawnedPopulationMultiplier *= popMultiplier;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (Object)(object)NetworkedManagerBase<GameManager>.instance != null)
		{
			NetworkedManagerBase<GameManager>.instance.maxAndSpawnedPopulationMultiplier /= popMultiplier;
		}
	}

	private void MirrorProcessed()
	{
	}
}
