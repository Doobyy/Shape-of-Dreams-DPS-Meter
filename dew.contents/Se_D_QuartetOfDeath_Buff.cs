using System;
using Mirror;

public class Se_D_QuartetOfDeath_Buff : StatusEffect
{
	public float speedBonusPercentagePerStack = 4f;

	public float duration = 6f;

	[NonSerialized]
	public int currentStack;

	private StatBonus _bonus;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_bonus = DoStatBonus(new StatBonus
			{
				attackSpeedPercentage = speedBonusPercentagePerStack * (float)currentStack
			});
			SetTimer(duration);
			ShowOnScreenTimer();
		}
	}

	public void StackAndRefresh(int markCount)
	{
		currentStack += markCount;
		_bonus.attackSpeedPercentage = speedBonusPercentagePerStack * (float)currentStack;
		ResetTimer();
		FxPlayNetworked(startEffect);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.Status.RemoveStatBonus(_bonus);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		currentStack = 0;
		_bonus = null;
	}

	private void MirrorProcessed()
	{
	}
}
