using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_R_SerpentineBlessing_Buff : StatusEffect
{
	public ScalingValue armorAmount;

	public ScalingValue hasteAmount;

	public ScalingValue summonHealRatio;

	public float perTargetCooldown;

	public float duration = 8f;

	public GameObject fxSecondWindActivate;

	[NonSerialized]
	public bool hideTimer;

	[NonSerialized]
	public bool doSecondWind;

	[NonSerialized]
	public float secondWindHealRatio;

	private EntityTransformModifier _mod;

	private float _baseDuration;

	private ScalingValue _baseHasteAmount;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_baseDuration = duration;
		_baseHasteAmount = hasteAmount;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (_mod != null)
		{
			_mod.Stop();
			_mod = null;
		}
		duration = _baseDuration;
		hasteAmount = _baseHasteAmount;
		doSecondWind = false;
		secondWindHealRatio = 0f;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		_mod = victim.Visual.GetNewTransformModifier();
		_mod.scaleMultiplier = Vector3.one * 1.4f;
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		victim.Control.outerRadius *= 1.4f;
		SetTimer(duration);
		if (!hideTimer)
		{
			ShowOnScreenTimer();
		}
		DoArmorBoost(GetValue(armorAmount));
		DoHaste(GetValue(hasteAmount));
		if (doSecondWind)
		{
			DoDeathInterrupt((EventInfoKill kill) =>
			{
				FxPlayNetworked(fxSecondWindActivate, victim);
				victim.Status.SetHealth(1f);
				Heal(victim.Status.maxHealth * secondWindHealRatio).Dispatch(victim);
				Destroy();
			}, 0);
		}
		if (victim is Summon)
		{
			Heal(GetValue(summonHealRatio) * victim.maxHealth).Dispatch(victim);
		}
		Dictionary<Entity, float> hitTimes = new Dictionary<Entity, float>();
		DoAttackEmpower((EventInfoAttackEffect effect, int i) =>
		{
			if (!effect.chain.DidReact(this) && effect.type != AttackEffectType.Others)
			{
				if (!hitTimes.TryGetValue(effect.victim, out var value))
				{
					value = float.NegativeInfinity;
					hitTimes[effect.victim] = float.NegativeInfinity;
				}
				if (!(Time.time - value < perTargetCooldown))
				{
					hitTimes[effect.victim] = Time.time;
					CreateAbilityInstance(Vector3.zero, null, new CastInfo(info.caster, effect.victim), (Ai_R_SerpentineBlessing_Projectile ai) =>
					{
						ai.SetCustomStartPosition(victim.Visual.GetCenterPosition());
						ai.strength = effect.strength;
						ai.healTarget = victim;
						ai.chain = effect.chain.New(this);
					});
				}
			}
		});
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (_mod != null)
		{
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		if (((NetworkBehaviour)this).isServer)
		{
			victim.Control.outerRadius = Mathf.Round(victim.Control.outerRadius / 1.4f * 100f) / 100f;
		}
		IEnumerator Routine()
		{
			Vector3 start = _mod.scaleMultiplier;
			for (float t = 0f; t < 0.5f; t += Time.deltaTime)
			{
				if (_mod == null)
				{
					yield break;
				}
				_mod.scaleMultiplier = Vector3.Lerp(start, Vector3.one, t / 0.5f);
				yield return null;
			}
			_mod.Stop();
			_mod = null;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_mod != null)
		{
			_mod.Stop();
			_mod = null;
		}
	}

	private void MirrorProcessed()
	{
	}
}
