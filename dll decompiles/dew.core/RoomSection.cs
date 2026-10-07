using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(RoomSavedObject))]
[RequireComponent(typeof(Section_Monsters))]
[RequireComponent(typeof(Section_Props))]
public class RoomSection : LogicBehaviour, IPlayerPathablePoint
{
	public Vector2[] vectors = new Vector2[6]
	{
		new Vector2(1f, 0f) * 5f,
		new Vector2(0.5f, 0.87f) * 5f,
		new Vector2(-0.5f, 0.87f) * 5f,
		new Vector2(-1f, 0f) * 5f,
		new Vector2(-0.5f, -0.87f) * 5f,
		new Vector2(0.5f, -0.87f) * 5f
	};

	[SerializeField]
	[HideInInspector]
	private Vector2[] _points;

	private PointsToTriangleVertices _wrapper;

	[SerializeField]
	[HideInInspector]
	private float[] _triAreas;

	[SerializeField]
	[HideInInspector]
	private RoomSectionFloatDictionary _distanceToSections;

	private readonly List<ActorRef<Entity>> _entities = new List<ActorRef<Entity>>();

	private readonly List<ActorRef<Entity>> _entitiesWithSectionTriggeringDisabled = new List<ActorRef<Entity>>();

	public UnityEvent onEntitiesChanged = new UnityEvent();

	public UnityEvent<Entity> onEntityEnter = new UnityEvent<Entity>();

	public UnityEvent<Entity> onEntityExit = new UnityEvent<Entity>();

	public UnityEvent onEnterFirstTime = new UnityEvent();

	public UnityEvent onEveryonePresent = new UnityEvent();

	public bool clearRoomOnEnterFirstTime;

	public bool waitForAggroedEnemiesToDie = true;

	[SaveVar(SaveVarFlags.Default)]
	private bool _didInvokeOnEnterFirstTime;

	[SaveVar(SaveVarFlags.Default)]
	private bool _didInvokeOnEveryonePresent;

	private DewCollider _dewCollider;

	public const float BanSpotsDistance = 3.25f;

	private const float NodeWallMinDistance = 1f;

	private const int NodeRetryLimit = 100;

	private const float RandomNodeMinSqrDistance = 12.25f;

	private const float RandomNodeBestSqrDistance = 25f;

	[HideInInspector]
	[SerializeField]
	private List<Vector3> _nodes = new List<Vector3>();

	internal List<int> _unusedNodeIndices = new List<int>();

	internal List<Vector2> _usedNodePositions = new List<Vector2>();

	public Section_Monsters monsters { get; private set; }

	public Section_Props props { get; private set; }

	[field: SerializeField]
	public float area { get; private set; }

	public IReadOnlyList<ActorRef<Entity>> entities => _entities;

	public Vector3 pathablePivot { get; private set; }

	public int numOfHeroes { get; private set; }

	Vector3 IPlayerPathablePoint.pathablePosition => pathablePivot;

	public IReadOnlyList<Vector3> nodes => _nodes;

	private void Awake()
	{
		UpdatePathablePivot();
		monsters = GetComponent<Section_Monsters>();
		props = GetComponent<Section_Props>();
		onEnterFirstTime.AddListener(() =>
		{
			if (NetworkServer.active && clearRoomOnEnterFirstTime && !SingletonDewNetworkBehaviour<Room>.instance.didClearRoom)
			{
				StartCoroutine(Routine());
			}
		});
		ResetUsedNodeIndices();
		IEnumerator Routine()
		{
			if (waitForAggroedEnemiesToDie)
			{
				yield return Dew.WaitForAggroedEnemiesRoutine();
			}
			SingletonDewNetworkBehaviour<Room>.instance.ClearRoom();
		}
	}

	private void UpdatePathablePivot()
	{
		pathablePivot = Dew.GetPositionOnGround(transform.position);
	}

	private void OnValidate()
	{
		_points = new Vector2[vectors.Length * 3 - 6];
		PolygonTools.GetVerticesArray(vectors, ref _points);
		_wrapper = new PointsToTriangleVertices(_points);
		_triAreas = new float[_wrapper.Count / 3];
		area = 0f;
		for (int i = 0; i < _wrapper.Count / 3; i++)
		{
			Vector2 v = _wrapper[i * 3 + 1] - _wrapper[i * 3];
			Vector2 v2 = _wrapper[i * 3 + 2] - _wrapper[i * 3];
			float num = Mathf.Abs(v.Cross(v2));
			_triAreas[i] = num;
			area += num;
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		_dewCollider = gameObject.AddComponent<DewCollider>();
		_dewCollider.shape = DewCollider.ColliderShape.Polygon;
		_dewCollider.points = vectors;
		_dewCollider.receiveEntityCallbacks = true;
		_dewCollider.UpdateProxyCollider();
		GameManager.CallOnReady(() =>
		{
			if (!(_dewCollider == null))
			{
				_dewCollider.onEntityEnter.AddListener(HandleEntityEnter);
				_dewCollider.onEntityExit.AddListener(HandleEntityExit);
				ListReturnHandle<Entity> handle;
				foreach (Entity entity in _dewCollider.GetEntities(out handle, new CollisionCheckSettings
				{
					includeUncollidable = true
				}))
				{
					HandleEntityEnter(entity);
				}
				handle.Return();
			}
		});
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (!NetworkServer.active)
		{
			return;
		}
		for (int num = _entitiesWithSectionTriggeringDisabled.Count - 1; num >= 0; num--)
		{
			Entity entity = _entitiesWithSectionTriggeringDisabled[num].Get();
			if (entity.IsNullOrInactive())
			{
				_entitiesWithSectionTriggeringDisabled.RemoveAt(num);
			}
			else if (!entity.Status.isSectionTriggeringDisabled && !NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition)
			{
				_entitiesWithSectionTriggeringDisabled.RemoveAt(num);
				AddEntity(entity);
			}
		}
		if (!_didInvokeOnEveryonePresent)
		{
			CheckEveryonePresent();
		}
	}

	private void HandleEntityEnter(Entity e)
	{
		if (NetworkServer.active)
		{
			if ((bool)e.Status.isSectionTriggeringDisabled || NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition)
			{
				_entitiesWithSectionTriggeringDisabled.Add(e);
			}
			else
			{
				AddEntity(e);
			}
		}
	}

	private void AddEntity(Entity e)
	{
		if (e._sections.Contains(this))
		{
			return;
		}
		e._sections.Add(this);
		e.lastSection = this;
		_entities.Add(e);
		UpdateResidentMetrics();
		try
		{
			onEntityEnter?.Invoke(e);
			onEntitiesChanged?.Invoke();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		if (!(e is Hero))
		{
			return;
		}
		if (!_didInvokeOnEnterFirstTime)
		{
			_didInvokeOnEnterFirstTime = true;
			try
			{
				onEnterFirstTime?.Invoke();
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
			}
		}
		if (!_didInvokeOnEveryonePresent)
		{
			CheckEveryonePresent();
		}
	}

	internal void HandleEntityExit(Entity e)
	{
		if (!NetworkServer.active)
		{
			return;
		}
		_entitiesWithSectionTriggeringDisabled.Remove(e);
		if (!_entities.Remove(e))
		{
			return;
		}
		e._sections.Remove(this);
		UpdateResidentMetrics();
		try
		{
			onEntityExit?.Invoke(e);
			onEntitiesChanged?.Invoke();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private void UpdateResidentMetrics()
	{
		numOfHeroes = 0;
		foreach (ActorRef<Entity> entity in _entities)
		{
			if (entity.Get() is Hero)
			{
				numOfHeroes++;
			}
		}
	}

	private void CheckEveryonePresent()
	{
		bool flag = false;
		foreach (Hero allHero in NetworkedManagerBase<ActorManager>.instance.allHeroes)
		{
			if (!allHero.IsNullInactiveDeadOrKnockedOut())
			{
				if (!_entities.Contains(allHero))
				{
					return;
				}
				flag = true;
			}
		}
		if (!flag)
		{
			return;
		}
		_didInvokeOnEveryonePresent = true;
		try
		{
			onEveryonePresent?.Invoke();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (_dewCollider != null)
		{
			UnityEngine.Object.Destroy(_dewCollider);
		}
		foreach (ActorRef<Entity> entity in _entities)
		{
			entity.Get()?._sections.Remove(this);
		}
		_entities.Clear();
		_entitiesWithSectionTriggeringDisabled.Clear();
	}

	public Vector3 GetRandomWorldPosition(DewRandom random = null)
	{
		return transform.TransformPoint(GetRandomLocalPosition(random));
	}

	public Vector3 GetRandomLocalPosition(DewRandom random = null)
	{
		if (random == null)
		{
			random = DewRandom.instance;
		}
		Vector2[] verts = new Vector2[3];
		SelectRandomTriangle(ref verts, random);
		return PolygonTools.GetRandomPositionInTriangle(verts, random).ToXZ();
	}

	private int SelectRandomTriangle(ref Vector2[] verts, DewRandom random)
	{
		if (_wrapper == null)
		{
			_wrapper = new PointsToTriangleVertices(_points);
		}
		float num = random.Range(0f, area);
		for (int i = 0; i < _wrapper.Count / 3; i++)
		{
			if (num < _triAreas[i])
			{
				verts[0] = _wrapper[i * 3];
				verts[1] = _wrapper[i * 3 + 1];
				verts[2] = _wrapper[i * 3 + 2];
				return i;
			}
			num -= _triAreas[i];
		}
		Vector2[] obj = verts;
		PointsToTriangleVertices wrapper = _wrapper;
		obj[0] = wrapper[wrapper.Count - 3];
		Vector2[] obj2 = verts;
		PointsToTriangleVertices wrapper2 = _wrapper;
		obj2[1] = wrapper2[wrapper2.Count - 2];
		Vector2[] obj3 = verts;
		PointsToTriangleVertices wrapper3 = _wrapper;
		obj3[2] = wrapper3[wrapper3.Count - 1];
		return _wrapper.Count / 3 - 1;
	}

	public float GetNavDistanceTo(RoomSection sec)
	{
		return _distanceToSections[sec];
	}

	public bool OverlapPoint(Vector2 point)
	{
		return _dewCollider.OverlapPoint(point);
	}

	public void ResetUsedNodeIndices()
	{
		_unusedNodeIndices.Clear();
		for (int i = 0; i < _nodes.Count; i++)
		{
			_unusedNodeIndices.Add(i);
		}
		_usedNodePositions.Clear();
	}

	public Vector3 GetGoodWanderPosition(Vector3 from)
	{
		if (!Application.IsPlaying(this))
		{
			throw new InvalidOperationException("This is supposed to be used in runtime");
		}
		if (_nodes.Count == 0)
		{
			Debug.LogWarning("Section " + name + " of " + ((UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.instance).name + " does not have node setup");
			return Dew.GetValidAgentDestination_Closest(from, GetRandomWorldPosition());
		}
		return Dew.GetValidAgentDestination_Closest(from, Dew.SelectBestWithScore(_nodes, GetScore, 0.4f));
		float GetScore(Vector3 pos, int index)
		{
			float num = 1f;
			for (int i = 0; i < _usedNodePositions.Count + 1; i++)
			{
				Vector2 vector = ((i == _usedNodePositions.Count) ? from.ToXY() : _usedNodePositions[i]);
				float num2 = Vector2.SqrMagnitude(pos.ToXY() - vector);
				if (!(num2 > 25f))
				{
					num = ((!(num2 > 12.25f)) ? Mathf.Min(num, -1f - (12.25f - num2) * 3f) : Mathf.Min(num, (num2 - 12.25f) / 12.75f));
				}
			}
			return num;
		}
	}

	public bool TryGetGoodNodePosition(out Vector3 position, DewRandom random = null)
	{
		if (!Application.IsPlaying(this))
		{
			throw new InvalidOperationException("This is supposed to be used in runtime");
		}
		if (random == null)
		{
			random = DewRandom.instance;
		}
		if (_nodes.Count == 0)
		{
			Debug.LogException(new InvalidOperationException("Section " + name + " of " + ((UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.instance).name + " does not have node setup"));
			position = GetRandomWorldPosition();
			return false;
		}
		if (_nodes.Count == 0)
		{
			throw new InvalidOperationException("Section " + name + " of " + ((UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.instance).name + " does not have node setup");
		}
		if (_unusedNodeIndices.Count == 0)
		{
			position = _nodes[random.Range(0, _nodes.Count)];
			if (_usedNodePositions.Count == 0)
			{
				Debug.LogWarning("Section " + name + " of " + ((UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.instance).name + " has good node indices uninitialized. This isn't supposed to happen. Using random pos...");
			}
			else
			{
				Debug.LogWarning("Section " + name + " of " + ((UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.instance).name + " used all node indices. Using random node pos...");
			}
			return false;
		}
		int num = 0;
		float num2 = float.NegativeInfinity;
		for (int i = 0; i < _nodes.Count; i++)
		{
			Vector3 pos = _nodes[i];
			float num3 = GetScore(pos, i);
			num3 *= 1f + random.Range(-0.1f, 0.1f);
			if (num3 > num2)
			{
				num2 = num3;
				num = i;
			}
		}
		position = _nodes[num];
		_unusedNodeIndices.Remove(num);
		_usedNodePositions.Add(position.ToXY());
		if (num2 < 0f)
		{
			Debug.LogWarning("Section " + name + " of " + ((UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.instance).name + " could not find good node position, using sub-optimal pos...");
			return false;
		}
		return true;
		float GetScore(Vector3 v, int index)
		{
			float num4 = 1f;
			for (int j = 0; j < _usedNodePositions.Count; j++)
			{
				float num5 = Vector2.SqrMagnitude(v.ToXY() - _usedNodePositions[j]);
				if (!(num5 > 25f))
				{
					num4 = ((!(num5 > 12.25f)) ? Mathf.Min(num4, -1f - (12.25f - num5) * 3f) : Mathf.Min(num4, (num5 - 12.25f) / 12.75f));
				}
			}
			return num4;
		}
	}

	public Vector3 GetAnyRandomNode()
	{
		return _nodes[UnityEngine.Random.Range(0, _nodes.Count)];
	}
}
