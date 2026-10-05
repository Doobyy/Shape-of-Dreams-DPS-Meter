using System;
using Mirror;

public class Treasure_SparklingElixir : Treasure
{
	public float healHpRatio = 0.4f;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			ActorEvent_OnDoHeal += new Action<EventInfoHeal>(ActorEventOnDoHeal);
			Heal(healHpRatio * hero.maxHealth).Dispatch(hero);
			ActorEvent_OnDoHeal -= new Action<EventInfoHeal>(ActorEventOnDoHeal);
			Destroy();
		}
	}

	private void ActorEventOnDoHeal(EventInfoHeal obj)
	{
		if (obj.heal.discardedAmount > 0f)
		{
			GiveShield(hero, obj.heal.discardedAmount, float.PositiveInfinity);
		}
	}

	private void MirrorProcessed()
	{
	}
}
