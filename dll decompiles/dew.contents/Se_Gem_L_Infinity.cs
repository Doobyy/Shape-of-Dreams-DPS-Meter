public class Se_Gem_L_Infinity : StatusEffect
{
	private Gem_L_Infinity _parent;

	private int _maxCharge;

	private SkillBonus _bonus;

	internal float _time;

	internal SkillTrigger _skillToEmpower;

	internal AbilityLockHandle _lockHandle;

	protected override void OnCreate()
	{
	}

	protected override void OnDestroyActor()
	{
	}

	private void MirrorProcessed()
	{
	}
}
