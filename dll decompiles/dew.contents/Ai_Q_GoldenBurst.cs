using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Q_GoldenBurst : AbilityInstance
{
	public ScalingValue damageAmount;

	public ScalingValue healAmount;

	public GameObject fxHit;

	public GameObject fxActualHit;

	public ScalingValue selfDamageBaseRatio;

	public ScalingValue selfDamagePerWitherRatio;

	public float singleTargetBonusAmp = 1f;

	public FxCameraShake shake;

	public DewAudioSource baseAudio;

	public AnimationCurve baseAudioPitch;

	public AnimationCurve baseAudioVolume;

	public DewAudioSource flavourAudio;

	public AnimationCurve flavourAudioPitch;

	public AnimationCurve flavourAudioVolume;

	public AnimationCurve shakeMultiplier;

	public DewCollider range;

	[NonSerialized]
	public bool isCastForFree;

	private ScalingValue _baseDamageAmount;

	private float _baseBaseAudioPitchMul;

	private float _baseBaseAudioVolumeMul;

	private float _baseFlavourAudioPitchMul;

	private float _baseFlavourAudioVolumeMul;

	private float _baseShakeAmplitude;

	private Vector3 _baseStartEffectNoStopScale;

	private Vector3 _baseRangeScale;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_baseDamageAmount = damageAmount;
		if (baseAudio != null)
		{
			_baseBaseAudioPitchMul = baseAudio.pitchMultiplier;
			_baseBaseAudioVolumeMul = baseAudio.volumeMultiplier;
		}
		if (flavourAudio != null)
		{
			_baseFlavourAudioPitchMul = flavourAudio.pitchMultiplier;
			_baseFlavourAudioVolumeMul = flavourAudio.volumeMultiplier;
		}
		if (shake != null)
		{
			_baseShakeAmplitude = shake.amplitude;
		}
		_baseStartEffectNoStopScale = startEffectNoStop.transform.localScale;
		_baseRangeScale = range.transform.localScale;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		damageAmount = _baseDamageAmount;
	}

	public float GetSelfDamageAmount(Entity e)
	{
		if (e.Status.TryGetStatusEffect<Se_Q_GoldenBurst_Wither>(out var effect))
		{
			return (selfDamageBaseRatio.GetValue(skillLevel, e) + selfDamagePerWitherRatio.GetValue(skillLevel, e) * (float)effect.stack) * e.maxHealth;
		}
		return selfDamageBaseRatio.GetValue(skillLevel, e) * e.maxHealth;
	}

	protected override void OnCreate()
	{
		int num = 0;
		if (info.caster.Status.TryGetStatusEffect<Se_Q_GoldenBurst_Wither>(out var effect))
		{
			num = effect.stack;
		}
		if (baseAudio != null)
		{
			baseAudio.pitchMultiplier = _baseBaseAudioPitchMul * baseAudioPitch.Evaluate(num);
			baseAudio.volumeMultiplier = _baseBaseAudioVolumeMul * baseAudioVolume.Evaluate(num);
		}
		if (flavourAudio != null)
		{
			flavourAudio.pitchMultiplier = _baseFlavourAudioPitchMul * flavourAudioPitch.Evaluate(num);
			flavourAudio.volumeMultiplier = _baseFlavourAudioVolumeMul * flavourAudioVolume.Evaluate(num);
		}
		if (shake != null)
		{
			shake.amplitude = _baseShakeAmplitude * shakeMultiplier.Evaluate(num);
		}
		startEffectNoStop.transform.localScale = _baseStartEffectNoStopScale * (1f + (float)num * 0.1f);
		range.transform.localScale = _baseRangeScale * (1f + (float)num * 0.1f);
		base.OnCreate();
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		float num = GetSelfDamageAmount(info.caster);
		if (firstTrigger is St_Q_GoldenBurst st_Q_GoldenBurst)
		{
			num *= st_Q_GoldenBurst.sacrificeHpMultiplier;
		}
		if (num > info.caster.currentHealth - 1E-05f)
		{
			num = info.caster.currentHealth - 1E-05f;
		}
		if (num > 0f && !isCastForFree)
		{
			PureDamage(num, 0f).SetAttr(DamageAttribute.IgnoreShield).Dispatch(info.caster);
		}
		IEnumerable<List<Entity>> enumerable = range.SweepEntitiesFromOrigin(3);
		List<Entity> list = DewPool.GetList(out ListReturnHandle<Entity> handle);
		foreach (List<Entity> ents in enumerable)
		{
			yield return new SI.WaitForSeconds(0.01f);
			foreach (Entity item in ents)
			{
				if (!((UnityEngine.Object)(object)item == (UnityEngine.Object)(object)info.caster) && !list.Contains(item) && (tvDefaultHarmfulEffectTargets.Evaluate(item) || tvDefaultUsefulEffectTargets.Evaluate(item)))
				{
					list.Add(item);
					FxPlayNewNetworked(fxHit, item);
				}
			}
		}
		bool flag = list.Count <= 1;
		foreach (Entity item2 in list)
		{
			FxPlayNewNetworked(fxActualHit, item2);
			if (tvDefaultHarmfulEffectTargets.Evaluate(item2))
			{
				DamageData damageData = Damage(damageAmount, flag ? 1f : 0.75f).SetDirection(info.forward).SetElemental(ElementalType.Light);
				if (flag)
				{
					damageData.SetAttr(DamageAttribute.IsCrit);
					damageData.ApplyAmplification(singleTargetBonusAmp);
				}
				damageData.Dispatch(item2);
			}
			else if (tvDefaultUsefulEffectTargets.Evaluate(item2))
			{
				HealData healData = Heal(GetValue(healAmount));
				if (flag)
				{
					healData.SetCrit();
					healData.ApplyAmplification(singleTargetBonusAmp);
				}
				healData.Dispatch(item2);
			}
		}
		info.caster.Status.TryGetStatusEffect<Se_Q_GoldenBurst_Wither>(out var effect);
		if ((UnityEngine.Object)(object)effect != null)
		{
			effect.AddStack();
		}
		else
		{
			CreateStatusEffect<Se_Q_GoldenBurst_Wither>(info.caster);
		}
		Destroy();
		handle.Return();
	}

	private void MirrorProcessed()
	{
	}
}
