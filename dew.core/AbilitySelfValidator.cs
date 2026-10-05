using System;
using UnityEngine;

[Serializable]
public class AbilitySelfValidator : IEntityValidator
{
	public static AbilitySelfValidator Default = new AbilitySelfValidator();

	public bool isMovementAbility;

	public bool allowWhileDashing;

	public bool allowWhileDodging = true;

	public bool allowWhileDisabled;

	public bool Evaluate(Entity self)
	{
		if ((UnityEngine.Object)(object)self != null && self.isActive && (allowWhileDisabled || (!self.Control.isAirborne && !self.Status.hasStun && !self.Status.hasSilence)) && (!self.Control.isDashing || allowWhileDashing || (allowWhileDodging && self.Control.isDodging)))
		{
			if (isMovementAbility)
			{
				return !self.Status.hasRoot;
			}
			return true;
		}
		return false;
	}
}
