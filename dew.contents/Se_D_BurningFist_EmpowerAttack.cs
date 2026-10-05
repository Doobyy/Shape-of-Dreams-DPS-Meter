using Mirror;
using UnityEngine;

public class Se_D_BurningFist_EmpowerAttack : StatusEffect
{
	public float disposeDuration = 6f;

	private AbilityTrigger _trigger;

	private float _originalWalkSpeed;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			SetTimer(disposeDuration);
			_trigger = firstTrigger;
			DoAttackOverride<At_D_BurningFist_Attack>(() =>
			{
				Dew.CallDelayed(DestroyIfActive);
			});
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!_trigger.IsNullOrInactive() && normalizedDuration.HasValue)
		{
			_trigger.fillAmount = normalizedDuration.Value;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if ((Object)(object)_trigger != null)
		{
			_trigger.fillAmount = 0f;
		}
	}

	private void MirrorProcessed()
	{
	}
}
