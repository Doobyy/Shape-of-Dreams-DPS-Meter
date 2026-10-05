using Mirror;
using UnityEngine;

public class Se_D_ParryMaster_Buff : StatusEffect
{
	public float duration = 8f;

	public ScalingValue bonusAmount;

	public int maxStack = 5;

	private int _currentStack;

	private StatBonus _bonus;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			float value = GetValue(bonusAmount);
			_bonus = DoStatBonus(new StatBonus
			{
				abilityPowerFlat = value,
				attackDamageFlat = value
			});
			_currentStack = 1;
			SetTimer(duration);
			ShowOnScreenTimer("St_D_ParryMaster");
		}
	}

	public void StackAndReset()
	{
		if (_currentStack < maxStack)
		{
			_currentStack++;
			float num = GetValue(bonusAmount) * (float)_currentStack;
			_bonus.abilityPowerFlat = num;
			_bonus.attackDamageFlat = num;
		}
		Debug.Log($"Stacked {_currentStack} times");
		ResetTimer();
	}

	private void MirrorProcessed()
	{
	}
}
