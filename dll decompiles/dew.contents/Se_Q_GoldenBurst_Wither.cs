using Mirror;
using UnityEngine;

public class Se_Q_GoldenBurst_Wither : StackedStatusEffect
{
	private AbilityTrigger _trigger;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.Visual.genericStackIndicatorMax = maxStack;
			victim.Visual.genericStackIndicatorValue = stack;
			ShowOnScreenTimer("Q_GoldenBurst_StatusEffect_Wither");
			_trigger = firstTrigger;
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && !_trigger.IsNullOrInactive() && stack > 0)
		{
			_trigger.fillAmount = remainingDecayTime / decayTime;
		}
	}

	protected override void OnStackChange(int oldStack, int newStack)
	{
		base.OnStackChange(oldStack, newStack);
		if (((NetworkBehaviour)this).isServer)
		{
			victim.Visual.genericStackIndicatorValue = newStack;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((Object)(object)victim != null)
			{
				victim.Visual.genericStackIndicatorValue = 0;
			}
			if (!_trigger.IsNullOrInactive())
			{
				_trigger.fillAmount = 0f;
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
