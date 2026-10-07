using UnityEngine;

[DisallowMultipleComponent]
public class FxGameObject : FxInterpolatedEffectBase
{
	public bool disableWhenStopped = true;

	[SerializeField]
	[HideInInspector]
	private Vector3 _startScale = new Vector3(float.NaN, float.NaN, float.NaN);

	protected override void OnInit()
	{
		base.OnInit();
		if (float.IsNaN(_startScale.x))
		{
			_startScale = transform.localScale;
		}
	}

	public override void Play()
	{
		base.Play();
		currentValue = 0.001f;
		ValueSetter(0.001f);
	}

	protected override void ValueSetter(float value)
	{
		transform.localScale = value * _startScale;
		if (disableWhenStopped && !_isInActiveTransition)
		{
			gameObject.SetActive(isEmitting || value > 0f);
		}
	}
}
