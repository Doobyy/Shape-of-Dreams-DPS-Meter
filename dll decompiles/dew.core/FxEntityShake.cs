using UnityEngine;

public class FxEntityShake : FxInterpolatedEffectBase, IAttachableToEntity
{
	public float shakeIntensity = 0.1f;

	public float shakeInterval = 1f / 60f;

	public bool useUnscaledInterval;

	public bool onlyOnXZPlane = true;

	private EntityTransformModifier _modifier;

	private Entity _target;

	private float _lastShakeTime = float.NegativeInfinity;

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
	}

	protected override void Update()
	{
		base.Update();
		float num = (useUnscaledInterval ? Time.unscaledTime : Time.time);
		if (_modifier != null && num - _lastShakeTime > shakeInterval)
		{
			_lastShakeTime = num;
			Vector3 vector = (onlyOnXZPlane ? Random.insideUnitCircle.ToXZ() : Random.insideUnitSphere);
			_modifier.worldOffset = vector * (shakeIntensity * currentValue);
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
