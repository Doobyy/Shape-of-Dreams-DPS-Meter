using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ScreenFx;

[VolumeComponentMenuForRenderPipeline("Weather (ScreenFxSample)", new Type[] { typeof(UniversalRenderPipeline) })]
internal class WeatherSettings : VolumeComponent
{
	public ClampedFloatParameter _snow = new ClampedFloatParameter(0f, 0f, 1f, false);

	public ClampedFloatParameter _sun = new ClampedFloatParameter(0f, 0f, 1f, false);

	public ClampedFloatParameter _clouds = new ClampedFloatParameter(0f, 0f, 1f, false);

	public WeatherSettings()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected Obj, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected Obj, but got Unknown
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Expected Obj, but got Unknown
	}
}
