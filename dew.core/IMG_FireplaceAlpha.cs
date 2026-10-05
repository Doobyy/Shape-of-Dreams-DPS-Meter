using UnityEngine;
using UnityEngine.UI;

public class IMG_FireplaceAlpha : ImageComponentAnimated
{
	public float smoothTime = 0.25f;

	public Vector2 alphaRange = new Vector2(0f, 1f);

	public Vector2 alphaInterval = new Vector2(0.05f, 0.1f);

	private float _nextAlphaChangeTime;

	private float _targetAlpha;

	private float _cv;

	public override void Init()
	{
		base.Init();
		_targetAlpha = ((Graphic)image).color.a;
	}

	public override void Tick()
	{
		base.Tick();
		((Graphic)image).color = ((Graphic)image).color.WithA(Mathf.SmoothDamp(((Graphic)image).color.a, _targetAlpha, ref _cv, smoothTime, float.PositiveInfinity, deltaTime));
		if (time > _nextAlphaChangeTime)
		{
			_targetAlpha = Random.Range(alphaRange.x, alphaRange.y);
			_nextAlphaChangeTime = time + Random.Range(alphaInterval.x, alphaInterval.y);
		}
	}
}
