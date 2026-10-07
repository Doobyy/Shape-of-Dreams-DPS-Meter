using Mirror;

public class Se_R_BoneCrusher_UnstoppableAndShield : StatusEffect
{
	public ScalingValue shieldMaxHpRatio;

	public float shieldDisappearTime = 1f;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			SetTimer(shieldDisappearTime);
			DoShield(victim.maxHealth * GetValue(shieldMaxHpRatio));
		}
	}

	private void MirrorProcessed()
	{
	}
}
