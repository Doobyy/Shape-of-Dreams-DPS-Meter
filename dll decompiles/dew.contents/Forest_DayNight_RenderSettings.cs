using System;
using UnityEngine;

public class Forest_DayNight_RenderSettings : Forest_DayNight_Base
{
	[Serializable]
	public struct Data
	{
		public Color ambientSky;

		public Color ambientEquator;

		public Color ambientGround;

		public Color fog;

		public float fogStartDist;

		public float fogEndDist;
	}

	public Data day;

	public Data night;

	public override void Update()
	{
		base.Update();
		float t = Mathf.PingPong(animatedValue, 1f);
		RenderSettings.ambientSkyColor = Color.Lerp(day.ambientSky, night.ambientSky, t);
		RenderSettings.ambientEquatorColor = Color.Lerp(day.ambientEquator, night.ambientEquator, t);
		RenderSettings.ambientGroundColor = Color.Lerp(day.ambientGround, night.ambientGround, t);
		RenderSettings.fogColor = Color.Lerp(day.fog, night.fog, t);
		RenderSettings.fogStartDistance = Mathf.Lerp(day.fogStartDist, night.fogStartDist, t);
		RenderSettings.fogEndDistance = Mathf.Lerp(day.fogEndDist, night.fogEndDist, t);
	}
}
