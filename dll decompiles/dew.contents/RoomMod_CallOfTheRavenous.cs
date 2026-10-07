using Mirror;

public class RoomMod_CallOfTheRavenous : RoomModifierBase
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer || !isNewInstance)
		{
			return;
		}
		SingletonDewNetworkBehaviour<Room>.instance.rifts.openedSidetrackRifts.Clear();
		SingletonDewNetworkBehaviour<Room>.instance.rifts.openedSidetrackRifts.Add("Rift_Sidetrack_CallOfTheRavenous");
		ModifyEntities((Entity e) =>
		{
			if (e is Monster)
			{
				e.CreateStatusEffect<Se_CallOfTheRavenous>(e, new CastInfo(e));
			}
		}, (Entity e) =>
		{
			if (e.Status.TryGetStatusEffect<Se_CallOfTheRavenous>(out var effect))
			{
				effect.Destroy();
			}
		});
	}

	private void MirrorProcessed()
	{
	}
}
