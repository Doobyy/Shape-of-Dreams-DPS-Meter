using Mirror;

public class Se_Mon_Special_BossPolaris_Monster_PizzaLightning_Rage : StatusEffect
{
	public float speedAmount = 40f;

	public float hasteAmount = 35f;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoSpeed(speedAmount);
			DoHaste(hasteAmount);
			DoUnstoppable();
		}
	}

	private void MirrorProcessed()
	{
	}
}
