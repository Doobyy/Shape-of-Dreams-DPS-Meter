using System;
using Mirror;
using UnityEngine;

public class Se_Star_Bismuth_D_AtkSpdFromDismantle : StarEffect
{
	public float dismantleReduction = 0.15f;

	public StarScalingValue atkSpdPercentage;

	public float nonDuplicateBonusAmp = 0.5f;

	public float commonMult = 0.7f;

	public float rareMult = 1f;

	public float epicMult = 1.5f;

	public float legendaryMult = 3f;

	[SaveVar(SaveVarFlags.Default)]
	private StatBonus _bonus;

	public override Type heroType => typeof(Hero_Bismuth);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			player.dismantleDreamDustMultiplier *= 1f - dismantleReduction;
			NetworkedManagerBase<ClientEventManager>.instance.OnDismantled += new Action<Hero, NetworkBehaviour>(OnDismantled);
			_bonus = DoStatBonus(_bonus);
		}
	}

	private void OnDismantled(Hero arg1, NetworkBehaviour arg2)
	{
		if ((UnityEngine.Object)(object)arg1 != (UnityEngine.Object)(object)hero)
		{
			return;
		}
		Rarity rarity;
		if (arg2 is SkillTrigger skillTrigger)
		{
			rarity = skillTrigger.rarity;
		}
		else
		{
			if (!(arg2 is Gem gem))
			{
				return;
			}
			rarity = gem.rarity;
		}
		switch (rarity)
		{
		case Rarity.Character:
			rarity = Rarity.Epic;
			break;
		case Rarity.Unique:
			rarity = Rarity.Legendary;
			break;
		}
		float num = 1f;
		switch (rarity)
		{
		case Rarity.Common:
			num = commonMult;
			break;
		case Rarity.Rare:
			num = rareMult;
			break;
		case Rarity.Epic:
			num = epicMult;
			break;
		case Rarity.Legendary:
			num = legendaryMult;
			break;
		}
		float num2 = GetValue(atkSpdPercentage) * num;
		if (!((Hero_Bismuth)hero).HasSameTravelerMemory())
		{
			num2 *= 1f + nonDuplicateBonusAmp;
		}
		_bonus.attackSpeedPercentage += num2;
		player.TpcShowCenterMessage(CenterMessageType.General, "InGame_Message_Chaos_AttackSpeed", new string[1] { num2.ToString("#,##0.#") });
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)player != null)
			{
				player.dismantleDreamDustMultiplier /= 1f - dismantleReduction;
			}
			if ((UnityEngine.Object)(object)NetworkedManagerBase<ClientEventManager>.instance != null)
			{
				NetworkedManagerBase<ClientEventManager>.instance.OnDismantled -= new Action<Hero, NetworkBehaviour>(OnDismantled);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
