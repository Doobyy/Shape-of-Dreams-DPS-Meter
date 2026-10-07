using UnityEngine;

public static class IHoldableInHandExtension
{
	public static bool IsHoldableObjectNullOrInactive(this IItem ito)
	{
		if (ito != null)
		{
			if (ito is Actor actor)
			{
				if (!((Object)(object)actor == null))
				{
					return !actor.isActive;
				}
				return true;
			}
			return false;
		}
		return true;
	}
}
