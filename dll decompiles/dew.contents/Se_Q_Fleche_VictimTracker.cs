using Mirror;
using UnityEngine;

public class Se_Q_Fleche_VictimTracker : StatusEffect
{
	public GameObject healEffect;

	public ScalingValue healAmount;

	public float cooldownReduction = 1.5f;

	internal bool _isBeingReplaced;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		_isBeingReplaced = false;
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !_isBeingReplaced && !info.caster.IsNullInactiveDeadOrKnockedOut() && victim.IsNullInactiveDeadOrKnockedOut())
		{
			if (GetValue(healAmount) > 0f)
			{
				FxPlayNetworked(healEffect, info.caster);
				DoHeal(new HealData(GetValue(healAmount)), info.caster);
			}
			if ((Object)(object)firstTrigger != null)
			{
				ApplyCooldownReductionByRatio(firstTrigger, cooldownReduction);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
