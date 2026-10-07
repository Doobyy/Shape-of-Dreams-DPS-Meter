using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine.Rendering;

public static class DOTweenVolumeExtension
{
	public static TweenerCore<float, float, FloatOptions> DOWeight(this Volume v, float endValue, float duration)
	{
		return TweenSettingsExtensions.SetId<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => v.weight), (DOSetter<float>)((float val) =>
		{
			v.weight = val;
		}), endValue, duration), (object)v);
	}
}
