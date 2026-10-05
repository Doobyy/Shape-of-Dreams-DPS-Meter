using Mirror;

public class RoomMod_AcceleratedTime : RoomModifierBase
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
				e.CreateStatusEffect<Se_AcceleratedTime>(e, new CastInfo(e));
			}
		}, (Entity e) =>
		{
			if (e.Status.TryGetStatusEffect<Se_AcceleratedTime>(out var effect))
			{
				effect.Destroy();
			}
		});
	}

	private void MirrorProcessed()
	{
	}
}
