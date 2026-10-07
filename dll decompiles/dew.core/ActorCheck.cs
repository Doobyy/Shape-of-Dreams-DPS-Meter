using UnityEngine;

public static class ActorCheck
{
	public static bool IsNullOrInactive(this Actor a)
	{
		if (!((Object)(object)a == null))
		{
			return !a.isActive;
		}
		return true;
	}

	public static bool IsNullOrInactive<T>(this ActorRef<T> r) where T : Actor
	{
		return r.Get().IsNullOrInactive();
	}
}
