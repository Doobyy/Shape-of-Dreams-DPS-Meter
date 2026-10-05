using Mirror;

public class Se_D_Resolve : StatusEffect
{
	public ScalingValue healthBonus;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoStatBonus(new StatBonus
			{
				maxHealthFlat = GetValue(healthBonus)
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
