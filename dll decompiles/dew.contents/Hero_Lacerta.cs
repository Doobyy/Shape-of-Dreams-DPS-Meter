using System;
using FIMSpace.FTail;
using UnityEngine;

public class Hero_Lacerta : Hero
{
	[NonSerialized]
	public TailAnimator2 tailAnimator;

	[NonSerialized]
	public bool actDeadInTutorial;

	private float _defaultWavingSpeed;

	public override void OnModelLoaded()
	{
		base.OnModelLoaded();
		tailAnimator = Visual.model.GetCustomMapping<TailAnimator2>("tailAnimator");
		_defaultWavingSpeed = tailAnimator.WavingSpeed;
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if ((bool)(UnityEngine.Object)(object)tailAnimator)
		{
			tailAnimator.WavingSpeed = ((isKnockedOut || actDeadInTutorial) ? 0f : _defaultWavingSpeed);
			tailAnimator.Gravity = Vector3.MoveTowards(tailAnimator.Gravity, (isKnockedOut || actDeadInTutorial) ? new Vector3(0f, -10f, 0f) : Vector3.zero, dt * 5f);
		}
	}

	private void MirrorProcessed()
	{
	}
}
