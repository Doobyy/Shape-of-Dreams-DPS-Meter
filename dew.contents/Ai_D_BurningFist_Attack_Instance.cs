public class Ai_D_BurningFist_Attack_Instance : InstantDamageInstance
{
	public ScalingValue healAmount;

	public override bool reuseInRoom => true;

	protected override void OnAfterDelay()
	{
		base.OnAfterDelay();
		Heal(healAmount).Dispatch(info.caster);
	}

	private void MirrorProcessed()
	{
	}
}
