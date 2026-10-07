using UnityEngine;

public abstract class ImageComponentAnimated : ImageComponent
{
	public bool useUnscaledTime = true;

	public float time
	{
		get
		{
			if (!useUnscaledTime)
			{
				return Time.time;
			}
			return Time.unscaledTime;
		}
	}

	public float deltaTime
	{
		get
		{
			if (!useUnscaledTime)
			{
				return Time.deltaTime;
			}
			return Time.unscaledDeltaTime;
		}
	}
}
