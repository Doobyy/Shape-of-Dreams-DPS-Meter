using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Hero_Bismuth_BookController : DewNetworkBehaviour
{
	private static readonly int MotionTime;

	[NonSerialized]
	public Transform bookTransform;

	[NonSerialized]
	public Animator bookAnimator;

	public AnimationCurve openAmountCurve;

	public AnimationCurve castRecoilCurve;

	[CompilerGenerated]
	[SyncVar]
	private Vector3? targetPos__BackingField;

	private Vector3 _currentPosition;

	private Vector3 _lastShakeRandom;

	private float _lastShakeRandomTime;

	private float _lastCastTime;

	private Quaternion _lastCastRot;

	private Vector3 _cv;

	private Quaternion _rotCv;

	private Hero_Bismuth _hero;

	private Rigidbody _rb;

	private const float TargetUpdateInterval = 0.1f;

	private float _nextTargetUpdateTime;

	public Vector3? targetPos
	{
		[CompilerGenerated]
		get
		{
			return targetPos__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CtargetPos_003Ek__BackingField = value;
		}
	}

	public Vector3? targetPosClamped
	{
		get
		{
			if (!targetPos.HasValue)
			{
				return null;
			}
			return _hero.position + Vector3.ClampMagnitude(targetPos.Value - _hero.position, 2f);
		}
	}

	public Vector3? Network_003CtargetPos_003Ek__BackingField
	{
		get
		{
			return targetPos__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<Vector3?>(value, ref targetPos__BackingField, 1uL, (Action<Vector3?, Vector3?>)null);
		}
	}

	private void Start()
	{
		_hero = ((Component)(object)this).GetComponent<Hero_Bismuth>();
		_hero.Visual.ClientEvent_OnModelLoaded += new Action(ClientEventOnModelLoaded);
		ClientEventOnModelLoaded();
	}

	private void ClientEventOnModelLoaded()
	{
		if (bookTransform != null)
		{
			UnityEngine.Object.Destroy(bookTransform.gameObject);
		}
		if ((bool)_hero.Visual.model)
		{
			bookTransform = _hero.Visual.model.GetCustomMapping<Transform>("book");
			bookAnimator = _hero.Visual.model.GetCustomMapping<Animator>("book");
			_rb = _hero.Visual.model.GetCustomMapping<Rigidbody>("book");
			Dew.CallDelayed(() =>
			{
				bookTransform.parent = null;
			}, 2);
		}
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (bookTransform == null || !((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (_hero.IsNullInactiveDeadOrKnockedOut())
		{
			if (targetPos.HasValue)
			{
				Network_003CtargetPos_003Ek__BackingField = null;
			}
			if (_rb.isKinematic)
			{
				_rb.isKinematic = false;
				_rb.rotation = Quaternion.Euler(0f, 0f, 70f) * _rb.rotation;
				_rb.linearVelocity = UnityEngine.Random.onUnitSphere * 8f;
				bookAnimator.SetFloat(MotionTime, 0f);
				PhysX3DSimulationDriver.Register(this);
			}
			return;
		}
		if (!_rb.isKinematic)
		{
			PhysX3DSimulationDriver.Unregister(this);
		}
		_rb.isKinematic = true;
		if ((UnityEngine.Object)(object)_hero.Ability.attackAbility == null || Time.time < _nextTargetUpdateTime)
		{
			return;
		}
		_nextTargetUpdateTime = Time.time + 0.1f;
		List<Entity> targetEntities = Hero_Bismuth.GetTargetEntities(out var handle, _hero, canBeNeutral: false, 10f);
		if (targetEntities.Count > 0)
		{
			Vector3 agentPosition = targetEntities[0].agentPosition;
			if (!targetPos.HasValue || targetPos.Value != agentPosition)
			{
				Network_003CtargetPos_003Ek__BackingField = agentPosition;
			}
		}
		else if (targetPos.HasValue)
		{
			Network_003CtargetPos_003Ek__BackingField = null;
		}
		handle.Return();
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (!((UnityEngine.Object)(object)bookAnimator == null) && _rb.isKinematic)
		{
			float num = 1.6f;
			if (Time.time - _lastShakeRandomTime > 0.05f)
			{
				_lastShakeRandom = UnityEngine.Random.insideUnitSphere;
				_lastShakeRandomTime = Time.time;
			}
			Vector3 vector;
			float smoothTime;
			if (targetPosClamped.HasValue)
			{
				vector = targetPosClamped.Value;
				smoothTime = 0.125f;
			}
			else if (_hero.Control.isWalking)
			{
				vector = _hero.Visual.GetCenterPosition().WithY(_hero.position.y) + ((Component)(object)this).transform.forward * (0f - num);
				smoothTime = 0.2f;
			}
			else
			{
				vector = _hero.Visual.GetCenterPosition().WithY(_hero.position.y) + ((Component)(object)this).transform.forward * (0f - num);
				smoothTime = 0.2f;
			}
			vector += Vector3.up * (Mathf.Sin((Time.time - _hero.creationTime) * 2f) * 0.25f + 0.25f);
			Vector3 b = vector;
			Vector3 vector2 = Vector3.Lerp(vector, b, 1f - (Time.time - _lastCastTime));
			if (Vector3.Distance(_currentPosition, vector2) > 10f)
			{
				_currentPosition = vector2;
				_cv = default;
			}
			else
			{
				_currentPosition = Vector3.SmoothDamp(_currentPosition, vector2, ref _cv, smoothTime);
			}
			Vector3 vector3 = _lastCastRot * Vector3.back * castRecoilCurve.Evaluate(Time.time - _lastCastTime);
			vector3 += _lastShakeRandom * (castRecoilCurve.Evaluate(Time.time - _lastCastTime) * 0.55f);
			vector3 += _hero.rotation * _hero.Visual.etLocalOffset + _hero.Visual.etWorldOffset;
			bookTransform.position = _currentPosition + vector3;
			Quaternion a = Quaternion.Slerp(_lastCastRot, _lastCastRot.Flattened(), (Time.time - _lastCastTime) * 1.4f);
			Quaternion b2;
			if (targetPos.HasValue)
			{
				b2 = Quaternion.LookRotation(targetPos.Value - bookTransform.position).Flattened();
			}
			else
			{
				float num2 = Vector3.SignedAngle(bookTransform.position - _hero.position, bookTransform.position + _cv * 0.2f - _hero.position, Vector3.up);
				b2 = Quaternion.Euler(Mathf.Sin(Time.time * 0.784f) * 10f, (Mathf.Sin(Time.time) * 15f + Mathf.Sin(Time.time * 0.32f) * 12f) * (0.5f + _cv.magnitude * 0.5f) - num2, Mathf.Sin(Time.time * 0.921f) * 10f) * bookTransform.rotation.Flattened();
			}
			Quaternion target = Quaternion.Slerp(a, b2, (Time.time - _lastCastTime) * 2f);
			float time = Mathf.Lerp(0.05f, 0.2f, (Time.time - _lastCastTime) * 1.4f);
			bookTransform.rotation = QuaternionUtil.SmoothDamp(bookTransform.rotation, target, ref _rotCv, time);
			bookAnimator.SetFloat(MotionTime, openAmountCurve.Evaluate(Time.time - _lastCastTime));
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		PhysX3DSimulationDriver.Unregister(this);
		if (bookTransform != null)
		{
			UnityEngine.Object.Destroy(bookTransform.gameObject);
		}
	}

	[ClientRpc]
	public void RpcBookCast(Quaternion rot)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteQuaternion((NetworkWriter)(object)val, rot);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Hero_Bismuth_BookController::RpcBookCast(UnityEngine.Quaternion)", -150928420, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	public void RpcBookCast(Vector3 pos)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteVector3((NetworkWriter)(object)val, pos);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Hero_Bismuth_BookController::RpcBookCast(UnityEngine.Vector3)", -28427928, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	static Hero_Bismuth_BookController()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected Obj, but got Unknown
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected Obj, but got Unknown
		MotionTime = Animator.StringToHash("Motion Time");
		RemoteProcedureCalls.RegisterRpc(typeof(Hero_Bismuth_BookController), "System.Void Hero_Bismuth_BookController::RpcBookCast(UnityEngine.Quaternion)", (RemoteCallDelegate)InvokeUserCode_RpcBookCast__Quaternion);
		RemoteProcedureCalls.RegisterRpc(typeof(Hero_Bismuth_BookController), "System.Void Hero_Bismuth_BookController::RpcBookCast(UnityEngine.Vector3)", (RemoteCallDelegate)InvokeUserCode_RpcBookCast__Vector3);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcBookCast__Quaternion(Quaternion rot)
	{
		_lastCastRot = rot;
		_lastCastTime = Time.time;
	}

	protected static void InvokeUserCode_RpcBookCast__Quaternion(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcBookCast called on server.");
		}
		else
		{
			((Hero_Bismuth_BookController)(object)obj).UserCode_RpcBookCast__Quaternion(NetworkReaderExtensions.ReadQuaternion(reader));
		}
	}

	protected void UserCode_RpcBookCast__Vector3(Vector3 pos)
	{
		if (!(bookTransform == null))
		{
			_lastCastRot = Quaternion.LookRotation(pos - bookTransform.position).Flattened();
			_lastCastTime = Time.time;
		}
	}

	protected static void InvokeUserCode_RpcBookCast__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcBookCast called on server.");
		}
		else
		{
			((Hero_Bismuth_BookController)(object)obj).UserCode_RpcBookCast__Vector3(NetworkReaderExtensions.ReadVector3(reader));
		}
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		((NetworkBehaviour)this).SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteVector3Nullable(writer, targetPos__BackingField);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 1L) != 0L)
		{
			NetworkWriterExtensions.WriteVector3Nullable(writer, targetPos__BackingField);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		((NetworkBehaviour)this).DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Vector3?>(ref targetPos__BackingField, (Action<Vector3?, Vector3?>)null, NetworkReaderExtensions.ReadVector3Nullable(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 1L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Vector3?>(ref targetPos__BackingField, (Action<Vector3?, Vector3?>)null, NetworkReaderExtensions.ReadVector3Nullable(reader));
		}
	}
}
