using System;
using Mirror;

public class Se_Star_Yubar_D_ConvertADToAP : StarEffect
{
	public StarScalingValue ratio;

	public override Type heroType => typeof(Hero_Yubar);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.Status.finalStatsProcessors.Add(Processor);
			victim.Status.CalculateStats();
		}
	}

	private void Processor(ref FinalStats data)
	{
		data.abilityPower += data.attackDamage * GetValue(ratio) * (1f + victim.Status.bonusStats.abilityPowerPercentage * 0.01f);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.Status.finalStatsProcessors.Remove(Processor);
			victim.Status.CalculateStats();
		}
	}

	private void MirrorProcessed()
	{
	}
}
