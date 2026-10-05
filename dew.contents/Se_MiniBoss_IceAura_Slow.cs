using Mirror;

public class Se_MiniBoss_IceAura_Slow : StatusEffect
{
	public float duration;

	public float slowAmount;

	public float atkSpeedSlowAmount;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			SetTimer(duration);
			DoSlow(slowAmount);
			DoStatBonus(new StatBonus
			{
				attackSpeedPercentage = 0f - atkSpeedSlowAmount
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
