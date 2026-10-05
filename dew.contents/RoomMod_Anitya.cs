using UnityEngine;

public class RoomMod_Anitya : RoomModifierBase
{
	public override void OnStartServer()
	{
		base.OnStartServer();
		if (isNewInstance)
		{
			GameManager.CallOnReady(() =>
			{
				SingletonDewNetworkBehaviour<Room>.instance.GetFinalSection().TryGetGoodNodePosition(out var pos);
				CreateActor<Shrine_Anitya>(pos, Quaternion.AngleAxis(ManagerBase<CameraManager>.instance.entityCamAngle + 180f, Vector3.up));
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
