using Mirror;
using UnityEngine;

public class Se_R_Frostbite_EmpowerAttacks : StatusEffect
{
	public GameObject firstHitEffect;

	public GameObject secondHitEffect;

	public float hasteStrength = 50f;

	public float duration = 8f;

	public float shieldDuration = 1.5f;

	public ScalingValue firstDmg;

	public ScalingValue firstShieldAmount;

	public ScalingValue secondDmg;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if ((Object)(object)victim.Ability.attackAbility != null)
		{
			ResetCooldown(victim.Ability.attackAbility);
		}
		SetTimer(duration);
		ShowOnScreenTimer();
		DoHaste(hasteStrength);
		DoAttackEmpower((EventInfoAttackEffect effect, int i) =>
		{
			if (!effect.chain.DidReact(this))
			{
				switch (i)
				{
				case 0:
					if (effect.type != AttackEffectType.BasicAttackSub)
					{
						GiveShield(victim, GetValue(firstShieldAmount) * effect.strength, shieldDuration);
					}
					Damage(firstDmg).ApplyStrength(effect.strength).SetElemental(ElementalType.Cold).SetOriginPosition(victim.position)
						.Dispatch(effect.victim, effect.chain.New(this));
					FxPlayNewNetworked(firstHitEffect, effect.victim);
					break;
				case 1:
					Damage(secondDmg).ApplyStrength(effect.strength).SetElemental(ElementalType.Cold).SetOriginPosition(victim.position)
						.SetAttr(DamageAttribute.IsCrit)
						.Dispatch(effect.victim, effect.chain.New(this));
					FxPlayNewNetworked(secondHitEffect, effect.victim);
					break;
				}
			}
		}, 2, DestroyIfActive);
	}

	private void MirrorProcessed()
	{
	}
}
