using UnityEngine;

public class Ai_Mon_Ink_BossDarkMoon_ThrowSpear_AoE : TickDamageInstance
{
	private float _baseRangeRadius;

	private Vector3 _baseFxLoopScale;

	protected override void Awake()
	{
		base.Awake();
		if (range != null)
		{
			_baseRangeRadius = range.radius;
		}
		if (fxLoop != null)
		{
			_baseFxLoopScale = fxLoop.transform.localScale;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (range != null)
		{
			range.radius = _baseRangeRadius;
		}
		if (fxLoop != null)
		{
			fxLoop.transform.localScale = _baseFxLoopScale;
		}
	}

	private void MirrorProcessed()
	{
	}
}
