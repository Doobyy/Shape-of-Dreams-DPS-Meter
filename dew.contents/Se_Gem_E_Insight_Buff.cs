using Mirror;

public class Se_Gem_E_Insight_Buff : StatusEffect
{
	public ScalingValue gainedAbilityHaste;

	public float duration;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoStatBonus(new StatBonus
			{
				abilityHasteFlat = GetValue(gainedAbilityHaste)
			});
			SetTimer(duration);
		}
	}

	private void MirrorProcessed()
	{
	}
}
