public class RoomMod_LingeringAuraOfGuidance : RoomModifierBase
{
	public override void OnStartServer()
	{
		base.OnStartServer();
		if (!isNewInstance)
		{
			return;
		}
		foreach (RoomSection section in SingletonDewNetworkBehaviour<Room>.instance.sections)
		{
			section.TryGetGoodNodePosition(out var pos);
			CreateActor<Shrine_Guidance>(pos, null);
		}
		ModifyEntities((Entity e) =>
		{
			CreateStatusEffect<Se_LingeringAuraOfGuidance>(e, new CastInfo(e));
		}, (Entity e) =>
		{
			if (e.Status.TryGetStatusEffect<Se_LingeringAuraOfGuidance>(out var effect))
			{
				effect.Destroy();
			}
		});
	}

	private void MirrorProcessed()
	{
	}
}
