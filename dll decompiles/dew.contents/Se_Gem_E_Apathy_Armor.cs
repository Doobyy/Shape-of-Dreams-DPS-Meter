using Mirror;

public class Se_Gem_E_Apathy_Armor : StatusEffect
{
	public float duration;

	public ScalingValue addedArmorAmount;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoArmorBoost(GetValue(addedArmorAmount));
			SetTimer(duration);
			ShowOnScreenTimer("Gem_E_Apathy");
		}
	}

	private void MirrorProcessed()
	{
	}
}
