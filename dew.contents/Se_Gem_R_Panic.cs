using Mirror;

public class Se_Gem_R_Panic : StatusEffect
{
	public float duration = 3f;

	public ScalingValue speedAmount;

	public ScalingValue memoryHaste;

	private SkillBonus _bonus;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			SetTimer(duration);
			DoSpeed(GetValue(speedAmount));
			ShowOnScreenTimer("Gem_R_Panic");
			if (firstTrigger is SkillTrigger skillTrigger)
			{
				_bonus = skillTrigger.AddSkillBonus(new SkillBonus
				{
					cooldownMultiplier = 100f / (100f + GetValue(memoryHaste))
				});
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && _bonus != null)
		{
			_bonus.Stop();
			_bonus = null;
		}
	}

	private void MirrorProcessed()
	{
	}
}
