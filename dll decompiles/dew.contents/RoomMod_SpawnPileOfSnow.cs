using Mirror;
using UnityEngine;

public class RoomMod_SpawnPileOfSnow : RoomModifierBase
{
	public Vector2Int pileCount;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (isNewInstance && ((NetworkBehaviour)this).isServer)
		{
			int num = Random.Range(pileCount.x, pileCount.y + 1);
			for (int i = 0; i < num; i++)
			{
				SingletonDewNetworkBehaviour<Room>.instance.props.TryGetGoodNodePosition(out var pos);
				CreateActor<Shrine_PileOfSnow>(pos, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
