public class RoomMod_SymbioteHabitat : RoomModifierBase
{
	public override void OnStartServer()
	{
		base.OnStartServer();
		ModifyEntities((Entity e) =>
		{
			if (e is Monster || e is Hero || e is Summon)
			{
				e.CreateStatusEffect<Se_RoomMod_Symbiote>(e, new CastInfo(e));
			}
		}, (Entity e) =>
		{
			if (e.Status.TryGetStatusEffect<Se_RoomMod_Symbiote>(out var effect))
			{
				effect.Destroy();
			}
		});
	}

	private void MirrorProcessed()
	{
	}
}
