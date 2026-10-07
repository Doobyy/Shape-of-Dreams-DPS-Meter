using UnityEngine;

public class Forest_DayNight_Effect : Forest_DayNight_Base
{
	public GameObject toDayStart;

	public GameObject toNightStart;

	public override void OnTargetValueChanged()
	{
		base.OnTargetValueChanged();
		if (!Application.IsPlaying(this))
		{
			return;
		}
		if (Mathf.RoundToInt(targetValue) % 2 == 0)
		{
			if (toDayStart != null)
			{
				DewEffect.Play(toDayStart);
			}
		}
		else if (toNightStart != null)
		{
			DewEffect.Play(toNightStart);
		}
	}
}
