using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class Projectile : AbilityInstance
{
	public enum ProjectileMode
	{
		Point,
		Direction,
		Target
	}

	public enum StartPositionType
	{
		Base,
		Center,
		Muzzle,
		Custom
	}

	public struct EntityHit
	{
		public Entity entity;

		public Vector3 point;
	}

	public enum RotationType
	{
		None,
		Velocity,
		Target
	}

	public enum AddEffectTarget
	{
		Fly,
		Entity,
		Complete,
		Dissipate
	}

	private const float GoalDistanceToTargetWorldPosition = 0.1f;

	public SafeAction<EntityHit> onEntity;

	[SyncVar]
	public ProjectileMode mode;

	[SyncVar]
	public float endDistance = 8f;

	[SyncVar]
	public StartPositionType start = StartPositionType.Center;

	[SyncVar]
	public float startInFrontDistance = 0.5f;

	public bool endsWithSameHeight = true;

	public float customEndHeight;

	public RotationType rotationType = RotationType.Velocity;

	[FormerlySerializedAs("killOnComplete")]
	public bool destroyOnComplete = true;

	public bool killOnDissipate = true;

	public AnimationCurve verticalOffsetOverTrajectory = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 0f), new Keyframe(1f, 0f));

	public bool scaleVerticalOffsetOnLength;

	public AnimationCurve horizontalOffsetOverTrajectory = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 0f), new Keyframe(1f, 0f));

	public bool scaleHorizontalOffsetOnLength;

	public float collisionRadius = 0.25f;

	public bool canCollideMidFlight;

	public bool dissipateOnTerrain;

	public AbilityTargetValidator collisionTargets;

	public GameObject effectOnFly;

	public GameObject effectOnEntity;

	public GameObject effectOnComplete;

	public bool preserveCompleteEffectPositionAndRotation;

	public GameObject effectOnDissipate;

	[SyncVar]
	private Entity _targetEntity;

	private Collider _targetEntityCollider;

	private HashSet<Entity> _collidedEntities = new HashSet<Entity>();

	private HashSet<ICollidableWithProjectile> _collidedCollidables = new HashSet<ICollidableWithProjectile>();

	private Vector3 _lastCollisionCheckAnythingPosition;

	private Vector3 _lastCollisionCheckTargetEntityPosition;

	[SyncVar]
	private bool _isCompleted;

	[SyncVar]
	private bool _isFlying;

	[SyncVar]
	private bool _entityMode;

	[SyncVar]
	private Vector3 _targetPosition;

	[SyncVar]
	private Vector3 _startPosition;

	private float _trajectoryLength;

	protected bool _hasTrajectoryOffset;

	protected Vector3 _estimatedVelocity;

	private Vector3? _lastKnownTargetPosition;

	private Vector3 _ipEndPos;

	private Quaternion _ipEndRot;

	private float _ipRemainingTime;

	private int _startFrame;

	private static readonly RaycastHit2D[] _raycastHit2DResults;

	private static readonly Comparer<RaycastHit2D> _raycastHit2DComparer;

	private List<GameObject> _runtimeAddedEffects;

	private bool _capturedOrigEffectFields;

	private GameObject _origEffectOnFly;

	private GameObject _origEffectOnEntity;

	private GameObject _origEffectOnComplete;

	private GameObject _origEffectOnDissipate;

	protected NetworkBehaviourSyncVar ____targetEntityNetId;

	protected virtual bool destroyOnEntityHit => false;

	protected Entity targetEntity => Network_targetEntity;

	public bool isCompleted => _isCompleted;

	public bool isFlying => _isFlying;

	protected Vector3 targetPosition
	{
		get
		{
			return _targetPosition;
		}
		set
		{
			Network_targetPosition = value;
		}
	}

	protected virtual bool detachEntityHitEffect => destroyOnEntityHit;

	public ProjectileMode Networkmode
	{
		get
		{
			return mode;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<ProjectileMode>(value, ref mode, 64uL, (Action<ProjectileMode, ProjectileMode>)null);
		}
	}

	public float NetworkendDistance
	{
		get
		{
			return endDistance;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref endDistance, 128uL, (Action<float, float>)null);
		}
	}

	public StartPositionType Networkstart
	{
		get
		{
			return start;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<StartPositionType>(value, ref start, 256uL, (Action<StartPositionType, StartPositionType>)null);
		}
	}

	public float NetworkstartInFrontDistance
	{
		get
		{
			return startInFrontDistance;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref startInFrontDistance, 512uL, (Action<float, float>)null);
		}
	}

	public Entity Network_targetEntity
	{
		get
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return ((NetworkBehaviour)this).GetSyncVarNetworkBehaviour<Entity>(____targetEntityNetId, ref _targetEntity);
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter_NetworkBehaviour<Entity>(value, ref _targetEntity, 1024uL, (Action<Entity, Entity>)null, ref ____targetEntityNetId);
		}
	}

	public bool Network_isCompleted
	{
		get
		{
			return _isCompleted;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref _isCompleted, 2048uL, (Action<bool, bool>)null);
		}
	}

	public bool Network_isFlying
	{
		get
		{
			return _isFlying;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref _isFlying, 4096uL, (Action<bool, bool>)null);
		}
	}

	public bool Network_entityMode
	{
		get
		{
			return _entityMode;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref _entityMode, 8192uL, (Action<bool, bool>)null);
		}
	}

	public Vector3 Network_targetPosition
	{
		get
		{
			return _targetPosition;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<Vector3>(value, ref _targetPosition, 16384uL, (Action<Vector3, Vector3>)null);
		}
	}

	public Vector3 Network_startPosition
	{
		get
		{
			return _startPosition;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<Vector3>(value, ref _startPosition, 32768uL, (Action<Vector3, Vector3>)null);
		}
	}

	public override void ClearPooledEventsAndProcessors()
	{
		base.ClearPooledEventsAndProcessors();
		onEntity?.Clear();
	}

	protected abstract Vector3 PositionSolver(float dt);

	protected virtual Quaternion RotationSolver(float dt)
	{
		switch (rotationType)
		{
		case RotationType.None:
			return ((Component)(object)this).transform.rotation;
		case RotationType.Velocity:
			if (_estimatedVelocity.sqrMagnitude > 0.001f)
			{
				return Quaternion.LookRotation(_estimatedVelocity);
			}
			return ((Component)(object)this).transform.rotation;
		case RotationType.Target:
		{
			Vector3 forward = GetTargetWorldPosition() - ((Component)(object)this).transform.position;
			if (forward.sqrMagnitude > 0.001f)
			{
				return Quaternion.LookRotation(forward);
			}
			return ((Component)(object)this).transform.rotation;
		}
		default:
			throw new Exception($"Couldn't solve rotation for projectile ({this}), unknown RotationType: {rotationType}");
		}
	}

	protected Vector3 CalculateOffsetOverTrajectoryInWorldSpace(float normalizedPosition, Quaternion rotation)
	{
		float num = verticalOffsetOverTrajectory.Evaluate(normalizedPosition);
		float num2 = horizontalOffsetOverTrajectory.Evaluate(normalizedPosition);
		if (scaleVerticalOffsetOnLength)
		{
			num *= _trajectoryLength;
		}
		if (scaleHorizontalOffsetOnLength)
		{
			num2 *= _trajectoryLength;
		}
		return Vector3.up * num + rotation * Vector3.right * num2;
	}

	private static bool CurveHasNonZero(AnimationCurve c)
	{
		if (c == null)
		{
			return false;
		}
		for (int i = 0; i < c.length; i++)
		{
			Keyframe keyframe = c[i];
			if (Mathf.Abs(keyframe.value) > 0.0001f || Mathf.Abs(keyframe.inTangent) > 0.0001f || Mathf.Abs(keyframe.outTangent) > 0.0001f)
			{
				return true;
			}
		}
		return false;
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		_lastCollisionCheckAnythingPosition = ((Component)(object)this).transform.position;
		_lastCollisionCheckTargetEntityPosition = ((Component)(object)this).transform.position;
	}

	protected override void OnPrepare()
	{
		base.OnPrepare();
		Network_isFlying = true;
		switch (start)
		{
		case StartPositionType.Base:
			Network_startPosition = info.caster.Visual.GetBasePosition();
			break;
		case StartPositionType.Center:
			Network_startPosition = info.caster.Visual.GetCenterPosition();
			break;
		case StartPositionType.Muzzle:
			Network_startPosition = info.caster.Visual.GetMuzzlePosition();
			break;
		case StartPositionType.Custom:
			Network_startPosition = position;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		switch (mode)
		{
		case ProjectileMode.Point:
			Network_entityMode = false;
			Network_targetPosition = info.point;
			Network_startPosition = _startPosition + (_targetPosition - _startPosition).Flattened().normalized * startInFrontDistance;
			break;
		case ProjectileMode.Direction:
			Network_entityMode = false;
			Network_targetPosition = _startPosition + info.forward * endDistance;
			_targetPosition.y = _startPosition.y;
			Network_startPosition = _startPosition + info.forward * startInFrontDistance;
			break;
		case ProjectileMode.Target:
			if (info.target == null)
			{
				Debug.LogError($"'{this}' is targeted projectile but has null info.target!");
				return;
			}
			Network_entityMode = true;
			Network_startPosition = _startPosition + (info.target.position - _startPosition).Flattened().normalized * startInFrontDistance;
			Network_targetEntity = info.target;
			Network_targetPosition = info.target.position;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		if (mode != ProjectileMode.Target)
		{
			RaycastHit val = default;
			RaycastHit val2 = default;
			if (endsWithSameHeight)
			{
				_targetPosition.y = _startPosition.y;
			}
			else if (Physics.Raycast(_targetPosition + Vector3.up * 5f, Vector3.down, ref val, 10f, LayerMasks.Ground))
			{
				_targetPosition.y = val.point.y + customEndHeight;
			}
			else if (Physics.Raycast(_startPosition + Vector3.up * 5f, Vector3.down, ref val2, 10f, LayerMasks.Ground))
			{
				_targetPosition.y = val2.point.y + customEndHeight;
			}
			else
			{
				Debug.LogWarning($"Failed to find ground position and calculate y-coord for projectile '{this}'");
			}
		}
	}

	private void Interpolate(Vector3 pos, Quaternion rot, float dt)
	{
		_ipEndPos = pos;
		_ipEndRot = rot;
		_ipRemainingTime = dt;
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (!_isFlying || Time.frameCount <= _startFrame + 1)
		{
			return;
		}
		Vector3 vector = ((Component)(object)this).transform.position;
		Vector3 vector2 = PositionSolver(dt);
		_estimatedVelocity = (vector2 - vector) / dt;
		Quaternion rot = RotationSolver(dt);
		Interpolate(vector2, rot, dt);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		DoAnythingCollisionChecks(vector);
		if (!_isFlying)
		{
			return;
		}
		Entity entity = targetEntity;
		if ((UnityEngine.Object)(object)entity != null && entity.isActive)
		{
			DoTargetEntityCollisionCheck(vector);
		}
		if (_isFlying && CheckForCompletion())
		{
			entity = targetEntity;
			if ((UnityEngine.Object)(object)entity != null && entity.isActive && _collidedEntities.Add(entity))
			{
				EntityHit hit = new EntityHit
				{
					entity = entity,
					point = ((Component)(object)entity).GetComponent<Collider>().ClosestPoint(((Component)(object)this).transform.position)
				};
				OnEntity(hit);
			}
			if (_isFlying)
			{
				Network_isCompleted = true;
				Network_isFlying = false;
				OnComplete();
			}
		}
	}

	protected virtual bool CheckForCompletion()
	{
		Vector3 targetWorldPosition = GetTargetWorldPosition();
		return (((Component)(object)this).transform.position - targetWorldPosition).sqrMagnitude < 0.010000001f;
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (isActive && _isFlying && _ipRemainingTime != 0f)
		{
			float deltaTime = Time.deltaTime;
			float num = deltaTime / _ipRemainingTime;
			_ipRemainingTime -= deltaTime;
			if (num > 1f)
			{
				_ipRemainingTime = 0f;
				num = 1f;
			}
			Transform transform = ((Component)(object)this).transform;
			Vector3 a = transform.position;
			transform.SetPositionAndRotation(Vector3.Lerp(a, _ipEndPos, num), Quaternion.Lerp(transform.rotation, _ipEndRot, num));
		}
	}

	protected virtual Vector3 GetTargetWorldPosition()
	{
		if (_entityMode)
		{
			Entity entity = targetEntity;
			if ((UnityEngine.Object)(object)entity == null)
			{
				if (_lastKnownTargetPosition.HasValue)
				{
					return _lastKnownTargetPosition.Value;
				}
				if (entity != null)
				{
					_lastKnownTargetPosition = entity.position;
					return _lastKnownTargetPosition.Value;
				}
				Debug.LogWarning(GetActorReadableName() + " is entity mode projectile but has an invalid target or does not have one");
				if (((NetworkBehaviour)this).isServer)
				{
					Destroy();
				}
				return Dew.GetPositionOnGround(((Component)(object)this).transform.position);
			}
			_lastKnownTargetPosition = entity.Visual.GetCenterPosition();
			return _lastKnownTargetPosition.Value;
		}
		return _targetPosition;
	}

	protected override void OnCreate()
	{
		_hasTrajectoryOffset = CurveHasNonZero(verticalOffsetOverTrajectory) || CurveHasNonZero(horizontalOffsetOverTrajectory);
		_trajectoryLength = Vector2.Distance(_startPosition.ToXY(), _targetPosition.ToXY());
		base.OnCreate();
		position = _startPosition;
		position = PositionSolver(0.0001f);
		_estimatedVelocity = (position - _startPosition).normalized * 0.1f;
		rotation = RotationSolver(0.0001f);
		if (effectOnFly != null && Adaptive_ShouldPlayEffects())
		{
			FxPlay(effectOnFly);
		}
		_startFrame = Time.frameCount;
		if (rotationType == RotationType.Target)
		{
			Vector3 forward = GetTargetWorldPosition() - ((Component)(object)this).transform.position;
			if (forward.sqrMagnitude > 0.01f)
			{
				((Component)(object)this).transform.rotation = Quaternion.LookRotation(forward);
			}
		}
		else if (rotationType == RotationType.Velocity)
		{
			Vector3 forward2 = PositionSolver(0.001f) - ((Component)(object)this).transform.position;
			if (forward2.sqrMagnitude > 0.01f)
			{
				((Component)(object)this).transform.rotation = Quaternion.LookRotation(forward2);
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			Network_isFlying = false;
		}
		if (effectOnFly != null)
		{
			FxStop(effectOnFly);
		}
	}

	[Server]
	private void DoAnythingCollisionChecks(Vector3 nextPosition)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Projectile::DoAnythingCollisionChecks(UnityEngine.Vector3)' called when server was not active");
			return;
		}
		if (!isActive || !isFlying || !canCollideMidFlight)
		{
			_lastCollisionCheckAnythingPosition = nextPosition;
			return;
		}
		Vector3 vector = nextPosition - _lastCollisionCheckAnythingPosition;
		int num = Physics2D.CircleCast(_lastCollisionCheckAnythingPosition.ToXY(), collisionRadius, vector.ToXY(), new ContactFilter2D
		{
			layerMask = (LayerMasks.Entity | LayerMasks.CollidableWithProjectile)
		}, _raycastHit2DResults, vector.magnitude);
		Array.Sort(_raycastHit2DResults, 0, num, _raycastHit2DComparer);
		Entity caster = info.caster;
		for (int i = 0; i < num; i++)
		{
			RaycastHit2D val = _raycastHit2DResults[i];
			if (!isActive || !isFlying || !canCollideMidFlight)
			{
				break;
			}
			if (((Component)(object)val.collider).gameObject == ((Component)(object)this).gameObject)
			{
				continue;
			}
			if (DewPhysics.TryGetEntity(val.collider, out var entity) && (caster == null || collisionTargets.Evaluate(caster, entity)) && !entity.Status.hasUncollidable && _collidedEntities.Add(entity))
			{
				EntityHit hit;
				if (val.point == Vector2.zero && val.distance == 0f)
				{
					Vector2 v = val.collider.ClosestPoint(_lastCollisionCheckAnythingPosition.ToXY());
					hit = new EntityHit
					{
						entity = entity,
						point = v.ToXZ()
					};
				}
				else
				{
					Vector3 point = val.point.ToXZ();
					point.y = position.y;
					hit = new EntityHit
					{
						entity = entity,
						point = point
					};
				}
				OnEntity(hit);
			}
			if (DewPhysics.TryGetCollidableWithProjectile(val.collider, out var collidable) && _collidedCollidables.Add(collidable))
			{
				CollidableHit hit2;
				if (val.point == Vector2.zero && val.distance == 0f)
				{
					Vector2 v2 = val.collider.ClosestPoint(_lastCollisionCheckAnythingPosition.ToXY());
					hit2 = new CollidableHit
					{
						projectile = this,
						point = v2.ToXZ()
					};
				}
				else
				{
					Vector3 point2 = val.point.ToXZ();
					point2.y = position.y;
					hit2 = new CollidableHit
					{
						projectile = this,
						point = point2
					};
				}
				collidable.OnProjectileCollision(hit2);
			}
		}
		if (dissipateOnTerrain)
		{
			RaycastHit[] array = DewPool.GetArray(out ArrayReturnHandle<RaycastHit> handle, 64);
			float magnitude = vector.magnitude;
			Vector3 vector2 = ((magnitude > 1E-06f) ? (vector / magnitude) : Vector3.forward);
			if (Physics.RaycastNonAlloc(_lastCollisionCheckAnythingPosition, vector2, array, magnitude, LayerMasks.Ground) > 0)
			{
				Dissipate();
			}
			handle.Return();
		}
		_lastCollisionCheckAnythingPosition = nextPosition;
	}

	[Server]
	private void DoTargetEntityCollisionCheck(Vector3 checkPosition)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Projectile::DoTargetEntityCollisionCheck(UnityEngine.Vector3)' called when server was not active");
			return;
		}
		if ((UnityEngine.Object)(object)_targetEntityCollider == null)
		{
			_targetEntityCollider = ((Component)(object)targetEntity).GetComponentInChildren<Collider>();
		}
		if ((UnityEngine.Object)(object)_targetEntityCollider == null)
		{
			Debug.LogWarning($"This projectile ({this})'s Target Entity ({targetEntity}) does not have a collider.");
			Destroy();
			return;
		}
		Vector3 vector = _targetEntityCollider.ClosestPoint(checkPosition);
		if ((vector - checkPosition).sqrMagnitude <= collisionRadius * collisionRadius && _collidedEntities.Add(targetEntity))
		{
			EntityHit hit = new EntityHit
			{
				entity = targetEntity,
				point = vector
			};
			OnEntity(hit);
		}
		_lastCollisionCheckTargetEntityPosition = checkPosition;
	}

	[Server]
	public void Dissipate()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Projectile::Dissipate()' called when server was not active");
			return;
		}
		Network_isFlying = false;
		RpcStopOnFlyEffect();
		OnDissipate();
		if (killOnDissipate)
		{
			Destroy();
		}
	}

	[Server]
	protected void StopFlying()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Projectile::StopFlying()' called when server was not active");
			return;
		}
		Network_isFlying = false;
		RpcStopOnFlyEffect();
	}

	protected virtual void OnEntity(EntityHit hit)
	{
		onEntity?.Invoke(hit);
		RpcPlayOnEntityEffect(hit);
	}

	[ClientRpc]
	private void RpcPlayOnEntityEffect(EntityHit hit)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_Projectile_002FEntityHit((NetworkWriter)(object)val, hit);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Projectile::RpcPlayOnEntityEffect(Projectile/EntityHit)", 1988428214, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	protected virtual void OnDissipate()
	{
		RpcPlayOnDissipateEffect();
	}

	[ClientRpc]
	private void RpcPlayOnDissipateEffect()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Projectile::RpcPlayOnDissipateEffect()", 825882477, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	protected virtual void OnComplete()
	{
		RpcPlayOnCompleteEffect();
		RpcStopOnFlyEffect();
		if (destroyOnComplete)
		{
			Destroy();
		}
	}

	[ClientRpc]
	private void RpcStopOnFlyEffect()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Projectile::RpcStopOnFlyEffect()", 86461196, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcPlayOnCompleteEffect()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Projectile::RpcPlayOnCompleteEffect()", -490060138, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public void AddEffect(AddEffectTarget target, GameObject eff)
	{
		switch (target)
		{
		case AddEffectTarget.Fly:
		{
			GameObject effect = AddEffectToTarget(eff, ref effectOnFly);
			if (isFlying)
			{
				FxPlay(effect);
			}
			break;
		}
		case AddEffectTarget.Entity:
			AddEffectToTarget(eff, ref effectOnEntity);
			break;
		case AddEffectTarget.Complete:
			AddEffectToTarget(eff, ref effectOnComplete);
			break;
		case AddEffectTarget.Dissipate:
			AddEffectToTarget(eff, ref effectOnDissipate);
			break;
		}
	}

	private GameObject AddEffectToTarget(GameObject eff, ref GameObject target)
	{
		if (!_capturedOrigEffectFields)
		{
			_capturedOrigEffectFields = true;
			_origEffectOnFly = effectOnFly;
			_origEffectOnEntity = effectOnEntity;
			_origEffectOnComplete = effectOnComplete;
			_origEffectOnDissipate = effectOnDissipate;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(eff);
		if (target == null)
		{
			target = gameObject;
			gameObject.transform.parent = ((Component)(object)this).transform;
		}
		else
		{
			gameObject.transform.parent = target.transform;
		}
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		(_runtimeAddedEffects ?? (_runtimeAddedEffects = new List<GameObject>())).Add(gameObject);
		return gameObject;
	}

	[Server]
	public void SetCustomStartPosition(Vector3 pos)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Projectile::SetCustomStartPosition(UnityEngine.Vector3)' called when server was not active");
			return;
		}
		if (start != StartPositionType.Custom)
		{
			Debug.LogWarning($"{this} start position is not set to custom");
			return;
		}
		Network_startPosition = pos;
		((Component)(object)this).transform.position = pos;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Network_targetEntity = null;
		_targetEntityCollider = null;
		_collidedEntities.Clear();
		_collidedCollidables.Clear();
		_ipRemainingTime = 0f;
		Network_isCompleted = false;
		Network_isFlying = false;
		_lastKnownTargetPosition = null;
		if (_runtimeAddedEffects == null || _runtimeAddedEffects.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < _runtimeAddedEffects.Count; i++)
		{
			if (_runtimeAddedEffects[i] != null)
			{
				UnityEngine.Object.Destroy(_runtimeAddedEffects[i]);
			}
		}
		_runtimeAddedEffects.Clear();
		effectOnFly = _origEffectOnFly;
		effectOnEntity = _origEffectOnEntity;
		effectOnComplete = _origEffectOnComplete;
		effectOnDissipate = _origEffectOnDissipate;
	}

	static Projectile()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected Obj, but got Unknown
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected Obj, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected Obj, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected Obj, but got Unknown
		_raycastHit2DResults = new RaycastHit2D[128];
		_raycastHit2DComparer = Comparer<RaycastHit2D>.Create((RaycastHit2D x, RaycastHit2D y) => x.distance.CompareTo(y.distance));
		RemoteProcedureCalls.RegisterRpc(typeof(Projectile), "System.Void Projectile::RpcPlayOnEntityEffect(Projectile/EntityHit)", (RemoteCallDelegate)InvokeUserCode_RpcPlayOnEntityEffect__EntityHit);
		RemoteProcedureCalls.RegisterRpc(typeof(Projectile), "System.Void Projectile::RpcPlayOnDissipateEffect()", (RemoteCallDelegate)InvokeUserCode_RpcPlayOnDissipateEffect);
		RemoteProcedureCalls.RegisterRpc(typeof(Projectile), "System.Void Projectile::RpcStopOnFlyEffect()", (RemoteCallDelegate)InvokeUserCode_RpcStopOnFlyEffect);
		RemoteProcedureCalls.RegisterRpc(typeof(Projectile), "System.Void Projectile::RpcPlayOnCompleteEffect()", (RemoteCallDelegate)InvokeUserCode_RpcPlayOnCompleteEffect);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcPlayOnEntityEffect__EntityHit(EntityHit hit)
	{
		if (effectOnEntity != null && Adaptive_ShouldPlayEffects())
		{
			effectOnEntity.transform.SetPositionAndRotation(hit.point, ((Component)(object)this).transform.rotation);
			if (_runtimeAddedEffects != null && _runtimeAddedEffects.Count > 0)
			{
				FxPlayNew(effectOnEntity, hit.entity);
			}
			else if (detachEntityHitEffect && !reuseInRoom)
			{
				FxPlayDetached(effectOnEntity, hit.entity);
			}
			else
			{
				DewEffect.PlayPooledLocal(((NetworkBehaviour)this).netIdentity, effectOnEntity, hit.entity, hit.point, ((Component)(object)this).transform.rotation);
			}
		}
	}

	protected static void InvokeUserCode_RpcPlayOnEntityEffect__EntityHit(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayOnEntityEffect called on server.");
		}
		else
		{
			((Projectile)(object)obj).UserCode_RpcPlayOnEntityEffect__EntityHit(GeneratedNetworkCode._Read_Projectile_002FEntityHit(reader));
		}
	}

	protected void UserCode_RpcPlayOnDissipateEffect()
	{
		if (effectOnDissipate != null && Adaptive_ShouldPlayEffects())
		{
			if (_runtimeAddedEffects != null && _runtimeAddedEffects.Count > 0)
			{
				FxPlayNew(effectOnDissipate, ((Component)(object)this).transform.position, ((Component)(object)this).transform.rotation);
			}
			else if (!reuseInRoom)
			{
				FxPlayDetached(effectOnDissipate, ((Component)(object)this).transform.position, ((Component)(object)this).transform.rotation);
			}
			else
			{
				DewEffect.PlayPooledLocal(((NetworkBehaviour)this).netIdentity, effectOnDissipate, null, ((Component)(object)this).transform.position, ((Component)(object)this).transform.rotation);
			}
		}
	}

	protected static void InvokeUserCode_RpcPlayOnDissipateEffect(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayOnDissipateEffect called on server.");
		}
		else
		{
			((Projectile)(object)obj).UserCode_RpcPlayOnDissipateEffect();
		}
	}

	protected void UserCode_RpcStopOnFlyEffect()
	{
		if (effectOnFly != null)
		{
			FxStop(effectOnFly);
		}
	}

	protected static void InvokeUserCode_RpcStopOnFlyEffect(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcStopOnFlyEffect called on server.");
		}
		else
		{
			((Projectile)(object)obj).UserCode_RpcStopOnFlyEffect();
		}
	}

	protected void UserCode_RpcPlayOnCompleteEffect()
	{
		if (!(effectOnComplete != null) || !Adaptive_ShouldPlayEffects())
		{
			return;
		}
		if (destroyOnComplete && !reuseInRoom)
		{
			if (preserveCompleteEffectPositionAndRotation)
			{
				FxPlayDetached(effectOnComplete);
			}
			else
			{
				FxPlayDetached(effectOnComplete, ((Component)(object)this).transform.position, ((Component)(object)this).transform.rotation);
			}
		}
		else if (destroyOnComplete && (_runtimeAddedEffects == null || _runtimeAddedEffects.Count == 0))
		{
			if (preserveCompleteEffectPositionAndRotation)
			{
				DewEffect.PlayPooledLocal(((NetworkBehaviour)this).netIdentity, effectOnComplete, null, effectOnComplete.transform.position, effectOnComplete.transform.rotation);
			}
			else
			{
				DewEffect.PlayPooledLocal(((NetworkBehaviour)this).netIdentity, effectOnComplete, null, ((Component)(object)this).transform.position, ((Component)(object)this).transform.rotation);
			}
		}
		else if (preserveCompleteEffectPositionAndRotation)
		{
			FxPlayNew(effectOnComplete);
		}
		else
		{
			FxPlayNew(effectOnComplete, ((Component)(object)this).transform.position, ((Component)(object)this).transform.rotation);
		}
	}

	protected static void InvokeUserCode_RpcPlayOnCompleteEffect(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayOnCompleteEffect called on server.");
		}
		else
		{
			((Projectile)(object)obj).UserCode_RpcPlayOnCompleteEffect();
		}
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			GeneratedNetworkCode._Write_Projectile_002FProjectileMode(writer, mode);
			NetworkWriterExtensions.WriteFloat(writer, endDistance);
			GeneratedNetworkCode._Write_Projectile_002FStartPositionType(writer, start);
			NetworkWriterExtensions.WriteFloat(writer, startInFrontDistance);
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_targetEntity);
			NetworkWriterExtensions.WriteBool(writer, _isCompleted);
			NetworkWriterExtensions.WriteBool(writer, _isFlying);
			NetworkWriterExtensions.WriteBool(writer, _entityMode);
			NetworkWriterExtensions.WriteVector3(writer, _targetPosition);
			NetworkWriterExtensions.WriteVector3(writer, _startPosition);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			GeneratedNetworkCode._Write_Projectile_002FProjectileMode(writer, mode);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, endDistance);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100L) != 0L)
		{
			GeneratedNetworkCode._Write_Projectile_002FStartPositionType(writer, start);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x200L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, startInFrontDistance);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x400L) != 0L)
		{
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_targetEntity);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x800L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, _isCompleted);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x1000L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, _isFlying);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x2000L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, _entityMode);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x4000L) != 0L)
		{
			NetworkWriterExtensions.WriteVector3(writer, _targetPosition);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x8000L) != 0L)
		{
			NetworkWriterExtensions.WriteVector3(writer, _startPosition);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<ProjectileMode>(ref mode, (Action<ProjectileMode, ProjectileMode>)null, GeneratedNetworkCode._Read_Projectile_002FProjectileMode(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref endDistance, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<StartPositionType>(ref start, (Action<StartPositionType, StartPositionType>)null, GeneratedNetworkCode._Read_Projectile_002FStartPositionType(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref startInFrontDistance, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<Entity>(ref _targetEntity, (Action<Entity, Entity>)null, reader, ref ____targetEntityNetId);
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isCompleted, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isFlying, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _entityMode, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Vector3>(ref _targetPosition, (Action<Vector3, Vector3>)null, NetworkReaderExtensions.ReadVector3(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Vector3>(ref _startPosition, (Action<Vector3, Vector3>)null, NetworkReaderExtensions.ReadVector3(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<ProjectileMode>(ref mode, (Action<ProjectileMode, ProjectileMode>)null, GeneratedNetworkCode._Read_Projectile_002FProjectileMode(reader));
		}
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref endDistance, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x100L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<StartPositionType>(ref start, (Action<StartPositionType, StartPositionType>)null, GeneratedNetworkCode._Read_Projectile_002FStartPositionType(reader));
		}
		if ((num & 0x200L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref startInFrontDistance, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x400L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<Entity>(ref _targetEntity, (Action<Entity, Entity>)null, reader, ref ____targetEntityNetId);
		}
		if ((num & 0x800L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isCompleted, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x1000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isFlying, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x2000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _entityMode, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x4000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Vector3>(ref _targetPosition, (Action<Vector3, Vector3>)null, NetworkReaderExtensions.ReadVector3(reader));
		}
		if ((num & 0x8000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Vector3>(ref _startPosition, (Action<Vector3, Vector3>)null, NetworkReaderExtensions.ReadVector3(reader));
		}
	}
}
