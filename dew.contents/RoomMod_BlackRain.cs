public class RoomMod_BlackRain : RoomModifierBase
{
	public override void OnStartServer()
	{
		base.OnStartServer();
		ModifyEntities((Entity e) =>
		{
			if (e is Monster || e is Hero || e is Summon)
			{
				e.CreateStatusEffect<Se_RoomMod_BlackRain>(e, new CastInfo(e));
			}
		}, (Entity e) =>
		{
			if (e.Status.TryGetStatusEffect<Se_RoomMod_BlackRain>(out var effect))
			{
				effect.Destroy();
			}
		});
	}

	private void MirrorProcessed()
	{
	}
}
