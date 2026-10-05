using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_R_UnbreakableDetermination : StatusEffect
{
	private float _baseInitDuration;

	private ScalingValue _baseDamage;

	private float _baseInvulnerableDuration;

	private ScalingValue _baseHasteAmount;

	private ScalingValue _baseSkillHasteAmount;

	private float _baseHealLostHealthRatio;

	private float _baseHealMinAmount;

	public float initDuration;

	public float explodeDelay = 0.3f;

	public float damageDelay = 0.1f;

	public float invulnerableDuration;

	public float speedDuration;

	public float speedAmount = 100f;

	public TriggerChannelData startChannelData;

	public float endDazeDuration;

	public GameObject explodeEffect;

	public GameObject explosionHit;

	public ScalingValue damage;

	public DewCollider range;

	public ScalingValue hasteAmount;

	public ScalingValue skillHasteAmount;

	public float dmgAmp;

	public float addedDuration;

	public float healLostHealthRatio;

	public float healMinAmount;

	public GameObject healEffect;

	public GameObject prepareEffect;

	public GameObject fxNonUltimate;

	[NonSerialized]
	public bool skipExplosion;

	private StatBonus _givenBonus;

	private Se_D_AstridsMasterpieceEnGarde _enGardeStatusEffect;

	private Se_D_AstridsMasterpiecePriorite _prioriteStatusEffect;

	private Action<EventInfoDamage> _cachedAstridHeal;

	private Action<EventInfoAttackEffect> _cachedAttackEffect;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_baseInitDuration = initDuration;
		_baseDamage = damage;
		_baseInvulnerableDuration = invulnerableDuration;
		_baseHasteAmount = hasteAmount;
		_baseSkillHasteAmount = skillHasteAmount;
		_baseHealLostHealthRatio = healLostHealthRatio;
		_baseHealMinAmount = healMinAmount;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		initDuration = _baseInitDuration;
		damage = _baseDamage;
		invulnerableDuration = _baseInvulnerableDuration;
		hasteAmount = _baseHasteAmount;
		skillHasteAmount = _baseSkillHasteAmount;
		healLostHealthRatio = _baseHealLostHealthRatio;
		healMinAmount = _baseHealMinAmount;
		skipExplosion = false;
		_enGardeStatusEffect = null;
		_prioriteStatusEffect = null;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		DoHaste(GetValue(hasteAmount));
		_givenBonus = new StatBonus
		{
			abilityHasteFlat = GetValue(skillHasteAmount)
		};
		victim.Status.AddStatBonus(_givenBonus);
		SetTimer(initDuration + explodeDelay);
		ShowOnScreenTimer();
		if (victim.Status.TryGetStatusEffect<Se_D_AstridsMasterpieceEnGarde>(out var effect))
		{
			foreach (Actor child in effect.children)
			{
				if (child is Se_D_AstridsMasterpieceEnGarde_Exposed se_D_AstridsMasterpieceEnGarde_Exposed)
				{
					se_D_AstridsMasterpieceEnGarde_Exposed.chargeCount = se_D_AstridsMasterpieceEnGarde_Exposed.maxCharge;
				}
			}
			_enGardeStatusEffect = effect;
			_enGardeStatusEffect.ActorEvent_OnDealDamage += new Action<EventInfoDamage>(AstridHeal);
		}
		if (victim.Status.TryGetStatusEffect<Se_D_AstridsMasterpiecePriorite>(out var effect2))
		{
			effect2.Reset();
			_prioriteStatusEffect = effect2;
			_prioriteStatusEffect.ActorEvent_OnDealDamage += new Action<EventInfoDamage>(AstridHeal);
		}
		SkillTrigger movement = ((Hero)victim).Skill.Movement;
		movement.SetCharge(0, movement.configs[0].maxCharges);
		victim.dealtDamageProcessor.Add(AmpDamage);
		victim.EntityEvent_OnAttackEffectTriggered += new Action<EventInfoAttackEffect>(AttackEffect);
		if (victim.Status.TryGetStatusEffect<Se_Star_Mist_F_UD_NonUltimate>(out var _))
		{
			FxPlayNetworked(fxNonUltimate, victim);
			return;
		}
		CreateBasicEffect(victim, new UncollidableEffect(), invulnerableDuration, "mist_r_uncol");
		CreateBasicEffect(victim, new InvulnerableEffect(), invulnerableDuration, "mist_r_invul");
		CreateBasicEffect(victim, new SpeedEffect
		{
			strength = speedAmount
		}, speedDuration, "mist_r_speed");
		victim.Control.StartChannel(startChannelData.CreateChannel(Explode, Explode, new AbilitySelfValidator()));
		victim.Control.StartChannel(new Channel
		{
			blockedActions = (Channel.BlockedAction.Ability | Channel.BlockedAction.Attack),
			duration = startChannelData.duration + endDazeDuration
		});
		FxPlayNetworked(prepareEffect, victim);
	}

	private void Explode()
	{
		if (!skipExplosion)
		{
			StartSequence(ExplodeSequence());
		}
	}

	private IEnumerator ExplodeSequence()
	{
		yield return new SI.WaitForSeconds(explodeDelay);
		FxStopNetworked(prepareEffect);
		FxPlayNetworked(explodeEffect, victim);
		yield return new SI.WaitForSeconds(damageDelay);
		List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
		for (int i = 0; i < entities.Count; i++)
		{
			Entity entity = entities[i];
			Damage(damage).SetOriginPosition(victim.position).Dispatch(entity);
			FxPlayNewNetworked(explosionHit, entity);
		}
		handle.Return();
	}

	private void AstridHeal(EventInfoDamage obj)
	{
		FxPlayNetworked(healEffect, victim);
		float originalAmount = Mathf.Max(healMinAmount, victim.Status.missingHealth * healLostHealthRatio);
		DoHeal(new HealData(originalAmount), victim, obj.chain.New(this));
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if ((UnityEngine.Object)(object)victim != null)
		{
			victim.Status.RemoveStatBonus(_givenBonus);
			victim.dealtDamageProcessor.Remove(AmpDamage);
			victim.EntityEvent_OnAttackEffectTriggered -= _cachedAttackEffect;
			if ((UnityEngine.Object)(object)_enGardeStatusEffect != null)
			{
				_enGardeStatusEffect.ActorEvent_OnDealDamage -= _cachedAstridHeal;
			}
			if ((UnityEngine.Object)(object)_prioriteStatusEffect != null)
			{
				_prioriteStatusEffect.ActorEvent_OnDealDamage -= _cachedAstridHeal;
			}
		}
		FxStopNetworked(prepareEffect);
		FxStopNetworked(fxNonUltimate);
	}

	private void AmpDamage(ref DamageData data, Actor actor, Entity target)
	{
		if (!data.IsAmountModifiedBy(this) && victim.CheckEnemyOrNeutral(target))
		{
			data.ApplyAmplification(dmgAmp);
			data.SetAmountModifiedBy(this);
		}
	}

	private void AttackEffect(EventInfoAttackEffect obj)
	{
		SetTimer(initDuration, Mathf.Min(remainingDuration.Value + addedDuration * obj.strength, initDuration));
		CreateAbilityInstance(obj.victim.position, null, new CastInfo(info.caster), (Ai_R_UnbreakableDetermination_SubExplosion s) =>
		{
			s.strengthMultiplier = obj.strength;
			s.chain = obj.chain.New(this);
		});
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		position = info.caster.agentPosition;
	}

	private void MirrorProcessed()
	{
	}
}
