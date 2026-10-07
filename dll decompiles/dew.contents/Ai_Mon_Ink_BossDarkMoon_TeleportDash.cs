using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_BossDarkMoon_TeleportDash : AbilityInstance
{
	public float dashDistanceThreshold;

	public float maxDashDistance;

	public float minDashDistance;

	public float randomDashChance;

	public float dashSpeed;

	public float dashRandomMagnitude;

	public DewEase dashEase;

	public float maxTeleportDistance;

	public float teleportRandomMagnitude;

	public float teleportDuration;

	public float teleportPostDelay;

	public bool doInvulnerable;

	public bool doUncollidable;

	public GameObject fxTeleportStart;

	public GameObject fxTeleportEnd;

	public GameObject fxDisableRenderer;

	private Channel _channel;

	private bool _isRage;

	private float _baseMaxDashDistance;

	private float _baseMaxTeleportDistance;

	private float _baseDashSpeed;

	private float _baseTeleportDuration;

	private float _baseTeleportPostDelay;

	protected override void Awake()
	{
		base.Awake();
		_baseMaxDashDistance = maxDashDistance;
		_baseMaxTeleportDistance = maxTeleportDistance;
		_baseDashSpeed = dashSpeed;
		_baseTeleportDuration = teleportDuration;
		_baseTeleportPostDelay = teleportPostDelay;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		maxDashDistance = _baseMaxDashDistance;
		maxTeleportDistance = _baseMaxTeleportDistance;
		dashSpeed = _baseDashSpeed;
		teleportDuration = _baseTeleportDuration;
		teleportPostDelay = _baseTeleportPostDelay;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		float duration;
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			_channel = info.caster.Control.StartChannel(new Channel
			{
				blockedActions = Channel.BlockedAction.Everything,
				duration = float.PositiveInfinity
			});
			_isRage = ((Mon_Ink_BossDarkMoon)info.caster)._isRage;
			if (_isRage)
			{
				maxDashDistance *= 1.5f;
				maxTeleportDistance *= 1.5f;
				dashSpeed *= 2f;
				teleportDuration *= 0.5f;
				teleportPostDelay *= 0.5f;
			}
			_ = info.caster.agentPosition;
			Vector3 vector = info.target.GetAIAgentPosition(info.caster) - info.caster.agentPosition;
			Vector3 normalized = vector.normalized;
			Vector3 vector2;
			if (vector.magnitude >= dashDistanceThreshold)
			{
				vector2 = info.caster.agentPosition + normalized * maxDashDistance;
			}
			else if (vector.magnitude < minDashDistance)
			{
				vector2 = info.caster.agentPosition - normalized * Random.Range(minDashDistance, maxDashDistance) + Random.insideUnitCircle.ToXZ() * dashRandomMagnitude;
			}
			else
			{
				vector2 = ((!(Random.value < randomDashChance)) ? (info.caster.agentPosition + normalized * Random.Range(minDashDistance, maxDashDistance) + Random.insideUnitCircle.ToXZ() * dashRandomMagnitude) : (info.caster.agentPosition + Random.insideUnitCircle.ToXZ().normalized * Random.Range(minDashDistance, maxDashDistance) + Random.insideUnitCircle.ToXZ() * dashRandomMagnitude));
			}
			vector2 = Dew.GetPositionOnGround(vector2);
			vector2 = Dew.GetValidAgentDestination_LinearSweep(info.caster.agentPosition, vector2);
			duration = (vector2 - info.caster.agentPosition).magnitude / dashSpeed;
			info.caster.Control.RotateTowards(info.target, immediately: false, duration);
			info.caster.Control.StartDisplacement(new DispByDestination
			{
				affectedByMovementSpeed = false,
				canGoOverTerrain = true,
				destination = vector2,
				rotateForward = false,
				ease = dashEase,
				duration = duration,
				isFriendly = true,
				isCanceledByCC = false,
				onCancel = DestroyIfActive,
				onFinish = () =>
				{
					TeleportRoutine();
				}
			});
		}
		void TeleportRoutine()
		{
			if (doInvulnerable)
			{
				CreateBasicEffect(info.caster, new InvulnerableEffect(), duration, "darkmoon_invul");
			}
			if (doUncollidable)
			{
				CreateBasicEffect(info.caster, new UncollidableEffect(), duration, "darkmoon_uncol");
			}
			FxPlayNetworked(fxTeleportStart, info.caster);
			FxPlayNetworked(fxDisableRenderer, info.caster);
			Vector3 agentPosition = info.caster.agentPosition;
			Hero target = Dew.GetClosestAliveHero(info.caster.agentPosition, fallbackToDead: true, info.caster);
			Vector3 vector3 = AbilityTrigger.PredictPoint_Simple(info.caster, NetworkedManagerBase<GameManager>.instance.GetPredictionStrength(), target, teleportDuration);
			Vector3 vector4 = vector3 - info.caster.agentPosition;
			Vector3 normalized2 = vector4.normalized;
			agentPosition = ((!(vector4.magnitude < maxTeleportDistance)) ? (info.caster.agentPosition + normalized2 * maxTeleportDistance) : (vector3 + Random.insideUnitCircle.ToXZ() * teleportRandomMagnitude));
			agentPosition = Dew.GetPositionOnGround(agentPosition);
			agentPosition = Dew.GetValidAgentDestination_Closest(info.caster.agentPosition, agentPosition);
			info.caster.Control.StartDisplacement(new DispByDestination
			{
				affectedByMovementSpeed = false,
				canGoOverTerrain = true,
				duration = teleportDuration,
				destination = agentPosition,
				isCanceledByCC = false,
				isFriendly = true,
				onCancel = DestroyIfActive,
				onFinish = () =>
				{
					info.caster.Control.Rotate(target.GetAIAgentPosition(info.caster) - info.caster.agentPosition, immediately: true);
					FxPlayNetworked(fxTeleportEnd, info.caster);
					FxStopNetworked(fxDisableRenderer);
					info.caster.Control.StartDaze(teleportPostDelay);
					DestroyIfActive();
				}
			});
		}
		yield break;
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
			FxStopNetworked(fxDisableRenderer);
		}
	}

	private void MirrorProcessed()
	{
	}
}
