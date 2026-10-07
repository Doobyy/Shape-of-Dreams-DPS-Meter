using UnityEngine;

public abstract class FxInterpolatedEffectBase : MonoBehaviour, IEffectComponent, IEffectWithSpeed
{
	public bool isLoop = true;

	public float delay;

	public float startTime = 0.25f;

	public float sustainTime;

	public float decayTime = 0.25f;

	private float _emitStartTime;

	private bool _didInit;

	protected bool _isInActiveTransition;

	private float _origDelay;

	private float _origStartTime;

	private float _origSustainTime;

	private float _origDecayTime;

	private static readonly EaseFunction s_easeInOutSine = EasingFunction.GetEasingFunction(DewEase.EaseInOutSine);

	private EaseFunction _easeFunction;

	public bool isPlaying
	{
		get
		{
			if (!isEmitting)
			{
				return currentValue > 0f;
			}
			return true;
		}
	}

	public bool isLooping => isLoop;

	public float currentValue { get; set; }

	public float currentLinearValue { get; set; }

	public bool isEmitting { get; private set; }

	protected virtual void OnInit()
	{
		_easeFunction = s_easeInOutSine;
	}

	private void InitIfDidnt()
	{
		if (!_didInit)
		{
			_didInit = true;
			_origDelay = delay;
			_origStartTime = startTime;
			_origSustainTime = sustainTime;
			_origDecayTime = decayTime;
			OnInit();
		}
	}

	protected void Awake()
	{
		InitIfDidnt();
	}

	protected virtual void Start()
	{
		ValueSetter(currentValue);
	}

	public virtual void Play()
	{
		InitIfDidnt();
		currentLinearValue = 0f;
		currentValue = 0f;
		if (isLoop)
		{
			sustainTime = float.PositiveInfinity;
		}
		isEmitting = true;
		_emitStartTime = Time.time;
		if (delay <= 0.0001f && startTime <= 0.0001f)
		{
			currentLinearValue = 1f;
			currentValue = _easeFunction(0f, 1f, currentLinearValue);
		}
		ValueSetter(currentValue);
	}

	public virtual void Stop()
	{
		InitIfDidnt();
		if (isPlaying)
		{
			isEmitting = false;
			if (decayTime <= 0.0001f || !isActiveAndEnabled)
			{
				currentLinearValue = 0f;
				currentValue = _easeFunction(0f, 1f, currentLinearValue);
				ValueSetter(currentValue);
			}
		}
	}

	protected virtual void OnEnable()
	{
		if (_didInit)
		{
			_isInActiveTransition = true;
			ValueSetter(currentValue);
			_isInActiveTransition = false;
		}
	}

	protected virtual void OnDisable()
	{
		_isInActiveTransition = true;
		ValueSetter(0f);
		_isInActiveTransition = false;
		if (!isEmitting)
		{
			currentLinearValue = 0f;
			currentValue = 0f;
		}
	}

	protected virtual void Update()
	{
		if (!isEmitting && currentLinearValue <= 0f)
		{
			return;
		}
		float num = Time.time - _emitStartTime;
		if (isEmitting && num > delay)
		{
			if (startTime <= 0.0001f)
			{
				currentLinearValue = 1f;
			}
			else
			{
				currentLinearValue = Mathf.MoveTowards(currentLinearValue, 1f, 1f / startTime * Time.deltaTime);
			}
		}
		else if (!isEmitting)
		{
			if (decayTime <= 0f)
			{
				currentLinearValue = 0f;
			}
			else
			{
				currentLinearValue = Mathf.MoveTowards(currentLinearValue, 0f, 1f / decayTime * Time.deltaTime);
			}
		}
		if (isEmitting && num > delay + startTime + sustainTime)
		{
			isEmitting = false;
		}
		float num2 = _easeFunction(0f, 1f, currentLinearValue);
		if (currentValue != num2)
		{
			currentValue = num2;
			ValueSetter(currentValue);
		}
	}

	protected abstract void ValueSetter(float value);

	public void ApplySpeedMultiplier(float speed)
	{
		InitIfDidnt();
		delay = _origDelay / speed;
		startTime = _origStartTime / speed;
		sustainTime = (float.IsInfinity(sustainTime) ? sustainTime : (_origSustainTime / speed));
		decayTime = _origDecayTime / speed;
	}
}
