using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class Se_E_UmbralEdge : StatusEffect
{
	public bool resetAttack;

	public float duration;

	public GameObject hitEffectOnVictim;

	public GameObject hitEffectOnCasterWhenMelee;

	public ScalingValue critChance;

	public ScalingValue hasteAmount;

	public ScalingValue hitDamage;

	public float procCoefficient;

	public float damageAmpForMelee;

	public bool addDurationOnKill;

	public ScalingValue addedDuration;

	public float hitDelay;

	public float startSlow;

	public float startSlowDuration;

	public bool startSlowDecay;

	private Hero _hero;

	private int _generation;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		int gen = _generation;
		_hero = (Hero)info.caster;
		_hero.ClientHeroEvent_OnKillOrAssist += new Action<EventInfoKill>(ClientHeroEventOnKillOrAssist);
		if (resetAttack)
		{
			ResetCooldown(_hero.Ability.attackAbility);
		}
		SetTimer(duration);
		ShowOnScreenTimer();
		DoAttackEmpower((EventInfoAttackEffect effect, int i) =>
		{
			if (!((UnityEngine.Object)(object)_hero == null) && _hero.isActive && !effect.chain.DidReact(this))
			{
				((MonoBehaviour)(object)_hero).StartCoroutine(Routine());
			}
			IEnumerator Routine()
			{
				FxPlayNewNetworked(hitEffectOnCasterWhenMelee, info.caster.position, Quaternion.LookRotation(effect.victim.position - info.caster.position).Flattened());
				float mult = 1f;
				if (_hero.IsMeleeHero())
				{
					mult *= 1f + damageAmpForMelee;
				}
				if (hitDelay > 0.0001f)
				{
					yield return new WaitForSeconds(hitDelay);
				}
				if (!((UnityEngine.Object)(object)_hero == null) && _hero.isActive && !((UnityEngine.Object)(object)this == null) && isActive && gen == _generation)
				{
					FxPlayNewNetworked(hitEffectOnVictim, effect.victim);
					Damage(hitDamage, procCoefficient).ApplyStrength(effect.strength).ApplyRawMultiplier(mult).SetElemental(ElementalType.Dark)
						.SetOriginPosition(info.caster.position)
						.Dispatch(effect.victim, effect.chain.New(this));
				}
			}
		});
		DoStatBonus(new StatBonus
		{
			critChanceFlat = GetValue(critChance)
		});
		DoHaste(GetValue(hasteAmount));
		if (startSlow > 0.001f)
		{
			CreateBasicEffect(victim, new SlowEffect
			{
				decay = startSlowDecay,
				strength = startSlow
			}, startSlowDuration, "umbraledge_slow");
		}
	}

	private void ClientHeroEventOnKillOrAssist(EventInfoKill obj)
	{
		if (addDurationOnKill)
		{
			SetTimer(duration, Mathf.Clamp(remainingDuration.Value + GetValue(addedDuration), 0f, duration));
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)_hero != null)
		{
			_hero.ClientHeroEvent_OnKillOrAssist -= new Action<EventInfoKill>(ClientHeroEventOnKillOrAssist);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_generation++;
	}

	private void MirrorProcessed()
	{
	}
}
