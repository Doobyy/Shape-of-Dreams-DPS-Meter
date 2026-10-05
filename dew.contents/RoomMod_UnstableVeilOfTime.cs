using Mirror;

public class RoomMod_UnstableVeilOfTime : RoomModifierBase
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
			if (e is Hero || e is Monster)
			{
				CreateStatusEffect<Se_RoomMod_UnstableVeilOfTime_ModifyCooldowns>(e, default);
			}
		}, (Entity e) =>
		{
			if (e.Status.TryGetStatusEffect<Se_RoomMod_UnstableVeilOfTime_ModifyCooldowns>(out var effect))
			{
				effect.Destroy();
			}
		});
	}

	private void MirrorProcessed()
	{
	}
}
