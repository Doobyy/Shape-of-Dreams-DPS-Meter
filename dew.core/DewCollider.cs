using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class DewCollider : MonoBehaviour
{
	public enum ColliderShape
	{
		Circle,
		Box,
		Polygon
	}

	public ColliderShape shape;

	public float radius = 1f;

	public Vector2 size = Vector2.one;

	public Vector2 offset;

	public Vector2[] points;

	public int maxDecimals = 2;

	public bool receiveEntityCallbacks;

	public bool invokeEventsOnClients;

	public UnityEvent<Entity> onEntityEnter = new UnityEvent<Entity>();

	public UnityEvent<Entity> onEntityExit = new UnityEvent<Entity>();

	internal Collider2D _proxy;

	private EntityCallbackTrigger _proxyTrigger;

	private static readonly Stack<CircleCollider2D> _circlePool = new Stack<CircleCollider2D>();

	private static readonly Stack<CircleCollider2D> _circleTrigPool = new Stack<CircleCollider2D>();

	private static readonly Stack<BoxCollider2D> _boxPool = new Stack<BoxCollider2D>();

	private static readonly Stack<BoxCollider2D> _boxTrigPool = new Stack<BoxCollider2D>();

	private static readonly Stack<PolygonCollider2D> _polygonPool = new Stack<PolygonCollider2D>();

	private static readonly Stack<PolygonCollider2D> _polygonTrigPool = new Stack<PolygonCollider2D>();

	private void OnEnable()
	{
		if (!((UnityEngine.Object)(object)_proxy != null))
		{
			UpdateProxyCollider();
		}
	}

	private void OnDisable()
	{
		ReturnProxy();
	}

	private void OnDestroy()
	{
		ReturnProxy();
	}

	internal void HandleEntityEnter(Entity e)
	{
		if (!e.IsNullInactiveDeadOrKnockedOut() && (invokeEventsOnClients || NetworkServer.active))
		{
			onEntityEnter?.Invoke(e);
		}
	}

	internal void HandleEntityExit(Entity e)
	{
		if (invokeEventsOnClients || NetworkServer.active)
		{
			onEntityExit?.Invoke(e);
		}
	}

	private static T RentProxy<T>(Stack<T> pool, bool needTrigger) where T : Collider2D
	{
		while (pool.Count > 0)
		{
			T val = pool.Pop();
			if ((UnityEngine.Object)(object)val != null)
			{
				return val;
			}
		}
		GameObject gameObject = DewPhysics.AddEmptyObject("DewCollider Proxy");
		gameObject.SetActive(value: false);
		T result = gameObject.AddComponent<T>();
		if (needTrigger)
		{
			gameObject.AddComponent<EntityCallbackTrigger>();
		}
		return result;
	}

	private void ReturnProxy()
	{
		if ((UnityEngine.Object)(object)_proxy == null)
		{
			return;
		}
		if (_proxyTrigger != null)
		{
			_proxyTrigger.owner = null;
		}
		GameObject gameObject = ((Component)(object)_proxy).gameObject;
		gameObject.SetActive(value: false);
		Collider2D proxy = _proxy;
		CircleCollider2D val = (CircleCollider2D)(object)((proxy is CircleCollider2D) ? proxy : null);
		if (val == null)
		{
			BoxCollider2D val2 = (BoxCollider2D)(object)((proxy is BoxCollider2D) ? proxy : null);
			if (val2 == null)
			{
				PolygonCollider2D val3 = (PolygonCollider2D)(object)((proxy is PolygonCollider2D) ? proxy : null);
				if (val3 != null)
				{
					((_proxyTrigger != null) ? _polygonTrigPool : _polygonPool).Push(val3);
				}
				else
				{
					UnityEngine.Object.Destroy(gameObject);
				}
			}
			else
			{
				((_proxyTrigger != null) ? _boxTrigPool : _boxPool).Push(val2);
			}
		}
		else
		{
			((_proxyTrigger != null) ? _circleTrigPool : _circlePool).Push(val);
		}
		_proxy = null;
		_proxyTrigger = null;
	}

	public void UpdateProxyCollider()
	{
		UpdateProxyCollider_Imp();
	}

	private void UpdateProxyCollider_Imp()
	{
		ReturnProxy();
		bool flag = receiveEntityCallbacks;
		Collider2D val2;
		switch (shape)
		{
		case ColliderShape.Circle:
		{
			CircleCollider2D val4 = RentProxy<CircleCollider2D>(flag ? _circleTrigPool : _circlePool, flag);
			val4.radius = radius;
			((Collider2D)val4).offset = offset;
			((Collider2D)val4).isTrigger = true;
			val2 = (Collider2D)(object)val4;
			break;
		}
		case ColliderShape.Box:
		{
			BoxCollider2D val3 = RentProxy<BoxCollider2D>(flag ? _boxTrigPool : _boxPool, flag);
			val3.size = size;
			((Collider2D)val3).offset = offset;
			((Collider2D)val3).isTrigger = true;
			val2 = (Collider2D)(object)val3;
			break;
		}
		case ColliderShape.Polygon:
		{
			PolygonCollider2D val = RentProxy<PolygonCollider2D>(flag ? _polygonTrigPool : _polygonPool, flag);
			val.points = points;
			((Collider2D)val).isTrigger = true;
			val2 = (Collider2D)(object)val;
			break;
		}
		default:
			throw new ArgumentException($"Unknown Collider Shape: {shape}");
		}
		_proxy = val2;
		GameObject gameObject = ((Component)(object)val2).gameObject;
		if (flag)
		{
			_proxyTrigger = gameObject.GetComponent<EntityCallbackTrigger>();
			_proxyTrigger.owner = this;
		}
		PositionColliders(syncNow: false);
		gameObject.SetActive(value: true);
	}

	private void PositionColliders(bool syncNow = true)
	{
		Transform transform = base.transform;
		Vector3 position = transform.position;
		Vector3 lossyScale = transform.lossyScale;
		Vector3 vector = new Vector3(position.x, position.z, 0f);
		Quaternion quaternion = Quaternion.Euler(0f, 0f, 0f - transform.rotation.eulerAngles.y);
		Vector3 vector2 = new Vector3(lossyScale.x, lossyScale.z, 0f);
		Transform transform2 = ((Component)(object)_proxy).transform;
		if (!(transform2.position == vector) || !(transform2.rotation == quaternion) || !(transform2.localScale == vector2))
		{
			transform2.SetPositionAndRotation(vector, quaternion);
			transform2.localScale = vector2;
			if (syncNow)
			{
				Physics2D.autoSyncTransforms = false;
				Physics2D.SyncTransforms();
				Physics2D.autoSyncTransforms = true;
			}
		}
	}

	public bool OverlapPoint(Vector2 point)
	{
		PositionColliders();
		if (_proxy.OverlapPoint(point))
		{
			return true;
		}
		return false;
	}

	public List<Entity> GetEntities(out ListReturnHandle<Entity> handle, CollisionCheckSettings settings = default(CollisionCheckSettings))
	{
		return GetEntities_Imp(out handle, null, null, null, null, settings);
	}

	public List<Entity> GetEntities(out ListReturnHandle<Entity> handle, IEntityValidator validator, CollisionCheckSettings settings = default(CollisionCheckSettings))
	{
		return GetEntities_Imp(out handle, validator, null, null, null, settings);
	}

	public List<Entity> GetEntities(out ListReturnHandle<Entity> handle, IBinaryEntityValidator validator, Entity self, CollisionCheckSettings settings = default(CollisionCheckSettings))
	{
		return GetEntities_Imp(out handle, null, validator, self, null, settings);
	}

	public List<Entity> GetEntities(out ListReturnHandle<Entity> handle, Func<Entity, bool> validator, CollisionCheckSettings settings = default(CollisionCheckSettings))
	{
		return GetEntities_Imp(out handle, null, null, null, validator, settings);
	}

	private List<Entity> GetEntities_Imp(out ListReturnHandle<Entity> handle, IEntityValidator validator, IBinaryEntityValidator binaryValidator, Entity binarySelf, Func<Entity, bool> funcValidator, CollisionCheckSettings settings)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		if (this == null)
		{
			Debug.Log("Tried to GetEntities on destroyed DewCollider");
			return DewPool.GetList(out handle);
		}
		PositionColliders();
		List<Collider2D> list = DewPool.GetList(out ListReturnHandle<Collider2D> handle2);
		List<Entity> list2 = DewPool.GetList(out handle);
		Physics2D.autoSyncTransforms = false;
		Physics2D.SyncTransforms();
		Physics2D.autoSyncTransforms = true;
		_proxy.Overlap(new ContactFilter2D
		{
			useLayerMask = true,
			layerMask = LayerMasks.Entity
		}, list);
		for (int i = 0; i < list.Count; i++)
		{
			if (DewPhysics.TryGetEntity(list[i], out var entity) && (validator == null || validator.Evaluate(entity)) && (binaryValidator == null || binaryValidator.Evaluate(binarySelf, entity)) && (funcValidator == null || funcValidator(entity)) && (settings.includeUncollidable || !entity.Status.hasUncollidable) && entity.isActive && !list2.Contains(entity))
			{
				list2.Add(entity);
			}
		}
		handle2.Return();
		DewPhysics.SortList(settings, list2, transform.position);
		return list2;
	}

	public IEnumerable<List<Entity>> SweepEntitiesFromOrigin(int iterations, bool includeUncollidable = false)
	{
		return SweepEntitiesFromOrigin(iterations, (Entity _) => true, includeUncollidable);
	}

	public IEnumerable<List<Entity>> SweepEntitiesFromOrigin(int iterations, IEntityValidator validator, bool includeUncollidable = false)
	{
		return SweepEntitiesFromOrigin(iterations, (Entity _) => validator.Evaluate(_), includeUncollidable);
	}

	public IEnumerable<List<Entity>> SweepEntitiesFromOrigin(int iterations, IBinaryEntityValidator validator, Entity self, bool includeUncollidable = false)
	{
		return SweepEntitiesFromOrigin(iterations, (Entity _) => validator.Evaluate(self, _), includeUncollidable);
	}

	public IEnumerable<List<Entity>> SweepEntitiesFromOrigin(int iterations, Func<Entity, bool> validator, bool includeUncollidable = false)
	{
		DewCollider[] colliders = GetComponentsInChildren<DewCollider>(includeInactive: true);
		EdgeCollider2D helperCol = DewPhysics.AddEmptyObject("SweepHelper of " + name).AddComponent<EdgeCollider2D>();
		((Collider2D)helperCol).isTrigger = true;
		List<Collider2D> colsTemp = DewPool.GetList(out ListReturnHandle<Collider2D> colsTempHandle);
		List<Collider2D> colsMain = DewPool.GetList(out ListReturnHandle<Collider2D> colsMainHandle);
		List<Collider2D> colsHelper = DewPool.GetList(out ListReturnHandle<Collider2D> colsHelperHandle);
		List<Entity> affectedEnts = DewPool.GetList(out ListReturnHandle<Entity> affectedHandle);
		List<Entity> ents = new List<Entity>();
		Vector2[] points = new Vector2[13];
		try
		{
			float maxDistance = -1f;
			foreach (DewCollider dewCollider in colliders)
			{
				Collider2D proxy = dewCollider._proxy;
				CircleCollider2D val = (CircleCollider2D)(object)((proxy is CircleCollider2D) ? proxy : null);
				if (val != null)
				{
					maxDistance = Mathf.Max(maxDistance, Vector3.Distance(dewCollider.transform.position, transform.position) + val.radius * dewCollider.transform.lossyScale.x);
					continue;
				}
				Collider2D proxy2 = dewCollider._proxy;
				PolygonCollider2D val2 = (PolygonCollider2D)(object)((proxy2 is PolygonCollider2D) ? proxy2 : null);
				if (val2 != null)
				{
					for (int j = 0; j < val2.points.Length; j++)
					{
						float b = Vector3.Distance(transform.position, dewCollider.transform.TransformPoint(val2.points[j].ToXZ()));
						maxDistance = Mathf.Max(maxDistance, b);
					}
					continue;
				}
				Collider2D proxy3 = dewCollider._proxy;
				BoxCollider2D val3 = (BoxCollider2D)(object)((proxy3 is BoxCollider2D) ? proxy3 : null);
				if (val3 != null)
				{
					Vector2 vector = val3.size * 0.5f;
					Vector2 vector2 = ((Collider2D)val3).offset;
					float b2 = Vector3.Distance(transform.position, dewCollider.transform.TransformPoint((new Vector2(0f - vector.x, 0f - vector.y) + vector2).ToXZ()));
					maxDistance = Mathf.Max(maxDistance, b2);
					b2 = Vector3.Distance(transform.position, dewCollider.transform.TransformPoint((new Vector2(vector.x, 0f - vector.y) + vector2).ToXZ()));
					maxDistance = Mathf.Max(maxDistance, b2);
					b2 = Vector3.Distance(transform.position, dewCollider.transform.TransformPoint((new Vector2(vector.x, vector.y) + vector2).ToXZ()));
					maxDistance = Mathf.Max(maxDistance, b2);
					b2 = Vector3.Distance(transform.position, dewCollider.transform.TransformPoint((new Vector2(0f - vector.x, vector.y) + vector2).ToXZ()));
					maxDistance = Mathf.Max(maxDistance, b2);
				}
			}
			if (maxDistance < 0f)
			{
				throw new Exception("Calculated max distance of DewCollider is invalid");
			}
			for (int iter = 0; iter < iterations; iter++)
			{
				PositionColliders();
				GeneratePointsInCircle(12, maxDistance / (float)iterations * (float)(iter + 1), 360f, points);
				points[12] = points[0];
				helperCol.points = points;
				helperCol.edgeRadius = maxDistance / (float)iterations;
				((Component)(object)helperCol).transform.position = new Vector3(transform.position.x, transform.position.z, 0f);
				((Component)(object)helperCol).transform.rotation = Quaternion.Euler(0f, 0f, 0f - transform.rotation.eulerAngles.y);
				Physics2D.autoSyncTransforms = false;
				Physics2D.SyncTransforms();
				Physics2D.autoSyncTransforms = true;
				colsMain.Clear();
				for (int k = 0; k < colliders.Length; k++)
				{
					colsTemp.Clear();
					Physics2D.OverlapCollider(colliders[k]._proxy, new ContactFilter2D
					{
						useLayerMask = true,
						layerMask = LayerMasks.Entity
					}, colsTemp);
					for (int l = 0; l < colsTemp.Count; l++)
					{
						if (!colsMain.Contains(colsTemp[l]))
						{
							colsMain.Add(colsTemp[l]);
						}
					}
				}
				colsHelper.Clear();
				Physics2D.OverlapCollider((Collider2D)(object)helperCol, new ContactFilter2D
				{
					useLayerMask = true,
					layerMask = LayerMasks.Entity
				}, colsHelper);
				ents.Clear();
				for (int m = 0; m < colsMain.Count; m++)
				{
					Collider2D val4 = colsMain[m];
					if (colsHelper.Contains(val4) && DewPhysics.TryGetEntity(val4, out var entity) && !affectedEnts.Contains(entity) && validator(entity) && (includeUncollidable || !entity.Status.hasUncollidable))
					{
						affectedEnts.Add(entity);
						ents.Add(entity);
					}
				}
				yield return ents;
			}
		}
		finally
		{
			colsTempHandle.Return();
			colsMainHandle.Return();
			colsHelperHandle.Return();
			affectedHandle.Return();
			if ((UnityEngine.Object)(object)helperCol != null)
			{
				UnityEngine.Object.Destroy(((Component)(object)helperCol).gameObject);
			}
		}
	}

	public void GeneratePolygonPoints_Circle(float circleRadius, int accuracy = 20)
	{
		GeneratePolygonPoints_ArcDonut(0f, circleRadius, 360f, accuracy);
	}

	public void GeneratePolygonPoints_Arc(float arcRadius, float arcAngle, int accuracy = 20)
	{
		GeneratePolygonPoints_ArcDonut(0f, arcRadius, arcAngle, accuracy);
	}

	public void GeneratePolygonPoints_Donut(float innerRadius, float outerRadius, int accuracy = 20)
	{
		GeneratePolygonPoints_ArcDonut(innerRadius, outerRadius, 360f, accuracy);
	}

	public void GeneratePolygonPoints_ArcDonut(float innerRadius, float outerRadius, float arcAngle = 360f, int accuracy = 20)
	{
		if (shape != ColliderShape.Polygon)
		{
			throw new InvalidOperationException();
		}
		if (innerRadius > 0.01f && arcAngle < 359f)
		{
			int num = Mathf.CeilToInt((float)accuracy / 360f * arcAngle);
			Vector2[] destinationArray = new Vector2[num * 2];
			Vector2[] sourceArray = GeneratePointsInCircle(num, outerRadius, arcAngle);
			Vector2[] sourceArray2 = GeneratePointsInCircle(num, innerRadius, arcAngle).Reverse().ToArray();
			Array.Copy(sourceArray, 0, destinationArray, 0, num);
			Array.Copy(sourceArray2, 0, destinationArray, num, num);
			points = destinationArray;
		}
		else if (innerRadius > 0.01f)
		{
			Vector2[] array = new Vector2[accuracy * 2 + 2];
			int num2 = Mathf.CeilToInt((float)accuracy / 360f * arcAngle);
			Vector2[] sourceArray3 = GeneratePointsInCircle(num2, outerRadius, arcAngle);
			Vector2[] sourceArray4 = GeneratePointsInCircle(num2, innerRadius, arcAngle).Reverse().ToArray();
			Array.Copy(sourceArray3, 0, array, 0, num2);
			Array.Copy(sourceArray4, 0, array, num2 + 1, num2);
			array[num2] = array[0];
			array[num2 * 2 + 1] = array[num2 + 1];
			points = array;
		}
		else if (arcAngle < 359f)
		{
			int num3 = Mathf.CeilToInt((float)accuracy / 360f * arcAngle);
			Vector2[] array2 = new Vector2[num3 + 1];
			Array.Copy(GeneratePointsInCircle(num3, outerRadius, arcAngle), 0, array2, 0, num3);
			array2[array2.Length - 1] = Vector2.zero;
			points = array2;
		}
		else
		{
			int count = Mathf.CeilToInt((float)accuracy / 360f * arcAngle);
			points = GeneratePointsInCircle(count, outerRadius, arcAngle);
		}
	}

	public static Vector2[] GeneratePointsInCircle(int count, float radius, float angle)
	{
		Vector2[] result = new Vector2[count];
		GeneratePointsInCircle(count, radius, angle, result);
		return result;
	}

	public static void GeneratePointsInCircle(int count, float radius, float angle, Vector2[] points)
	{
		float num = (float)Math.PI * -2f * angle / 360f * 0.5f;
		float b = 0f - num;
		for (int i = 0; i < count; i++)
		{
			float f = Mathf.Lerp(num, b, (float)i / (float)(points.Length - 1));
			points[i] = new Vector2(Mathf.Sin(f) * radius, Mathf.Cos(f) * radius);
		}
	}
}
