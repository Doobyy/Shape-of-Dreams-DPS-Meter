using Mirror;
using UnityEngine;

public class Se_Gem_L_Liberty_Buff : StatusEffect
{
	public ScalingValue adBonus;

	public ScalingValue apBonus;

	public ScalingValue maxHealthBonus;

	public float duration = 2.5f;

	public float multiplier = 2f;

	public GameObject fxBuff;

	public GameObject fxBuffOnUlt;

	private StatBonus _bonus;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		_bonus = null;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			bool flag = gem.skill.type == SkillType.Ultimate;
			float num = (flag ? (1f + multiplier) : 1f);
			FxPlayNetworked(flag ? fxBuffOnUlt : fxBuff, victim);
			_bonus = new StatBonus
			{
				attackDamageFlat = GetValue(adBonus) * num,
				abilityPowerFlat = GetValue(apBonus) * num,
				maxHealthFlat = GetValue(maxHealthBonus) * num
			};
			DoStatBonus(_bonus);
			SetTimer(duration * num);
			ShowOnScreenTimer("Gem_L_Liberty");
			if (flag)
			{
				CreateBasicEffect(info.caster, new UnstoppableEffect(), duration * num, "Liberty_Buff").DestroyOnDestroy(this);
			}
		}
	}

	public void ResetAndApply()
	{
		bool flag = gem.skill.type == SkillType.Ultimate;
		float num = (flag ? (1f + multiplier) : 1f);
		_bonus.attackDamageFlat = GetValue(adBonus) * num;
		_bonus.abilityPowerFlat = GetValue(apBonus) * num;
		_bonus.maxHealthFlat = GetValue(maxHealthBonus) * num;
		SetTimer(duration * num);
		ShowOnScreenTimer("Gem_L_Liberty");
		if (flag)
		{
			CreateBasicEffect(info.caster, new UnstoppableEffect(), duration * num, "Liberty_Buff").DestroyOnDestroy(this);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxBuffOnUlt);
			FxStopNetworked(fxBuff);
		}
	}

	private void MirrorProcessed()
	{
	}
}
