using UnityEngine;

public class FxCanvasGroup : FxInterpolatedEffectBase
{
	public Vector2 alphaRange = new Vector2(0f, 1f);

	private CanvasGroup _cg;

	protected override void ValueSetter(float value)
	{
		if (!(Object)(object)_cg)
		{
			_cg = GetComponent<CanvasGroup>();
		}
		if ((bool)(Object)(object)_cg)
		{
			_cg.alpha = alphaRange.Lerp(value);
		}
	}
}
