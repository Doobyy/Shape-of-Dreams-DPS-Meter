using UnityEngine;

[ExecuteAlways]
public class AttachToCamera : MonoBehaviour
{
	public bool preserveLocalPosition;

	public bool preserveLocalRotation;

	private Vector3 _localPos;

	private Quaternion _localRot = Quaternion.identity;

	private void Awake()
	{
		if (preserveLocalPosition)
		{
			_localPos = transform.localPosition;
		}
		if (preserveLocalRotation)
		{
			_localRot = transform.localRotation;
		}
	}

	private void LateUpdate()
	{
		if (!(Dew.mainCamera == null))
		{
			Transform transform = Dew.mainCamera.transform;
			base.transform.SetPositionAndRotation(transform.TransformPoint(_localPos), _localRot * transform.rotation);
		}
	}
}
