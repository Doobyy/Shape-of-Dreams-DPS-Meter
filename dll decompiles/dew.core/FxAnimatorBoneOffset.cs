using UnityEngine;

public class FxAnimatorBoneOffset : FxInterpolatedEffectBase, IAttachableToEntity
{
	public Animator animator;

	public HumanBodyBones bone;

	public Vector3 localPosition;

	public Vector3 worldPosition;

	public Vector3 localRotation;

	public Vector3 worldRotation;

	public Vector3 localScale = Vector3.one;

	private Transform _boneTransform;

	protected override void ValueSetter(float value)
	{
	}

	private void LateUpdate()
	{
		if ((bool)_boneTransform)
		{
			Quaternion rotation = _boneTransform.rotation;
			Vector3 position = _boneTransform.position + rotation * (localPosition * currentValue) + worldPosition * currentValue;
			Quaternion rotation2 = Quaternion.Euler(worldRotation * currentValue) * rotation * Quaternion.Euler(localRotation * currentValue);
			_boneTransform.SetPositionAndRotation(position, rotation2);
			_boneTransform.localScale = Vector3.Lerp(Vector3.one, localScale, currentValue);
		}
	}

	public void OnAttachToEntity(Entity target)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		animator = (((Object)(object)target == null) ? null : target.Animation.animator);
		_boneTransform = (((Object)(object)animator == null) ? null : animator.GetBoneTransform(bone));
	}
}
