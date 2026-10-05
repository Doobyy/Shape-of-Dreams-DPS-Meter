using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(EntityTransformSync))]
public class EntityControl : EntityComponent, ICleanup
{
	internal struct OngoingDisplacementInternalData
	{
		public DewEase knownEase;

		public EaseFunction easeFunction;

		public Vector3 start;

		public Vector3 last;
	}

	private struct PositionSyncData : IEquatable<PositionSyncData>
	{
		public double timestamp;

		public Vector3 position;

		public Vector3 velocity;

		public float desiredAngle;

		public bool Equals(PositionSyncData o)
		{
			if (timestamp == o.timestamp && position == o.position && velocity == o.velocity)
			{
				return desiredAngle == o.desiredAngle;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is PositionSyncData o)
			{
				return Equals(o);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (((((timestamp.GetHashCode() * 397) ^ position.GetHashCode()) * 397) ^ velocity.GetHashCode()) * 397) ^ desiredAngle.GetHashCode();
		}
	}

	public enum BlockableAction
	{
		Move,
		Ability,
		Attack,
		Dodge
	}

	public enum BlockStatus
	{
		Allowed,
		Blocked,
		BlockedCancelable
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CDecrementBlockCounters_003Ed__161 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public bool delayOneFrame;

		public Channel.BlockedAction action;

		public EntityControl _003C_003E4__this;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_011a: Unknown result type (might be due to invalid IL or missing references)
			//IL_011f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_0101: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			EntityControl entityControl = _003C_003E4__this;
			checked
			{
				try
				{
					Awaiter val2;
					if (num != 0)
					{
						if (!delayOneFrame)
						{
							goto IL_013c;
						}
						if ((action & Channel.BlockedAction.Cancelable) != 0)
						{
							if ((action & Channel.BlockedAction.Move) != 0)
							{
								entityControl.Network_blockMoveCancelable = (byte)(unchecked((uint)entityControl._blockMoveCancelable) - 1u);
								entityControl.Network_blockMove = (byte)(unchecked((uint)entityControl._blockMove) + 1u);
							}
							if ((action & Channel.BlockedAction.Ability) != 0)
							{
								entityControl.Network_blockAbilityCancelable = (byte)(unchecked((uint)entityControl._blockAbilityCancelable) - 1u);
								entityControl.Network_blockAbility = (byte)(unchecked((uint)entityControl._blockAbility) + 1u);
							}
							if ((action & Channel.BlockedAction.Attack) != 0)
							{
								entityControl.Network_blockAttackCancelable = (byte)(unchecked((uint)entityControl._blockAttackCancelable) - 1u);
								entityControl.Network_blockAttack = (byte)(unchecked((uint)entityControl._blockAttack) + 1u);
							}
							if ((action & Channel.BlockedAction.Dodge) != 0)
							{
								entityControl.Network_blockDodgeCancelable = (byte)(unchecked((uint)entityControl._blockDodgeCancelable) - 1u);
								entityControl.Network_blockDodge = (byte)(unchecked((uint)entityControl._blockDodge) + 1u);
							}
							action &= ~Channel.BlockedAction.Cancelable;
						}
						UniTask val = UniTask.NextFrame();
						val2 = val.GetAwaiter();
						if (!val2.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val2;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CDecrementBlockCounters_003Ed__161>(ref val2, ref this);
							return;
						}
					}
					else
					{
						val2 = _003C_003Eu__1;
						_003C_003Eu__1 = default;
						num = (_003C_003E1__state = -1);
					}
					val2.GetResult();
					goto IL_013c;
					IL_013c:
					if ((action & Channel.BlockedAction.Cancelable) != 0)
					{
						if ((action & Channel.BlockedAction.Move) != 0)
						{
							entityControl.Network_blockMoveCancelable = (byte)(unchecked((uint)entityControl._blockMoveCancelable) - 1u);
						}
						if ((action & Channel.BlockedAction.Ability) != 0)
						{
							entityControl.Network_blockAbilityCancelable = (byte)(unchecked((uint)entityControl._blockAbilityCancelable) - 1u);
						}
						if ((action & Channel.BlockedAction.Attack) != 0)
						{
							entityControl.Network_blockAttackCancelable = (byte)(unchecked((uint)entityControl._blockAttackCancelable) - 1u);
						}
						if ((action & Channel.BlockedAction.Dodge) != 0)
						{
							entityControl.Network_blockDodgeCancelable = (byte)(unchecked((uint)entityControl._blockDodgeCancelable) - 1u);
						}
					}
					else
					{
						if ((action & Channel.BlockedAction.Move) != 0)
						{
							entityControl.Network_blockMove = (byte)(unchecked((uint)entityControl._blockMove) - 1u);
						}
						if ((action & Channel.BlockedAction.Ability) != 0)
						{
							entityControl.Network_blockAbility = (byte)(unchecked((uint)entityControl._blockAbility) - 1u);
						}
						if ((action & Channel.BlockedAction.Attack) != 0)
						{
							entityControl.Network_blockAttack = (byte)(unchecked((uint)entityControl._blockAttack) - 1u);
						}
						if ((action & Channel.BlockedAction.Dodge) != 0)
						{
							entityControl.Network_blockDodge = (byte)(unchecked((uint)entityControl._blockDodge) - 1u);
						}
					}
				}
				catch (Exception exception)
				{
					_003C_003E1__state = -2;
					_003C_003Et__builder.SetException(exception);
					return;
				}
				_003C_003E1__state = -2;
				_003C_003Et__builder.SetResult();
			}
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			_003C_003Et__builder.SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	public SafeAction<float, float> ClientEvent_OnOuterRadiusChanged;

	public SafeAction<float, float> ClientEvent_OnInnerRadiusChanged;

	public SafeAction<Vector3, Vector3> ClientEvent_OnTeleport;

	public List<Channel> ongoingChannels = new List<Channel>();

	[SyncVar(hook = "OuterRadiusChanged")]
	[SerializeField]
	private float _outerRadius = 0.45f;

	[SyncVar(hook = "InnerRadiusChanged")]
	[SerializeField]
	private float _innerRadius = 0.25f;

	[SyncVar]
	[SerializeField]
	private bool _freeMovement;

	public bool obstacleAvoidance = true;

	public float baseAgentSpeed = 5f;

	public float rotationSmoothTime = 0.05f;

	public float normalizedAcceleration = 4f;

	public float rotateSpeed = 720f;

	internal NavMeshAgent _agent;

	[CompilerGenerated]
	[SyncVar]
	private float? overridenDesiredAngle__BackingField;

	private float _overrideAngleLifeTime;

	private float _overrideAngle;

	private Entity _overrideAngleEntity;

	private Vector3? _overrideAnglePosition;

	[CompilerGenerated]
	[SyncVar]
	private bool isControlReversed__BackingField;

	private static readonly List<Transform> s_LayerStack;

	[NonSerialized]
	private bool _didPreloadStart;

	[NonSerialized]
	private Action<EventInfoKill> _cachedEntityEventOnDeath;

	private const float NonHeroActionTickInterval = 1f / 30f;

	private float _nextActionTickTime;

	private Vector3 _lastMoveCheckPosition;

	[SyncVar]
	private int _gamepadRotationLockCounter;

	public const float QueuedActionExpireTime = 0.2f;

	private List<ActionBase> _queuedActions = new List<ActionBase>();

	[NonSerialized]
	public float lastMoveTime;

	private ActionBase _moveToActionOwner;

	[SyncVar]
	private Vector3 _moveToActionDestination;

	[SyncVar]
	private float _moveToActionRequiredDistance;

	[NonSerialized]
	public Entity attackTarget;

	private bool _isMoveToAttackActive;

	private float _lastAttackMoveCheckTime;

	private const float OutOfRangeRetargetInterval = 0.2f;

	private float _lastOutOfRangeRetargetTime;

	private float _lastAttackStartTime;

	private float _lastAttackMovSpdMultiplier;

	private float _lastAttackMovSpdDuration;

	[SyncVar]
	private byte _blockMove;

	[SyncVar]
	private byte _blockAbility;

	[SyncVar]
	private byte _blockAttack;

	[SyncVar]
	private byte _blockDodge;

	[SyncVar]
	private byte _blockMoveCancelable;

	[SyncVar]
	private byte _blockAbilityCancelable;

	[SyncVar]
	private byte _blockAttackCancelable;

	[SyncVar]
	private byte _blockDodgeCancelable;

	private const float ChannelTimeEpsilon = 0.0001f;

	internal const float DispByTargetGoalDistance = 0.05f;

	private const int MaxCommandsPerTick = 100;

	private const float SetDestinationMinInterval = 0.1f;

	private const float SetDestinationMinSqrMagnitude = 0.2f;

	private const float CommandBufferLifetime = 0.15f;

	public const float CommandStallTimeout = 0.15f;

	public SafeAction<Displacement> ClientEvent_OnDisplacementStarted;

	public SafeAction<Displacement> ClientEvent_OnDisplacementFinished;

	public SafeAction<Displacement> ClientEvent_OnDisplacementCanceled;

	private OngoingDisplacementInternalData _dispData;

	private float _lastStardustInteractTime;

	private const float AgentGoalSqrDistance = 0.1f;

	private const float SetAgentDestinationInterval = 0.25f;

	private const float GroundSnapReuseThresholdSqr = 0.0144f;

	private Vector2 _lastGroundSnapXZ;

	private float _lastGroundSnapY;

	private bool _hasGroundSnap;

	private Vector2 _lastNavSnapXZ;

	private float _lastNavSnapY;

	private bool _hasNavSnap;

	[CompilerGenerated]
	[SyncVar]
	private bool forceWalking__BackingField;

	private float _localWalkStrength;

	[SyncVar]
	private float _syncedWalkStrength;

	private Vector3 _localVelocity;

	private float _lastAgentDestinationTime;

	internal Vector3? _desiredAgentDestination;

	private float _localDesiredAngle;

	private float _desiredAngleCv;

	public static float SyncFixWarpDistance;

	public static float SyncFixSmoothTime;

	public static float SyncFixSnapshotLifetime;

	public static float SyncAgentVelocityLifetime;

	public static float SyncExtrapolateMaxTime;

	public static float SyncExtrapolateStrength;

	private Vector3? _movementVector;

	private bool _isMovementVectorDirection;

	private float _destinationMovementSpeedMultiplier;

	[SyncVar(hook = "OnMovementSyncDataReceived")]
	private PositionSyncData _positionSyncData;

	private float _positionSyncDataUnscaledTime;

	private float _positionSyncDataTime;

	private double _positionSyncDataTimestampLowerBound;

	private Vector3 _fixCv;

	private const float GroundYSampleInterval = 1f / 30f;

	private float _lastObserverGroundSampleTime = float.MinValue;

	private Vector2 _lastObserverGroundSampleXZ;

	private const float PositionSyncDataSendRateMinInterval = 1f / 60f;

	private float _lastPositionSyncDataSendTime;

	private const double PositionSyncDataHeartbeatInterval = 1.0;

	public Action<float, float> _Mirror_SyncVarHookDelegate__outerRadius;

	public Action<float, float> _Mirror_SyncVarHookDelegate__innerRadius;

	public Action<PositionSyncData, PositionSyncData> _Mirror_SyncVarHookDelegate__positionSyncData;

	public float currentMaxAgentSpeed => baseAgentSpeed * entity.Status.movementSpeedMultiplier * GetMovementSpeedMultiplierByAttack();

	public float outerRadius
	{
		get
		{
			return _outerRadius;
		}
		set
		{
			Network_outerRadius = value;
		}
	}

	public float innerRadius
	{
		get
		{
			return _innerRadius;
		}
		set
		{
			Network_innerRadius = value;
		}
	}

	public bool freeMovement
	{
		get
		{
			return _freeMovement;
		}
		set
		{
			Network_freeMovement = value;
		}
	}

	public Vector3 agentPosition
	{
		get
		{
			if ((UnityEngine.Object)(object)_agent != null && ((Behaviour)(object)_agent).enabled)
			{
				return _agent.nextPosition;
			}
			return Dew.GetPositionOnGround(((Component)(object)this).transform.position);
		}
	}

	public Quaternion desiredRotation => Quaternion.Euler(0f, desiredAngle, 0f);

	public float desiredAngle
	{
		get
		{
			if (!isLocalMovementProcessor)
			{
				return _positionSyncData.desiredAngle;
			}
			return _localDesiredAngle;
		}
	}

	public ProxyCollider proxyCollider { get; private set; }

	public float? overridenDesiredAngle
	{
		[CompilerGenerated]
		get
		{
			return overridenDesiredAngle__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CoverridenDesiredAngle_003Ek__BackingField = value;
		}
	}

	public bool isControlReversed
	{
		[CompilerGenerated]
		get
		{
			return isControlReversed__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CisControlReversed_003Ek__BackingField = value;
		}
	}

	public bool isGamepadRotationLocked => _gamepadRotationLockCounter > 0;

	public bool canDestroy => true;

	public IReadOnlyList<ActionBase> queuedActions => _queuedActions;

	public bool isDoingMoveToAction { get; private set; }

	public bool isAirborne
	{
		get
		{
			if (isDisplacing)
			{
				return !ongoingDisplacement.isFriendly;
			}
			return false;
		}
	}

	public bool isDashing
	{
		get
		{
			if (isDisplacing)
			{
				return ongoingDisplacement.isFriendly;
			}
			return false;
		}
	}

	public bool isDodging
	{
		get
		{
			if (isDisplacing)
			{
				return ongoingDisplacement.isDodging;
			}
			return false;
		}
	}

	public bool isDisplacing => ongoingDisplacement != null;

	public Displacement ongoingDisplacement { get; private set; }

	public bool isClientSideMovement
	{
		get
		{
			DewPlayer owner = entity.owner;
			if ((UnityEngine.Object)(object)owner != null)
			{
				return owner.isHumanPlayer;
			}
			return false;
		}
	}

	public bool isLocalMovementProcessor
	{
		get
		{
			bool flag = isClientSideMovement;
			if (!flag || !((NetworkBehaviour)this).isOwned)
			{
				if (!flag)
				{
					return ((NetworkBehaviour)this).isServer;
				}
				return false;
			}
			return true;
		}
	}

	public float walkStrength => FilterWalkStrength(isLocalMovementProcessor ? _localWalkStrength : _syncedWalkStrength);

	public bool forceWalking
	{
		[CompilerGenerated]
		get
		{
			return forceWalking__BackingField;
		}
		[CompilerGenerated]
		internal set
		{
			Network_003CforceWalking_003Ek__BackingField = value;
		}
	}

	public bool isWalking => walkStrength > 0.01f;

	public Vector3 agentVelocity
	{
		get
		{
			if (!isLocalMovementProcessor)
			{
				return _positionSyncData.velocity;
			}
			return _localVelocity;
		}
	}

	private float _positionSyncDataUnscaledElapsedTime => Time.unscaledTime - _positionSyncDataUnscaledTime;

	private float _positionSyncDataElapsedTime => Time.time - _positionSyncDataTime;

	private float _positionSyncDataExtrapolateTime => Mathf.Clamp((float)(NetworkTime.time - _positionSyncData.timestamp), 0f, SyncExtrapolateMaxTime);

	public float Network_outerRadius
	{
		get
		{
			return _outerRadius;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _outerRadius, 1uL, _Mirror_SyncVarHookDelegate__outerRadius);
		}
	}

	public float Network_innerRadius
	{
		get
		{
			return _innerRadius;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _innerRadius, 2uL, _Mirror_SyncVarHookDelegate__innerRadius);
		}
	}

	public bool Network_freeMovement
	{
		get
		{
			return _freeMovement;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref _freeMovement, 4uL, (Action<bool, bool>)null);
		}
	}

	public float? Network_003CoverridenDesiredAngle_003Ek__BackingField
	{
		get
		{
			return overridenDesiredAngle__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float?>(value, ref overridenDesiredAngle__BackingField, 8uL, (Action<float?, float?>)null);
		}
	}

	public bool Network_003CisControlReversed_003Ek__BackingField
	{
		get
		{
			return isControlReversed__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref isControlReversed__BackingField, 16uL, (Action<bool, bool>)null);
		}
	}

	public int Network_gamepadRotationLockCounter
	{
		get
		{
			return _gamepadRotationLockCounter;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref _gamepadRotationLockCounter, 32uL, (Action<int, int>)null);
		}
	}

	public Vector3 Network_moveToActionDestination
	{
		get
		{
			return _moveToActionDestination;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<Vector3>(value, ref _moveToActionDestination, 64uL, (Action<Vector3, Vector3>)null);
		}
	}

	public float Network_moveToActionRequiredDistance
	{
		get
		{
			return _moveToActionRequiredDistance;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _moveToActionRequiredDistance, 128uL, (Action<float, float>)null);
		}
	}

	public byte Network_blockMove
	{
		get
		{
			return _blockMove;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<byte>(value, ref _blockMove, 256uL, (Action<byte, byte>)null);
		}
	}

	public byte Network_blockAbility
	{
		get
		{
			return _blockAbility;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<byte>(value, ref _blockAbility, 512uL, (Action<byte, byte>)null);
		}
	}

	public byte Network_blockAttack
	{
		get
		{
			return _blockAttack;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<byte>(value, ref _blockAttack, 1024uL, (Action<byte, byte>)null);
		}
	}

	public byte Network_blockDodge
	{
		get
		{
			return _blockDodge;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<byte>(value, ref _blockDodge, 2048uL, (Action<byte, byte>)null);
		}
	}

	public byte Network_blockMoveCancelable
	{
		get
		{
			return _blockMoveCancelable;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<byte>(value, ref _blockMoveCancelable, 4096uL, (Action<byte, byte>)null);
		}
	}

	public byte Network_blockAbilityCancelable
	{
		get
		{
			return _blockAbilityCancelable;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<byte>(value, ref _blockAbilityCancelable, 8192uL, (Action<byte, byte>)null);
		}
	}

	public byte Network_blockAttackCancelable
	{
		get
		{
			return _blockAttackCancelable;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<byte>(value, ref _blockAttackCancelable, 16384uL, (Action<byte, byte>)null);
		}
	}

	public byte Network_blockDodgeCancelable
	{
		get
		{
			return _blockDodgeCancelable;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<byte>(value, ref _blockDodgeCancelable, 32768uL, (Action<byte, byte>)null);
		}
	}

	public bool Network_003CforceWalking_003Ek__BackingField
	{
		get
		{
			return forceWalking__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref forceWalking__BackingField, 65536uL, (Action<bool, bool>)null);
		}
	}

	public float Network_syncedWalkStrength
	{
		get
		{
			return _syncedWalkStrength;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _syncedWalkStrength, 131072uL, (Action<float, float>)null);
		}
	}

	public PositionSyncData Network_positionSyncData
	{
		get
		{
			return _positionSyncData;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<PositionSyncData>(value, ref _positionSyncData, 262144uL, _Mirror_SyncVarHookDelegate__positionSyncData);
		}
	}

	public override void ClearPooledEventsAndProcessors()
	{
		base.ClearPooledEventsAndProcessors();
		ClientEvent_OnOuterRadiusChanged?.Clear();
		ClientEvent_OnInnerRadiusChanged?.Clear();
		ClientEvent_OnTeleport?.Clear();
		ClientEvent_OnDisplacementStarted?.Clear();
		ClientEvent_OnDisplacementFinished?.Clear();
		ClientEvent_OnDisplacementCanceled?.Clear();
	}

	private static void SetLayerRecursive(GameObject root)
	{
		if (root == null)
		{
			return;
		}
		int layer = 8;
		s_LayerStack.Clear();
		s_LayerStack.Add(root.transform);
		while (s_LayerStack.Count > 0)
		{
			int index = s_LayerStack.Count - 1;
			Transform transform = s_LayerStack[index];
			s_LayerStack.RemoveAt(index);
			GameObject gameObject = transform.gameObject;
			if (!gameObject.TryGetComponent<ParticleSystem>(out var _))
			{
				gameObject.layer = layer;
				int childCount = transform.childCount;
				for (int i = 0; i < childCount; i++)
				{
					s_LayerStack.Add(transform.GetChild(i));
				}
			}
		}
	}

	public void PreloadStart()
	{
		if (!_didPreloadStart)
		{
			_didPreloadStart = true;
			SetLayerRecursive(((Component)(object)this).gameObject);
			Rigidbody val = ((Component)(object)this).gameObject.AddComponent<Rigidbody>();
			val.useGravity = false;
			val.isKinematic = true;
			val.collisionDetectionMode = (CollisionDetectionMode)3;
		}
	}

	public override void OnStart()
	{
		base.OnStart();
		PreloadStart();
		proxyCollider = DewPhysics.AddProxyOfEntity(entity);
		Vector3 position = entity.position;
		_agent = ((Component)(object)this).gameObject.AddComponent<NavMeshAgent>();
		((Behaviour)(object)_agent).enabled = !ShouldDisableAgent();
		_agent.updateRotation = false;
		_agent.updatePosition = false;
		_agent.autoBraking = false;
		if (!((Behaviour)(object)_agent).enabled)
		{
			entity.position = position;
		}
		UpdateObstacleAvoidanceAndPriority();
		DoSyncStart();
		_nextActionTickTime = Time.time + UnityEngine.Random.value * (1f / 30f);
	}

	private void UpdateObstacleAvoidanceAndPriority()
	{
		_agent.obstacleAvoidanceType = (ObstacleAvoidanceType)((obstacleAvoidance && !entity.Status.hasUncollidable) ? ((!(entity is Hero) && !entity.IsAnyBoss()) ? 1 : 4) : 0);
		float num = innerRadius;
		if (Math.Abs(_agent.radius - num) > 0.01f)
		{
			_agent.radius = num;
			if (((Behaviour)(object)_agent).enabled)
			{
				((Behaviour)(object)_agent).enabled = false;
				((Behaviour)(object)_agent).enabled = true;
			}
		}
		if (((Behaviour)(object)_agent).enabled && _agent.isOnNavMesh)
		{
			int num2;
			if (entity.Control.isDisplacing)
			{
				num2 = 52;
			}
			else
			{
				num2 = ((!entity.Control.isWalking) ? 50 : 51);
			}
			if (entity.Status.hasCrowdControlImmunity)
			{
				num2 -= 10;
			}
			if (entity is Summon)
			{
				num2 += 20;
			}
			if (entity is Monster)
			{
				num2--;
			}
			num2 = Mathf.Clamp(num2, 0, 99);
			if (num2 != _agent.avoidancePriority)
			{
				_agent.avoidancePriority = num2;
			}
		}
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		_localDesiredAngle = CastInfo.GetAngle(((Component)(object)this).transform.forward);
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		entity.EntityEvent_OnDeath += new Action<EventInfoKill>(EntityEventOnDeath);
	}

	private bool HasAuthorityWithLog()
	{
		if (!((NetworkBehaviour)this).isOwned)
		{
			UnityEngine.Debug.LogWarning("No authority on " + entity.GetActorReadableName());
			return false;
		}
		return true;
	}

	private void EntityEventOnDeath(EventInfoKill obj)
	{
		RpcOnDeath();
	}

	[ClientRpc]
	private void RpcOnDeath()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void EntityControl::RpcOnDeath()", -69985209, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public override void OnStop()
	{
		base.OnStop();
		UnityEngine.Object.Destroy((UnityEngine.Object)(object)((Component)(object)this).GetComponent<CapsuleCollider>());
		UnityEngine.Object.Destroy((UnityEngine.Object)(object)((Component)(object)this).GetComponent<Rigidbody>());
	}

	public override void OnStopServer()
	{
		base.OnStopServer();
		NavMeshAgent component = ((Component)(object)this).GetComponent<NavMeshAgent>();
		if ((UnityEngine.Object)(object)component != null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)component);
		}
	}

	private bool ShouldTickActionsThisFrame()
	{
		if (entity is Hero || entity.IsAnyBoss())
		{
			return true;
		}
		if ((UnityEngine.Object)(object)entity.owner != null && (UnityEngine.Object)(object)entity.owner.controllingEntity == (UnityEngine.Object)(object)entity)
		{
			return true;
		}
		if (Time.time < _nextActionTickTime)
		{
			return false;
		}
		_nextActionTickTime = Time.time + 1f / 30f;
		return true;
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (!entity.isSleeping)
		{
			if (ShouldTickActionsThisFrame())
			{
				DoAttackFrameUpdate();
				DoActionFrameUpdate();
			}
			else
			{
				DoAttackSlowdownTick();
			}
			DoMovementFrameUpdate();
		}
	}

	private void DoOverrideRotationLogicUpdate(float dt)
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (_overrideAngleLifeTime <= 0f)
		{
			if (overridenDesiredAngle.HasValue)
			{
				Network_003CoverridenDesiredAngle_003Ek__BackingField = null;
			}
			return;
		}
		_overrideAngleLifeTime = Mathf.MoveTowards(_overrideAngleLifeTime, 0f, dt);
		if ((UnityEngine.Object)(object)_overrideAngleEntity != null)
		{
			if (!_overrideAngleEntity.isActive || _overrideAngleEntity.Status.isDead)
			{
				_overrideAngleLifeTime = 0f;
				Network_003CoverridenDesiredAngle_003Ek__BackingField = null;
			}
			else
			{
				Network_003CoverridenDesiredAngle_003Ek__BackingField = CastInfo.GetAngle(_overrideAngleEntity.GetAIPosition(entity) - entity.position);
			}
		}
		else if (_overrideAnglePosition.HasValue)
		{
			Network_003CoverridenDesiredAngle_003Ek__BackingField = CastInfo.GetAngle(_overrideAnglePosition.Value - entity.position);
		}
		else if (overridenDesiredAngle != _overrideAngle)
		{
			Network_003CoverridenDesiredAngle_003Ek__BackingField = _overrideAngle;
		}
	}

	private bool ShouldDisableAgent()
	{
		if (!freeMovement && !entity.Visual.isSpawning && !entity.IsNullInactiveDeadOrKnockedOut())
		{
			if (isDisplacing)
			{
				return ShouldDisplacementDisableAgent();
			}
			return false;
		}
		return true;
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (entity.isSleeping)
		{
			return;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			DoOverrideRotationLogicUpdate(dt);
			Vector3 position = ((Component)(object)this).transform.position;
			if ((position - _lastMoveCheckPosition).sqrMagnitude > 0.25f)
			{
				UpdateLastMoveTime();
				_lastMoveCheckPosition = position;
			}
		}
		DoMovementLogicUpdate();
		if (((NetworkBehaviour)this).isServer && ongoingChannels.Count > 0)
		{
			TickChannels(dt);
		}
		bool flag = ShouldDisableAgent();
		if (flag && ((Behaviour)(object)_agent).enabled)
		{
			((Behaviour)(object)_agent).enabled = false;
		}
		else if (!flag && !((Behaviour)(object)_agent).enabled)
		{
			Vector3 position2 = ((Component)(object)this).transform.position;
			position2 = Dew.GetValidAgentPosition(position2);
			((Component)(object)this).transform.position = position2;
			((Behaviour)(object)_agent).enabled = true;
		}
		if (((NetworkBehaviour)this).isServer && (entity.IsNullInactiveDeadOrKnockedOut() || entity.Visual.isSpawning))
		{
			Stop();
		}
		if (proxyCollider != null && (UnityEngine.Object)(object)proxyCollider.entity == (UnityEngine.Object)(object)entity)
		{
			DewPhysics.UpdateProxyPosition(proxyCollider, ((Component)(object)this).transform.position);
		}
	}

	[Server]
	public void StartDisplacement(Displacement disp)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void EntityControl::StartDisplacement(Displacement)' called when server was not active");
			return;
		}
		if (!disp.isFriendly && entity.Status.hasCrowdControlImmunity)
		{
			CancelImmediately();
			return;
		}
		if (!Dew.IsOkay(disp))
		{
			CancelImmediately();
			return;
		}
		StartDisplacementLocal(disp);
		RpcStartDisplacement(disp);
		void CancelImmediately()
		{
			disp.hasStarted = true;
			disp.isAlive = false;
			disp.onCancel?.Invoke();
			ClientEvent_OnDisplacementCanceled?.Invoke(disp);
			NetworkedManagerBase<ClientEventManager>.instance.InvokeOnIgnoreCC(entity);
		}
	}

	[ClientRpc]
	private void RpcStartDisplacement(Displacement disp)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkWriter)(object)val).WriteDisplacement(disp);
		((NetworkBehaviour)this).SendRPCInternal("System.Void EntityControl::RpcStartDisplacement(Displacement)", -1028026508, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void StartDisplacementLocal(Displacement disp)
	{
		if (disp.hasStarted)
		{
			UnityEngine.Debug.LogWarning("Tried to start a displacement that has already started!");
			return;
		}
		if (isDisplacing)
		{
			CancelOngoingDisplacementLocal();
		}
		ongoingDisplacement = disp;
		ongoingDisplacement.hasStarted = true;
		ongoingDisplacement.isAlive = true;
		_dispData = default;
		ClientEvent_OnDisplacementStarted?.Invoke(disp);
	}

	[Server]
	public void CancelOngoingDisplacement()
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void EntityControl::CancelOngoingDisplacement()' called when server was not active");
			return;
		}
		RpcCancelOngoingDisplacement();
		CancelOngoingDisplacementLocal();
	}

	[ClientRpc]
	private void RpcCancelOngoingDisplacement()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void EntityControl::RpcCancelOngoingDisplacement()", -2097004086, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void CancelOngoingDisplacementLocal()
	{
		if (isDisplacing)
		{
			Displacement displacement = ongoingDisplacement;
			ongoingDisplacement = null;
			_dispData = default;
			displacement.isAlive = false;
			displacement.onCancel?.Invoke();
			ClientEvent_OnDisplacementCanceled?.Invoke(displacement);
		}
	}

	[Server]
	public void Rotate(Vector3 forward, bool immediately, float overrideDuration = -1f)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void EntityControl::Rotate(UnityEngine.Vector3,System.Boolean,System.Single)' called when server was not active");
		}
		else if (!(forward.Flattened().sqrMagnitude < 0.01f))
		{
			Rotate(Quaternion.LookRotation(forward.Flattened()), immediately, overrideDuration);
		}
	}

	[Server]
	public void RotateTowards(Vector3 position, bool immediately, float overrideDuration = -1f)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void EntityControl::RotateTowards(UnityEngine.Vector3,System.Boolean,System.Single)' called when server was not active");
			return;
		}
		if (overrideDuration > 0f)
		{
			_overrideAngleEntity = null;
			_overrideAnglePosition = position;
			_overrideAngleLifeTime = overrideDuration;
			DoOverrideRotationLogicUpdate(0f);
		}
		Rotate(position - ((Component)(object)this).transform.position, immediately);
	}

	[Server]
	public void RotateTowards(Entity ent, bool immediately, float overrideDuration = -1f)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void EntityControl::RotateTowards(Entity,System.Boolean,System.Single)' called when server was not active");
		}
		else if (!((UnityEngine.Object)(object)ent == null))
		{
			if (overrideDuration > 0f)
			{
				_overrideAngleEntity = ent;
				_overrideAnglePosition = null;
				_overrideAngleLifeTime = overrideDuration;
				DoOverrideRotationLogicUpdate(0f);
			}
			Rotate(ent.GetAIPosition(entity) - ((Component)(object)this).transform.position, immediately);
		}
	}

	[Server]
	public void Rotate(Quaternion rotation, bool immediately, float overrideDuration = -1f)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void EntityControl::Rotate(UnityEngine.Quaternion,System.Boolean,System.Single)' called when server was not active");
		}
		else
		{
			Rotate(rotation.eulerAngles.y, immediately, overrideDuration);
		}
	}

	[Server]
	public void Rotate(float angle, bool immediately, float overrideDuration = -1f)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void EntityControl::Rotate(System.Single,System.Boolean,System.Single)' called when server was not active");
			return;
		}
		if (isLocalMovementProcessor)
		{
			_localDesiredAngle = angle;
		}
		else
		{
			TpcSetDesiredAngle(angle);
		}
		if (immediately)
		{
			RpcRotateImmediately(angle);
		}
		if (overrideDuration > 0f)
		{
			_overrideAngleEntity = null;
			_overrideAnglePosition = null;
			_overrideAngle = angle;
			_overrideAngleLifeTime = overrideDuration;
			DoOverrideRotationLogicUpdate(0f);
		}
	}

	[Command]
	public void CmdCancelOngoingChannels()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendCommandInternal("System.Void EntityControl::CmdCancelOngoingChannels()", -1975460018, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Server]
	public void CancelOngoingChannels()
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void EntityControl::CancelOngoingChannels()' called when server was not active");
			return;
		}
		foreach (Channel ongoingChannel in ongoingChannels)
		{
			if (ongoingChannel.isAlive)
			{
				ongoingChannel.Cancel();
			}
		}
	}

	[TargetRpc]
	private void TpcSetDesiredAngle(float angle)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteFloat((NetworkWriter)(object)val, angle);
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)null, "System.Void EntityControl::TpcSetDesiredAngle(System.Single)", 1502117120, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcRotateImmediately(float angle)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteFloat((NetworkWriter)(object)val, angle);
		((NetworkBehaviour)this).SendRPCInternal("System.Void EntityControl::RpcRotateImmediately(System.Single)", 698520438, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Server]
	public void StopOverrideRotation()
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void EntityControl::StopOverrideRotation()' called when server was not active");
			return;
		}
		_overrideAngleEntity = null;
		_overrideAnglePosition = null;
		_overrideAngleLifeTime = 0f;
		Network_003CoverridenDesiredAngle_003Ek__BackingField = null;
	}

	[Server]
	public void LockGamepadRotation()
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void EntityControl::LockGamepadRotation()' called when server was not active");
		}
		else
		{
			Network_gamepadRotationLockCounter = _gamepadRotationLockCounter + 1;
		}
	}

	[Server]
	public void UnlockGamepadRotation()
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void EntityControl::UnlockGamepadRotation()' called when server was not active");
		}
		else
		{
			Network_gamepadRotationLockCounter = _gamepadRotationLockCounter - 1;
		}
	}

	[Server]
	public void Teleport(Vector3 position)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void EntityControl::Teleport(UnityEngine.Vector3)' called when server was not active");
			return;
		}
		entity._wasStuckCounter = 0;
		TeleportLocal(position);
		RpcTeleport(position);
		Network_positionSyncData = new PositionSyncData
		{
			timestamp = NetworkTime.time,
			position = ((Component)(object)this).transform.position,
			velocity = Vector3.zero,
			desiredAngle = desiredAngle
		};
	}

	[ClientRpc]
	private void RpcTeleport(Vector3 position)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteVector3((NetworkWriter)(object)val, position);
		((NetworkBehaviour)this).SendRPCInternal("System.Void EntityControl::RpcTeleport(UnityEngine.Vector3)", -720064226, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void TeleportLocal(Vector3 position)
	{
		if (!((UnityEngine.Object)(object)_agent == null))
		{
			Vector3 nextPosition = _agent.nextPosition;
			entity.Visual.DoActionBeforeTeleport();
			try
			{
				_agent.Warp(position);
			}
			finally
			{
				entity.Visual.DoActionAfterTeleport();
			}
			if (proxyCollider != null && (UnityEngine.Object)(object)proxyCollider.entity == (UnityEngine.Object)(object)entity)
			{
				DewPhysics.UpdateProxyPosition(proxyCollider, ((Component)(object)this).transform.position);
			}
			ClientEvent_OnTeleport?.Invoke(nextPosition, position);
			if (((NetworkBehaviour)entity).isOwned && (UnityEngine.Object)(object)ManagerBase<ControlManager>.instance.controllingEntity == (UnityEngine.Object)(object)entity && Vector3.Distance(nextPosition, position) > 13f)
			{
				ManagerBase<CameraManager>.instance.SnapCameraToFocusedEntity();
			}
		}
	}

	[Command]
	public void CmdLookInDirection(Vector3 dir)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteVector3((NetworkWriter)(object)val, dir);
		((NetworkBehaviour)this).SendCommandInternal("System.Void EntityControl::CmdLookInDirection(UnityEngine.Vector3)", -1864142367, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public void SetAgentPosition(Vector3 position)
	{
		if (isLocalMovementProcessor && !((UnityEngine.Object)(object)_agent == null))
		{
			_agent.nextPosition = position;
		}
	}

	private void OuterRadiusChanged(float oldValue, float newValue)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		ClientEvent_OnOuterRadiusChanged?.Invoke(oldValue, newValue);
		if (!(proxyCollider == null))
		{
			((CircleCollider2D)proxyCollider.collider).radius = newValue;
		}
	}

	private void InnerRadiusChanged(float oldValue, float newValue)
	{
		ClientEvent_OnInnerRadiusChanged?.Invoke(oldValue, newValue);
	}

	public void OnCleanup()
	{
		Stop();
		CancelOngoingDisplacement();
	}

	[Server]
	public void UpdateLastMoveTime()
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void EntityControl::UpdateLastMoveTime()' called when server was not active");
		}
		else if (!NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition && ManagerBase<TransitionManager>.instance.state != TransitionManager.StateType.Loading)
		{
			lastMoveTime = Time.time;
		}
	}

	[Server]
	private void SetMoveToActionState(bool state)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void EntityControl::SetMoveToActionState(System.Boolean)' called when server was not active");
			return;
		}
		if (isLocalMovementProcessor)
		{
			SetMoveToActionStateLocal(state);
			return;
		}
		TpcSetMoveToActionState(state);
		isDoingMoveToAction = state;
	}

	[TargetRpc]
	private void TpcSetMoveToActionState(bool state)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteBool((NetworkWriter)(object)val, state);
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)null, "System.Void EntityControl::TpcSetMoveToActionState(System.Boolean)", -437273642, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	private void SetMoveToActionStateLocal(bool state)
	{
		if (isDoingMoveToAction && !state)
		{
			_desiredAgentDestination = null;
		}
		isDoingMoveToAction = state;
		if (state)
		{
			_destinationMovementSpeedMultiplier = 1f;
			SetAgentDestinationLocal(_moveToActionDestination, important: true);
		}
	}

	[Server]
	public void Cast(AbilityTrigger trigger, int configIndex, CastInfo info, bool allowMoveToCast = true, bool skipRangeCheck = false)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void EntityControl::Cast(AbilityTrigger,System.Int32,CastInfo,System.Boolean,System.Boolean)' called when server was not active");
		}
		else if (!trigger.IsNullOrInactive() && configIndex < trigger.configs.Length)
		{
			AddAction(new ActionCast
			{
				trigger = trigger,
				configIndex = configIndex,
				isAllowedToMove = allowMoveToCast,
				info = info,
				isPredictedOnCast = false,
				skipRangeCheck = skipRangeCheck
			});
		}
	}

	[Server]
	public void Cast(AbilityTrigger trigger, int configIndex, Entity predictTarget, bool allowMoveToCast = true)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void EntityControl::Cast(AbilityTrigger,System.Int32,Entity,System.Boolean)' called when server was not active");
		}
		else if (!trigger.IsNullOrInactive() && configIndex < trigger.configs.Length)
		{
			AddAction(new ActionCast
			{
				trigger = trigger,
				configIndex = configIndex,
				isAllowedToMove = allowMoveToCast,
				predictTarget = predictTarget,
				isPredictedOnCast = true
			});
		}
	}

	[Server]
	public void Cast(AbilityTrigger trigger, CastInfo info, bool allowMoveToCast = true)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void EntityControl::Cast(AbilityTrigger,CastInfo,System.Boolean)' called when server was not active");
		}
		else
		{
			Cast(trigger, trigger.currentConfigIndex, info, allowMoveToCast);
		}
	}

	[Server]
	public void Cast(AbilityTrigger trigger, Entity predictTarget, bool allowMoveToCast = true)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void EntityControl::Cast(AbilityTrigger,Entity,System.Boolean)' called when server was not active");
		}
		else
		{
			Cast(trigger, trigger.currentConfigIndex, predictTarget, allowMoveToCast);
		}
	}

	[Command]
	public void CmdCast(AbilityTrigger trigger, int configIndex, CastInfo info, bool allowMoveToCast, bool skipRangeCheck)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)trigger);
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, configIndex);
		GeneratedNetworkCode._Write_CastInfo((NetworkWriter)(object)val, info);
		NetworkWriterExtensions.WriteBool((NetworkWriter)(object)val, allowMoveToCast);
		NetworkWriterExtensions.WriteBool((NetworkWriter)(object)val, skipRangeCheck);
		((NetworkBehaviour)this).SendCommandInternal("System.Void EntityControl::CmdCast(AbilityTrigger,System.Int32,CastInfo,System.Boolean,System.Boolean)", 1776035736, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Server]
	private void AddAction(ActionBase newAction)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void EntityControl::AddAction(ActionBase)' called when server was not active");
			return;
		}
		newAction._entity = entity;
		newAction._addedUnscaledTime = Time.unscaledTime;
		newAction._didDoFirstTick = false;
		_queuedActions.Add(newAction);
	}

	private void DoActionFrameUpdate()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (!isDoingMoveToAction)
		{
			_moveToActionOwner = null;
		}
		bool flag = false;
		for (int num = _queuedActions.Count - 1; num >= 0; num--)
		{
			ActionBase actionBase = _queuedActions[num];
			if (!isDoingMoveToAction && actionBase._didDoFirstTick && actionBase.isAllowedToMove)
			{
				if (actionBase.ShouldCancelIfDisallowedToMove())
				{
					RemoveAction(num);
					continue;
				}
				actionBase.isAllowedToMove = false;
			}
			try
			{
				if (actionBase.Tick())
				{
					if (_queuedActions.Count == 0)
					{
						break;
					}
					RemoveAction(num);
					continue;
				}
				if (actionBase.isAllowedToMove && !flag)
				{
					Vector3? moveDestination = actionBase.GetMoveDestination();
					if (moveDestination.HasValue)
					{
						Network_moveToActionDestination = moveDestination.Value;
						Network_moveToActionRequiredDistance = actionBase.GetMoveDestinationRequiredDistance();
						_moveToActionOwner = actionBase;
						if (actionBase.isFirstTick)
						{
							SetMoveToActionState(state: true);
						}
						actionBase._didDoFirstTick = true;
						flag = true;
						continue;
					}
				}
				actionBase._didDoFirstTick = true;
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogWarning($"Action {actionBase} removed from {entity.GetActorReadableName()} due to exception below");
				UnityEngine.Debug.LogException(exception);
				RemoveAction(num);
				continue;
			}
			if (Time.unscaledTime - actionBase._addedUnscaledTime > 0.2f)
			{
				RemoveAction(num);
			}
		}
		if (_moveToActionOwner == null && isDoingMoveToAction)
		{
			SetMoveToActionState(state: false);
		}
	}

	internal void RemoveAction(int index)
	{
		if (index >= 0 && index < _queuedActions.Count)
		{
			ActionBase actionBase = _queuedActions[index];
			_queuedActions.RemoveAt(index);
			if (_moveToActionOwner == actionBase)
			{
				_moveToActionOwner = null;
			}
		}
	}

	private void StopDoingMoveToAction()
	{
		if (isLocalMovementProcessor && isDoingMoveToAction)
		{
			_desiredAgentDestination = null;
			isDoingMoveToAction = false;
			if (((NetworkBehaviour)this).isServer)
			{
				_moveToActionOwner = null;
			}
			else
			{
				CmdStopDoingMoveToAction();
			}
		}
	}

	[Command]
	private void CmdStopDoingMoveToAction()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendCommandInternal("System.Void EntityControl::CmdStopDoingMoveToAction()", -1348140044, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Command]
	public void CmdAttack(Entity target, bool doChase)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)target);
		NetworkWriterExtensions.WriteBool((NetworkWriter)(object)val, doChase);
		((NetworkBehaviour)this).SendCommandInternal("System.Void EntityControl::CmdAttack(Entity,System.Boolean)", -720151533, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Server]
	public void Attack(Entity target, bool doChase)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void EntityControl::Attack(Entity,System.Boolean)' called when server was not active");
			return;
		}
		attackTarget = target;
		_isMoveToAttackActive = doChase;
	}

	[Server]
	public void AttackMove(Vector3 destination, bool useDistanceFromDestination = false)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void EntityControl::AttackMove(UnityEngine.Vector3,System.Boolean)' called when server was not active");
			return;
		}
		AddAction(new ActionAttackMove
		{
			destination = destination,
			isAllowedToMove = true,
			useDistanceFromDestination = useDistanceFromDestination
		});
	}

	[Command]
	public void CmdAttackMove(Vector3 destination, bool useDistanceFromDestination)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteVector3((NetworkWriter)(object)val, destination);
		NetworkWriterExtensions.WriteBool((NetworkWriter)(object)val, useDistanceFromDestination);
		((NetworkBehaviour)this).SendCommandInternal("System.Void EntityControl::CmdAttackMove(UnityEngine.Vector3,System.Boolean)", -1264557694, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Command]
	private void CmdCancelMoveToAttack()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendCommandInternal("System.Void EntityControl::CmdCancelMoveToAttack()", -917930445, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	internal void DoAttackSlowdownTick()
	{
		if (isLocalMovementProcessor && !isWalking)
		{
			_lastAttackStartTime -= Time.deltaTime;
		}
	}

	private void DoAttackFrameUpdate()
	{
		DoAttackSlowdownTick();
		if (!((NetworkBehaviour)this).isServer || attackTarget == null || (attackTarget.IsNullInactiveDeadOrKnockedOut() && !TryGetOtherTarget()))
		{
			return;
		}
		AbilityTrigger attackAbility = entity.Ability.attackAbility;
		if (attackAbility.IsNullOrInactive())
		{
			return;
		}
		if (_isMoveToAttackActive)
		{
			for (int num = _queuedActions.Count - 1; num >= 0; num--)
			{
				ActionBase actionBase = _queuedActions[num];
				if (actionBase is ActionCast actionCast && (UnityEngine.Object)(object)actionCast.trigger == (UnityEngine.Object)(object)attackAbility)
				{
					if (actionBase.isAllowedToMove)
					{
						return;
					}
					RemoveAction(num);
				}
			}
			Cast(attackAbility, -1, attackTarget);
		}
		else if (attackAbility.currentConfig.CheckRange(entity, attackTarget))
		{
			if (attackAbility.CanBeCast())
			{
				Cast(attackAbility, -1, attackTarget, allowMoveToCast: false);
			}
		}
		else if (Time.time - _lastOutOfRangeRetargetTime > 0.2f)
		{
			_lastOutOfRangeRetargetTime = Time.time;
			TryGetOtherTarget();
		}
	}

	private bool TryGetOtherTarget()
	{
		AbilityTrigger attackAbility = entity.Ability.attackAbility;
		if (attackAbility.IsNullOrInactive())
		{
			return false;
		}
		CollisionCheckSettings.pivot = (((UnityEngine.Object)(object)attackTarget != null) ? ((Component)(object)attackTarget).transform.position : agentPosition);
		List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, agentPosition, attackAbility.currentConfig.effectiveRange, attackAbility.currentConfig.targetValidator, entity, new CollisionCheckSettings
		{
			sortComparer = CollisionCheckSettings.DistanceFromPivot
		});
		if (list.Count > 0)
		{
			attackTarget = list[0];
			handle.Return();
			return true;
		}
		attackTarget = null;
		handle.Return();
		return false;
	}

	[Server]
	internal void SetAttackMovSpdDisadvantage(float channelDuration)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void EntityControl::SetAttackMovSpdDisadvantage(System.Single)' called when server was not active");
		}
		else if (isLocalMovementProcessor)
		{
			SetAttackMovSpdDisadvantage_Imp(channelDuration);
		}
		else if (isClientSideMovement && entity.owner.isHumanPlayer)
		{
			TpcSetAttackMovSpdDisadvantage(channelDuration);
		}
	}

	[TargetRpc]
	internal void TpcSetAttackMovSpdDisadvantage(float channelDuration)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteFloat((NetworkWriter)(object)val, channelDuration);
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)null, "System.Void EntityControl::TpcSetAttackMovSpdDisadvantage(System.Single)", 1424288161, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	private void SetAttackMovSpdDisadvantage_Imp(float channelDuration)
	{
		_lastAttackStartTime = Time.time;
		_lastAttackMovSpdMultiplier = 0.5f;
		_lastAttackMovSpdDuration = channelDuration * 2f;
	}

	private float GetMovementSpeedMultiplierByAttack()
	{
		float num = Time.time - _lastAttackStartTime;
		if (num > _lastAttackMovSpdDuration)
		{
			return 1f;
		}
		return Mathf.Lerp(_lastAttackMovSpdMultiplier, 1f, num / _lastAttackMovSpdDuration);
	}

	private void TickChannel(Channel channel, float dt)
	{
		if (!channel.isAlive)
		{
			ongoingChannels.Remove(channel);
			return;
		}
		bool flag = false;
		if (!entity.isActive || entity is Hero { isKnockedOut: not false })
		{
			flag = true;
		}
		else if (channel._validators != null)
		{
			for (int i = 0; i < channel._validators.Count; i++)
			{
				if (!channel._validators[i]())
				{
					flag = true;
					break;
				}
			}
		}
		if (flag && dt != 0.0001f)
		{
			channel.isAlive = false;
			DecrementBlockCounters(channel);
			ongoingChannels.Remove(channel);
			channel.onCancel?.Invoke();
			return;
		}
		if (channel.uncancellableTime > 0f && channel.elapsedTime < channel.uncancellableTime && channel.elapsedTime + dt >= channel.uncancellableTime && (channel.blockedActions & Channel.BlockedAction.Cancelable) != 0)
		{
			DecrementBlockCounters(channel);
			IncrementBlockCounters(channel.blockedActions);
		}
		channel.elapsedTime += dt;
		channel.onTick?.Invoke(dt);
		if (channel.elapsedTime >= channel.duration && !flag)
		{
			channel.elapsedTime = channel.duration;
			channel.isAlive = false;
			DecrementBlockCounters(channel, channel.duration > 0.0001f);
			ongoingChannels.Remove(channel);
			channel.onComplete?.Invoke();
		}
	}

	private void TickChannels(float dt)
	{
		for (int num = ongoingChannels.Count - 1; num >= 0; num--)
		{
			Channel channel = ongoingChannels[num];
			TickChannel(channel, dt);
		}
	}

	public Channel StartDaze(float duration)
	{
		Channel channel = new Channel
		{
			blockedActions = Channel.BlockedAction.Everything,
			duration = duration
		};
		if (duration <= 0.0001f)
		{
			return channel;
		}
		StartChannel(channel);
		return channel;
	}

	[Server]
	public Channel StartChannel(Channel channel)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'Channel EntityControl::StartChannel(Channel)' called when server was not active");
			return null;
		}
		channel._owner = entity;
		channel.isAlive = true;
		if (channel.uncancellableTime > 0f && (channel.blockedActions & Channel.BlockedAction.Cancelable) != 0)
		{
			IncrementBlockCounters(channel.blockedActions & ~Channel.BlockedAction.Cancelable);
		}
		else
		{
			IncrementBlockCounters(channel.blockedActions);
		}
		ongoingChannels.Add(channel);
		TickChannel(channel, 0.0001f);
		return channel;
	}

	[Server]
	public void IncrementBlockCounters(Channel.BlockedAction action)
	{
		checked
		{
			if (!NetworkServer.active)
			{
				UnityEngine.Debug.LogWarning("[Server] function 'System.Void EntityControl::IncrementBlockCounters(Channel/BlockedAction)' called when server was not active");
			}
			else if ((action & Channel.BlockedAction.Cancelable) != 0)
			{
				if ((action & Channel.BlockedAction.Move) != 0)
				{
					Network_blockMoveCancelable = (byte)(unchecked((uint)_blockMoveCancelable) + 1u);
				}
				if ((action & Channel.BlockedAction.Ability) != 0)
				{
					Network_blockAbilityCancelable = (byte)(unchecked((uint)_blockAbilityCancelable) + 1u);
				}
				if ((action & Channel.BlockedAction.Attack) != 0)
				{
					Network_blockAttackCancelable = (byte)(unchecked((uint)_blockAttackCancelable) + 1u);
				}
				if ((action & Channel.BlockedAction.Dodge) != 0)
				{
					Network_blockDodgeCancelable = (byte)(unchecked((uint)_blockDodgeCancelable) + 1u);
				}
			}
			else
			{
				if ((action & Channel.BlockedAction.Move) != 0)
				{
					Network_blockMove = (byte)(unchecked((uint)_blockMove) + 1u);
				}
				if ((action & Channel.BlockedAction.Ability) != 0)
				{
					Network_blockAbility = (byte)(unchecked((uint)_blockAbility) + 1u);
				}
				if ((action & Channel.BlockedAction.Attack) != 0)
				{
					Network_blockAttack = (byte)(unchecked((uint)_blockAttack) + 1u);
				}
				if ((action & Channel.BlockedAction.Dodge) != 0)
				{
					Network_blockDodge = (byte)(unchecked((uint)_blockDodge) + 1u);
				}
			}
		}
	}

	[Server]
	private void DecrementBlockCounters(Channel channel, bool delayOneFrame = true)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void EntityControl::DecrementBlockCounters(Channel,System.Boolean)' called when server was not active");
		}
		else if (channel.uncancellableTime <= 0f || (channel.blockedActions & Channel.BlockedAction.Cancelable) == 0)
		{
			DecrementBlockCounters(channel.blockedActions, delayOneFrame);
		}
		else if (channel.elapsedTime < channel.uncancellableTime)
		{
			DecrementBlockCounters(channel.blockedActions & ~Channel.BlockedAction.Cancelable, delayOneFrame);
		}
		else
		{
			DecrementBlockCounters(channel.blockedActions, delayOneFrame);
		}
	}

	[AsyncStateMachine(typeof(_003CDecrementBlockCounters_003Ed__161))]
	[Server]
	public UniTaskVoid DecrementBlockCounters(Channel.BlockedAction action, bool delayOneFrame = true)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'Cysharp.Threading.Tasks.UniTaskVoid EntityControl::DecrementBlockCounters(Channel/BlockedAction,System.Boolean)' called when server was not active");
			return default;
		}
		_003CDecrementBlockCounters_003Ed__161 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		obj._003C_003E4__this = this;
		obj.action = action;
		obj.delayOneFrame = delayOneFrame;
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CDecrementBlockCounters_003Ed__161>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	public BlockStatus IsActionBlocked(BlockableAction action)
	{
		switch (action)
		{
		case BlockableAction.Move:
			if (_blockMove > 0)
			{
				return BlockStatus.Blocked;
			}
			if (_blockMoveCancelable > 0)
			{
				return BlockStatus.BlockedCancelable;
			}
			return BlockStatus.Allowed;
		case BlockableAction.Ability:
			if (_blockAbility > 0)
			{
				return BlockStatus.Blocked;
			}
			if (_blockAbilityCancelable > 0)
			{
				return BlockStatus.BlockedCancelable;
			}
			return BlockStatus.Allowed;
		case BlockableAction.Attack:
			if (_blockAttack > 0)
			{
				return BlockStatus.Blocked;
			}
			if (_blockAttackCancelable > 0)
			{
				return BlockStatus.BlockedCancelable;
			}
			return BlockStatus.Allowed;
		case BlockableAction.Dodge:
			if (_blockDodge > 0)
			{
				return BlockStatus.Blocked;
			}
			if (_blockDodgeCancelable > 0)
			{
				return BlockStatus.BlockedCancelable;
			}
			return BlockStatus.Allowed;
		default:
			throw new Exception($"Unknown action type provided for block check: {action}");
		}
	}

	[Command]
	private void CmdDisobeyBlock(BlockableAction action)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_EntityControl_002FBlockableAction((NetworkWriter)(object)val, action);
		((NetworkBehaviour)this).SendCommandInternal("System.Void EntityControl::CmdDisobeyBlock(EntityControl/BlockableAction)", -1524326419, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Server]
	public void DisobeyBlock(BlockableAction action)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void EntityControl::DisobeyBlock(EntityControl/BlockableAction)' called when server was not active");
			return;
		}
		switch (action)
		{
		case BlockableAction.Move:
			if (_blockMove > 0)
			{
				throw new Exception($"Cannot disobey {action} block, it's not cancelable!");
			}
			if (_blockMoveCancelable == 0)
			{
				throw new Exception($"Cannot disobey {action} block, it's not prohibited!");
			}
			KillChannels(Channel.BlockedAction.Move);
			if (_blockMoveCancelable > 0)
			{
				throw new Exception($"Disobeyed {action} block, but it's not lifted by the channels!");
			}
			break;
		case BlockableAction.Ability:
			if (_blockAbility > 0)
			{
				throw new Exception($"Cannot disobey {action} block, it's not cancelable!");
			}
			if (_blockAbilityCancelable == 0)
			{
				throw new Exception($"Cannot disobey {action} block, it's not prohibited!");
			}
			KillChannels(Channel.BlockedAction.Ability);
			if (_blockAbilityCancelable > 0)
			{
				throw new Exception($"Disobeyed {action} block, but it's not lifted by the channels!");
			}
			break;
		case BlockableAction.Attack:
			if (_blockAttack > 0)
			{
				throw new Exception($"Cannot disobey {action} block, it's not cancelable!");
			}
			if (_blockAttackCancelable == 0)
			{
				throw new Exception($"Cannot disobey {action} block, it's not prohibited!");
			}
			KillChannels(Channel.BlockedAction.Attack);
			if (_blockAttackCancelable > 0)
			{
				throw new Exception($"Disobeyed {action} block, but it's not lifted by the channels!");
			}
			break;
		case BlockableAction.Dodge:
			if (_blockDodge > 0)
			{
				throw new Exception($"Cannot disobey {action} block, it's not cancelable!");
			}
			if (_blockDodgeCancelable == 0)
			{
				throw new Exception($"Cannot disobey {action} block, it's not prohibited!");
			}
			KillChannels(Channel.BlockedAction.Dodge);
			if (_blockDodgeCancelable > 0)
			{
				throw new Exception($"Disobeyed {action} block, but it's not lifted by the channels!");
			}
			break;
		default:
			throw new Exception($"Unknown action type provided for block disobeying: {action}");
		}
		void KillChannels(Channel.BlockedAction block)
		{
			for (int i = 0; i < ongoingChannels.Count; i++)
			{
				Channel channel = ongoingChannels[i];
				if (channel.isAlive && (channel.blockedActions & block) == block)
				{
					channel.isAlive = false;
					channel.onCancel?.Invoke();
					DecrementBlockCounters(channel, delayOneFrame: false);
				}
			}
		}
	}

	private void DoDisplacement()
	{
		//IL_05dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0684: Unknown result type (might be due to invalid IL or missing references)
		if (!entity.isActive || (!ongoingDisplacement.isFriendly && (entity.Status.hasInvulnerable || entity.Status.hasUnstoppable)))
		{
			CancelOngoingDisplacementLocal();
			return;
		}
		if (!Dew.IsOkay(ongoingDisplacement))
		{
			CancelOngoingDisplacementLocal();
			return;
		}
		float num = (ongoingDisplacement.affectedByMovementSpeed ? entity.Status.movementSpeedMultiplier : 1f);
		if (entity is Monster && num > 2.5f)
		{
			num = 2.5f;
		}
		if (ongoingDisplacement is DispByDestination dispByDestination)
		{
			if (dispByDestination.duration < 0.0001f)
			{
				dispByDestination.isAlive = false;
				ongoingDisplacement = null;
				_dispData = default;
				try
				{
					dispByDestination.onFinish?.Invoke();
				}
				catch (Exception exception)
				{
					UnityEngine.Debug.LogException(exception);
				}
				try
				{
					ClientEvent_OnDisplacementFinished?.Invoke(dispByDestination);
					return;
				}
				catch (Exception exception2)
				{
					UnityEngine.Debug.LogException(exception2);
					return;
				}
			}
			if (_dispData.knownEase != dispByDestination.ease || _dispData.easeFunction == null)
			{
				_dispData.easeFunction = EasingFunction.GetEasingFunction(dispByDestination.ease);
				_dispData.knownEase = dispByDestination.ease;
				_dispData.start = ((Component)(object)this).transform.position;
				_dispData.last = ((Component)(object)this).transform.position;
			}
			dispByDestination.elapsedTime += Time.deltaTime;
			float num2 = dispByDestination.elapsedTime / dispByDestination.duration;
			num2 = ((!float.IsNaN(num2)) ? Mathf.Clamp01(num2) : 1f);
			float t = _dispData.easeFunction(0f, 1f, num2);
			Vector2 vector = _dispData.start.ToXY();
			Vector2 vector2 = dispByDestination.destination.ToXY();
			Vector3 vector3;
			if ((vector - vector2).sqrMagnitude < 0.01f)
			{
				vector3 = _dispData.last;
			}
			else
			{
				Matrix4x4 inverse = CreateTransformABTo01(vector, vector2).inverse;
				Vector2 p = TransformPoint(new Vector2(dispByDestination.curve.x, dispByDestination.curve.y), inverse);
				Vector2 p2 = TransformPoint(new Vector2(dispByDestination.curve.z, dispByDestination.curve.w), inverse);
				Vector2 vector4 = GetPointOnBezierCurve(vector, p, p2, vector2, t);
				if (dispByDestination.curveHorizontalDistance.HasValue)
				{
					Vector2 normalized = (vector2 - vector).normalized;
					Vector2 vector5 = new Vector2(normalized.y, 0f - normalized.x);
					float num3 = dispByDestination.curveHorizontalDistance.Value / (vector2 - vector).magnitude * 4f;
					Vector2 rhs = vector4 - vector;
					Vector2 vector6 = normalized * Vector2.Dot(normalized, rhs) + vector5 * (Vector2.Dot(vector5, rhs) * num3);
					vector4 = vector + vector6;
				}
				vector3 = vector4.ToXZ().WithY(Mathf.Lerp(_dispData.start.y, dispByDestination.destination.y, t));
			}
			Vector3 v = vector3 - _dispData.last;
			if (Math.Abs(num - 1f) > 0.001f)
			{
				dispByDestination.destination += v * (num - 1f);
				v *= num;
				vector3 = _dispData.last + v;
			}
			_dispData.last = vector3;
			Dew.FilterNonOkayValues(ref v);
			if (_agent.isOnNavMesh)
			{
				_agent.Move(v);
				((Component)(object)this).transform.position = _agent.nextPosition;
			}
			else
			{
				((Component)(object)this).transform.position += v;
			}
			if (dispByDestination.rotateForward)
			{
				SetRotation(v, dispByDestination.rotateSmoothly);
			}
			else
			{
				DoRotateTowardsDesiredAngleTick(desiredAngle);
			}
			if (dispByDestination.elapsedTime > dispByDestination.duration)
			{
				dispByDestination.isAlive = false;
				ongoingDisplacement = null;
				_dispData = default;
				try
				{
					dispByDestination.onFinish?.Invoke();
				}
				catch (Exception exception3)
				{
					UnityEngine.Debug.LogException(exception3);
				}
				try
				{
					ClientEvent_OnDisplacementFinished?.Invoke(dispByDestination);
				}
				catch (Exception exception4)
				{
					UnityEngine.Debug.LogException(exception4);
				}
			}
		}
		else
		{
			if (!(ongoingDisplacement is DispByTarget dispByTarget))
			{
				return;
			}
			Vector3 position = ((Component)(object)dispByTarget.target).transform.position;
			Vector3 position2 = ((Component)(object)this).transform.position;
			Vector3 vector7 = position + (dispByTarget.target.Control.outerRadius + outerRadius + dispByTarget.goalDistance) * (position2 - position).normalized;
			Vector3 v2 = Vector3.MoveTowards(position2, vector7, Time.deltaTime * dispByTarget.speed * num);
			Dew.FilterNonOkayValues(ref v2);
			((Component)(object)this).transform.position = v2;
			_agent.nextPosition = v2;
			dispByTarget.elapsedTime += Time.deltaTime;
			if (dispByTarget.rotateForward)
			{
				SetRotation(position - ((Component)(object)this).transform.position, dispByTarget.rotateSmoothly);
			}
			if (Vector2.Distance(((Component)(object)this).transform.position.ToXY(), vector7.ToXY()) < 0.05f)
			{
				if ((int)Dew.GetNavMeshPathStatus(dispByTarget.target.position, ((Component)(object)this).transform.position) != 0 && ((NetworkBehaviour)this).isServer)
				{
					Teleport(Dew.GetValidAgentDestination_Closest(dispByTarget.target.agentPosition, agentPosition));
				}
				dispByTarget.isAlive = false;
				ongoingDisplacement = null;
				try
				{
					dispByTarget.onFinish?.Invoke();
				}
				catch (Exception exception5)
				{
					UnityEngine.Debug.LogException(exception5);
				}
				try
				{
					ClientEvent_OnDisplacementFinished?.Invoke(dispByTarget);
					return;
				}
				catch (Exception exception6)
				{
					UnityEngine.Debug.LogException(exception6);
					return;
				}
			}
			if (dispByTarget.elapsedTime > dispByTarget.cancelTime)
			{
				if (((NetworkBehaviour)this).isServer && (int)Dew.GetNavMeshPathStatus(dispByTarget.target.position, ((Component)(object)this).transform.position) != 0)
				{
					Teleport(Dew.GetValidAgentDestination_Closest(dispByTarget.target.agentPosition, agentPosition));
				}
				dispByTarget.onCancel?.Invoke();
				dispByTarget.isAlive = false;
				ongoingDisplacement = null;
				ClientEvent_OnDisplacementFinished?.Invoke(dispByTarget);
			}
		}
		void SetRotation(Vector3 forward, bool smooth)
		{
			forward.Flatten();
			if (!(forward.sqrMagnitude < 0.01f))
			{
				if (isLocalMovementProcessor)
				{
					_localDesiredAngle = CastInfo.GetAngle(forward);
				}
				if (smooth)
				{
					DoRotateTowardsDesiredAngleTick(CastInfo.GetAngle(forward));
				}
				else
				{
					((Component)(object)this).transform.rotation = Quaternion.LookRotation(forward);
				}
			}
		}
	}

	public static Vector2 GetPointOnBezierCurve(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
	{
		t = Mathf.Clamp01(t);
		float num = 1f - t;
		return num * num * num * p0 + 3f * num * num * t * p1 + 3f * num * t * t * p2 + t * t * t * p3;
	}

	public static Matrix4x4 CreateTransformABTo01(Vector2 a, Vector2 b)
	{
		Matrix4x4 matrix4x = Matrix4x4.Translate(new Vector3(0f - a.x, 0f - a.y, 0f));
		Vector2 vector = b - a;
		Vector2 vector2 = new Vector2(1f, 1f);
		float num = Mathf.Atan2(vector.y, vector.x);
		float num2 = Mathf.Atan2(vector2.y, vector2.x) - num;
		Matrix4x4 matrix4x2 = Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, num2 * 57.29578f));
		float magnitude = vector.magnitude;
		float num3 = vector2.magnitude / magnitude;
		return Matrix4x4.Scale(new Vector3(num3, num3, 1f)) * matrix4x2 * matrix4x;
	}

	public static Vector2 TransformPoint(Vector2 point, Matrix4x4 transform)
	{
		Vector3 point2 = new Vector3(point.x, point.y, 0f);
		Vector3 vector = transform.MultiplyPoint3x4(point2);
		return new Vector2(vector.x, vector.y);
	}

	private bool ShouldDisplacementDisableAgent()
	{
		if (ongoingDisplacement == null)
		{
			return false;
		}
		if (ongoingDisplacement is DispByDestination dispByDestination)
		{
			return dispByDestination.canGoOverTerrain;
		}
		if (ongoingDisplacement is DispByTarget)
		{
			return true;
		}
		return false;
	}

	[Server]
	public void Stop()
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void EntityControl::Stop()' called when server was not active");
			return;
		}
		ClearMovement();
		ClearActionQueue();
		if (isLocalMovementProcessor)
		{
			StopMovementProcessor();
		}
		else
		{
			TpcStopMovementProcessor();
		}
	}

	public void CmdStop()
	{
		if (HasAuthorityWithLog())
		{
			CmdClearMovement();
			if (isLocalMovementProcessor)
			{
				StopMovementProcessor();
			}
			else
			{
				CmdStopMovementProcessor();
			}
			CmdClearActionQueue();
		}
	}

	[Command]
	private void CmdClearActionQueue()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendCommandInternal("System.Void EntityControl::CmdClearActionQueue()", 462041075, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Server]
	public void ClearActionQueue()
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void EntityControl::ClearActionQueue()' called when server was not active");
			return;
		}
		_queuedActions.Clear();
		_moveToActionOwner = null;
	}

	[TargetRpc]
	private void TpcStopMovementProcessor()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)null, "System.Void EntityControl::TpcStopMovementProcessor()", -1383202127, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	[Command]
	private void CmdStopMovementProcessor()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendCommandInternal("System.Void EntityControl::CmdStopMovementProcessor()", -40884866, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void StopMovementProcessor()
	{
		_desiredAgentDestination = null;
		if (_agent.hasPath)
		{
			_agent.ResetPath();
		}
		StopDoingMoveToAction();
		if (((NetworkBehaviour)this).isServer)
		{
			_isMoveToAttackActive = false;
			Attack(null, doChase: false);
		}
		else
		{
			CmdCancelMoveToAttack();
			CmdAttack(null, doChase: false);
		}
	}

	[Server]
	public void Interact(IInteractable interactable, bool isAlt, bool isMouse)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void EntityControl::Interact(IInteractable,System.Boolean,System.Boolean)' called when server was not active");
			return;
		}
		if (interactable is Shrine_Stardust)
		{
			_lastStardustInteractTime = Time.time;
		}
		else if (Time.time - _lastStardustInteractTime < 0.75f)
		{
			return;
		}
		AddAction(new ActionInteract
		{
			interactable = interactable,
			isAllowedToMove = true,
			isAlt = isAlt,
			noActivation = (isMouse && !interactable.canInteractWithMouse)
		});
	}

	[Command]
	public void CmdInteract(IInteractable interactable, bool isAlt, bool isMouse)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkWriter)(object)val).WriteIInteractable(interactable);
		NetworkWriterExtensions.WriteBool((NetworkWriter)(object)val, isAlt);
		NetworkWriterExtensions.WriteBool((NetworkWriter)(object)val, isMouse);
		((NetworkBehaviour)this).SendCommandInternal("System.Void EntityControl::CmdInteract(IInteractable,System.Boolean,System.Boolean)", -1209342242, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private Vector3 GroundSnapCached(Vector3 pos)
	{
		if (_hasGroundSnap)
		{
			float num = pos.x - _lastGroundSnapXZ.x;
			float num2 = pos.z - _lastGroundSnapXZ.y;
			if (num * num + num2 * num2 < 0.0144f)
			{
				pos.y = _lastGroundSnapY;
				return pos;
			}
		}
		pos = Dew.GetPositionOnGround(pos);
		_lastGroundSnapXZ = new Vector2(pos.x, pos.z);
		_lastGroundSnapY = pos.y;
		_hasGroundSnap = true;
		return pos;
	}

	private float NavSnapYCached(Vector3 pos)
	{
		if (_hasNavSnap)
		{
			float num = pos.x - _lastNavSnapXZ.x;
			float num2 = pos.z - _lastNavSnapXZ.y;
			if (num * num + num2 * num2 < 0.0144f)
			{
				return _lastNavSnapY;
			}
		}
		float y = Dew.GetValidAgentPosition(pos).y;
		_lastNavSnapXZ = new Vector2(pos.x, pos.z);
		_lastNavSnapY = y;
		_hasNavSnap = true;
		return y;
	}

	private float FilterWalkStrength(float original)
	{
		if (forceWalking)
		{
			return 1f;
		}
		if (IsActionBlocked(BlockableAction.Move) != BlockStatus.Allowed || currentMaxAgentSpeed < 0.1f || entity.Status.hasRoot || entity.Status.hasStun || entity.Control.isDisplacing)
		{
			return 0f;
		}
		return original;
	}

	private void DoMovementFrameUpdate()
	{
		if (NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition)
		{
			CancelOngoingDisplacementLocal();
		}
		else if (isLocalMovementProcessor)
		{
			DoMovementProcessorFrameUpdate();
		}
		else
		{
			DoMovementObserverFrameUpdate();
		}
	}

	private void DoMovementLogicUpdate()
	{
		if (obstacleAvoidance && (UnityEngine.Object)(object)_agent != null)
		{
			UpdateObstacleAvoidanceAndPriority();
		}
	}

	private void DoMovementProcessorFrameUpdate()
	{
		if (entity.Visual.isSpawning)
		{
			return;
		}
		if (isDoingMoveToAction)
		{
			if (Vector2.Distance(_moveToActionDestination.ToXY(), _agent.nextPosition.ToXY()) < _moveToActionRequiredDistance - 0.001f)
			{
				_desiredAgentDestination = null;
			}
			else
			{
				_desiredAgentDestination = _moveToActionDestination;
			}
		}
		bool flag = entity.Status.hasRoot || entity.Status.hasStun || isDisplacing;
		if (_agent.isOnNavMesh)
		{
			if ((flag || !_desiredAgentDestination.HasValue) && _agent.hasPath)
			{
				_agent.ResetPath();
			}
			if (_desiredAgentDestination.HasValue && Time.time - _lastAgentDestinationTime > 0.25f)
			{
				Vector3 destination = _agent.destination;
				Vector3? desiredAgentDestination = _desiredAgentDestination;
				if (destination != desiredAgentDestination)
				{
					_lastAgentDestinationTime = Time.time;
					_agent.SetDestination(_desiredAgentDestination.Value);
				}
			}
		}
		if (overridenDesiredAngle.HasValue)
		{
			_localDesiredAngle = overridenDesiredAngle.Value;
		}
		if (isDisplacing)
		{
			DoDisplacement();
			_localVelocity = default;
			UpdatePositionSyncData(new PositionSyncData
			{
				timestamp = NetworkTime.time,
				position = ((Component)(object)this).transform.position,
				velocity = _localVelocity,
				desiredAngle = _localDesiredAngle
			});
			if (Math.Abs(0f - _localWalkStrength) > 0.001f)
			{
				SetWalkStrength(0f);
			}
			return;
		}
		float num = 0f;
		Vector3 targetVelocity = default;
		float num2 = currentMaxAgentSpeed;
		if (_movementVector.HasValue && !_isMovementVectorDirection && Vector2.SqrMagnitude(_agent.destination.ToXY() - _agent.nextPosition.ToXY()) < 0.1f)
		{
			_agent.ResetPath();
			_movementVector = null;
			_desiredAgentDestination = null;
		}
		if ((IsActionBlocked(BlockableAction.Move) != BlockStatus.Allowed) | flag)
		{
			targetVelocity = default;
		}
		else if (_agent.hasPath)
		{
			if (_agent.desiredVelocity.sqrMagnitude > 0.01f)
			{
				Vector3 normalized = _agent.desiredVelocity.normalized;
				num = _destinationMovementSpeedMultiplier;
				targetVelocity = _destinationMovementSpeedMultiplier * num2 * normalized;
			}
		}
		else if (_movementVector.HasValue && _isMovementVectorDirection)
		{
			Vector3 value = _movementVector.Value;
			targetVelocity = num2 * value;
			num = value.magnitude;
		}
		_agent.velocity = Vector3.zero;
		DoVelocityUpdate(targetVelocity);
		Vector3 vector = _localVelocity * Time.deltaTime;
		NavMeshAgent agent = _agent;
		agent.nextPosition += vector;
		if (!overridenDesiredAngle.HasValue && targetVelocity.sqrMagnitude > 0.1f && _localVelocity.sqrMagnitude > 0.1f)
		{
			_localDesiredAngle = CastInfo.GetAngle(_localVelocity);
		}
		DoRotateTowardsDesiredAngleTick(_localDesiredAngle);
		if (Math.Abs(num - _localWalkStrength) > 0.001f)
		{
			if (num == 0f && _localWalkStrength > 0f)
			{
				num = Mathf.MoveTowards(_localWalkStrength, 0f, Time.deltaTime * 20f);
			}
			SetWalkStrength(num);
		}
		Vector3 vector2 = _agent.nextPosition;
		if (_agent.isOnNavMesh)
		{
			vector2.y = NavSnapYCached(vector2);
		}
		else
		{
			vector2 = GroundSnapCached(vector2);
		}
		((Component)(object)this).transform.position = vector2;
		UpdatePositionSyncData(new PositionSyncData
		{
			timestamp = NetworkTime.time,
			position = vector2,
			velocity = _localVelocity,
			desiredAngle = _localDesiredAngle
		});
	}

	private void DoVelocityUpdate(Vector3 targetVelocity)
	{
		float magnitude = _localVelocity.magnitude;
		float num = ((!(magnitude > 0.1f)) ? ((Component)(object)this).transform.rotation.eulerAngles.y : Vector3.SignedAngle(Vector3.forward, _localVelocity, Vector3.up));
		float magnitude2 = targetVelocity.magnitude;
		float target = ((!(magnitude2 > 0.1f)) ? num : Vector3.SignedAngle(Vector3.forward, targetVelocity, Vector3.up));
		float num2 = ((entity.Status.movementSpeedMultiplier > 0.001f || magnitude2 > magnitude) ? (normalizedAcceleration * currentMaxAgentSpeed) : (Mathf.Clamp(baseAgentSpeed, 0.1f, 1000f) * 5f));
		float num3 = Mathf.MoveTowardsAngle(num, target, rotateSpeed * entity.Status.movementSpeedMultiplier * Time.deltaTime);
		float num4 = Mathf.MoveTowards(magnitude, magnitude2, num2 * Time.deltaTime * ((magnitude > magnitude2) ? 2f : 1f));
		if (float.IsNaN(num3))
		{
			num3 = num;
		}
		if (float.IsNaN(num4))
		{
			num4 = magnitude;
		}
		Vector3 vector = Quaternion.Euler(0f, num3, 0f) * Vector3.forward * num4;
		if (vector.IsNaN())
		{
			vector = Vector3.zero;
		}
		_localVelocity = vector;
	}

	private void DoRotateTowardsDesiredAngleTick(float target)
	{
		Transform transform = ((Component)(object)this).transform;
		float y = transform.rotation.eulerAngles.y;
		float num = rotationSmoothTime;
		if (Mathf.Abs(Mathf.DeltaAngle(y, target)) < 60f)
		{
			num *= 2f;
		}
		float num2 = Mathf.SmoothDampAngle(y, target, ref _desiredAngleCv, num);
		if (float.IsNaN(num2))
		{
			_desiredAngleCv = 0f;
		}
		else
		{
			transform.rotation = Quaternion.Euler(0f, num2, 0f);
		}
	}

	private void SetWalkStrength(float strength)
	{
		if (isLocalMovementProcessor)
		{
			if (((NetworkBehaviour)this).isServer)
			{
				Network_syncedWalkStrength = strength;
				_localWalkStrength = strength;
			}
			else
			{
				_localWalkStrength = strength;
				CmdSetWalkStrength(strength);
			}
		}
	}

	[Command]
	private void CmdSetWalkStrength(float strength)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteFloat((NetworkWriter)(object)val, strength);
		((NetworkBehaviour)this).SendCommandInternal("System.Void EntityControl::CmdSetWalkStrength(System.Single)", 1641380514, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Server]
	public void MoveToDestination(Vector3 destination, bool immediately, float speedMult = 1f)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void EntityControl::MoveToDestination(UnityEngine.Vector3,System.Boolean,System.Single)' called when server was not active");
			return;
		}
		speedMult = Mathf.Clamp01(speedMult);
		if (isLocalMovementProcessor)
		{
			Attack(null, doChase: false);
			MoveToDestination_Imp(destination, immediately, speedMult);
		}
		else
		{
			TpcMoveToDestination(destination, immediately, speedMult);
		}
	}

	[TargetRpc]
	private void TpcMoveToDestination(Vector3 destination, bool immediately, float speedMult)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteVector3((NetworkWriter)(object)val, destination);
		NetworkWriterExtensions.WriteBool((NetworkWriter)(object)val, immediately);
		NetworkWriterExtensions.WriteFloat((NetworkWriter)(object)val, speedMult);
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)null, "System.Void EntityControl::TpcMoveToDestination(UnityEngine.Vector3,System.Boolean,System.Single)", -95381029, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	public void CmdMoveToDestination(Vector3 destination, bool immediately, float speedMult = 1f)
	{
		if (HasAuthorityWithLog())
		{
			MoveToDestination_Imp(destination, immediately, speedMult);
			if ((UnityEngine.Object)(object)entity.Ability.attackAbility != null && entity.Ability.attackAbility.currentConfig.channel.duration > 0.0001f)
			{
				CmdAttack(null, doChase: false);
			}
		}
	}

	private void MoveToDestination_Imp(Vector3 destination, bool immediately, float speedMult)
	{
		if (IsActionBlocked(BlockableAction.Move) == BlockStatus.BlockedCancelable)
		{
			if (((NetworkBehaviour)this).isServer)
			{
				DisobeyBlock(BlockableAction.Move);
			}
			else
			{
				CmdDisobeyBlock(BlockableAction.Move);
			}
		}
		_destinationMovementSpeedMultiplier = speedMult;
		CmdSetMovementState(destination, isDirection: false);
		SetAgentDestination(destination, immediately);
		StopDoingMoveToAction();
		if (((NetworkBehaviour)this).isServer)
		{
			_isMoveToAttackActive = false;
		}
		else
		{
			CmdCancelMoveToAttack();
		}
	}

	public void CmdMoveWithDirection(Vector3 direction)
	{
		if (HasAuthorityWithLog())
		{
			if (IsActionBlocked(BlockableAction.Move) == BlockStatus.BlockedCancelable)
			{
				CmdDisobeyBlock(BlockableAction.Move);
			}
			CmdSetMovementState(direction, isDirection: true);
			StopDoingMoveToAction();
			CmdCancelMoveToAttack();
		}
	}

	[Server]
	public void ClearMovement()
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void EntityControl::ClearMovement()' called when server was not active");
		}
		else if (isLocalMovementProcessor)
		{
			SetMovementStateLocal(null, isDirection: false);
		}
		else
		{
			TpcSetMovementState(null, isDirection: false);
		}
	}

	public void CmdClearMovement()
	{
		if (HasAuthorityWithLog())
		{
			CmdSetMovementState(null, isDirection: false);
		}
	}

	public void SetAgentDestination(Vector3 destination, bool important)
	{
		if (isLocalMovementProcessor)
		{
			SetAgentDestinationLocal(destination, important);
		}
		else
		{
			CmdSetAgentDestination(destination, important);
		}
	}

	private void SetAgentDestinationLocal(Vector3 destination, bool important)
	{
		if (isLocalMovementProcessor && !destination.IsNaN())
		{
			_desiredAgentDestination = destination;
			if (important && _agent.isOnNavMesh)
			{
				_agent.path = Dew.GetNavMeshPath(_agent.nextPosition, destination);
			}
		}
	}

	[Command]
	private void CmdSetAgentDestination(Vector3 destination, bool important)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteVector3((NetworkWriter)(object)val, destination);
		NetworkWriterExtensions.WriteBool((NetworkWriter)(object)val, important);
		((NetworkBehaviour)this).SendCommandInternal("System.Void EntityControl::CmdSetAgentDestination(UnityEngine.Vector3,System.Boolean)", -2073988596, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void CmdSetMovementState(Vector3? vector, bool isDirection)
	{
		if (isLocalMovementProcessor)
		{
			SetMovementStateLocal(vector, isDirection);
		}
		else
		{
			CmdSetMovementState_Imp(vector, isDirection);
		}
	}

	[Command]
	private void CmdSetMovementState_Imp(Vector3? vector, bool isDirection)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteVector3Nullable((NetworkWriter)(object)val, vector);
		NetworkWriterExtensions.WriteBool((NetworkWriter)(object)val, isDirection);
		((NetworkBehaviour)this).SendCommandInternal("System.Void EntityControl::CmdSetMovementState_Imp(System.Nullable`1<UnityEngine.Vector3>,System.Boolean)", 477684443, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[TargetRpc]
	private void TpcSetMovementState(Vector3? vector, bool isDirection)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteVector3Nullable((NetworkWriter)(object)val, vector);
		NetworkWriterExtensions.WriteBool((NetworkWriter)(object)val, isDirection);
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)null, "System.Void EntityControl::TpcSetMovementState(System.Nullable`1<UnityEngine.Vector3>,System.Boolean)", 873017755, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	private void SetMovementStateLocal(Vector3? vector, bool isDirection)
	{
		_movementVector = vector;
		if ((!vector.HasValue | isDirection) && _desiredAgentDestination.HasValue)
		{
			_desiredAgentDestination = null;
		}
		_isMovementVectorDirection = isDirection;
		if (isDirection && _agent.hasPath)
		{
			_agent.ResetPath();
		}
	}

	private void OnMovementSyncDataReceived(PositionSyncData _, PositionSyncData __)
	{
		_positionSyncDataTime = Time.time;
		_positionSyncDataUnscaledTime = Time.unscaledTime;
	}

	private void DoMovementObserverFrameUpdate()
	{
		if (isDisplacing)
		{
			DoDisplacement();
			return;
		}
		if (_positionSyncData.timestamp <= _positionSyncDataTimestampLowerBound)
		{
			return;
		}
		DoRotateTowardsDesiredAngleTick(_positionSyncData.desiredAngle);
		if (entity.Visual.isSpawning)
		{
			return;
		}
		float positionSyncDataUnscaledElapsedTime = _positionSyncDataUnscaledElapsedTime;
		if (positionSyncDataUnscaledElapsedTime > SyncAgentVelocityLifetime)
		{
			_positionSyncData.velocity = default;
		}
		Transform transform = ((Component)(object)this).transform;
		bool flag = positionSyncDataUnscaledElapsedTime <= SyncFixSnapshotLifetime;
		if (_positionSyncData.velocity == default(Vector3) && _lastObserverGroundSampleTime > _positionSyncDataUnscaledTime && (!flag || Vector2.SqrMagnitude(transform.position.ToXY() - _positionSyncData.position.ToXY()) < 0.0001f))
		{
			_fixCv = default;
			return;
		}
		Vector3 vector = transform.position;
		vector += _positionSyncData.velocity * Time.deltaTime;
		if (NetworkServer.active)
		{
			vector = Dew.GetValidAgentDestination_LinearSweep(transform.position, vector);
		}
		if (flag)
		{
			Vector3 vector2 = _positionSyncData.position + _positionSyncData.velocity * (_positionSyncDataElapsedTime + _positionSyncDataExtrapolateTime * SyncExtrapolateStrength);
			vector = ((!(Vector2.Distance(vector.ToXY(), vector2.ToXY()) > SyncFixWarpDistance)) ? Vector3.SmoothDamp(vector, vector2, ref _fixCv, SyncFixSmoothTime) : vector2);
		}
		int num;
		if (!(Time.unscaledTime - _lastObserverGroundSampleTime >= 1f / 30f))
		{
			num = ((Vector2.SqrMagnitude(vector.ToXY() - _lastObserverGroundSampleXZ) > 1f) ? 1 : 0);
			if (num == 0)
			{
				goto IL_01fe;
			}
		}
		else
		{
			num = 1;
		}
		_lastObserverGroundSampleTime = Time.unscaledTime;
		_lastObserverGroundSampleXZ = vector.ToXY();
		if ((UnityEngine.Object)(object)_agent != null && _agent.isOnNavMesh)
		{
			vector.y = NavSnapYCached(vector);
		}
		else
		{
			vector = GroundSnapCached(vector);
		}
		goto IL_01fe;
		IL_01fe:
		transform.position = vector;
		if (num != 0 && (UnityEngine.Object)(object)_agent != null)
		{
			_agent.Warp(vector);
		}
	}

	private bool HasMeaningfulSyncDataChange(in PositionSyncData data)
	{
		if (!(data.timestamp - _positionSyncData.timestamp >= 1.0) && !((data.position - _positionSyncData.position).sqrMagnitude > 0.0001f) && !((data.velocity - _positionSyncData.velocity).sqrMagnitude > 0.0001f))
		{
			return Mathf.Abs(Mathf.DeltaAngle(data.desiredAngle, _positionSyncData.desiredAngle)) > 0.1f;
		}
		return true;
	}

	private void UpdatePositionSyncData(PositionSyncData data)
	{
		if (NetworkServer.isLoadingScene || (NetworkClient.active && !NetworkClient.ready))
		{
			return;
		}
		if (isClientSideMovement)
		{
			if (HasMeaningfulSyncDataChange(in data) && Time.unscaledTime - _lastPositionSyncDataSendTime >= 1f / 60f)
			{
				_lastPositionSyncDataSendTime = Time.unscaledTime;
				CmdPositionSyncData(data);
			}
		}
		else if (!((NetworkBehaviour)this).isServer)
		{
			UnityEngine.Debug.LogWarning("Tried to set sync data without authority");
		}
		else if (!(Time.unscaledTime - _lastPositionSyncDataSendTime < 1f / 60f))
		{
			_lastPositionSyncDataSendTime = Time.unscaledTime;
			Dew.FilterNonOkayValues(ref data.position, ((Component)(object)this).transform.position);
			Dew.FilterNonOkayValues(ref data.velocity);
			Dew.FilterNonOkayValues(ref data.desiredAngle);
			if (HasMeaningfulSyncDataChange(in data))
			{
				Network_positionSyncData = data;
			}
		}
	}

	[Command]
	private void CmdPositionSyncData(PositionSyncData data)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_EntityControl_002FPositionSyncData((NetworkWriter)(object)val, data);
		((NetworkBehaviour)this).SendCommandInternal("System.Void EntityControl::CmdPositionSyncData(EntityControl/PositionSyncData)", -1630240464, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void DoSyncStart()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			Network_positionSyncData = new PositionSyncData
			{
				timestamp = NetworkTime.time,
				desiredAngle = CastInfo.GetAngle(((Component)(object)this).transform.forward),
				position = entity.position,
				velocity = Vector3.zero
			};
		}
		ClientEvent_OnTeleport += (Action<Vector3, Vector3>)((Vector3 _, Vector3 _) =>
		{
			_positionSyncDataTimestampLowerBound = NetworkTime.time;
		});
		ClientEvent_OnDisplacementFinished += (Action<Displacement>)((Displacement _) =>
		{
			_positionSyncDataTimestampLowerBound = NetworkTime.time;
		});
		ClientEvent_OnDisplacementCanceled += (Action<Displacement>)((Displacement _) =>
		{
			_positionSyncDataTimestampLowerBound = NetworkTime.time;
		});
	}

	public EntityControl()
	{
		_Mirror_SyncVarHookDelegate__outerRadius = OuterRadiusChanged;
		_Mirror_SyncVarHookDelegate__innerRadius = InnerRadiusChanged;
		_Mirror_SyncVarHookDelegate__positionSyncData = OnMovementSyncDataReceived;
	}

	static EntityControl()
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected Obj, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected Obj, but got Unknown
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Expected Obj, but got Unknown
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Expected Obj, but got Unknown
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Expected Obj, but got Unknown
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Expected Obj, but got Unknown
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Expected Obj, but got Unknown
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Expected Obj, but got Unknown
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Expected Obj, but got Unknown
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Expected Obj, but got Unknown
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Expected Obj, but got Unknown
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Expected Obj, but got Unknown
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Expected Obj, but got Unknown
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Expected Obj, but got Unknown
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Expected Obj, but got Unknown
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Expected Obj, but got Unknown
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Expected Obj, but got Unknown
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Expected Obj, but got Unknown
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Expected Obj, but got Unknown
		//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Expected Obj, but got Unknown
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Expected Obj, but got Unknown
		//IL_030d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Expected Obj, but got Unknown
		//IL_032d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0337: Expected Obj, but got Unknown
		//IL_034d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0357: Expected Obj, but got Unknown
		//IL_036d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0377: Expected Obj, but got Unknown
		//IL_038d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0397: Expected Obj, but got Unknown
		s_LayerStack = new List<Transform>(64);
		SyncFixWarpDistance = 5f;
		SyncFixSmoothTime = 0.1f;
		SyncFixSnapshotLifetime = 0.5f;
		SyncAgentVelocityLifetime = 1f;
		SyncExtrapolateMaxTime = 0.15f;
		SyncExtrapolateStrength = 1f;
		RemoteProcedureCalls.RegisterCommand(typeof(EntityControl), "System.Void EntityControl::CmdCancelOngoingChannels()", (RemoteCallDelegate)InvokeUserCode_CmdCancelOngoingChannels, true);
		RemoteProcedureCalls.RegisterCommand(typeof(EntityControl), "System.Void EntityControl::CmdLookInDirection(UnityEngine.Vector3)", (RemoteCallDelegate)InvokeUserCode_CmdLookInDirection__Vector3, true);
		RemoteProcedureCalls.RegisterCommand(typeof(EntityControl), "System.Void EntityControl::CmdCast(AbilityTrigger,System.Int32,CastInfo,System.Boolean,System.Boolean)", (RemoteCallDelegate)InvokeUserCode_CmdCast__AbilityTrigger__Int32__CastInfo__Boolean__Boolean, true);
		RemoteProcedureCalls.RegisterCommand(typeof(EntityControl), "System.Void EntityControl::CmdStopDoingMoveToAction()", (RemoteCallDelegate)InvokeUserCode_CmdStopDoingMoveToAction, true);
		RemoteProcedureCalls.RegisterCommand(typeof(EntityControl), "System.Void EntityControl::CmdAttack(Entity,System.Boolean)", (RemoteCallDelegate)InvokeUserCode_CmdAttack__Entity__Boolean, true);
		RemoteProcedureCalls.RegisterCommand(typeof(EntityControl), "System.Void EntityControl::CmdAttackMove(UnityEngine.Vector3,System.Boolean)", (RemoteCallDelegate)InvokeUserCode_CmdAttackMove__Vector3__Boolean, true);
		RemoteProcedureCalls.RegisterCommand(typeof(EntityControl), "System.Void EntityControl::CmdCancelMoveToAttack()", (RemoteCallDelegate)InvokeUserCode_CmdCancelMoveToAttack, true);
		RemoteProcedureCalls.RegisterCommand(typeof(EntityControl), "System.Void EntityControl::CmdDisobeyBlock(EntityControl/BlockableAction)", (RemoteCallDelegate)InvokeUserCode_CmdDisobeyBlock__BlockableAction, true);
		RemoteProcedureCalls.RegisterCommand(typeof(EntityControl), "System.Void EntityControl::CmdClearActionQueue()", (RemoteCallDelegate)InvokeUserCode_CmdClearActionQueue, true);
		RemoteProcedureCalls.RegisterCommand(typeof(EntityControl), "System.Void EntityControl::CmdStopMovementProcessor()", (RemoteCallDelegate)InvokeUserCode_CmdStopMovementProcessor, true);
		RemoteProcedureCalls.RegisterCommand(typeof(EntityControl), "System.Void EntityControl::CmdInteract(IInteractable,System.Boolean,System.Boolean)", (RemoteCallDelegate)InvokeUserCode_CmdInteract__IInteractable__Boolean__Boolean, true);
		RemoteProcedureCalls.RegisterCommand(typeof(EntityControl), "System.Void EntityControl::CmdSetWalkStrength(System.Single)", (RemoteCallDelegate)InvokeUserCode_CmdSetWalkStrength__Single, true);
		RemoteProcedureCalls.RegisterCommand(typeof(EntityControl), "System.Void EntityControl::CmdSetAgentDestination(UnityEngine.Vector3,System.Boolean)", (RemoteCallDelegate)InvokeUserCode_CmdSetAgentDestination__Vector3__Boolean, true);
		RemoteProcedureCalls.RegisterCommand(typeof(EntityControl), "System.Void EntityControl::CmdSetMovementState_Imp(System.Nullable`1<UnityEngine.Vector3>,System.Boolean)", (RemoteCallDelegate)InvokeUserCode_CmdSetMovementState_Imp__Nullable_00601__Boolean, true);
		RemoteProcedureCalls.RegisterCommand(typeof(EntityControl), "System.Void EntityControl::CmdPositionSyncData(EntityControl/PositionSyncData)", (RemoteCallDelegate)InvokeUserCode_CmdPositionSyncData__PositionSyncData, true);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityControl), "System.Void EntityControl::RpcOnDeath()", (RemoteCallDelegate)InvokeUserCode_RpcOnDeath);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityControl), "System.Void EntityControl::RpcStartDisplacement(Displacement)", (RemoteCallDelegate)InvokeUserCode_RpcStartDisplacement__Displacement);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityControl), "System.Void EntityControl::RpcCancelOngoingDisplacement()", (RemoteCallDelegate)InvokeUserCode_RpcCancelOngoingDisplacement);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityControl), "System.Void EntityControl::RpcRotateImmediately(System.Single)", (RemoteCallDelegate)InvokeUserCode_RpcRotateImmediately__Single);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityControl), "System.Void EntityControl::RpcTeleport(UnityEngine.Vector3)", (RemoteCallDelegate)InvokeUserCode_RpcTeleport__Vector3);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityControl), "System.Void EntityControl::TpcSetDesiredAngle(System.Single)", (RemoteCallDelegate)InvokeUserCode_TpcSetDesiredAngle__Single);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityControl), "System.Void EntityControl::TpcSetMoveToActionState(System.Boolean)", (RemoteCallDelegate)InvokeUserCode_TpcSetMoveToActionState__Boolean);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityControl), "System.Void EntityControl::TpcSetAttackMovSpdDisadvantage(System.Single)", (RemoteCallDelegate)InvokeUserCode_TpcSetAttackMovSpdDisadvantage__Single);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityControl), "System.Void EntityControl::TpcStopMovementProcessor()", (RemoteCallDelegate)InvokeUserCode_TpcStopMovementProcessor);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityControl), "System.Void EntityControl::TpcMoveToDestination(UnityEngine.Vector3,System.Boolean,System.Single)", (RemoteCallDelegate)InvokeUserCode_TpcMoveToDestination__Vector3__Boolean__Single);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityControl), "System.Void EntityControl::TpcSetMovementState(System.Nullable`1<UnityEngine.Vector3>,System.Boolean)", (RemoteCallDelegate)InvokeUserCode_TpcSetMovementState__Nullable_00601__Boolean);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcOnDeath()
	{
		Collider component = ((Component)(object)this).GetComponent<Collider>();
		if ((UnityEngine.Object)(object)component != null)
		{
			component.enabled = false;
		}
	}

	protected static void InvokeUserCode_RpcOnDeath(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("RPC RpcOnDeath called on server.");
		}
		else
		{
			((EntityControl)(object)obj).UserCode_RpcOnDeath();
		}
	}

	protected void UserCode_RpcStartDisplacement__Displacement(Displacement disp)
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			StartDisplacementLocal(disp);
		}
	}

	protected static void InvokeUserCode_RpcStartDisplacement__Displacement(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("RPC RpcStartDisplacement called on server.");
		}
		else
		{
			((EntityControl)(object)obj).UserCode_RpcStartDisplacement__Displacement(reader.ReadDisplacement());
		}
	}

	protected void UserCode_RpcCancelOngoingDisplacement()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			CancelOngoingDisplacementLocal();
		}
	}

	protected static void InvokeUserCode_RpcCancelOngoingDisplacement(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("RPC RpcCancelOngoingDisplacement called on server.");
		}
		else
		{
			((EntityControl)(object)obj).UserCode_RpcCancelOngoingDisplacement();
		}
	}

	protected void UserCode_CmdCancelOngoingChannels()
	{
		CancelOngoingChannels();
	}

	protected static void InvokeUserCode_CmdCancelOngoingChannels(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command CmdCancelOngoingChannels called on client.");
		}
		else
		{
			((EntityControl)(object)obj).UserCode_CmdCancelOngoingChannels();
		}
	}

	protected void UserCode_TpcSetDesiredAngle__Single(float angle)
	{
		_localDesiredAngle = angle;
	}

	protected static void InvokeUserCode_TpcSetDesiredAngle__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("TargetRPC TpcSetDesiredAngle called on server.");
		}
		else
		{
			((EntityControl)(object)obj).UserCode_TpcSetDesiredAngle__Single(NetworkReaderExtensions.ReadFloat(reader));
		}
	}

	protected void UserCode_RpcRotateImmediately__Single(float angle)
	{
		((Component)(object)this).transform.rotation = Quaternion.Euler(0f, angle, 0f);
	}

	protected static void InvokeUserCode_RpcRotateImmediately__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("RPC RpcRotateImmediately called on server.");
		}
		else
		{
			((EntityControl)(object)obj).UserCode_RpcRotateImmediately__Single(NetworkReaderExtensions.ReadFloat(reader));
		}
	}

	protected void UserCode_RpcTeleport__Vector3(Vector3 position)
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			TeleportLocal(position);
		}
	}

	protected static void InvokeUserCode_RpcTeleport__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("RPC RpcTeleport called on server.");
		}
		else
		{
			((EntityControl)(object)obj).UserCode_RpcTeleport__Vector3(NetworkReaderExtensions.ReadVector3(reader));
		}
	}

	protected void UserCode_CmdLookInDirection__Vector3(Vector3 dir)
	{
		if (isGamepadRotationLocked || entity.IsNullInactiveDeadOrKnockedOut() || entity.Control.IsActionBlocked(BlockableAction.Move) != BlockStatus.Allowed || entity.Status.hasStun)
		{
			return;
		}
		foreach (Channel ongoingChannel in entity.Control.ongoingChannels)
		{
			if (ongoingChannel.isAttack)
			{
				return;
			}
		}
		Rotate(dir, immediately: false, 0.4f);
	}

	protected static void InvokeUserCode_CmdLookInDirection__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command CmdLookInDirection called on client.");
		}
		else
		{
			((EntityControl)(object)obj).UserCode_CmdLookInDirection__Vector3(NetworkReaderExtensions.ReadVector3(reader));
		}
	}

	protected void UserCode_TpcSetMoveToActionState__Boolean(bool state)
	{
		SetMoveToActionStateLocal(state);
	}

	protected static void InvokeUserCode_TpcSetMoveToActionState__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("TargetRPC TpcSetMoveToActionState called on server.");
		}
		else
		{
			((EntityControl)(object)obj).UserCode_TpcSetMoveToActionState__Boolean(NetworkReaderExtensions.ReadBool(reader));
		}
	}

	protected void UserCode_CmdCast__AbilityTrigger__Int32__CastInfo__Boolean__Boolean(AbilityTrigger trigger, int configIndex, CastInfo info, bool allowMoveToCast, bool skipRangeCheck)
	{
		Cast(trigger, configIndex, info, allowMoveToCast, skipRangeCheck);
	}

	protected static void InvokeUserCode_CmdCast__AbilityTrigger__Int32__CastInfo__Boolean__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command CmdCast called on client.");
		}
		else
		{
			((EntityControl)(object)obj).UserCode_CmdCast__AbilityTrigger__Int32__CastInfo__Boolean__Boolean(NetworkReaderExtensions.ReadNetworkBehaviour<AbilityTrigger>(reader), NetworkReaderExtensions.ReadInt(reader), GeneratedNetworkCode._Read_CastInfo(reader), NetworkReaderExtensions.ReadBool(reader), NetworkReaderExtensions.ReadBool(reader));
		}
	}

	protected void UserCode_CmdStopDoingMoveToAction()
	{
		isDoingMoveToAction = false;
	}

	protected static void InvokeUserCode_CmdStopDoingMoveToAction(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command CmdStopDoingMoveToAction called on client.");
		}
		else
		{
			((EntityControl)(object)obj).UserCode_CmdStopDoingMoveToAction();
		}
	}

	protected void UserCode_CmdAttack__Entity__Boolean(Entity target, bool doChase)
	{
		Attack(target, doChase);
	}

	protected static void InvokeUserCode_CmdAttack__Entity__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command CmdAttack called on client.");
		}
		else
		{
			((EntityControl)(object)obj).UserCode_CmdAttack__Entity__Boolean(NetworkReaderExtensions.ReadNetworkBehaviour<Entity>(reader), NetworkReaderExtensions.ReadBool(reader));
		}
	}

	protected void UserCode_CmdAttackMove__Vector3__Boolean(Vector3 destination, bool useDistanceFromDestination)
	{
		AttackMove(destination, useDistanceFromDestination);
	}

	protected static void InvokeUserCode_CmdAttackMove__Vector3__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command CmdAttackMove called on client.");
		}
		else
		{
			((EntityControl)(object)obj).UserCode_CmdAttackMove__Vector3__Boolean(NetworkReaderExtensions.ReadVector3(reader), NetworkReaderExtensions.ReadBool(reader));
		}
	}

	protected void UserCode_CmdCancelMoveToAttack()
	{
		_isMoveToAttackActive = false;
	}

	protected static void InvokeUserCode_CmdCancelMoveToAttack(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command CmdCancelMoveToAttack called on client.");
		}
		else
		{
			((EntityControl)(object)obj).UserCode_CmdCancelMoveToAttack();
		}
	}

	protected void UserCode_TpcSetAttackMovSpdDisadvantage__Single(float channelDuration)
	{
		SetAttackMovSpdDisadvantage_Imp(channelDuration);
	}

	protected static void InvokeUserCode_TpcSetAttackMovSpdDisadvantage__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("TargetRPC TpcSetAttackMovSpdDisadvantage called on server.");
		}
		else
		{
			((EntityControl)(object)obj).UserCode_TpcSetAttackMovSpdDisadvantage__Single(NetworkReaderExtensions.ReadFloat(reader));
		}
	}

	protected void UserCode_CmdDisobeyBlock__BlockableAction(BlockableAction action)
	{
		if (IsActionBlocked(action) == BlockStatus.BlockedCancelable)
		{
			DisobeyBlock(action);
		}
	}

	protected static void InvokeUserCode_CmdDisobeyBlock__BlockableAction(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command CmdDisobeyBlock called on client.");
		}
		else
		{
			((EntityControl)(object)obj).UserCode_CmdDisobeyBlock__BlockableAction(GeneratedNetworkCode._Read_EntityControl_002FBlockableAction(reader));
		}
	}

	protected void UserCode_CmdClearActionQueue()
	{
		ClearActionQueue();
	}

	protected static void InvokeUserCode_CmdClearActionQueue(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command CmdClearActionQueue called on client.");
		}
		else
		{
			((EntityControl)(object)obj).UserCode_CmdClearActionQueue();
		}
	}

	protected void UserCode_TpcStopMovementProcessor()
	{
		StopMovementProcessor();
	}

	protected static void InvokeUserCode_TpcStopMovementProcessor(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("TargetRPC TpcStopMovementProcessor called on server.");
		}
		else
		{
			((EntityControl)(object)obj).UserCode_TpcStopMovementProcessor();
		}
	}

	protected void UserCode_CmdStopMovementProcessor()
	{
		StopMovementProcessor();
	}

	protected static void InvokeUserCode_CmdStopMovementProcessor(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command CmdStopMovementProcessor called on client.");
		}
		else
		{
			((EntityControl)(object)obj).UserCode_CmdStopMovementProcessor();
		}
	}

	protected void UserCode_CmdInteract__IInteractable__Boolean__Boolean(IInteractable interactable, bool isAlt, bool isMouse)
	{
		Interact(interactable, isAlt, isMouse);
	}

	protected static void InvokeUserCode_CmdInteract__IInteractable__Boolean__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command CmdInteract called on client.");
		}
		else
		{
			((EntityControl)(object)obj).UserCode_CmdInteract__IInteractable__Boolean__Boolean(reader.ReadIInteractable(), NetworkReaderExtensions.ReadBool(reader), NetworkReaderExtensions.ReadBool(reader));
		}
	}

	protected void UserCode_CmdSetWalkStrength__Single(float strength)
	{
		Network_syncedWalkStrength = strength;
	}

	protected static void InvokeUserCode_CmdSetWalkStrength__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command CmdSetWalkStrength called on client.");
		}
		else
		{
			((EntityControl)(object)obj).UserCode_CmdSetWalkStrength__Single(NetworkReaderExtensions.ReadFloat(reader));
		}
	}

	protected void UserCode_TpcMoveToDestination__Vector3__Boolean__Single(Vector3 destination, bool immediately, float speedMult)
	{
		CmdMoveToDestination(destination, immediately, speedMult);
	}

	protected static void InvokeUserCode_TpcMoveToDestination__Vector3__Boolean__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("TargetRPC TpcMoveToDestination called on server.");
		}
		else
		{
			((EntityControl)(object)obj).UserCode_TpcMoveToDestination__Vector3__Boolean__Single(NetworkReaderExtensions.ReadVector3(reader), NetworkReaderExtensions.ReadBool(reader), NetworkReaderExtensions.ReadFloat(reader));
		}
	}

	protected void UserCode_CmdSetAgentDestination__Vector3__Boolean(Vector3 destination, bool important)
	{
		SetAgentDestinationLocal(destination, important);
	}

	protected static void InvokeUserCode_CmdSetAgentDestination__Vector3__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command CmdSetAgentDestination called on client.");
		}
		else
		{
			((EntityControl)(object)obj).UserCode_CmdSetAgentDestination__Vector3__Boolean(NetworkReaderExtensions.ReadVector3(reader), NetworkReaderExtensions.ReadBool(reader));
		}
	}

	protected void UserCode_CmdSetMovementState_Imp__Nullable_00601__Boolean(Vector3? vector, bool isDirection)
	{
		SetMovementStateLocal(vector, isDirection);
	}

	protected static void InvokeUserCode_CmdSetMovementState_Imp__Nullable_00601__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command CmdSetMovementState_Imp called on client.");
		}
		else
		{
			((EntityControl)(object)obj).UserCode_CmdSetMovementState_Imp__Nullable_00601__Boolean(NetworkReaderExtensions.ReadVector3Nullable(reader), NetworkReaderExtensions.ReadBool(reader));
		}
	}

	protected void UserCode_TpcSetMovementState__Nullable_00601__Boolean(Vector3? vector, bool isDirection)
	{
		SetMovementStateLocal(vector, isDirection);
	}

	protected static void InvokeUserCode_TpcSetMovementState__Nullable_00601__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("TargetRPC TpcSetMovementState called on server.");
		}
		else
		{
			((EntityControl)(object)obj).UserCode_TpcSetMovementState__Nullable_00601__Boolean(NetworkReaderExtensions.ReadVector3Nullable(reader), NetworkReaderExtensions.ReadBool(reader));
		}
	}

	protected void UserCode_CmdPositionSyncData__PositionSyncData(PositionSyncData data)
	{
		if (isClientSideMovement && Dew.IsOkay(data.timestamp))
		{
			Dew.FilterNonOkayValues(ref data.position, ((Component)(object)this).transform.position);
			Dew.FilterNonOkayValues(ref data.velocity);
			Dew.FilterNonOkayValues(ref data.desiredAngle);
			if (HasMeaningfulSyncDataChange(in data))
			{
				Network_positionSyncData = data;
			}
		}
	}

	protected static void InvokeUserCode_CmdPositionSyncData__PositionSyncData(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command CmdPositionSyncData called on client.");
		}
		else
		{
			((EntityControl)(object)obj).UserCode_CmdPositionSyncData__PositionSyncData(GeneratedNetworkCode._Read_EntityControl_002FPositionSyncData(reader));
		}
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		((NetworkBehaviour)this).SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteFloat(writer, _outerRadius);
			NetworkWriterExtensions.WriteFloat(writer, _innerRadius);
			NetworkWriterExtensions.WriteBool(writer, _freeMovement);
			writer.WriteNullableFloat(overridenDesiredAngle__BackingField);
			NetworkWriterExtensions.WriteBool(writer, isControlReversed__BackingField);
			NetworkWriterExtensions.WriteInt(writer, _gamepadRotationLockCounter);
			NetworkWriterExtensions.WriteVector3(writer, _moveToActionDestination);
			NetworkWriterExtensions.WriteFloat(writer, _moveToActionRequiredDistance);
			NetworkWriterExtensions.WriteByte(writer, _blockMove);
			NetworkWriterExtensions.WriteByte(writer, _blockAbility);
			NetworkWriterExtensions.WriteByte(writer, _blockAttack);
			NetworkWriterExtensions.WriteByte(writer, _blockDodge);
			NetworkWriterExtensions.WriteByte(writer, _blockMoveCancelable);
			NetworkWriterExtensions.WriteByte(writer, _blockAbilityCancelable);
			NetworkWriterExtensions.WriteByte(writer, _blockAttackCancelable);
			NetworkWriterExtensions.WriteByte(writer, _blockDodgeCancelable);
			NetworkWriterExtensions.WriteBool(writer, forceWalking__BackingField);
			NetworkWriterExtensions.WriteFloat(writer, _syncedWalkStrength);
			GeneratedNetworkCode._Write_EntityControl_002FPositionSyncData(writer, _positionSyncData);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 1L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _outerRadius);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 2L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _innerRadius);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 4L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, _freeMovement);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 8L) != 0L)
		{
			writer.WriteNullableFloat(overridenDesiredAngle__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x10L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, isControlReversed__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x20L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, _gamepadRotationLockCounter);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteVector3(writer, _moveToActionDestination);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _moveToActionRequiredDistance);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100L) != 0L)
		{
			NetworkWriterExtensions.WriteByte(writer, _blockMove);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x200L) != 0L)
		{
			NetworkWriterExtensions.WriteByte(writer, _blockAbility);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x400L) != 0L)
		{
			NetworkWriterExtensions.WriteByte(writer, _blockAttack);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x800L) != 0L)
		{
			NetworkWriterExtensions.WriteByte(writer, _blockDodge);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x1000L) != 0L)
		{
			NetworkWriterExtensions.WriteByte(writer, _blockMoveCancelable);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x2000L) != 0L)
		{
			NetworkWriterExtensions.WriteByte(writer, _blockAbilityCancelable);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x4000L) != 0L)
		{
			NetworkWriterExtensions.WriteByte(writer, _blockAttackCancelable);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x8000L) != 0L)
		{
			NetworkWriterExtensions.WriteByte(writer, _blockDodgeCancelable);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x10000L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, forceWalking__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x20000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _syncedWalkStrength);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40000L) != 0L)
		{
			GeneratedNetworkCode._Write_EntityControl_002FPositionSyncData(writer, _positionSyncData);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		((NetworkBehaviour)this).DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _outerRadius, _Mirror_SyncVarHookDelegate__outerRadius, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _innerRadius, _Mirror_SyncVarHookDelegate__innerRadius, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _freeMovement, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float?>(ref overridenDesiredAngle__BackingField, (Action<float?, float?>)null, reader.ReadNullableFloat());
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isControlReversed__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref _gamepadRotationLockCounter, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Vector3>(ref _moveToActionDestination, (Action<Vector3, Vector3>)null, NetworkReaderExtensions.ReadVector3(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _moveToActionRequiredDistance, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<byte>(ref _blockMove, (Action<byte, byte>)null, NetworkReaderExtensions.ReadByte(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<byte>(ref _blockAbility, (Action<byte, byte>)null, NetworkReaderExtensions.ReadByte(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<byte>(ref _blockAttack, (Action<byte, byte>)null, NetworkReaderExtensions.ReadByte(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<byte>(ref _blockDodge, (Action<byte, byte>)null, NetworkReaderExtensions.ReadByte(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<byte>(ref _blockMoveCancelable, (Action<byte, byte>)null, NetworkReaderExtensions.ReadByte(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<byte>(ref _blockAbilityCancelable, (Action<byte, byte>)null, NetworkReaderExtensions.ReadByte(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<byte>(ref _blockAttackCancelable, (Action<byte, byte>)null, NetworkReaderExtensions.ReadByte(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<byte>(ref _blockDodgeCancelable, (Action<byte, byte>)null, NetworkReaderExtensions.ReadByte(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref forceWalking__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _syncedWalkStrength, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<PositionSyncData>(ref _positionSyncData, _Mirror_SyncVarHookDelegate__positionSyncData, GeneratedNetworkCode._Read_EntityControl_002FPositionSyncData(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 1L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _outerRadius, _Mirror_SyncVarHookDelegate__outerRadius, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 2L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _innerRadius, _Mirror_SyncVarHookDelegate__innerRadius, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 4L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _freeMovement, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 8L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float?>(ref overridenDesiredAngle__BackingField, (Action<float?, float?>)null, reader.ReadNullableFloat());
		}
		if ((num & 0x10L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isControlReversed__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x20L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref _gamepadRotationLockCounter, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Vector3>(ref _moveToActionDestination, (Action<Vector3, Vector3>)null, NetworkReaderExtensions.ReadVector3(reader));
		}
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _moveToActionRequiredDistance, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x100L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<byte>(ref _blockMove, (Action<byte, byte>)null, NetworkReaderExtensions.ReadByte(reader));
		}
		if ((num & 0x200L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<byte>(ref _blockAbility, (Action<byte, byte>)null, NetworkReaderExtensions.ReadByte(reader));
		}
		if ((num & 0x400L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<byte>(ref _blockAttack, (Action<byte, byte>)null, NetworkReaderExtensions.ReadByte(reader));
		}
		if ((num & 0x800L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<byte>(ref _blockDodge, (Action<byte, byte>)null, NetworkReaderExtensions.ReadByte(reader));
		}
		if ((num & 0x1000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<byte>(ref _blockMoveCancelable, (Action<byte, byte>)null, NetworkReaderExtensions.ReadByte(reader));
		}
		if ((num & 0x2000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<byte>(ref _blockAbilityCancelable, (Action<byte, byte>)null, NetworkReaderExtensions.ReadByte(reader));
		}
		if ((num & 0x4000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<byte>(ref _blockAttackCancelable, (Action<byte, byte>)null, NetworkReaderExtensions.ReadByte(reader));
		}
		if ((num & 0x8000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<byte>(ref _blockDodgeCancelable, (Action<byte, byte>)null, NetworkReaderExtensions.ReadByte(reader));
		}
		if ((num & 0x10000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref forceWalking__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x20000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _syncedWalkStrength, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x40000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<PositionSyncData>(ref _positionSyncData, _Mirror_SyncVarHookDelegate__positionSyncData, GeneratedNetworkCode._Read_EntityControl_002FPositionSyncData(reader));
		}
	}
}
