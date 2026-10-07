[SaveActor(true)]
public class Se_RoomMod_MisticIceVein_Shield : StatusEffect
{
	[SaveVar(SaveVarFlags.Default)]
	public ShieldEffect shield;

	protected override void OnCreate()
	{
	}

	protected override void ActiveLogicUpdate(float dt)
	{
	}

	private void MirrorProcessed()
	{
	}
}
