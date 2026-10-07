public class St_D_ThirstOfVengence : SkillTrigger
{
	public float reducedMaxHealthPercentage;

	public ScalingValue reducedCooldownTime;

	private StatBonus _bonus;

	protected override void OnEquip(Entity newOwner)
	{
	}

	protected override void OnUnequip(Entity formerOwner)
	{
	}

	private void EntityEventOnAttackFired(EventInfoAttackFired obj)
	{
	}

	private void MirrorProcessed()
	{
	}
}
