using Mirror;

[SaveActor(true)]
public class Se_Star_Mist_L_PotionShield_PersistentShield : StatusEffect
{
	[SaveVar(SaveVarFlags.Default)]
	public ShieldEffect shield;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			if (shield == null)
			{
				shield = DoShield(0f);
			}
			else
			{
				DoBasicEffect(shield);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
