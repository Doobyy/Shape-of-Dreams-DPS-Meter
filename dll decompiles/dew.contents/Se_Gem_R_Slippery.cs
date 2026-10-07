using Mirror;

public class Se_Gem_R_Slippery : StatusEffect
{
	public float moveSpeedBonus = 10f;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoSpeed(moveSpeedBonus);
			SetTimer(float.PositiveInfinity);
		}
	}

	private void MirrorProcessed()
	{
	}
}
