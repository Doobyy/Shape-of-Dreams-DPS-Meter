using System;

public class St_M_ParryMaster : SkillTrigger
{
	[NonSerialized]
	public int nextParryAnimIndex;

	protected override void OnLevelChange(int oldLevel, int newLevel)
	{
	}

	private void MirrorProcessed()
	{
	}
}
