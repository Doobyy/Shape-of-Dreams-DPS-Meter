using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_BossDarkMoon_ThrowSpear_Spawner : AbilityInstance
{
	public float attackDistance;

	public float projectileSpeed;

	public float projectileBackSpeed;

	public float minDelay;

	public float searchTargetRadius;

	public float postDelay;

	public GameObject fxThrow;

	public GameObject fxTake;

	public DewAnimationClip startClip;

	public DewAnimationClip takeClip;

	public float teleportChance;

	public float teleportDelay;

	public float teleportDuration;

	public float teleportPostDelay;

	public float teleportAtkDelay;

	public DewEase teleportEase;

	public GameObject fxTeleportAtkTelegraph;

	public GameObject fxDisbleRenderer;

	public GameObject fxTeleportStart;

	public GameObject fxTeleportEnd;

	public GameObject fxRageTelegraph;

	public DewAnimationClip teleportAtkCastClip;

	public DewAnimationClip teleportAtkEndClip;

	public float tickInterval;

	public int tickCount;

	public int tickCountOnTeleport;

	public GameObject fxFakeAoE;

	private Channel _channel;

	private bool _isRage;

	private Mon_Ink_BossDarkMoon _boss;

	private float _baseSearchTargetRadius;

	private float _baseTeleportChance;

	private Vector3 _baseFxFakeAoEScale;

	protected override void Awake()
	{
		base.Awake();
		_baseSearchTargetRadius = searchTargetRadius;
		_baseTeleportChance = teleportChance;
		if (fxFakeAoE != null)
		{
			_baseFxFakeAoEScale = fxFakeAoE.transform.localScale;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		searchTargetRadius = _baseSearchTargetRadius;
		teleportChance = _baseTeleportChance;
		if (fxFakeAoE != null)
		{
			fxFakeAoE.transform.localScale = _baseFxFakeAoEScale;
		}
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		CreateBasicEffect(info.caster, new UnstoppableEffect(), float.PositiveInfinity).DestroyOnDestroy(this);
		_boss = (Mon_Ink_BossDarkMoon)info.caster;
		_boss.StopSpearEffectNetworked();
		_isRage = _boss._isRage;
		if (_isRage)
		{
			fxFakeAoE.transform.localScale = Vector3.one * 1.5f;
			searchTargetRadius *= 1.5f;
		}
		_channel = info.caster.Control.StartChannel(new Channel
		{
			blockedActions = Channel.BlockedAction.Everything,
			duration = float.PositiveInfinity,
			onCancel = DestroyIfActive
		});
		float targetSpeed = 15f;
		float acc = 60f;
		Vector3 targetPoint = info.caster.agentPosition + info.forward * attackDistance;
		Vector3 validAgentDestination_Closest = Dew.GetValidAgentDestination_Closest(info.caster.agentPosition, targetPoint);
		float distance = (targetPoint - info.caster.agentPosition).magnitude;
		float terrainDitance = (validAgentDestination_Closest - info.caster.agentPosition).magnitude;
		FxPlayNetworked(fxThrow, info.caster);
		Ai_Mon_Ink_BossDarkMoon_ThrowSpear_Projectile ai = CreateAbilityInstance(info.caster.agentPosition, info.rotation, new CastInfo(info.caster, targetPoint), (Ai_Mon_Ink_BossDarkMoon_ThrowSpear_Projectile b) =>
		{
			b.initialSpeed = projectileSpeed;
			b.targetSpeed = targetSpeed;
			b.acceleration = acc;
			b.SetCustomStartPosition(info.caster.agentPosition);
			if (_isRage)
			{
				b.effectOnFly.transform.localScale = Vector3.one * 1.5f;
				b.collisionRadius *= 1.5f;
			}
		});
		yield return new SI.WaitForSeconds(0.25f);
		info.caster.Animation.PlayAbilityAnimation(startClip);
		yield return new SI.WaitForCondition(() => ai.normalizedPosition > 0.999f);
		FxPlayNetworked(fxFakeAoE, targetPoint, null);
		bool enableTeleport = false;
		int ticks = tickCount;
		if (DewPhysics.OverlapCircleAllEntities(out var handle, targetPoint, searchTargetRadius, tvDefaultHarmfulEffectTargets).Count > 0)
		{
			enableTeleport = true;
		}
		handle.Return();
		enableTeleport = enableTeleport && Random.value < teleportChance;
		if (enableTeleport)
		{
			ticks = tickCountOnTeleport;
		}
		float aoeDuration = (float)ticks * tickInterval;
		if (distance > terrainDitance)
		{
			aoeDuration *= 0.1f;
			teleportChance = 0f;
		}
		Ai_Mon_Ink_BossDarkMoon_ThrowSpear_AoE aoeInstance = CreateAbilityInstance(targetPoint, null, new CastInfo(info.caster, targetPoint), (Ai_Mon_Ink_BossDarkMoon_ThrowSpear_AoE b) =>
		{
			b.tickInterval = tickInterval;
			b.ticks = ticks;
			if (_isRage)
			{
				b.fxLoop.transform.localScale = Vector3.one * 1.5f;
				b.range.radius *= 1.5f;
			}
		});
		yield return new SI.WaitForSeconds(minDelay);
		if (enableTeleport && Random.value < teleportChance)
		{
			yield return new SI.WaitForSeconds(teleportDelay);
			FxPlayNetworked(fxTeleportStart, info.caster);
			FxPlayNetworked(fxDisbleRenderer, info.caster);
			info.caster.Control.StartDisplacement(new DispByDestination
			{
				affectedByMovementSpeed = false,
				canGoOverTerrain = true,
				destination = targetPoint,
				duration = teleportDuration,
				ease = teleportEase,
				isCanceledByCC = false,
				isFriendly = true,
				onCancel = DestroyIfActive,
				onFinish = () =>
				{
					((MonoBehaviour)(object)this).StartCoroutine(Routine());
				}
			});
			yield break;
		}
		yield return new SI.WaitForSeconds(aoeDuration - minDelay);
		ai = CreateAbilityInstance(targetPoint, null, new CastInfo(info.caster, info.caster.agentPosition), (Ai_Mon_Ink_BossDarkMoon_ThrowSpear_Projectile b) =>
		{
			b.initialSpeed = projectileBackSpeed;
			b.SetCustomStartPosition(targetPoint);
			if (_isRage)
			{
				b.effectOnFly.transform.localScale = Vector3.one * 1.5f;
				b.collisionRadius *= 1.5f;
			}
		});
		FxStopNetworked(fxFakeAoE);
		yield return new SI.WaitForCondition(() => ai.normalizedPosition > 0.001f);
		aoeInstance.DestroyIfActive();
		yield return new SI.WaitForCondition(() => ai.normalizedPosition < 0.999f);
		yield return new SI.WaitForSeconds(0.2f);
		FxPlayNetworked(fxTake, info.caster);
		_boss.PlaySpawnSpearEffectNetworked();
		info.caster.Animation.PlayAbilityAnimation(takeClip);
		yield return new SI.WaitForSeconds(postDelay);
		DestroyIfActive();
		IEnumerator Routine()
		{
			Hero closestAliveHero = Dew.GetClosestAliveHero(info.caster.agentPosition, fallbackToDead: true, info.caster);
			if ((Object)(object)closestAliveHero != null)
			{
				info.caster.Control.Rotate(closestAliveHero.GetAIAgentPosition(info.caster) - info.caster.agentPosition, immediately: true);
			}
			FxStopNetworked(fxFakeAoE);
			aoeInstance.DestroyIfActive();
			_boss.PlaySpawnSpearEffectNetworked();
			info.caster.Animation.PlayAbilityAnimation(teleportAtkCastClip);
			FxStopNetworked(fxDisbleRenderer);
			FxPlayNetworked(fxTeleportEnd, info.caster);
			yield return new WaitForSeconds(teleportPostDelay);
			if (_isRage)
			{
				FxPlayNetworked(fxRageTelegraph, info.caster.agentPosition, info.caster.rotation);
			}
			FxPlayNetworked(fxTeleportAtkTelegraph, info.caster.agentPosition, null);
			yield return new WaitForSeconds(teleportAtkDelay);
			info.caster.Animation.PlayAbilityAnimation(teleportAtkEndClip);
			CreateAbilityInstance(info.caster.agentPosition, null, new CastInfo(info.caster), (Ai_Mon_Ink_BossDarkMoon_ThrowSpear_TeleportAtk b) =>
			{
				if (_isRage)
				{
					b.isRage = true;
				}
			});
			DestroyIfActive();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if (_channel != null)
			{
				_channel.Cancel();
				_channel = null;
			}
			if ((Object)(object)_boss != null)
			{
				_boss.PlaySpawnSpearEffectNetworked();
			}
			FxStopNetworked(fxDisbleRenderer);
			FxStopNetworked(fxFakeAoE);
		}
	}

	private void MirrorProcessed()
	{
	}
}
