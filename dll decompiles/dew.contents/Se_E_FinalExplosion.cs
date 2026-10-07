using Mirror;

public class Se_E_FinalExplosion : StatusEffect
{
	public float duration = 3f;

	public ScalingValue speedAmount;

	public ScalingValue armorAmount;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoSpeed(GetValue(speedAmount));
			DoArmorBoost(GetValue(armorAmount));
			SetTimer(duration);
			ShowOnScreenTimer();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			CreateAbilityInstance<Ai_E_FinalExplosion_Explosion>(info.caster.position, null, new CastInfo(info.caster));
		}
	}

	private void MirrorProcessed()
	{
	}
}
