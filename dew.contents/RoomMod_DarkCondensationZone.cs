public class RoomMod_DarkCondensationZone : RoomModifierBase
{
	public override void OnStartServer()
	{
		base.OnStartServer();
		ModifyEntities((Entity e) =>
		{
			if (e is Monster)
			{
				e.CreateStatusEffect<Se_DarkCondensationZone>(e, new CastInfo(e));
			}
		}, (Entity e) =>
		{
			if (e.Status.TryGetStatusEffect<Se_DarkCondensationZone>(out var effect))
			{
				effect.Destroy();
			}
		});
	}

	private void MirrorProcessed()
	{
	}
}
