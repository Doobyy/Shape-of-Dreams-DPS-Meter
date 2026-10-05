using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_GhostSpear_Atk : AbilityInstance
{
	public DewCollider swingRange;

	public DewCollider stingRange;

	public ScalingValue dmgFactor;

	public Knockback knockback;

	public AbilitySelfValidator channelValidator;

	public float delay;

	public float postDelay;

	public float stingChargeTime;

	public float swingChargeTime;

	public float swingAtkChance;

	public DewAnimationClip startAnim;

	public DewAnimationClip endAnim;

	public GameObject fxSwingStart;

	public GameObject fxStingStart;

	public GameObject fxChargeSwing;

	public GameObject fxChargeSting;

	public GameObject hitEffect;

	private DewCollider _range;

	private GameObject _chargeEffect;

	private GameObject _startEffect;

	private float _chargeDuration;

	private float _speedMultiplier;

	private ParticleSystem[] _ownParticleSystems;

	private float[] _ownBaseSimulationSpeeds;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		base.Awake();
		_ownParticleSystems = ((Component)(object)this).GetComponentsInChildren<ParticleSystem>(true);
		_ownBaseSimulationSpeeds = new float[_ownParticleSystems.Length];
		for (int i = 0; i < _ownParticleSystems.Length; i++)
		{
			float[] ownBaseSimulationSpeeds = _ownBaseSimulationSpeeds;
			int num = i;
			MainModule main = _ownParticleSystems[i].main;
			ownBaseSimulationSpeeds[num] = main.simulationSpeed;
		}
	}

	protected override IEnumerator OnCreateSequenced()
	{
		_speedMultiplier = info.caster.Status.attackSpeedMultiplier;
		for (int i = 0; i < _ownParticleSystems.Length; i++)
		{
			if (!((Object)(object)_ownParticleSystems[i] == null))
			{
				MainModule main = _ownParticleSystems[i].main;
				main.simulationSpeed = _ownBaseSimulationSpeeds[i];
			}
		}
		FxApplySpeedMultiplier(((Component)(object)this).gameObject, _speedMultiplier);
		FxApplySpeedMultiplier(hitEffect, _speedMultiplier);
		int index;
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			index = ((!(swingAtkChance > Random.value)) ? 1 : 0);
			if (index == 0)
			{
				_range = swingRange;
				_startEffect = fxSwingStart;
				_chargeEffect = fxChargeSwing;
				_chargeDuration = swingChargeTime / _speedMultiplier;
			}
			if (index == 1)
			{
				_range = stingRange;
				_startEffect = fxStingStart;
				_chargeEffect = fxChargeSting;
				_chargeDuration = stingChargeTime / _speedMultiplier;
			}
			info.caster.Control.Rotate(info.forward, immediately: false);
			info.caster.Animation.PlayAbilityAnimation(startAnim, 1f, (float)index * 0.5f);
			FxPlayNetworked(_chargeEffect, info.caster);
			info.caster.Control.StartChannel(new Channel
			{
				blockedActions = Channel.BlockedAction.Everything,
				duration = _chargeDuration + postDelay,
				isAttack = true,
				onCancel = () =>
				{
					info.caster.Animation.StopAbilityAnimation(startAnim);
					FxStopNetworked(_chargeEffect);
					ResetCooldown(firstTrigger);
					Destroy();
				},
				onComplete = () =>
				{
					((MonoBehaviour)(object)this).StartCoroutine(Routine());
				}
			}.AddValidation(channelValidator));
		}
		IEnumerator Routine()
		{
			info.caster.Animation.PlayAbilityAnimation(endAnim, 1f, (float)index * 0.5f);
			FxPlayNetworked(_startEffect);
			List<Entity> entities = _range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
			for (int j = 0; j < entities.Count; j++)
			{
				Entity entity = entities[j];
				FxPlayNewNetworked(hitEffect, entity);
				float num = 1.55f;
				float num2 = GetValue(dmgFactor);
				if (index == 1)
				{
					num2 *= num;
				}
				CreateDamage(DamageData.SourceType.Default, num2).SetElemental(ElementalType.Dark).SetDirection(((Component)(object)info.caster).transform.forward).SetOriginPosition(info.caster.agentPosition)
					.Dispatch(entity);
				knockback.ApplyWithOrigin(info.caster.agentPosition, entity);
			}
			handle.Return();
			info.caster.Control.StartDaze(postDelay);
			yield return new SI.WaitForSeconds(postDelay);
			Destroy();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(_chargeEffect);
		}
	}

	private void MirrorProcessed()
	{
	}
}
