using System;
using Mirror;

public class Se_Star_Yubar_D_SkillHaste : StarEffect
{
	public StarScalingValue bonusAmount;

	public override Type heroType => typeof(Hero_Yubar);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoStatBonus(new StatBonus
			{
				abilityHasteFlat = GetValue(bonusAmount)
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
