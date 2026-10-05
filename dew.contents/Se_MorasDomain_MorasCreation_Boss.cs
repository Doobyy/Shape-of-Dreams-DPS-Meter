using System;

public class Se_MorasDomain_MorasCreation_Boss : Se_MorasDomain_MorasCreation
{
	private float _lastBlobSpawnTime;

	private AbilityTrigger _demonMainSkill;

	private Action<EventInfoCast> _onDemonMainSkillBeforePrepare;

	private Action<EventInfoCast> _onDemonMainSkillComplete;

	private AbilityTrigger _skollSummonSword;

	private Action<EventInfoCast> _onSkollSummonSwordComplete;

	private AbilityTrigger _infernusAtk;

	private Action<EventInfoCast> _onInfernusAtkComplete;

	public override bool reuseInRoom
	{
		get
		{
			/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
		}
	}

	protected override void OnCreate()
	{
	}

	protected override void OnDestroyActor()
	{
	}

	protected override void EntityEventOnTakeDamage(EventInfoDamage obj)
	{
	}

	private void MirrorProcessed()
	{
	}
}
