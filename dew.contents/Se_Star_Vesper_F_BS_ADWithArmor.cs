using System;
using Mirror;
using UnityEngine;

public class Se_Star_Vesper_F_BS_ADWithArmor : StarEffect
{
	public float armorPerHit = 10f;

	public float armorPerHitBoss = 40f;

	public override Type heroType => typeof(Hero_Vesper);

	public override Type skillType => typeof(St_R_BaptismOfSun);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(OnAbilityInstanceBeforePrepare);
			victim.ActorEvent_OnAbilityInstanceCreated += new Action<EventInfoAbilityInstance>(OnAbilityInstanceCreated);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			victim.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(OnAbilityInstanceBeforePrepare);
			victim.ActorEvent_OnAbilityInstanceCreated -= new Action<EventInfoAbilityInstance>(OnAbilityInstanceCreated);
		}
	}

	private void OnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Se_R_BaptismOfSun_Buff se_R_BaptismOfSun_Buff)
		{
			se_R_BaptismOfSun_Buff.disableHaste = true;
		}
	}

	private void OnAbilityInstanceCreated(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Se_R_BaptismOfSun_Buff se_R_BaptismOfSun_Buff)
		{
			se_R_BaptismOfSun_Buff.DoStatBonus(new StatBonus
			{
				attackDamagePercentage = se_R_BaptismOfSun_Buff.GetValue(se_R_BaptismOfSun_Buff.hasteAmount),
				armorFlat = (float)se_R_BaptismOfSun_Buff.hitCountNonBoss * armorPerHit + (float)se_R_BaptismOfSun_Buff.hitCountBoss * armorPerHitBoss
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
