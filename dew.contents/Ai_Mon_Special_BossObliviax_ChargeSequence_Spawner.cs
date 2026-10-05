using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossObliviax_ChargeSequence_Spawner : AbilityInstance
{
	public float initDelay;

	public float duration;

	public GameObject fxDisappear;

	public float fsDelay;

	public float fsDuration;

	public GameObject fsTelegraphEffect;

	public float ssDelay;

	public int ssWaveCount;

	public int ssAtkCountPerWave;

	public float ssStartPointRadius;

	public int ssStartPointCount;

	public float ssAtkDelay;

	public float ssAtkInterval;

	public float ssWaveInterval;

	public GameObject ssTelegraphEffect;

	public GameObject ssStartEffect;

	public GameObject ssCloneEffect;

	public DewAnimationClip ssCloneAnimation;

	public float tsDelay;

	public float tsTelegraphInterval;

	public float tsAtkDelay;

	public GameObject tsStartTelegraphEffect;

	public GameObject tsFirstAtkEffect;

	public ScalingValue tsFirstAtkDmgFactor;

	public DewAnimationClip tsLandingAnimation;

	public float tsPostDelay;

	[Space(15f)]
	[Header("Donut Atk")]
	public int donutRadiusAdder = 4;

	public int donutCount;

	public DewCollider tsDonutRange;

	public GameObject tsDonutTelegraphEffect;

	public GameObject tsDonutAtkEffect;

	public ScalingValue tsDonutDmgFactor;

	private Vector3 _roomCenterPos;

	private BoxTelegraphController _boxTelegraphController;

	private ArcTelegraphController _arcTelegraphController;

	private DewCollider _donutCollider;

	private float _donutStartRadius;

	private float _baseArcInnerRadius;

	private float _baseArcOuterRadius;

	private Vector3 _baseDonutAtkScale;

	protected override void Awake()
	{
		base.Awake();
		_arcTelegraphController = tsDonutTelegraphEffect.GetComponentInChildren<ArcTelegraphController>(includeInactive: true);
		_baseArcInnerRadius = _arcTelegraphController.innerRadius;
		_baseArcOuterRadius = _arcTelegraphController.outerRadius;
		_baseDonutAtkScale = tsDonutAtkEffect.transform.localScale;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_arcTelegraphController.innerRadius = _baseArcInnerRadius;
		_arcTelegraphController.outerRadius = _baseArcOuterRadius;
		tsDonutAtkEffect.transform.localScale = _baseDonutAtkScale;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		if (SingletonBehaviour<Obliviax_BossRoomCenter>.instance != null)
		{
			_roomCenterPos = SingletonBehaviour<Obliviax_BossRoomCenter>.instance.transform.position;
		}
		else
		{
			_roomCenterPos = info.caster.section.transform.position;
		}
		_roomCenterPos = Dew.GetPositionOnGround(_roomCenterPos);
		_boxTelegraphController = ssTelegraphEffect.GetComponentInChildren<BoxTelegraphController>();
		_donutCollider = tsDonutRange.GetComponent<DewCollider>();
		_donutStartRadius = _donutCollider.radius;
		duration = initDelay + fsDelay + ssDelay + ssAtkInterval * (float)ssAtkCountPerWave * ssWaveInterval + ssWaveInterval * (float)ssWaveCount + tsDelay + tsAtkDelay;
		FxPlayNetworked(fxDisappear, info.caster);
		CreateBasicEffect(info.caster, new UntargetableEffect(), duration);
		CreateBasicEffect(info.caster, new InvisibleEffect
		{
			ignoreReveal = true
		}, duration);
		CreateBasicEffect(info.caster, new InvulnerableEffect(), duration);
		CreateBasicEffect(info.caster, new UncollidableEffect(), duration);
		info.caster.Visual.DisableRenderers();
		info.caster.Control.freeMovement = true;
		info.caster.Control.StartDaze(duration);
		yield return new SI.WaitForSeconds(initDelay);
		List<(Hero, Vector3)> heros = new List<(Hero, Vector3)>();
		foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
		{
			Hero hero = gamePlayer.hero;
			if (!hero.IsNullInactiveDeadOrKnockedOut())
			{
				Vector3 aIAgentPosition = hero.GetAIAgentPosition(info.caster);
				heros.Add((hero, aIAgentPosition));
				FxPlayNewNetworked(fsTelegraphEffect, aIAgentPosition, null);
			}
		}
		info.caster.Control.StartDaze(fsDelay);
		yield return new SI.WaitForSeconds(fsDelay);
		foreach (var item in heros)
		{
			var (hero2, pos) = item;
			if (!hero2.IsNullInactiveDeadOrKnockedOut())
			{
				CreateAbilityInstance(hero2.GetAIAgentPosition(info.caster), null, new CastInfo(info.caster, hero2), (Ai_Mon_Special_BossObliviax_ChargeSequence_MissileAtk b) =>
				{
					b.initPoint = pos;
					b.duration = fsDuration;
				});
			}
		}
		yield return new SI.WaitForSeconds(ssDelay);
		ssAtkCountPerWave += DewPlayer.gamePlayers.Count - 1;
		float ssAngle = 360f / (float)ssStartPointCount;
		List<int> availableIndices = new List<int>();
		for (int i = 0; i < ssWaveCount; i++)
		{
			for (int num = 0; num < ssStartPointCount; num++)
			{
				availableIndices.Add(num);
			}
			for (int j = 0; j < ssAtkCountPerWave; j++)
			{
				int num2 = Random.Range(0, availableIndices.Count);
				availableIndices.RemoveAt(num2);
				Vector3 normalized = (Quaternion.AngleAxis((float)num2 * ssAngle, Vector3.up) * Vector3.forward).Flattened().normalized;
				Vector3 startPos = _roomCenterPos + normalized * ssStartPointRadius;
				startPos = Dew.GetValidAgentDestination_LinearSweep(_roomCenterPos, startPos);
				Quaternion finalRot = default;
				float finalAngle = 0f;
				if (j < DewPlayer.gamePlayers.Count && !DewPlayer.gamePlayers[j].hero.IsNullInactiveDeadOrKnockedOut())
				{
					normalized = (AbilityTrigger.PredictPoint_Simple(info.caster, Random.Range(0.7f, 1f), DewPlayer.gamePlayers[j].hero, ssAtkDelay) - startPos).normalized;
					finalRot = Quaternion.AngleAxis(finalAngle = Vector3.Angle(Vector3.forward, normalized), Vector3.up);
				}
				else
				{
					normalized = Quaternion.Euler(0f, Random.Range(-20f, 20f), 0f) * -normalized;
					finalAngle = Vector3.Angle(Vector3.forward, normalized);
					finalRot = Quaternion.LookRotation(normalized);
				}
				SpawnEntity(startPos, finalRot, DewPlayer.creep, info.caster.level, (Mon_Special_ObliviaxHallucination b) =>
				{
					((MonoBehaviour)(object)this).StartCoroutine(Routine());
					IEnumerator Routine()
					{
						b.Visual.spawnDuration = ssAtkDelay;
						b.Control.StartDaze(3f);
						yield return new WaitForSeconds(ssAtkDelay);
						CreateAbilityInstance<Ai_Mon_Special_BossObliviax_ChargeSequence_LinearAtk>(startPos, finalRot, new CastInfo(info.caster, finalAngle));
						FxPlayNewNetworked(ssCloneEffect, b);
						b.Animation.PlayAbilityAnimation(ssCloneAnimation);
						b.Control.StartDisplacement(new DispByDestination
						{
							affectedByMovementSpeed = false,
							canGoOverTerrain = true,
							destination = b.agentPosition + ((Component)(object)b).transform.forward * _boxTelegraphController.height,
							duration = 0.35f,
							isFriendly = true,
							isCanceledByCC = false,
							onFinish = () =>
							{
								FxStopNetworked(ssCloneEffect);
								b.Destroy();
							}
						});
						yield return new WaitForSeconds(0.6f);
					}
				});
				FxPlayNewNetworked(ssTelegraphEffect, startPos + normalized * (_boxTelegraphController.height / 2f), finalRot);
				yield return new SI.WaitForSeconds(ssAtkInterval);
			}
			availableIndices.Clear();
			yield return new SI.WaitForSeconds(ssWaveInterval);
		}
		yield return new SI.WaitForSeconds(tsDelay);
		Hero hero3 = Dew.SelectRandomAliveHero(fallbackToDead: true, skipStealthed: true);
		Vector3 targetPos = Dew.GetPositionOnGround(hero3.GetAIAgentPosition(info.caster));
		info.caster.Teleport(info.caster, targetPos);
		FxPlayNetworked(tsStartTelegraphEffect, targetPos, null);
		yield return new SI.WaitForSeconds(tsAtkDelay);
		FxPlayNetworked(tsFirstAtkEffect, targetPos, null);
		info.caster.Visual.EnableRenderers();
		info.caster.Animation.PlayAbilityAnimation(tsLandingAnimation);
		info.caster.Control.StartDaze(tsPostDelay);
		info.caster.Control.freeMovement = false;
		tsDonutRange.transform.position = targetPos;
		List<Entity> entities = tsDonutRange.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
		for (int num3 = 0; num3 < entities.Count; num3++)
		{
			Entity entity = entities[num3];
			if (!entity.IsNullInactiveDeadOrKnockedOut())
			{
				CreateDamage(DamageData.SourceType.Default, tsFirstAtkDmgFactor).SetOriginPosition(targetPos).Dispatch(entity);
				entity.Visual.KnockUp(KnockUpStrength.Big, isFriendly: false);
			}
		}
		handle.Return();
		yield return new SI.WaitForSeconds(0.35f);
		for (int i = 0; i < donutCount; i++)
		{
			_arcTelegraphController.innerRadius = _baseArcOuterRadius + (float)(donutRadiusAdder * i);
			_arcTelegraphController.outerRadius = _arcTelegraphController.innerRadius + (float)donutRadiusAdder;
			FxPlayNewNetworked(tsDonutTelegraphEffect, targetPos, null);
			yield return new SI.WaitForSeconds(tsTelegraphInterval);
		}
		yield return new SI.WaitForSeconds(1f);
		int effectCount = 12;
		float effectScale = 0.1f;
		float angle = 360f / (float)effectCount;
		float outerRadius = _donutStartRadius;
		for (int i = 0; i < donutCount; i++)
		{
			float num4 = outerRadius;
			outerRadius += (float)donutRadiusAdder;
			tsDonutAtkEffect.transform.localScale = _baseDonutAtkScale * (1f + effectScale * (float)i);
			float num5 = num4 + (outerRadius - num4) / 2f;
			for (int num6 = 0; num6 < effectCount + i * 3; num6++)
			{
				Vector3 normalized2 = (Quaternion.AngleAxis(angle * (float)num6, Vector3.up) * Vector3.forward).Flattened().normalized;
				Vector3 vector = targetPos + normalized2 * num5;
				FxPlayNewNetworked(tsDonutAtkEffect, vector, null);
			}
			_donutCollider.GeneratePolygonPoints_Donut(num4, outerRadius);
			_donutCollider.UpdateProxyCollider();
			entities = tsDonutRange.GetEntities(out handle, tvDefaultHarmfulEffectTargets);
			for (int num7 = 0; num7 < entities.Count; num7++)
			{
				Entity entity2 = entities[num7];
				if (!entity2.IsNullInactiveDeadOrKnockedOut())
				{
					CreateDamage(DamageData.SourceType.Default, tsDonutDmgFactor).SetOriginPosition(_roomCenterPos).Dispatch(entity2);
				}
			}
			handle.Return();
			yield return new SI.WaitForSeconds(tsTelegraphInterval / 2f);
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
