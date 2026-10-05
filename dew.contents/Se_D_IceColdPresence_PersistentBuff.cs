using Mirror;
using UnityEngine;

[SaveActor(true)]
public class Se_D_IceColdPresence_PersistentBuff : PersistentStatBonusEffect
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		StatusEffect[] array = victim.Status.statusEffects.ToArray();
		foreach (StatusEffect statusEffect in array)
		{
			if (!((Object)(object)statusEffect == (Object)(object)this) && statusEffect is Se_D_IceColdPresence_PersistentBuff se_D_IceColdPresence_PersistentBuff)
			{
				se_D_IceColdPresence_PersistentBuff.bonus.Add(bonus);
				Destroy();
				break;
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
