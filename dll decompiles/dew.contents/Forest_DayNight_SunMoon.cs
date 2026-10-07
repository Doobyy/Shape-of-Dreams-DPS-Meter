using System;
using UnityEngine;
using UnityEngine.Rendering;

public class Forest_DayNight_SunMoon : Forest_DayNight_Base
{
	[Serializable]
	public struct LightSourceSettings
	{
		public Light light;

		public float intensity;

		public Volume volume;
	}

	public AnimationCurve lightIntensityByDotCurve;

	public LightSourceSettings[] lightSources;

	public Vector3 dayAngleOffset;

	public Vector3 nightAngleOffset;

	public override void Update()
	{
		base.Update();
		transform.localRotation = Quaternion.Euler(new Vector3(180f * animatedValue, 0f, 0f) + Vector3.Lerp(dayAngleOffset, nightAngleOffset, Mathf.Abs(Mathf.PingPong(animatedValue, 1f))));
		LightSourceSettings[] array = lightSources;
		for (int i = 0; i < array.Length; i++)
		{
			LightSourceSettings lightSourceSettings = array[i];
			float time = Vector3.Dot(lightSourceSettings.light.transform.forward, Vector3.down);
			float num = lightIntensityByDotCurve.Evaluate(time);
			lightSourceSettings.light.intensity = lightSourceSettings.intensity * num;
			lightSourceSettings.light.enabled = lightSourceSettings.light.intensity > 0.0001f;
			lightSourceSettings.volume.weight = num;
		}
	}
}
