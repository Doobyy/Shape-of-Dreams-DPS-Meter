using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossErebos_StarRain_Spawner : AbilityInstance
{
	public float postDelay;

	public float minDistance;

	public float atkDuration;

	public float maxDeviation;

	public float backStepDistance;

	public float curveSize;

	public float atkInterval;

	public int atkCount;

	public int enhancedAddedCount;

	public DewEase ease;

	public GameObject fxMeshTrail;

	private int _baseAtkCount;

	private float _baseAtkInterval;

	private bool _cachedBase;

	protected override void Awake()
	{
		base.Awake();
		if (!_cachedBase)
		{
			_baseAtkCount = atkCount;
			_baseAtkInterval = atkInterval;
			_cachedBase = true;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (_cachedBase)
		{
			atkCount = _baseAtkCount;
			atkInterval = _baseAtkInterval;
		}
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		if (((Mon_Special_BossErebos)info.caster)._isPhaseChanged)
		{
			atkInterval = (float)atkCount * atkInterval / (float)(atkCount + enhancedAddedCount);
			atkCount += enhancedAddedCount;
		}
		info.caster.Control.StartChannel(new Channel
		{
			blockedActions = Channel.BlockedAction.Everything,
			duration = atkInterval * (float)atkCount + postDelay,
			isAttack = false,
			onCancel = DestroyIfActive
		});
		if ((info.target.GetAIAgentPosition(info.caster) - info.caster.agentPosition).sqrMagnitude <= minDistance * minDistance)
		{
			FxPlayNetworked(fxMeshTrail, info.caster);
			yield return ((MonoBehaviour)(object)this).StartCoroutine(StartDisplacementRoutine());
		}
		for (int i = 0; i < atkCount; i++)
		{
			Vector3 vector = AbilityTrigger.PredictPoint_Simple(info.caster, Random.Range(0, 2), info.target, atkDuration) + Random.insideUnitCircle.ToXZ() * Random.Range(0f, maxDeviation);
			vector = Dew.GetPositionOnGround(vector);
			CreateAbilityInstance(info.caster.position, null, new CastInfo(info.caster, vector), (Ai_Mon_Special_BossErebos_StarRain_Instance b) =>
			{
				b.duration = atkDuration;
			});
			yield return new SI.WaitForSeconds(atkInterval);
		}
		Destroy();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxMeshTrail);
		}
	}

	private IEnumerator StartDisplacementRoutine()
	{
		float num = atkInterval * (float)atkCount;
		Vector3 vector = (info.caster.agentPosition - info.target.GetAIAgentPosition(info.caster)).normalized * backStepDistance;
		Vector3 end = info.caster.agentPosition + vector;
		info.caster.Control.RotateTowards(info.target, immediately: false, num);
		int num2 = 15;
		int num3 = 0;
		Vector3 vector2 = info.caster.agentPosition;
		for (int i = -90; i < 90; i += num2)
		{
			Quaternion quaternion = Quaternion.AngleAxis(i, Vector3.up);
			Vector3 end2 = info.caster.agentPosition + quaternion * vector;
			end2 = Dew.GetValidAgentDestination_LinearSweep(info.caster.agentPosition, end2);
			if (!((info.caster.agentPosition - vector2).sqrMagnitude > (info.caster.agentPosition - end2).sqrMagnitude))
			{
				vector2 = end2;
				num3 = i;
			}
		}
		bool flag = (double)Random.value > 0.5;
		Vector4? vector3;
		if (flag && num3 < 0)
		{
			vector3 = new Vector4(0.85f, 0.15f, 0.85f, 0.15f);
		}
		else if (flag && num3 > 0)
		{
			vector3 = new Vector4(0.15f, 0.85f, 0.15f, 0.85f);
		}
		else
		{
			vector3 = null;
			Vector3 vector4 = Dew.GetValidAgentDestination_LinearSweep(info.caster.agentPosition, end);
			Vector3 validAgentDestination_LinearSweep = Dew.GetValidAgentDestination_LinearSweep(info.caster.agentPosition, info.caster.agentPosition - vector);
			if ((vector4 - info.caster.agentPosition).sqrMagnitude - (validAgentDestination_LinearSweep - info.caster.agentPosition).sqrMagnitude <= -2f)
			{
				vector4 = validAgentDestination_LinearSweep;
			}
			vector2 = vector4;
		}
		if (!vector3.HasValue)
		{
			info.caster.Control.StartDisplacement(new DispByDestination
			{
				affectedByMovementSpeed = false,
				rotateForward = false,
				canGoOverTerrain = true,
				destination = vector2,
				duration = num - 0.1f,
				ease = ease,
				isFriendly = true,
				onCancel = DestroyIfActive
			});
			yield break;
		}
		info.caster.Control.StartDisplacement(new DispByDestination
		{
			affectedByMovementSpeed = false,
			canGoOverTerrain = true,
			rotateForward = false,
			destination = vector2,
			curve = vector3.Value,
			curveHorizontalDistance = curveSize,
			duration = num - 0.1f,
			ease = ease,
			isFriendly = true,
			onCancel = DestroyIfActive,
			onFinish = () =>
			{
				info.caster.Control.RotateTowards(info.target, immediately: true);
			}
		});
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer)
		{
			info.caster.Control.RotateTowards(info.target, immediately: true);
		}
	}

	private void MirrorProcessed()
	{
	}
}
