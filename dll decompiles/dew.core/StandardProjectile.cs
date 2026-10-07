using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public abstract class StandardProjectile : Projectile
{
	public bool use3DVelocity;

	[SyncVar]
	public float _initialSpeed = 5f;

	[SyncVar]
	public float _targetSpeed = 5f;

	[SyncVar]
	public float _acceleration;

	public bool enableRandomCurve;

	public float randomCurveMagnitude = 3f;

	public bool ensurePositiveY = true;

	private float _ipNormalizedPositionTarget;

	private float _ipNormalizedPositionRemainingTime;

	private bool _isRefPosSet;

	private Vector3 _referencePosition;

	private float _referenceFlatVelocity;

	private Vector3 _referenceVelocity;

	private Vector3 _randomCurveVec;

	public float initialSpeed
	{
		get
		{
			return _initialSpeed;
		}
		set
		{
			Network_initialSpeed = value;
		}
	}

	public float targetSpeed
	{
		get
		{
			return _targetSpeed;
		}
		set
		{
			Network_targetSpeed = value;
		}
	}

	public float acceleration
	{
		get
		{
			return _acceleration;
		}
		set
		{
			Network_acceleration = value;
		}
	}

	public float normalizedPosition { get; private set; }

	public float Network_initialSpeed
	{
		get
		{
			return _initialSpeed;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _initialSpeed, 65536uL, (Action<float, float>)null);
		}
	}

	public float Network_targetSpeed
	{
		get
		{
			return _targetSpeed;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _targetSpeed, 131072uL, (Action<float, float>)null);
		}
	}

	public float Network_acceleration
	{
		get
		{
			return _acceleration;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _acceleration, 262144uL, (Action<float, float>)null);
		}
	}

	protected override void OnCreate()
	{
		if (enableRandomCurve)
		{
			_randomCurveVec = UnityEngine.Random.insideUnitSphere * randomCurveMagnitude;
			if (ensurePositiveY && _randomCurveVec.y < 0f)
			{
				_randomCurveVec.y *= -1f;
			}
		}
		if (use3DVelocity)
		{
			_referenceVelocity = (GetTargetWorldPosition() - _referencePosition).normalized * _initialSpeed;
		}
		else
		{
			_referenceFlatVelocity = _initialSpeed;
		}
		base.OnCreate();
	}

	protected override bool CheckForCompletion()
	{
		return normalizedPosition > 0.99999f;
	}

	protected override Vector3 PositionSolver(float dt)
	{
		Vector3 targetWorldPosition = GetTargetWorldPosition();
		Transform transform = ((Component)(object)this).transform;
		Vector3 vector = transform.position;
		if (!_isRefPosSet)
		{
			_isRefPosSet = true;
			_referencePosition = vector;
		}
		Vector3 result;
		if (use3DVelocity)
		{
			float num = Vector3.Distance(_referencePosition, targetWorldPosition);
			if (num < _referenceVelocity.magnitude * dt && Vector3.Angle(_referenceVelocity, targetWorldPosition - _referencePosition) < 60f)
			{
				_referencePosition = targetWorldPosition;
			}
			else
			{
				_referencePosition += _referenceVelocity * dt;
			}
			float afterDistance = Vector3.Distance(_referencePosition, targetWorldPosition);
			_referenceVelocity = Vector3.MoveTowards(_referenceVelocity, (targetWorldPosition - _referencePosition).normalized * _targetSpeed, _acceleration * dt);
			UpdateNormalizedTrajectory(num, afterDistance, dt);
			result = (_hasTrajectoryOffset ? (_referencePosition + CalculateOffsetOverTrajectoryInWorldSpace(normalizedPosition, Quaternion.LookRotation(_referenceVelocity))) : _referencePosition);
		}
		else
		{
			float num = Vector3.Distance(_referencePosition, targetWorldPosition);
			_referencePosition = Vector3.MoveTowards(_referencePosition, targetWorldPosition, _referenceFlatVelocity * dt);
			float afterDistance = Vector3.Distance(_referencePosition, targetWorldPosition);
			_referenceFlatVelocity = Mathf.MoveTowards(_referenceFlatVelocity, _targetSpeed, _acceleration * dt);
			UpdateNormalizedTrajectory(num, afterDistance, dt);
			if (_hasTrajectoryOffset)
			{
				Vector3 vector2 = targetWorldPosition - vector;
				result = ((Vector3.SqrMagnitude(vector2) < 1E-05f) ? (_referencePosition + CalculateOffsetOverTrajectoryInWorldSpace(normalizedPosition, transform.rotation)) : (_referencePosition + CalculateOffsetOverTrajectoryInWorldSpace(normalizedPosition, Quaternion.LookRotation(vector2))));
			}
			else
			{
				result = _referencePosition;
			}
		}
		if (!Adaptive_ShouldPlayEffects())
		{
			_referencePosition = targetWorldPosition;
			result = targetWorldPosition;
			normalizedPosition = 1f;
		}
		if (enableRandomCurve)
		{
			result += Mathf.Sin(normalizedPosition * (float)Math.PI) * _randomCurveVec;
		}
		return result;
	}

	private void UpdateNormalizedTrajectory(float beforeDistance, float afterDistance, float dt)
	{
		if (beforeDistance != 0f)
		{
			float num = normalizedPosition + (1f - normalizedPosition) * (beforeDistance - afterDistance) / beforeDistance;
			if (!(num < normalizedPosition))
			{
				num = Mathf.Clamp(num, 0f, 1f);
				_ipNormalizedPositionTarget = num;
				_ipNormalizedPositionRemainingTime = dt;
			}
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		if (_ipNormalizedPositionRemainingTime != 0f)
		{
			float num = Time.deltaTime / _ipNormalizedPositionRemainingTime;
			_ipNormalizedPositionRemainingTime -= Time.deltaTime;
			if (num > 1f)
			{
				_ipNormalizedPositionRemainingTime = 0f;
				num = 1f;
			}
			normalizedPosition = Mathf.Lerp(normalizedPosition, _ipNormalizedPositionTarget, num);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		normalizedPosition = 0f;
		_isRefPosSet = false;
		_ipNormalizedPositionTarget = 0f;
		_ipNormalizedPositionRemainingTime = 0f;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteFloat(writer, _initialSpeed);
			NetworkWriterExtensions.WriteFloat(writer, _targetSpeed);
			NetworkWriterExtensions.WriteFloat(writer, _acceleration);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x10000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _initialSpeed);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x20000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _targetSpeed);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _acceleration);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _initialSpeed, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _targetSpeed, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _acceleration, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x10000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _initialSpeed, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x20000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _targetSpeed, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x40000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _acceleration, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
