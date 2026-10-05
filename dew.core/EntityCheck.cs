using UnityEngine;

public static class EntityCheck
{
	public static bool IsNullInactiveDeadOrKnockedOut(this Entity e)
	{
		if (!((Object)(object)e == null) && e.isActive && !e.Status.isDead)
		{
			if (e is Hero hero)
			{
				return hero.isKnockedOut;
			}
			return false;
		}
		return true;
	}

	public static bool IsNullInactiveDeadOrKnockedOut<T>(this ActorRef<T> r) where T : Entity
	{
		return r.Get().IsNullInactiveDeadOrKnockedOut();
	}

	public static bool IsAnyBoss(this Entity e)
	{
		if (!(e is BossMonster))
		{
			if (e is Monster monster)
			{
				if (monster.type != Monster.MonsterType.Boss)
				{
					return monster.type == Monster.MonsterType.MiniBoss;
				}
				return true;
			}
			return false;
		}
		return true;
	}
}
