using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable]
[VolumeComponentMenuForRenderPipeline("Dew/Dew Post Processing", new Type[] { typeof(UniversalRenderPipeline) })]
public class DewPostProcessingComponent : VolumeComponent, IPostProcessComponent
{
	public FloatParameter sobelThickness = new FloatParameter(1f, false);

	public FloatParameter sobelMultiplier = new FloatParameter(1f, false);

	public FloatParameter highlightedBoost = new FloatParameter(100f, false);

	public FloatParameter cartoonOutline = new FloatParameter(0f, false);

	public ClampedFloatParameter vfIntensity = new ClampedFloatParameter(0f, 0f, 1f, false);

	public NoInterpFloatRangeParameter vfRange = new NoInterpFloatRangeParameter(new Vector2(0f, 10f), -100f, 100f, false);

	public ClampedFloatParameter vfIntensityBottom = new ClampedFloatParameter(0f, 0f, 1f, false);

	public NoInterpFloatRangeParameter vfRangeBottom = new NoInterpFloatRangeParameter(new Vector2(-100f, -50f), -100f, 100f, false);

	public NoInterpColorParameter vfColor = new NoInterpColorParameter(Color.black, false);

	public NoInterpTextureParameter vfNoise = new NoInterpTextureParameter((Texture)null, false);

	public NoInterpVector2Parameter vfNoiseScale = new NoInterpVector2Parameter(Vector2.one, false);

	public NoInterpFloatParameter vfNoiseWeight = new NoInterpFloatParameter(1f, false);

	public NoInterpVector2Parameter vfNoiseScroll = new NoInterpVector2Parameter(new Vector2(0f, 0f), false);

	public NoInterpIntParameter vfSteps = new NoInterpIntParameter(10, false);

	public bool IsActive()
	{
		return true;
	}

	public bool IsTileCompatible()
	{
		return true;
	}

	public DewPostProcessingComponent()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected Obj, but got Unknown
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected Obj, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected Obj, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected Obj, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected Obj, but got Unknown
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Expected Obj, but got Unknown
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected Obj, but got Unknown
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Expected Obj, but got Unknown
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Expected Obj, but got Unknown
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Expected Obj, but got Unknown
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Expected Obj, but got Unknown
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Expected Obj, but got Unknown
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Expected Obj, but got Unknown
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Expected Obj, but got Unknown
	}
}
