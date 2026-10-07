using Mirror;

public class St_E_ClutchesOfMalice : SkillTrigger
{
	public ScalingValue scale;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			ChangeCastCircleRadius();
		}
	}

	protected override void OnLevelChange(int oldLevel, int newLevel)
	{
		base.OnLevelChange(oldLevel, newLevel);
		if (((NetworkBehaviour)this).isServer)
		{
			ChangeCastCircleRadius();
		}
	}

	private void ChangeCastCircleRadius()
	{
		configs[0].castMethod._radius = GetValue(scale);
		SyncCastMethodChanges(0);
	}

	private void MirrorProcessed()
	{
	}
}
