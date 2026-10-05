using Mirror;

public class RoomMod_WarpingField : RoomModifierBase
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		ModifyEntities((Entity e) =>
		{
			CreateStatusEffect<Se_RoomMod_WarpingField_Unstable>(e, default);
		}, (Entity e) =>
		{
			if (e.Status.TryGetStatusEffect<Se_RoomMod_WarpingField_Unstable>(out var effect))
			{
				effect.Destroy();
			}
		});
	}

	private void MirrorProcessed()
	{
	}
}
