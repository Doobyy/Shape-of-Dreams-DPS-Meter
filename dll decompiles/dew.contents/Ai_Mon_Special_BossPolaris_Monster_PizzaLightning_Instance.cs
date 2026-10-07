using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class Ai_Mon_Special_BossPolaris_Monster_PizzaLightning_Instance : InstantDamageInstance
{
	public Vector2 deviateAngleRange;

	[NonSerialized]
	public float initialDeviationAngle = float.NaN;

	protected override void OnPrepare()
	{
		base.OnPrepare();
		initialDeviationAngle = float.NaN;
		if (float.IsNaN(initialDeviationAngle))
		{
			initialDeviationAngle = deviateAngleRange.RandomRange() * (float)((!(UnityEngine.Random.value < 0.5f)) ? 1 : (-1));
		}
	}

	protected override void OnCreate()
	{
		Quaternion quaternion = ((Component)(object)this).transform.rotation;
		((Component)(object)this).transform.rotation = Quaternion.Euler(0f, quaternion.eulerAngles.y + initialDeviationAngle, 0f);
		base.OnCreate();
		TweenSettingsExtensions.SetEase<TweenerCore<Quaternion, Quaternion, NoOptions>>(ShortcutExtensions.DORotateQuaternion(((Component)(object)this).transform, quaternion, damageDelay - 0.05f), (Ease)6);
	}

	private void MirrorProcessed()
	{
	}
}
