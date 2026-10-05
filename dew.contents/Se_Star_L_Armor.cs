using Mirror;

public class Se_Star_L_Armor : StarEffect
{
	public StarScalingValue armorAmount;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoArmorBoost(GetValue(armorAmount));
		}
	}

	private void MirrorProcessed()
	{
	}
}
