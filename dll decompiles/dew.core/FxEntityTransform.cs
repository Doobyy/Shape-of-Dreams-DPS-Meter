using UnityEngine;

public class FxEntityTransform : FxInterpolatedEffectBase, IAttachableToEntity
{
	public Vector3 worldOffset;

	public Vector3 localOffset;

	public Vector3 rotation;

	public Vector3 scaleMultiplier = Vector3.one;

	private EntityTransformModifier _modifier;

	private Entity _target;

	private void OnDestroy()
	{
		Cleanup();
	}

	public void OnAttachToEntity(Entity target)
	{
		_target = target;
	}

	protected override void ValueSetter(float value)
	{
		if (value < 0.0001f)
		{
			Cleanup();
		}
		else if ((Object)(object)_target != null && _modifier == null)
		{
			_modifier = _target.Visual.GetNewTransformModifier();
		}
		if (_modifier != null)
		{
			_modifier.worldOffset = worldOffset * value;
			_modifier.localOffset = localOffset * value;
			_modifier.rotation = Quaternion.Euler(rotation * value);
			_modifier.scaleMultiplier = Vector3.Lerp(Vector3.one, scaleMultiplier, value);
		}
	}

	private void Cleanup()
	{
		if (_modifier != null)
		{
			_modifier.Stop();
			_modifier = null;
		}
	}
}
