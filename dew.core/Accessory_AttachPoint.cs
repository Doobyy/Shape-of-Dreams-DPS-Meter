using System;
using System.Collections.Generic;
using UnityEngine;

public class Accessory_AttachPoint : MonoBehaviour
{
	[Serializable]
	public class AttachOffset
	{
		public string entityName;

		public Vector3 localPosition;

		public Vector3 localRotation;

		public float localScale = 1f;
	}

	public AccAttachPointType type;

	public List<AttachOffset> customOffsets = new List<AttachOffset>();

	private Transform _modelRoot;

	private Vector3 _modelOriginalLocalScale;

	private Transform _pointTransform;

	private AccAttachPointType _resolvedType;

	private Transform _entityRoot;

	private EntityModel _model;

	private string _cachedOriginalName;

	public void BeginRuntime(Transform entityRoot)
	{
		_entityRoot = entityRoot;
		_model = null;
	}

	private void LateUpdate()
	{
		if (!_entityRoot)
		{
			return;
		}
		if (!_model || !_model.gameObject.activeInHierarchy)
		{
			EntityModel componentInChildren = _entityRoot.GetComponentInChildren<EntityModel>();
			if (!componentInChildren)
			{
				_model = null;
				return;
			}
			if (componentInChildren != _model)
			{
				_model = componentInChildren;
				_cachedOriginalName = Dew.GetOriginalName(componentInChildren.name);
			}
		}
		UpdatePosition(_model.transform, _cachedOriginalName);
	}

	private void FindPointTransform()
	{
		if (!_modelRoot)
		{
			throw new NullReferenceException("_modelRoot");
		}
		_pointTransform = null;
		List<EntityVisualPoint> componentsInChildrenNonAlloc = ((Component)_modelRoot).GetComponentsInChildrenNonAlloc(out ListReturnHandle<EntityVisualPoint> handle);
		List<EntityAccPoint> componentsInChildrenNonAlloc2 = ((Component)_modelRoot).GetComponentsInChildrenNonAlloc(out ListReturnHandle<EntityAccPoint> handle2);
		Animator componentInChildren = _modelRoot.GetComponentInChildren<Animator>();
		if (type == AccAttachPointType.HatCustom || type == AccAttachPointType.HatHalo)
		{
			EntityVisualPoint entityVisualPoint = componentsInChildrenNonAlloc.Find((EntityVisualPoint v) => v.type == EntityVisualPointType.Head);
			if ((bool)entityVisualPoint)
			{
				_pointTransform = entityVisualPoint.transform;
			}
			else if ((bool)(UnityEngine.Object)(object)componentInChildren && componentInChildren.isHuman)
			{
				_pointTransform = componentInChildren.GetBoneTransform((HumanBodyBones)10);
			}
			if (!_pointTransform)
			{
				SetFallbackAndReportFailure();
			}
			handle.Return();
			handle2.Return();
		}
		else if (type == AccAttachPointType.HipCustom)
		{
			if ((bool)(UnityEngine.Object)(object)componentInChildren && componentInChildren.isHuman)
			{
				_pointTransform = componentInChildren.GetBoneTransform((HumanBodyBones)0);
			}
			if (!_pointTransform)
			{
				SetFallbackAndReportFailure();
			}
			handle.Return();
			handle2.Return();
		}
		else
		{
			EntityAccPoint entityAccPoint = componentsInChildrenNonAlloc2.Find((EntityAccPoint v) => v.type == type);
			if ((bool)entityAccPoint)
			{
				_pointTransform = entityAccPoint.transform;
			}
			handle.Return();
			handle2.Return();
			if (!_pointTransform)
			{
				SetFallbackAndReportFailure();
			}
		}
		void SetFallbackAndReportFailure()
		{
			_pointTransform = _modelRoot;
			Debug.LogWarning($"Failed to get {type} point from {_modelRoot}", _modelRoot);
		}
	}

	public void UpdatePosition(Transform modelRoot, string entityOrSkinName = null)
	{
		EnsureResolved(modelRoot);
		if (!_pointTransform)
		{
			return;
		}
		AttachOffset attachOffset = FindOffset(entityOrSkinName);
		Vector3 localScale = modelRoot.localScale;
		Vector3 vector = new Vector3(localScale.x / _modelOriginalLocalScale.x, localScale.y / _modelOriginalLocalScale.y, localScale.z / _modelOriginalLocalScale.z);
		Vector3 vector2;
		Quaternion quaternion;
		Vector3 vector3;
		if (attachOffset != null)
		{
			vector2 = _pointTransform.TransformPoint(attachOffset.localPosition);
			quaternion = _pointTransform.rotation * Quaternion.Euler(attachOffset.localRotation);
			vector3 = vector * attachOffset.localScale;
		}
		else
		{
			vector2 = _pointTransform.position;
			quaternion = _pointTransform.rotation;
			vector3 = vector;
		}
		if (IsFinite(vector2) && IsFinite(quaternion) && IsFinite(vector3))
		{
			transform.localScale = vector3;
			if (type == AccAttachPointType.HatHalo && Application.IsPlaying(this))
			{
				vector2 += Vector3.up * (Mathf.Sin(Time.time * 0.6f) * 0.07f);
				quaternion = quaternion.Flattened();
			}
			transform.SetPositionAndRotation(vector2, quaternion);
		}
	}

	public Transform ResolvePointTransform(Transform modelRoot)
	{
		if (!modelRoot)
		{
			return null;
		}
		EnsureResolved(modelRoot);
		return _pointTransform;
	}

	private void EnsureResolved(Transform modelRoot)
	{
		if (_modelRoot != modelRoot)
		{
			_modelRoot = modelRoot;
			_modelOriginalLocalScale = modelRoot.localScale;
			_resolvedType = type;
			FindPointTransform();
		}
		else if (_resolvedType != type)
		{
			_resolvedType = type;
			FindPointTransform();
		}
	}

	public AttachOffset FindOffset(string entityOrSkinName)
	{
		if (string.IsNullOrEmpty(entityOrSkinName))
		{
			return null;
		}
		List<AttachOffset> list = customOffsets;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].entityName == entityOrSkinName)
			{
				return list[i];
			}
		}
		if (!entityOrSkinName.StartsWith("Skin_", StringComparison.Ordinal))
		{
			return null;
		}
		int num = entityOrSkinName.IndexOf('_', 5);
		if (num < 0)
		{
			num = entityOrSkinName.Length;
		}
		int num2 = num - 5;
		if (num2 <= 0)
		{
			return null;
		}
		for (int j = 0; j < list.Count; j++)
		{
			string entityName = list[j].entityName;
			if (entityName == null || entityName.Length != 5 + num2 || entityName[0] != 'H' || entityName[1] != 'e' || entityName[2] != 'r' || entityName[3] != 'o' || entityName[4] != '_')
			{
				continue;
			}
			bool flag = true;
			for (int k = 0; k < num2; k++)
			{
				if (entityName[5 + k] != entityOrSkinName[5 + k])
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return list[j];
			}
		}
		return null;
	}

	private static bool IsFinite(Vector3 v)
	{
		if (!float.IsNaN(v.x) && !float.IsNaN(v.y) && !float.IsNaN(v.z) && !float.IsInfinity(v.x) && !float.IsInfinity(v.y))
		{
			return !float.IsInfinity(v.z);
		}
		return false;
	}

	private static bool IsFinite(Quaternion q)
	{
		if (!float.IsNaN(q.x) && !float.IsNaN(q.y) && !float.IsNaN(q.z) && !float.IsNaN(q.w) && !float.IsInfinity(q.x) && !float.IsInfinity(q.y) && !float.IsInfinity(q.z))
		{
			return !float.IsInfinity(q.w);
		}
		return false;
	}
}
