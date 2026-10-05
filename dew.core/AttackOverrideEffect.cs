using System;
using UnityEngine;

public class AttackOverrideEffect : BasicEffect
{
	private AbilityTrigger _trigger;

	public Action onUse;

	internal bool _shouldTriggerBeDisposed;

	public override BasicEffectMask mask => BasicEffectMask.AttackOverride;

	public AbilityTrigger trigger
	{
		get
		{
			return _trigger;
		}
		set
		{
			_trigger = value;
			if ((UnityEngine.Object)(object)victim != null)
			{
				victim.Status.DirtyStatusInfo();
			}
		}
	}
}
