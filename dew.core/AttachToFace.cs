using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class AttachToFace : MonoBehaviour
{
	public SkinnedMeshRenderer targetRenderer;

	public int vertexIndex0 = -1;

	public int vertexIndex1 = -1;

	public int vertexIndex2 = -1;

	public Vector3 relPosition;

	public Quaternion relRotation;

	public Vector3 lastSetupPosition;

	public Quaternion lastSetupRotation;

	private bool _hasLastSnapshot;

	private Vector3 _lastLocalPosition;

	private Quaternion _lastLocalRotation;

	private float _lastSnapshotTime;

	private float _lastSnapshotInterval;

	private Vector3 _estimatedLocalVelocity;

	private Vector3 _estimatedLocalAngularVelocity;

	private Vector3 _targetLocalPosition;

	private Quaternion _targetLocalRotation;

	private Vector3 _cv;

	private Quaternion _cav;

	private Mesh _editModeMesh;

	private void EnsureEditModeMesh()
	{
		if (!(_editModeMesh != null))
		{
			_editModeMesh = new Mesh
			{
				hideFlags = HideFlags.HideAndDontSave
			};
		}
	}

	private void Update()
	{
		if (!Application.IsPlaying(this))
		{
			if (transform.position == lastSetupPosition && transform.rotation == lastSetupRotation)
			{
				return;
			}
			lastSetupPosition = transform.position;
			lastSetupRotation = transform.rotation;
			if (targetRenderer == null)
			{
				targetRenderer = GetComponentInParent<SkinnedMeshRenderer>();
			}
			if (targetRenderer == null)
			{
				return;
			}
			EnsureEditModeMesh();
			targetRenderer.BakeMesh(_editModeMesh, useScale: false);
			List<int> list = DewPool.GetList(out ListReturnHandle<int> handle);
			List<Vector3> list2 = DewPool.GetList(out ListReturnHandle<Vector3> handle2);
			_editModeMesh.GetTriangles(list, 0);
			_editModeMesh.GetVertices(list2);
			vertexIndex0 = -1;
			float num = float.PositiveInfinity;
			Transform t = targetRenderer.transform;
			for (int i = 0; i < list.Count; i += 3)
			{
				Vector3 vector = list2[list[i]];
				Vector3 vector2 = list2[list[i + 1]];
				Vector3 vector3 = list2[list[i + 2]];
				Vector3 localPos = (vector + vector2 + vector3) / 3f;
				Vector3 b = TransformPointUnscaled(t, localPos);
				float num2 = Vector3.Distance(transform.position, b);
				if (num2 < num)
				{
					num = num2;
					vertexIndex0 = list[i];
					vertexIndex1 = list[i + 1];
					vertexIndex2 = list[i + 2];
				}
			}
			if (vertexIndex0 >= 0)
			{
				Vector3 vector4 = list2[vertexIndex0];
				Vector3 vector5 = list2[vertexIndex1];
				Vector3 vector6 = list2[vertexIndex2];
				Vector3 vector7 = (vector4 + vector5 + vector6) / 3f;
				Quaternion quaternion = Quaternion.Inverse(targetRenderer.transform.rotation);
				Quaternion faceRotation = GetFaceRotation(vector4, vector5, vector6);
				Quaternion quaternion2 = quaternion * transform.rotation;
				relRotation = Quaternion.Inverse(faceRotation) * quaternion2;
				Vector3 vector8 = InverseTransformPointUnscaled(t, transform.position);
				relPosition = Quaternion.Inverse(faceRotation) * (vector8 - vector7);
			}
			handle.Return();
			handle2.Return();
			return;
		}
		if (TryGetWorldPositionAndRotation(out var position, out var rotation))
		{
			Vector3 vector9 = transform.parent.InverseTransformPoint(position);
			Quaternion quaternion3 = Quaternion.Inverse(transform.parent.rotation) * rotation;
			if (!_hasLastSnapshot)
			{
				_hasLastSnapshot = true;
				_lastLocalPosition = vector9;
				_lastLocalRotation = quaternion3;
				_lastSnapshotTime = Time.time;
			}
			_lastSnapshotInterval = Time.time - _lastSnapshotTime;
			if (_lastSnapshotInterval < float.Epsilon)
			{
				_estimatedLocalVelocity = Vector3.zero;
				_estimatedLocalAngularVelocity = Vector3.zero;
			}
			else
			{
				_estimatedLocalVelocity = (vector9 - _lastLocalPosition) / _lastSnapshotInterval;
				(quaternion3 * Quaternion.Inverse(_lastLocalRotation)).ToAngleAxis(out var angle, out var axis);
				if (angle > 180f)
				{
					angle -= 360f;
				}
				_estimatedLocalAngularVelocity = axis * (angle / _lastSnapshotInterval);
			}
			_lastLocalPosition = vector9;
			_lastLocalRotation = quaternion3;
			_lastSnapshotTime = Time.time;
			_targetLocalPosition = vector9;
			_targetLocalRotation = quaternion3;
		}
		else
		{
			Vector3 localPosition = transform.localPosition;
			Quaternion localRotation = transform.localRotation;
			Vector3 targetLocalPosition = localPosition + _estimatedLocalVelocity * Time.deltaTime;
			Quaternion targetLocalRotation = localRotation * Quaternion.AngleAxis(_estimatedLocalAngularVelocity.magnitude * Time.deltaTime, _estimatedLocalAngularVelocity.normalized);
			_targetLocalPosition = targetLocalPosition;
			_targetLocalRotation = targetLocalRotation;
		}
		float num3 = _lastSnapshotInterval * 0.25f;
		Vector3 localPosition2 = Vector3.SmoothDamp(transform.localPosition, _targetLocalPosition, ref _cv, num3);
		Quaternion localRotation2 = QuaternionUtil.SmoothDamp(transform.localRotation, _targetLocalRotation, ref _cav, num3);
		transform.SetLocalPositionAndRotation(localPosition2, localRotation2);
	}

	private static Quaternion GetFaceRotation(Vector3 a, Vector3 b, Vector3 c)
	{
		Vector3 normalized = (b - a).normalized;
		Vector3 normalized2 = (c - a).normalized;
		Vector3 normalized3 = Vector3.Cross(normalized, normalized2).normalized;
		Vector3 vector = -(normalized + normalized2).normalized;
		if (normalized3 == Vector3.zero || vector == Vector3.zero)
		{
			return Quaternion.identity;
		}
		return Quaternion.LookRotation(normalized3, vector);
	}

	private bool TryPopulateVertices(List<Vector3> vertices)
	{
		if (targetRenderer == null)
		{
			return false;
		}
		if (Application.IsPlaying(this))
		{
			Dew.GetBakedMeshOptimized(targetRenderer, out var mesh, out var handle, out var isNew);
			if (isNew)
			{
				mesh.GetVertices(vertices);
			}
			handle.Return();
			return isNew;
		}
		EnsureEditModeMesh();
		targetRenderer.BakeMesh(_editModeMesh, useScale: false);
		_editModeMesh.GetVertices(vertices);
		return true;
	}

	private void OnDrawGizmosSelected()
	{
		if (vertexIndex0 >= 0 && !(targetRenderer == null))
		{
			List<Vector3> list = DewPool.GetList(out ListReturnHandle<Vector3> handle);
			if (!TryPopulateVertices(list))
			{
				handle.Return();
				return;
			}
			Vector3 localPos = list[vertexIndex0];
			Vector3 localPos2 = list[vertexIndex1];
			Vector3 localPos3 = list[vertexIndex2];
			Transform t = targetRenderer.transform;
			Vector3 vector = TransformPointUnscaled(t, localPos);
			Vector3 vector2 = TransformPointUnscaled(t, localPos2);
			Vector3 vector3 = TransformPointUnscaled(t, localPos3);
			Vector3 vector4 = (vector + vector2 + vector3) / 3f;
			Gizmos.color = Color.cyan;
			Gizmos.DrawLine(vector, vector2);
			Gizmos.DrawLine(vector2, vector3);
			Gizmos.DrawLine(vector3, vector);
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(vector4, transform.position);
			handle.Return();
		}
	}

	private bool TryGetWorldPositionAndRotation(out Vector3 position, out Quaternion rotation)
	{
		position = Vector3.zero;
		rotation = Quaternion.identity;
		if (vertexIndex0 < 0 || targetRenderer == null)
		{
			return false;
		}
		List<Vector3> list = DewPool.GetList(out ListReturnHandle<Vector3> handle);
		if (!TryPopulateVertices(list))
		{
			handle.Return();
			return false;
		}
		Vector3 vector = list[vertexIndex0];
		Vector3 vector2 = list[vertexIndex1];
		Vector3 vector3 = list[vertexIndex2];
		Vector3 vector4 = (vector + vector2 + vector3) / 3f;
		Quaternion faceRotation = GetFaceRotation(vector, vector2, vector3);
		Vector3 localPos = vector4 + faceRotation * relPosition;
		position = TransformPointUnscaled(targetRenderer.transform, localPos);
		rotation = targetRenderer.transform.rotation * faceRotation * relRotation;
		handle.Return();
		return true;
	}

	private Vector3 TransformPointUnscaled(Transform t, Vector3 localPos)
	{
		return t.position + t.rotation * localPos;
	}

	private Vector3 InverseTransformPointUnscaled(Transform t, Vector3 worldPos)
	{
		return Quaternion.Inverse(t.rotation) * (worldPos - t.position);
	}

	private void OnDestroy()
	{
		if (_editModeMesh != null)
		{
			if (Application.isPlaying)
			{
				Object.Destroy(_editModeMesh);
			}
			else
			{
				Object.DestroyImmediate(_editModeMesh);
			}
		}
	}
}
