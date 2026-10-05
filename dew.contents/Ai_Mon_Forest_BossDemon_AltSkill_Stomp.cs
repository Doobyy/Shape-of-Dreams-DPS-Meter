using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Mon_Forest_BossDemon_AltSkill_Stomp : AbilityInstance
{
	public float rotDuration;

	public float rotSpeed;

	public float maxRotSpeedMult;

	public float minRotSpeedMult;

	public float rotDistanceThreshold;

	public float stompAfterDelay;

	public float stompCount;

	public int treeCount;

	public float treeInterval;

	public float treeRadius;

	public float startWidth;

	public float endWidth;

	public float bendPower;

	public float maxBendAngle;

	public float distance;

	public float spreadCurve;

	public ScalingValue dmgFactor;

	public float radius;

	public GameObject fxStompReady;

	public GameObject fxStomp;

	public GameObject fxTarget;

	public GameObject fxHit;

	public DewAnimationClip readyAnim;

	public DewAnimationClip stompAnim;

	[SyncVar]
	private float _desiredAngle;

	private float _cv;

	private float _predictionValue;

	private bool _enableRotation;

	private Vector3 _targetPosition;

	private List<Vector3> _spawnedPositions = new List<Vector3>();

	private Entity _targetData;

	private Entity _target
	{
		get
		{
			return _targetData;
		}
		set
		{
			if ((UnityEngine.Object)(object)_targetData != (UnityEngine.Object)(object)value)
			{
				_targetData = value;
				OnTargetChanged(value);
			}
		}
	}

	public float Network_desiredAngle
	{
		get
		{
			return _desiredAngle;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _desiredAngle, 64uL, (Action<float, float>)null);
		}
	}

	private void OnTargetChanged(Entity newTarget)
	{
		if (((NetworkBehaviour)this).isServer && !newTarget.IsNullInactiveDeadOrKnockedOut())
		{
			FxStopNetworked(fxTarget);
			FxPlayNetworked(fxTarget, newTarget);
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
		info.caster.Control.IncrementBlockCounters(Channel.BlockedAction.Everything);
		BossMonster.RevealStealthedBeforeSpecialAttack();
		for (int i = 0; (float)i < stompCount; i++)
		{
			_predictionValue = UnityEngine.Random.Range(0.5f, 1f);
			_target = Dew.SelectBestWithScore((IList<DewPlayer>)DewPlayer.gamePlayers, (Func<DewPlayer, int, float>)((DewPlayer p, int _) =>
			{
				if (p.hero.IsNullInactiveDeadOrKnockedOut())
				{
					return -1000f;
				}
				return p.hero.Status.isUndetectableByNonAllies ? (-100f) : Vector3.Distance(info.caster.agentPosition, p.hero.GetAIAgentPosition(info.caster));
			}), 0.2f, (DewRandom)null).hero;
			if (!_target.IsNullInactiveDeadOrKnockedOut())
			{
				Network_desiredAngle = CastInfo.GetAngle(_target.GetAIAgentPosition(info.caster) - info.caster.agentPosition);
			}
			else
			{
				Network_desiredAngle = info.caster.rotation.eulerAngles.y;
			}
			FxPlayNetworked(fxStompReady, info.caster);
			info.caster.Animation.PlayAbilityAnimation(readyAnim);
			_enableRotation = true;
			yield return new SI.WaitForSeconds(rotDuration);
			info.caster.Animation.PlayAbilityAnimation(stompAnim);
			FxStopNetworked(fxStompReady);
			FxPlayNewNetworked(fxStomp, info.caster);
			List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, info.caster.agentPosition, radius, tvDefaultHarmfulEffectTargets);
			for (int num = 0; num < list.Count; num++)
			{
				Entity entity = list[num];
				CreateDamage(DamageData.SourceType.Default, dmgFactor).SetOriginPosition(info.caster.agentPosition).Dispatch(entity);
				entity.Visual.KnockUp(KnockUpStrength.Big, isFriendly: false);
				FxPlayNewNetworked(fxHit, entity);
			}
			handle.Return();
			_enableRotation = false;
			if (!_target.IsNullInactiveDeadOrKnockedOut())
			{
				_targetPosition = _target.GetAIAgentPosition(info.caster);
			}
			else
			{
				_targetPosition = ((Component)(object)info.caster).transform.forward * 20f;
			}
			_targetPosition = Dew.GetPositionOnGround(_targetPosition);
			for (int j = 0; j < treeCount; j++)
			{
				Vector3? spawnPoint = GetSpawnPoint(j);
				if (spawnPoint.HasValue)
				{
					Vector3 value = spawnPoint.Value;
					CreateAbilityInstance<Ai_Mon_Forest_BossDemon_AltSkill_Stomp_Tree>(value, null, new CastInfo(info.caster, value));
					yield return new SI.WaitForSeconds(treeInterval);
				}
			}
			yield return new SI.WaitForSeconds(stompAfterDelay);
			_spawnedPositions.Clear();
		}
		Destroy();
	}

	private Vector3? GetSpawnPoint(int index)
	{
		float num = (float)index / (float)Mathf.Max(1, treeCount - 1);
		num = Mathf.Clamp01(num + UnityEngine.Random.Range(-0.05f, 0.05f));
		Vector3 agentPosition = info.caster.agentPosition;
		Vector3 normalized = (_targetPosition - agentPosition).normalized;
		Vector3 vector = agentPosition + normalized * distance;
		Vector3 vector2 = (agentPosition + vector) * 0.5f;
		float num2 = Mathf.Clamp(maxBendAngle, 0f, 89f);
		float num3 = distance * 0.5f * Mathf.Tan(num2 * ((float)Math.PI / 180f));
		float num4 = Mathf.Clamp(bendPower, 0f - num3, num3);
		Vector3 normalized2 = Vector3.Cross(Vector3.up, normalized).normalized;
		Vector3 p = vector2 + normalized2 * num4;
		int num5 = 30;
		for (int i = 0; i < num5; i++)
		{
			Vector3 vector3 = CalculateQuadraticBezierPoint(num, agentPosition, p, vector);
			Vector3 normalized3 = Vector3.Cross(rhs: CalculateQuadraticBezierTangent(num, agentPosition, p, vector).normalized, lhs: Vector3.up).normalized;
			float num6 = Mathf.Lerp(startWidth, endWidth, Mathf.Pow(num, spreadCurve));
			float num7 = UnityEngine.Random.Range((0f - num6) * 0.5f, num6 * 0.5f);
			Vector3 vector4 = vector3 + normalized3 * num7;
			vector4 = Dew.GetPositionOnGround(vector4);
			if (!IsOverlapping(vector4))
			{
				_spawnedPositions.Add(vector4);
				return vector4;
			}
		}
		return null;
	}

	private bool IsOverlapping(Vector3 position)
	{
		float num = treeRadius * 2f;
		foreach (Vector3 spawnedPosition in _spawnedPositions)
		{
			if ((spawnedPosition - position).sqrMagnitude < num * num)
			{
				return true;
			}
		}
		return false;
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		float y = Mathf.SmoothDampAngle(rotation.eulerAngles.y, _desiredAngle, ref _cv, 0.1f);
		rotation = Quaternion.Euler(0f, y, 0f);
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && _enableRotation && !_target.IsNullInactiveDeadOrKnockedOut())
		{
			_targetPosition = AbilityTrigger.PredictPoint_Simple(info.caster, _predictionValue, _target, 0.5f);
			float target = AbilityTrigger.PredictAngle_Simple(info.caster, _predictionValue, _target, info.caster.agentPosition, 0f);
			float num = Vector3.Distance(_target.GetAIAgentPosition(info.caster), info.caster.agentPosition);
			float num2 = rotSpeed * Mathf.Lerp(maxRotSpeedMult, minRotSpeedMult, num / rotDistanceThreshold);
			Network_desiredAngle = Mathf.MoveTowardsAngle(_desiredAngle, target, num2 * dt);
			info.caster.Control.Rotate(rotation, immediately: false);
		}
	}

	protected override void OnDestroyActor()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxStompReady);
			FxStopNetworked(fxTarget);
			if (!info.caster.IsNullInactiveDeadOrKnockedOut())
			{
				info.caster.Control.DecrementBlockCounters(Channel.BlockedAction.Everything);
			}
		}
	}

	private Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
	{
		float num = 1f - t;
		float num2 = t * t;
		return num * num * p0 + 2f * num * t * p1 + num2 * p2;
	}

	private Vector3 CalculateQuadraticBezierTangent(float t, Vector3 p0, Vector3 p1, Vector3 p2)
	{
		float num = 1f - t;
		return 2f * num * (p1 - p0) + 2f * t * (p2 - p1);
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteFloat(writer, _desiredAngle);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _desiredAngle);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _desiredAngle, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _desiredAngle, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
