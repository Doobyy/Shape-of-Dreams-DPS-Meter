using System;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class PickupInstance : AbilityInstance
{
	private static Collider2D[] _colliders;

	[Header("Effects")]
	public GameObject mainEffect;

	public GameObject pickupEffect;

	public GameObject pickupEffectOnHero;

	public float variationStrength = 0.2f;

	[Header("Velocity")]
	public float fixDirectionMaxSpeedRad = 8f;

	public float maxInitialVelocity = 6f;

	public float maxVelocity = 8f;

	public float velocityDeceleration = 12f;

	public float velocityAccelerationMax = 30f;

	public float velocityAccelerationMin = 10f;

	[Header("Behavior")]
	public float pickupDelay = 0.5f;

	public float expirationTime = float.PositiveInfinity;

	public float attractionRange = 6f;

	public float pickupRange = 0.5f;

	public float yPosFromGround = 1f;

	[Header("Timeout-Magnet")]
	public bool enableTimeoutMagnet = true;

	public float timeoutMagnetTime = 4f;

	[SyncVar]
	private Vector2 _initialFlatVelocity;

	private Vector2 _currentFlatVelocity;

	private float _currentYVelocity;

	private float _expirationTimer;

	private float _startTime;

	[SyncVar]
	private int _variationSeed;

	[SyncVar]
	private Hero _target;

	private bool _baseCaptured;

	private float _baseMaxVelocity;

	private float _baseVelocityDeceleration;

	private float _baseVelocityAccelerationMax;

	private float _baseVelocityAccelerationMin;

	private float _baseAttractionRange;

	private float _basePickupRange;

	private float _baseYPosFromGround;

	private float _baseTimeoutMagnetTime;

	private float _basePickupDelay;

	protected NetworkBehaviourSyncVar ____targetNetId;

	public Vector2 Network_initialFlatVelocity
	{
		get
		{
			return _initialFlatVelocity;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<Vector2>(value, ref _initialFlatVelocity, 64uL, (Action<Vector2, Vector2>)null);
		}
	}

	public int Network_variationSeed
	{
		get
		{
			return _variationSeed;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref _variationSeed, 128uL, (Action<int, int>)null);
		}
	}

	public Hero Network_target
	{
		get
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return ((NetworkBehaviour)this).GetSyncVarNetworkBehaviour<Hero>(____targetNetId, ref _target);
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter_NetworkBehaviour<Hero>(value, ref _target, 256uL, (Action<Hero, Hero>)null, ref ____targetNetId);
		}
	}

	protected override void OnPrepare()
	{
		base.OnPrepare();
		Network_initialFlatVelocity = UnityEngine.Random.insideUnitCircle * maxInitialVelocity;
		Network_variationSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (mainEffect != null)
		{
			FxPlay(mainEffect);
		}
		_startTime = Time.time;
		_currentFlatVelocity = _initialFlatVelocity;
		_currentYVelocity = 0f;
		_expirationTimer = 0f;
		Network_target = null;
		if (!_baseCaptured)
		{
			_baseMaxVelocity = maxVelocity;
			_baseVelocityDeceleration = velocityDeceleration;
			_baseVelocityAccelerationMax = velocityAccelerationMax;
			_baseVelocityAccelerationMin = velocityAccelerationMin;
			_baseAttractionRange = attractionRange;
			_basePickupRange = pickupRange;
			_baseYPosFromGround = yPosFromGround;
			_baseTimeoutMagnetTime = timeoutMagnetTime;
			_basePickupDelay = pickupDelay;
			_baseCaptured = true;
		}
		uint s = (uint)(_variationSeed ^ -1640531527);
		if (s == 0)
		{
			s = 1u;
		}
		maxVelocity = _baseMaxVelocity * (1f + (NextVariation(ref s) * 2f - 1f) * variationStrength);
		velocityDeceleration = _baseVelocityDeceleration * (1f + (NextVariation(ref s) * 2f - 1f) * variationStrength);
		velocityAccelerationMax = _baseVelocityAccelerationMax * (1f + (NextVariation(ref s) * 2f - 1f) * variationStrength);
		velocityAccelerationMin = _baseVelocityAccelerationMin * (1f + (NextVariation(ref s) * 2f - 1f) * variationStrength);
		attractionRange = _baseAttractionRange * (1f + (NextVariation(ref s) * 2f - 1f) * variationStrength);
		pickupRange = _basePickupRange * (1f + (NextVariation(ref s) * 2f - 1f) * variationStrength);
		yPosFromGround = _baseYPosFromGround * (1f + (NextVariation(ref s) * 2f - 1f) * variationStrength);
		timeoutMagnetTime = _baseTimeoutMagnetTime * (1f + (NextVariation(ref s) * 2f - 1f) * variationStrength);
		pickupDelay = _basePickupDelay * (1f + (NextVariation(ref s) * 2f - 1f) * variationStrength);
	}

	private static float NextVariation(ref uint s)
	{
		s ^= s << 13;
		s ^= s >> 17;
		s ^= s << 5;
		return (float)(s & 0xFFFFFF) / 16777216f;
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (mainEffect != null)
		{
			FxStop(mainEffect);
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		if ((UnityEngine.Object)(object)Network_target != null)
		{
			float num = Vector2.Distance(Network_target.position.ToXY(), position.ToXY());
			float num2 = Mathf.Lerp(velocityAccelerationMin, velocityAccelerationMax, (num - pickupRange) / (attractionRange - pickupRange));
			Vector2 normalized = (Network_target.position - position).ToXY().normalized;
			Vector2 normalized2 = _currentFlatVelocity.normalized;
			float num3 = Mathf.Clamp(1f - (num - pickupRange) / (attractionRange - pickupRange), 0f, 1f);
			_currentFlatVelocity = Vector3.RotateTowards(normalized2, normalized, Time.deltaTime * fixDirectionMaxSpeedRad * num3, 0f) * _currentFlatVelocity.magnitude;
			_currentFlatVelocity = Vector2.MoveTowards(_currentFlatVelocity, normalized * maxVelocity, num2 * Time.deltaTime);
		}
		else
		{
			_currentFlatVelocity = Vector2.MoveTowards(_currentFlatVelocity, Vector3.zero, velocityDeceleration * Time.deltaTime);
		}
		Vector3 vector = position + _currentFlatVelocity.ToXZ() * Time.deltaTime + _currentYVelocity * Time.deltaTime * Vector3.up;
		RaycastHit val = default;
		if (Physics.Raycast(position + Vector3.up * 3f, Vector3.down, ref val, 6f, LayerMasks.Ground))
		{
			float num4 = val.point.y + yPosFromGround;
			float num5 = float.NegativeInfinity;
			if ((UnityEngine.Object)(object)Network_target != null)
			{
				num5 = Network_target.position.y + yPosFromGround;
			}
			if (vector.y < num4)
			{
				vector.y = num4;
				_currentYVelocity = 0f;
			}
			else if (vector.y < num5)
			{
				vector.y = Mathf.MoveTowards(vector.y, num5, Time.deltaTime * maxVelocity);
				_currentYVelocity = 0f;
			}
			else
			{
				_currentYVelocity += -18.8f * Time.deltaTime;
			}
		}
		position = vector;
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (enableTimeoutMagnet && Time.time - _startTime > timeoutMagnetTime)
		{
			ProcessTimeoutMagnet();
		}
		else
		{
			Hero hero = TryFindTarget();
			if ((UnityEngine.Object)(object)Network_target != null && (UnityEngine.Object)(object)hero == null)
			{
				SyncPosition();
			}
			Network_target = hero;
		}
		if ((UnityEngine.Object)(object)Network_target == null || !CanBeUsedBy(Network_target))
		{
			Network_target = null;
			_expirationTimer += dt;
			if (_expirationTimer > expirationTime)
			{
				Destroy();
			}
		}
		else if (Vector2.Distance(position.ToXY(), Network_target.position.ToXY()) < pickupRange && CanBeUsedBy(Network_target))
		{
			OnPickup(Network_target);
			Destroy();
		}
	}

	private void ProcessTimeoutMagnet()
	{
		if (!((UnityEngine.Object)(object)Network_target == null))
		{
			return;
		}
		Hero hero = null;
		float num = float.PositiveInfinity;
		foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
		{
			if (!((UnityEngine.Object)(object)gamePlayer.hero == null) && CanBeUsedBy(gamePlayer.hero))
			{
				float num2 = Vector2.Distance(position.ToXY(), gamePlayer.hero.position.ToXY());
				if (!(num < num2))
				{
					num = num2;
					hero = gamePlayer.hero;
				}
			}
		}
		if ((UnityEngine.Object)(object)hero != null)
		{
			SyncPosition();
			Network_target = hero;
		}
	}

	private Hero TryFindTarget()
	{
		int num = Physics2D.OverlapCircleNonAlloc(position.ToXY(), attractionRange, _colliders, LayerMasks.Entity);
		Hero result = null;
		float num2 = float.PositiveInfinity;
		for (int i = 0; i < num; i++)
		{
			if (DewPhysics.TryGetEntity(_colliders[i], out var entity) && entity.isActive && entity is Hero hero && CanBeUsedBy(hero))
			{
				float num3 = Vector2.Distance(position.ToXY(), hero.position.ToXY());
				if (!(num3 > attractionRange) && !(num3 > num2))
				{
					num2 = num3;
					result = hero;
				}
			}
		}
		return result;
	}

	protected virtual bool CanBeUsedBy(Hero hero)
	{
		if (Time.time - creationTime > pickupDelay)
		{
			return !hero.IsNullInactiveDeadOrKnockedOut();
		}
		return false;
	}

	protected virtual void OnPickup(Hero hero)
	{
		if (pickupEffect != null)
		{
			FxPlayNetworked(pickupEffect);
		}
		if (pickupEffectOnHero != null)
		{
			FxPlayNetworked(pickupEffectOnHero, hero);
		}
	}

	[Server]
	private void SyncPosition()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PickupInstance::SyncPosition()' called when server was not active");
		}
		else
		{
			RpcSyncPosition(position, _currentFlatVelocity);
		}
	}

	[ClientRpc]
	private void RpcSyncPosition(Vector3 pos, Vector2 vel)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteVector3((NetworkWriter)(object)val, pos);
		NetworkWriterExtensions.WriteVector2((NetworkWriter)(object)val, vel);
		((NetworkBehaviour)this).SendRPCInternal("System.Void PickupInstance::RpcSyncPosition(UnityEngine.Vector3,UnityEngine.Vector2)", 1612814072, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	static PickupInstance()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected Obj, but got Unknown
		_colliders = new Collider2D[10];
		RemoteProcedureCalls.RegisterRpc(typeof(PickupInstance), "System.Void PickupInstance::RpcSyncPosition(UnityEngine.Vector3,UnityEngine.Vector2)", (RemoteCallDelegate)InvokeUserCode_RpcSyncPosition__Vector3__Vector2);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcSyncPosition__Vector3__Vector2(Vector3 pos, Vector2 vel)
	{
		position = pos;
		_currentFlatVelocity = vel;
	}

	protected static void InvokeUserCode_RpcSyncPosition__Vector3__Vector2(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSyncPosition called on server.");
		}
		else
		{
			((PickupInstance)(object)obj).UserCode_RpcSyncPosition__Vector3__Vector2(NetworkReaderExtensions.ReadVector3(reader), NetworkReaderExtensions.ReadVector2(reader));
		}
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteVector2(writer, _initialFlatVelocity);
			NetworkWriterExtensions.WriteInt(writer, _variationSeed);
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_target);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteVector2(writer, _initialFlatVelocity);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, _variationSeed);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100L) != 0L)
		{
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_target);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Vector2>(ref _initialFlatVelocity, (Action<Vector2, Vector2>)null, NetworkReaderExtensions.ReadVector2(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref _variationSeed, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<Hero>(ref _target, (Action<Hero, Hero>)null, reader, ref ____targetNetId);
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Vector2>(ref _initialFlatVelocity, (Action<Vector2, Vector2>)null, NetworkReaderExtensions.ReadVector2(reader));
		}
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref _variationSeed, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x100L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<Hero>(ref _target, (Action<Hero, Hero>)null, reader, ref ____targetNetId);
		}
	}
}
