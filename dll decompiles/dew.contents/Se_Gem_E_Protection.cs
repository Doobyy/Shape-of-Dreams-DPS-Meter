using Mirror;

public class Se_Gem_E_Protection : StatusEffect
{
	public ScalingValue armorDuration;

	public ScalingValue armorAmount;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoArmorBoost(GetValue(armorAmount));
			DoUnstoppable();
			SetTimer(GetValue(armorDuration));
		}
	}

	private void MirrorProcessed()
	{
	}
}
