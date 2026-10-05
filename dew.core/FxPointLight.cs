using UnityEngine;

[RequireComponent(typeof(Light))]
public class FxPointLight : FxInterpolatedEffectBase
{
	public bool animateIntensity = true;

	public float intensityMultiplier = 1f;

	public bool animateRange;

	public float rangeMultiplier = 1f;

	public bool animateColor;

	public Gradient colorOverValue = new Gradient();

	[SerializeField]
	[HideInInspector]
	private bool _didGetValues;

	[SerializeField]
	[HideInInspector]
	private float _originalIntensity;

	[SerializeField]
	[HideInInspector]
	private float _originalRange;

	private Light _light;

	private bool _valueSetterInit;

	private bool _lastEnabled;

	private float _lastIntensity;

	private float _lastRange;

	private Color _lastColor;

	protected override void OnInit()
	{
		base.OnInit();
		_light = GetComponent<Light>();
		_valueSetterInit = false;
		if (!_didGetValues)
		{
			_originalIntensity = _light.intensity;
			_originalRange = _light.range;
			_didGetValues = true;
		}
	}

	protected override void ValueSetter(float value)
	{
		bool flag = value > 0f;
		if (!_valueSetterInit || flag != _lastEnabled)
		{
			_light.enabled = flag;
			_lastEnabled = flag;
		}
		if (animateIntensity)
		{
			float num = value * _originalIntensity * intensityMultiplier;
			if (!_valueSetterInit || num != _lastIntensity)
			{
				_light.intensity = num;
				_lastIntensity = num;
			}
		}
		if (animateRange)
		{
			float num2 = value * _originalRange * rangeMultiplier;
			if (!_valueSetterInit || num2 != _lastRange)
			{
				_light.range = num2;
				_lastRange = num2;
			}
		}
		if (animateColor)
		{
			Color color = colorOverValue.Evaluate(value);
			if (!_valueSetterInit || color != _lastColor)
			{
				_light.color = color;
				_lastColor = color;
			}
		}
		_valueSetterInit = true;
	}
}
