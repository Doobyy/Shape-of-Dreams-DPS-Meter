using System;
using System.Collections.Generic;
using UnityEngine;

public static class DewPhysics
{
	public class EntitySweep
	{
		private List<Entity> _current = new List<Entity>();

		private List<Entity> _collided = new List<Entity>();

		public float radius = 1f;

		public bool ignoreDuplicateHits = true;

		public bool includeUncollidable;

		public AbilityTargetValidator validator;

		public Entity validatorSelf;

		public IReadOnlyList<Entity> current => _current;

		public Vector3 lastPosition { get; private set; } = Vector3.positiveInfinity;

		private bool ValidateEntity(Entity ent)
		{
			if (validator != null && !validator.Evaluate(validatorSelf, ent))
			{
				return false;
			}
			if (!includeUncollidable && ent.Status.hasUncollidable)
			{
				return false;
			}
			return true;
		}

		public IReadOnlyList<Entity> Next(Vector3 position)
		{
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			if (lastPosition == Vector3.positiveInfinity)
			{
				List<Collider2D> list = DewPool.GetList(out ListReturnHandle<Collider2D> handle);
				Physics2D.OverlapCircle(position.ToXY(), radius, EntityFilter, list);
				for (int i = 0; i < list.Count; i++)
				{
					if (TryGetEntity(list[i], out var entity) && ValidateEntity(entity))
					{
						_current.Add(entity);
						_collided.Add(entity);
					}
				}
				handle.Return();
			}
			else
			{
				_current.Clear();
				List<RaycastHit2D> list2 = DewPool.GetList(out ListReturnHandle<RaycastHit2D> handle2);
				Physics2D.CircleCast(lastPosition.ToXY(), radius, (Vector2)(position - lastPosition), EntityFilter, list2, Vector3.Distance(position, lastPosition));
				for (int j = 0; j < list2.Count; j++)
				{
					RaycastHit2D val = list2[j];
					if (TryGetEntity(val.collider, out var entity2) && ValidateEntity(entity2) && (!ignoreDuplicateHits || !_collided.Contains(entity2)))
					{
						_current.Add(entity2);
						_collided.Add(entity2);
					}
				}
				handle2.Return();
			}
			lastPosition = position;
			return current;
		}
	}

	private static readonly Func<Entity, bool> _alwaysTrue = (Entity _) => true;

	private static Entity[] _sortEntitiesBuf;

	private static float[] _sortKeysBuf;

	private static readonly Dictionary<int, ProxyCollider> _proxyByCollider = new Dictionary<int, ProxyCollider>(256);

	private static readonly Stack<ProxyCollider> _entityProxyPool = new Stack<ProxyCollider>();

	private static ContactFilter2D EntityFilter
	{
		get
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			return new ContactFilter2D
			{
				useLayerMask = true,
				layerMask = LayerMasks.Entity,
				useTriggers = true
			};
		}
	}

	public static List<Entity> SphereCastAllEntities(out ListReturnHandle<Entity> handle, Vector3 origin, float radius, Vector3 direction, float maxDistance, CollisionCheckSettings settings = default(CollisionCheckSettings))
	{
		return SphereCastAllEntities(out handle, origin, radius, direction, maxDistance, _alwaysTrue, settings);
	}

	public static List<Entity> SphereCastAllEntities(out ListReturnHandle<Entity> handle, Vector3 origin, float radius, Vector3 direction, float maxDistance, IEntityValidator validator, CollisionCheckSettings settings = default(CollisionCheckSettings))
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		maxDistance = (direction.normalized * maxDistance).Flattened().magnitude;
		List<RaycastHit2D> list = DewPool.GetList(out ListReturnHandle<RaycastHit2D> handle2);
		Physics2D.CircleCast(origin.ToXY(), radius, direction.ToXY(), EntityFilter, list, maxDistance);
		List<Entity> list2 = DewPool.GetList(out handle);
		for (int i = 0; i < list.Count; i++)
		{
			RaycastHit2D val = list[i];
			if (TryGetEntity(val.collider, out var entity) && entity.isActive && validator.Evaluate(entity) && (settings.includeUncollidable || !entity.Status.hasUncollidable))
			{
				list2.Add(entity);
			}
		}
		handle2.Return();
		SortList(settings, list2, origin);
		return list2;
	}

	public static List<Entity> SphereCastAllEntities(out ListReturnHandle<Entity> handle, Vector3 origin, float radius, Vector3 direction, float maxDistance, IBinaryEntityValidator validator, Entity self, CollisionCheckSettings settings = default(CollisionCheckSettings))
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		maxDistance = (direction.normalized * maxDistance).Flattened().magnitude;
		List<RaycastHit2D> list = DewPool.GetList(out ListReturnHandle<RaycastHit2D> handle2);
		Physics2D.CircleCast(origin.ToXY(), radius, direction.ToXY(), EntityFilter, list, maxDistance);
		List<Entity> list2 = DewPool.GetList(out handle);
		for (int i = 0; i < list.Count; i++)
		{
			RaycastHit2D val = list[i];
			if (TryGetEntity(val.collider, out var entity) && entity.isActive && validator.Evaluate(self, entity) && (settings.includeUncollidable || !entity.Status.hasUncollidable))
			{
				list2.Add(entity);
			}
		}
		handle2.Return();
		SortList(settings, list2, origin);
		return list2;
	}

	public static List<Entity> SphereCastAllEntities(out ListReturnHandle<Entity> handle, Vector3 origin, float radius, Vector3 direction, float maxDistance, Func<Entity, bool> validator, CollisionCheckSettings settings = default(CollisionCheckSettings))
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		maxDistance = (direction.normalized * maxDistance).Flattened().magnitude;
		List<RaycastHit2D> list = DewPool.GetList(out ListReturnHandle<RaycastHit2D> handle2);
		Physics2D.CircleCast(origin.ToXY(), radius, direction.ToXY(), EntityFilter, list, maxDistance);
		List<Entity> list2 = DewPool.GetList(out handle);
		for (int i = 0; i < list.Count; i++)
		{
			RaycastHit2D val = list[i];
			if (TryGetEntity(val.collider, out var entity) && entity.isActive && validator(entity) && (settings.includeUncollidable || !entity.Status.hasUncollidable))
			{
				list2.Add(entity);
			}
		}
		handle2.Return();
		SortList(settings, list2, origin);
		return list2;
	}

	public static List<Entity> OverlapCircleAllEntities(out ListReturnHandle<Entity> handle, Vector3 center, float radius, CollisionCheckSettings settings = default(CollisionCheckSettings))
	{
		return OverlapCircleAllEntities(out handle, center, radius, _alwaysTrue, settings);
	}

	public static List<Entity> OverlapCircleAllEntities(out ListReturnHandle<Entity> handle, Vector3 center, float radius, IEntityValidator validator, CollisionCheckSettings settings = default(CollisionCheckSettings))
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		List<Collider2D> list = DewPool.GetList(out ListReturnHandle<Collider2D> handle2);
		Physics2D.OverlapCircle(center.ToXY(), radius, EntityFilter, list);
		List<Entity> list2 = DewPool.GetList(out handle);
		for (int i = 0; i < list.Count; i++)
		{
			if (TryGetEntity(list[i], out var entity) && entity.isActive && validator.Evaluate(entity) && (settings.includeUncollidable || !entity.Status.hasUncollidable))
			{
				list2.Add(entity);
			}
		}
		handle2.Return();
		SortList(settings, list2, center);
		return list2;
	}

	public static List<Entity> OverlapCircleAllEntities(out ListReturnHandle<Entity> handle, Vector3 center, float radius, IBinaryEntityValidator validator, Entity self, CollisionCheckSettings settings = default(CollisionCheckSettings))
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		List<Collider2D> list = DewPool.GetList(out ListReturnHandle<Collider2D> handle2);
		Physics2D.OverlapCircle(center.ToXY(), radius, EntityFilter, list);
		List<Entity> list2 = DewPool.GetList(out handle);
		for (int i = 0; i < list.Count; i++)
		{
			if (TryGetEntity(list[i], out var entity) && entity.isActive && validator.Evaluate(self, entity) && (settings.includeUncollidable || !entity.Status.hasUncollidable))
			{
				list2.Add(entity);
			}
		}
		handle2.Return();
		SortList(settings, list2, center);
		return list2;
	}

	public static List<Entity> OverlapCircleAllEntities(out ListReturnHandle<Entity> handle, Vector3 center, float radius, Func<Entity, bool> validator, CollisionCheckSettings settings = default(CollisionCheckSettings))
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		List<Collider2D> list = DewPool.GetList(out ListReturnHandle<Collider2D> handle2);
		Physics2D.OverlapCircle(center.ToXY(), radius, EntityFilter, list);
		List<Entity> list2 = DewPool.GetList(out handle);
		for (int i = 0; i < list.Count; i++)
		{
			if (TryGetEntity(list[i], out var entity) && entity.isActive && validator(entity) && (settings.includeUncollidable || !entity.Status.hasUncollidable))
			{
				list2.Add(entity);
			}
		}
		handle2.Return();
		SortList(settings, list2, center);
		return list2;
	}

	internal static void SortList(CollisionCheckSettings settings, List<Entity> list, Vector3 distanceCheckPivot)
	{
		if (settings.sortComparer == null)
		{
			return;
		}
		if (settings.sortComparer == CollisionCheckSettings.DistanceFromCenter)
		{
			SortByDistanceSquared(list, distanceCheckPivot);
		}
		else if (settings.sortComparer == CollisionCheckSettings.DistanceFromPivot)
		{
			SortByDistanceSquared(list, CollisionCheckSettings.pivot);
		}
		else if (settings.sortComparer == CollisionCheckSettings.Random)
		{
			int count = list.Count;
			while (count > 1)
			{
				int num = UnityEngine.Random.Range(0, count--);
				int index = count;
				int index2 = num;
				Entity entity = list[num];
				Entity entity2 = list[count];
				Entity entity3 = (list[index] = entity);
				entity3 = (list[index2] = entity2);
			}
		}
		else
		{
			list.Sort(0, list.Count, settings.sortComparer);
		}
	}

	private static void SortByDistanceSquared(List<Entity> list, Vector3 pivot)
	{
		int count = list.Count;
		if (count <= 1)
		{
			return;
		}
		if (_sortEntitiesBuf == null || _sortEntitiesBuf.Length < count)
		{
			int num = Mathf.Max(count, (_sortEntitiesBuf == null) ? 64 : (_sortEntitiesBuf.Length * 2));
			_sortEntitiesBuf = new Entity[num];
			_sortKeysBuf = new float[num];
		}
		float x = pivot.x;
		float y = pivot.y;
		float z = pivot.z;
		for (int i = 0; i < count; i++)
		{
			Entity entity = list[i];
			Vector3 position = entity.position;
			float num2 = position.x - x;
			float num3 = position.y - y;
			float num4 = position.z - z;
			_sortEntitiesBuf[i] = entity;
			_sortKeysBuf[i] = num2 * num2 + num3 * num3 + num4 * num4;
		}
		for (int j = 1; j < count; j++)
		{
			float num5 = _sortKeysBuf[j];
			Entity entity2 = _sortEntitiesBuf[j];
			int num6 = j - 1;
			while (num6 >= 0 && _sortKeysBuf[num6] > num5)
			{
				_sortKeysBuf[num6 + 1] = _sortKeysBuf[num6];
				_sortEntitiesBuf[num6 + 1] = _sortEntitiesBuf[num6];
				num6--;
			}
			_sortKeysBuf[num6 + 1] = num5;
			_sortEntitiesBuf[num6 + 1] = entity2;
		}
		list.Clear();
		for (int k = 0; k < count; k++)
		{
			list.Add(_sortEntitiesBuf[k]);
			_sortEntitiesBuf[k] = null;
		}
	}

	public static bool TryGetProxy(Collider2D collider, out ProxyCollider proxy)
	{
		if ((UnityEngine.Object)(object)collider == null)
		{
			proxy = null;
			return false;
		}
		int instanceID = ((UnityEngine.Object)(object)collider).GetInstanceID();
		if (_proxyByCollider.TryGetValue(instanceID, out proxy))
		{
			if (proxy != null)
			{
				return true;
			}
			_proxyByCollider.Remove(instanceID);
		}
		if (((Component)(object)collider).TryGetComponent(out proxy))
		{
			_proxyByCollider[instanceID] = proxy;
			return true;
		}
		return false;
	}

	public static bool TryGetEntity(Collider2D collider, out Entity entity)
	{
		if (!TryGetProxy(collider, out var proxy) || (UnityEngine.Object)(object)proxy.entity == null)
		{
			entity = null;
			return false;
		}
		entity = proxy.entity;
		return true;
	}

	public static bool TryGetInteractable(Collider2D collider, out IInteractable interactable)
	{
		if (!TryGetProxy(collider, out var proxy) || proxy.interactable == null)
		{
			interactable = null;
			return false;
		}
		interactable = proxy.interactable;
		return true;
	}

	public static bool TryGetCollidableWithProjectile(Collider2D collider, out ICollidableWithProjectile collidable)
	{
		if (!TryGetProxy(collider, out var proxy) || proxy.collidableWithProjectile == null)
		{
			collidable = null;
			return false;
		}
		collidable = proxy.collidableWithProjectile;
		return true;
	}

	internal static GameObject AddEmptyObject(string name)
	{
		Transform transform = ManagerBase<DewPhysicsManager>.instance.transform;
		GameObject gameObject = new GameObject(name);
		gameObject.transform.parent = transform.transform;
		gameObject.layer = 12;
		return gameObject;
	}

	internal static ProxyCollider AddProxyOfEntity(Entity entity)
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		ProxyCollider proxyCollider = null;
		while (_entityProxyPool.Count > 0)
		{
			ProxyCollider proxyCollider2 = _entityProxyPool.Pop();
			if (proxyCollider2 != null)
			{
				proxyCollider = proxyCollider2;
				break;
			}
		}
		if (proxyCollider == null)
		{
			GameObject gameObject = new GameObject("Entity Proxy");
			gameObject.transform.parent = ManagerBase<DewPhysicsManager>.instance.transform;
			gameObject.layer = 8;
			gameObject.SetActive(value: false);
			proxyCollider = gameObject.AddComponent<ProxyCollider>();
			proxyCollider.collider = (Collider2D)(object)gameObject.AddComponent<CircleCollider2D>();
			gameObject.AddComponent<Rigidbody2D>().isKinematic = true;
			proxyCollider.isEntityProxy = true;
		}
		proxyCollider.isPooled = false;
		((CircleCollider2D)proxyCollider.collider).radius = entity.Control.outerRadius;
		proxyCollider.entity = entity;
		UpdateProxyPosition(proxyCollider, entity.position);
		proxyCollider.gameObject.SetActive(value: true);
		_proxyByCollider[((UnityEngine.Object)(object)proxyCollider.collider).GetInstanceID()] = proxyCollider;
		return proxyCollider;
	}

	internal static ProxyCollider AddProxyOfCollidableWithProjectile(ICollidableWithProjectile collidable, float radius = 0.25f)
	{
		Transform transform = ManagerBase<DewPhysicsManager>.instance.transform;
		string text = "ICollidableWithProjectile";
		if (collidable is Component component)
		{
			text = component.name;
		}
		ProxyCollider proxyCollider = new GameObject("Collidable With Projectile Proxy - " + text).AddComponent<ProxyCollider>();
		proxyCollider.transform.parent = transform.transform;
		proxyCollider.gameObject.layer = 14;
		CircleCollider2D val = proxyCollider.gameObject.AddComponent<CircleCollider2D>();
		val.radius = radius;
		proxyCollider.collider = (Collider2D)(object)val;
		proxyCollider.collidableWithProjectile = collidable;
		_proxyByCollider[((UnityEngine.Object)(object)val).GetInstanceID()] = proxyCollider;
		return proxyCollider;
	}

	internal static void UpdateProxyPosition(ProxyCollider proxy, Vector3 position)
	{
		proxy.transform.position = new Vector3(position.x, position.z, 0f);
	}

	internal static void RemoveProxy(ProxyCollider proxy)
	{
		if (proxy == null || (proxy.isEntityProxy && proxy.isPooled))
		{
			return;
		}
		if (proxy.gameObject == null)
		{
			if ((UnityEngine.Object)(object)proxy.collider != null)
			{
				_proxyByCollider.Remove(((UnityEngine.Object)(object)proxy.collider).GetInstanceID());
			}
		}
		else if (proxy.isEntityProxy)
		{
			proxy.isPooled = true;
			proxy.gameObject.SetActive(value: false);
			if ((UnityEngine.Object)(object)proxy.collider != null)
			{
				_proxyByCollider.Remove(((UnityEngine.Object)(object)proxy.collider).GetInstanceID());
			}
			proxy.entity = null;
			proxy.interactable = null;
			proxy.collidableWithProjectile = null;
			_entityProxyPool.Push(proxy);
		}
		else
		{
			if ((UnityEngine.Object)(object)proxy.collider != null)
			{
				_proxyByCollider.Remove(((UnityEngine.Object)(object)proxy.collider).GetInstanceID());
			}
			UnityEngine.Object.Destroy(proxy.gameObject);
		}
	}
}
