using Mirror;
using UnityEngine;

public class Forest_LoopCat_Spawner : Actor
{
	public bool enableForceSpawn;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer && (enableForceSpawn || (NetworkedManagerBase<ZoneManager>.instance.loopIndex >= 1 && !SingletonDewNetworkBehaviour<Room>.instance.isRevisit && !(Random.value > 0.5f))))
		{
			Forest_LoopCat_SpawnPosition[] componentsInChildren = ((Component)(object)this).GetComponentsInChildren<Forest_LoopCat_SpawnPosition>();
			Vector3 vector = componentsInChildren[Random.Range(0, componentsInChildren.Length)].transform.position;
			Vector3 forward = ((Component)(object)Rift_RoomExit.instance).transform.position - vector;
			CreateActor<Shrine_LoopCat>(vector, Quaternion.LookRotation(forward));
		}
	}

	private void MirrorProcessed()
	{
	}
}
