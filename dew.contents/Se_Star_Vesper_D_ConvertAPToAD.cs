using System;
using Mirror;

public class Se_Star_Vesper_D_ConvertAPToAD : StarEffect
{
	public StarScalingValue ratio;

	public override Type heroType => typeof(Hero_Vesper);

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
		data.attackDamage += data.abilityPower * GetValue(ratio) * (1f + victim.Status.bonusStats.attackDamagePercentage * 0.01f);
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
