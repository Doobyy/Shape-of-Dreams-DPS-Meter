using Mirror;
using UnityEngine;

public class Se_R_Somersault_CritBonus : StatusEffect
{
	public ScalingValue critChangeAmount;

	public float effectDuration = 4f;

	private AbilityTrigger _trigger;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			SetTimer(effectDuration);
			DoStatBonus(new StatBonus
			{
				critChanceFlat = GetValue(critChangeAmount)
			});
			_trigger = firstTrigger;
			ShowOnScreenTimer();
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && (bool)(Object)(object)_trigger)
		{
			_trigger.fillAmount = normalizedDuration.Value;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (bool)(Object)(object)_trigger)
		{
			_trigger.fillAmount = 0f;
		}
	}

	private void MirrorProcessed()
	{
	}
}
