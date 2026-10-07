using Mirror;

public class Se_Gem_E_Reflex_ArmorBonus : StatusEffect
{
	public ScalingValue armorFlatAmount;

	public float armorDuration;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoArmorBoost(GetValue(armorFlatAmount));
			SetTimer(armorDuration);
		}
	}

	private void MirrorProcessed()
	{
	}
}
