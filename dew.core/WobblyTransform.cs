using UnityEngine;

public class WobblyTransform : MonoBehaviour
{
	public float positionTargetInterval = 0.25f;

	public Vector3 positionTargetMagnitude;

	public float positionSmoothTime = 0.8f;

	public float rotationTargetInterval = 0.25f;

	public Vector3 rotationTargetMagnitude;

	public float rotationSmoothTime = 0.8f;

	private Vector3 _originalPosition;

	private Quaternion _originalRotation;

	private Vector3 _targetPosition;

	private Quaternion _targetRotation;

	private float _lastPositionTargetUpdateTime;

	private float _lastRotationTargetUpdateTime;

	private Vector3 _cv;

	private Quaternion _cvRot;

	private void Start()
	{
		_originalPosition = transform.localPosition;
		_originalRotation = transform.localRotation;
	}

	private void Update()
	{
		if (Time.time - _lastPositionTargetUpdateTime > positionTargetInterval)
		{
			_lastPositionTargetUpdateTime = Time.time;
			_targetPosition = _originalPosition + new Vector3(Random.Range(0f - positionTargetMagnitude.x, positionTargetMagnitude.x), Random.Range(0f - positionTargetMagnitude.y, positionTargetMagnitude.y), Random.Range(0f - positionTargetMagnitude.z, positionTargetMagnitude.z));
		}
		if (Time.time - _lastRotationTargetUpdateTime > rotationTargetInterval)
		{
			_lastRotationTargetUpdateTime = Time.time;
			Vector3 euler = new Vector3(Random.Range(0f - rotationTargetMagnitude.x, rotationTargetMagnitude.x), Random.Range(0f - rotationTargetMagnitude.y, rotationTargetMagnitude.y), Random.Range(0f - rotationTargetMagnitude.z, rotationTargetMagnitude.z));
			_targetRotation = Quaternion.Euler(euler) * _originalRotation;
		}
		transform.localPosition = Vector3.SmoothDamp(transform.localPosition, _targetPosition, ref _cv, positionSmoothTime);
		transform.localRotation = QuaternionUtil.SmoothDamp(transform.localRotation, _targetRotation, ref _cvRot, rotationSmoothTime);
	}
}
