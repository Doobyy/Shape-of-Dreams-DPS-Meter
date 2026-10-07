using System;
using UnityEngine;

public class ShieldEffect : BasicEffect
{
	private float _amount;

	public SafeAction<EventInfoDamageNegatedByShield> onDamageNegated;

	public Action<float, float> onAmountModified;

	public override BasicEffectMask mask => BasicEffectMask.Shield;

	public float amount
	{
		get
		{
			return _amount;
		}
		set
		{
			float num = _amount;
			if (num == value)
			{
				return;
			}
			_amount = value;
			if ((UnityEngine.Object)(object)victim != null)
			{
				victim.Status.DirtyStatusInfo();
			}
			try
			{
				onAmountModified?.Invoke(num, value);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	public void AddAmount(float originalAmount, float totalMax = float.PositiveInfinity, bool processTotalMax = true, ReactionChain chain = default(ReactionChain))
	{
		if (originalAmount < 0.001f)
		{
			return;
		}
		if (processTotalMax)
		{
			totalMax = parent.ProcessShieldAmount(totalMax, parent.victim);
		}
		float num = parent.ProcessShieldAmount(originalAmount, parent.victim);
		if (!(amount > totalMax))
		{
			float discardedAmount = 0f;
			if (amount + num > totalMax)
			{
				float num2 = num;
				num = totalMax - amount;
				discardedAmount = num2 - num;
			}
			amount += num;
			EventInfoShield eventInfoShield = new EventInfoShield
			{
				originalAmount = originalAmount,
				finalAmount = num,
				discardedAmount = discardedAmount,
				target = victim,
				statusEffect = parent,
				chain = chain
			};
			parent.InvokeOnGiveShield(eventInfoShield);
			victim.EntityEvent_OnTakeShield?.Invoke(eventInfoShield);
		}
	}
}
