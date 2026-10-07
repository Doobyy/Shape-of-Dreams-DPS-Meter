using Mirror;
using UnityEngine;

[SaveActor(true)]
public class Se_Gem_R_Frost_Stat : PersistentStatBonusEffect
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
			if (!((Object)(object)statusEffect == (Object)(object)this) && statusEffect is Se_Gem_R_Frost_Stat se_Gem_R_Frost_Stat)
			{
				se_Gem_R_Frost_Stat.bonus.Add(bonus);
				Destroy();
				break;
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
