using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_BossWhiteNight_Oppression : AbilityInstance
{
	public ScalingValue dmgFactor;

	public DewCollider range;

	public GameObject fxHit;

	public GameObject fxInstance;

	public GameObject fxTelegraph;

	public DewAnimationClip atkClip;

	public DewAnimationClip endClip;

	public float startDelay;

	public float postDelay;

	public float stunDuration;

	public float dispDuration;

	public DewEase dispEase;

	[Space(15f)]
	public float rageInstanceStartDelay;

	public float rageInstanceDuration;

	public float rageInstanceRadius;

	public float rageMultiplier;

	public DewEase rageInstanceEase;

	public GameObject fxRageInstance;

	[Space(15f)]
	public float darkMoonBladeRange;

	public float darkMoonBladeDistance;

	public float darkMoonBladeChance;

	public float darkMoonBladeDelay;

	public float darkMoonHammerRange;

	public float darkMoonHammerChance;

	private bool _isRage;

	private Mon_Ink_BossDarkMoon _darkMoon;

	private bool _darkMoonUsedBlade;

	private Vector3 _darkMoonBladeTargetPos;

	private float _startDelaySnapshot;

	private BoxTelegraphController[] _telegraphBoxes;

	private float[] _telegraphBoxDurations;

	private int _generation;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_startDelaySnapshot = startDelay;
		if (fxTelegraph != null)
		{
			_telegraphBoxes = fxTelegraph.GetComponentsInChildren<BoxTelegraphController>(includeInactive: true);
			_telegraphBoxDurations = new float[_telegraphBoxes.Length];
			for (int i = 0; i < _telegraphBoxes.Length; i++)
			{
				_telegraphBoxDurations[i] = _telegraphBoxes[i].duration;
			}
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_generation++;
		_darkMoonUsedBlade = false;
		startDelay = _startDelaySnapshot;
		if (_telegraphBoxes == null)
		{
			return;
		}
		for (int i = 0; i < _telegraphBoxes.Length; i++)
		{
			if (_telegraphBoxes[i] != null)
			{
				_telegraphBoxes[i].duration = _telegraphBoxDurations[i];
			}
		}
	}

	protected override void OnPrepare()
	{
		base.OnPrepare();
		Mon_Ink_BossWhiteNight mon_Ink_BossWhiteNight = (Mon_Ink_BossWhiteNight)info.caster;
		_isRage = mon_Ink_BossWhiteNight._isRage;
		_darkMoon = mon_Ink_BossWhiteNight._bossDarkMoon;
		if (_isRage)
		{
			BoxTelegraphController[] componentsInChildren = fxTelegraph.GetComponentsInChildren<BoxTelegraphController>();
			startDelay *= rageMultiplier;
			BoxTelegraphController[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].duration = startDelay;
			}
		}
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer || !_isRage)
		{
			yield break;
		}
		yield return new SI.WaitForSeconds(rageInstanceStartDelay);
		FxPlayNetworked(fxRageInstance, info.point, Quaternion.identity);
		List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, info.point, rageInstanceRadius);
		for (int i = 0; i < list.Count; i++)
		{
			Entity entity = list[i];
			if (!entity.Status.hasCrowdControlImmunity)
			{
				Vector3 validAgentDestination_LinearSweep = Dew.GetValidAgentDestination_LinearSweep(entity.agentPosition, info.point + Random.insideUnitCircle.ToXZ() * Random.Range(0f, 0.5f));
				entity.Control.StartDisplacement(new DispByDestination
				{
					affectedByMovementSpeed = false,
					canGoOverTerrain = false,
					destination = validAgentDestination_LinearSweep,
					duration = rageInstanceDuration,
					ease = rageInstanceEase,
					isCanceledByCC = false,
					isFriendly = false,
					rotateForward = false
				});
			}
		}
		handle.Return();
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		DestroyOnDeath(info.caster);
		CreateBasicEffect(info.caster, new UnstoppableEffect(), float.PositiveInfinity).DestroyOnDestroy(this);
		if (!_darkMoon.IsNullOrInactive() && Random.value <= darkMoonBladeChance && _darkMoon.Control.IsActionBlocked(EntityControl.BlockableAction.Attack) == EntityControl.BlockStatus.Allowed && _darkMoon.Ability.attackAbility.currentConfig.selfValidator.Evaluate(_darkMoon) && Vector3.Distance(info.point, _darkMoon.position) <= darkMoonBladeRange)
		{
			Hero closestAliveHero = Dew.GetClosestAliveHero(info.point, fallbackToDead: true, info.caster);
			if (!closestAliveHero.IsNullInactiveDeadOrKnockedOut())
			{
				Vector3 normalized = (closestAliveHero.GetAIAgentPosition(info.caster) - info.point).normalized;
				Vector3 targetPoint = AbilityTrigger.PredictPoint_Simple(info.caster, NetworkedManagerBase<GameManager>.instance.GetPredictionStrength(), closestAliveHero, darkMoonBladeDelay) + normalized * Random.Range(0.8f, darkMoonBladeDistance);
				targetPoint = Dew.GetValidAgentDestination_Closest(info.point, targetPoint);
				_darkMoonBladeTargetPos = targetPoint;
				_darkMoon.AI.Aggro(closestAliveHero);
				_darkMoon.Ability.attackAbility.SetCharge(0, 0);
				CreateAbilityInstance(_darkMoon.position, null, new CastInfo(_darkMoon, closestAliveHero), (Ai_Mon_Ink_BossDarkMoon_Blade b) =>
				{
					b.forcedDest = targetPoint;
					b.whiteNightSpawnDelay = darkMoonBladeDelay;
					b.isWhiteNightSpawn = true;
				});
				_darkMoonUsedBlade = true;
			}
		}
		FxPlayNetworked(fxTelegraph, info.point, Quaternion.LookRotation(info.point - info.caster.position));
		info.caster.Control.StartChannel(new Channel
		{
			blockedActions = Channel.BlockedAction.Everything,
			duration = startDelay,
			onCancel = DestroyIfActive,
			onComplete = OnComplete
		});
	}

	private void OnComplete()
	{
		int gen = _generation;
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			info.caster.Control.StartDaze(postDelay);
			info.caster.Animation.PlayAbilityAnimation(atkClip);
			FxPlayNetworked(fxInstance, info.point, Quaternion.LookRotation(info.point - info.caster.position));
			yield return new WaitForSeconds(0.1f);
			if (isActive)
			{
				List<Entity> list = new List<Entity>();
				range.transform.position = info.point;
				List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
				for (int i = 0; i < entities.Count; i++)
				{
					Entity entity = entities[i];
					CreateDamage(DamageData.SourceType.Default, dmgFactor).SetOriginPosition(info.caster.position).Dispatch(entity);
					FxPlayNewNetworked(fxHit, entity);
					if (!entity.IsNullInactiveDeadOrKnockedOut() || !entity.Status.hasCrowdControlImmunity)
					{
						Vector3 normalized = (info.point - info.caster.agentPosition).normalized;
						float num = Vector3.Dot(entity.agentPosition - info.point, normalized);
						Vector3 end = info.point + normalized * num;
						entity.Control.StartDisplacement(new DispByDestination
						{
							affectedByMovementSpeed = false,
							canGoOverTerrain = false,
							destination = Dew.GetValidAgentDestination_LinearSweep(entity.agentPosition, end),
							duration = dispDuration,
							ease = dispEase,
							isCanceledByCC = false,
							isFriendly = false,
							onCancel = () =>
							{
								if (gen == _generation)
								{
									DestroyIfActive();
								}
							},
							onFinish = () =>
							{
								if (gen == _generation)
								{
									DestroyIfActive();
								}
							}
						});
						CreateBasicEffect(entity, new StunEffect(), stunDuration, "whitenight_stun");
						list.Add(entity);
					}
				}
				handle.Return();
				if (!_darkMoon.IsNullOrInactive() && !_darkMoonUsedBlade && Random.value <= darkMoonHammerChance && _darkMoon.Control.IsActionBlocked(EntityControl.BlockableAction.Ability) == EntityControl.BlockStatus.Allowed && _darkMoon.Ability.GetAbility<At_Mon_Ink_BossDarkMoon_Hammer>().currentConfig.selfValidator.Evaluate(_darkMoon) && Vector3.Distance(info.point, _darkMoon.position) <= darkMoonHammerRange)
				{
					Entity orDefault = list.GetOrDefault(Random.Range(0, list.Count));
					if (!orDefault.IsNullInactiveDeadOrKnockedOut())
					{
						_darkMoon.AI.Aggro(orDefault);
						At_Mon_Ink_BossDarkMoon_Hammer ability = _darkMoon.Ability.GetAbility<At_Mon_Ink_BossDarkMoon_Hammer>();
						ResetCooldown(ability);
						_darkMoon.Control.Cast(ability, 0, new CastInfo(_darkMoon, orDefault), allowMoveToCast: true, skipRangeCheck: true);
					}
				}
				yield return new WaitForSeconds(0.5f);
				if (isActive)
				{
					info.caster.Animation.PlayAbilityAnimation(endClip);
					Destroy();
				}
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
