using System;

public class Se_Star_Bismuth_F_VH_FullCharge : StarEffect
{
	public override Type heroType => typeof(Hero_Bismuth);

	public override Type skillType => typeof(St_QR_ValiantHeart);

	protected override void OnCreate()
	{
		base.OnCreate();
		SkillTrigger[] array = skills;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].configs[0].addedCharges = 999;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		SkillTrigger[] array = skills;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].configs[0].addedCharges = 1;
		}
	}

	private void MirrorProcessed()
	{
	}
}
