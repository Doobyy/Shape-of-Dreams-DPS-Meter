public class RoomMod_ToxicArea : RoomModifierBase
{
	public override void OnStartServer()
	{
		base.OnStartServer();
		ModifyEntities((Entity e) =>
		{
			if (e is Hero || e is Summon)
			{
				e.CreateStatusEffect<Se_RoomMod_ToxicArea_Debuff>(e, new CastInfo(e));
			}
		}, (Entity e) =>
		{
			if (e.Status.TryGetStatusEffect<Se_RoomMod_ToxicArea_Debuff>(out var effect))
			{
				effect.Destroy();
			}
		});
	}

	private void MirrorProcessed()
	{
	}
}
