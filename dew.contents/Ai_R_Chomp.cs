using Mirror;
using UnityEngine;

public class Ai_R_Chomp : AbilityInstance
{
	public float dashSpeed;

	public float dashGoalDist;

	public GameObject chompHitEffect;

	public ScalingValue damage;

	public ScalingValue healAmount;

	public float postDaze;

	public GameObject chompCasterEffect;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			info.caster.Control.StartDisplacement(new DispByTarget
			{
				affectedByMovementSpeed = false,
				cancelTime = 3f,
				goalDistance = dashGoalDist,
				isCanceledByCC = false,
				isFriendly = true,
				onCancel = Destroy,
				onFinish = OnFinish,
				rotateForward = true,
				speed = dashSpeed,
				target = info.target
			});
		}
	}

	private void OnFinish()
	{
		if (info.caster.IsNullInactiveDeadOrKnockedOut())
		{
			Destroy();
			return;
		}
		info.caster.Control.StartDaze(postDaze);
		FxPlayNetworked(chompHitEffect, info.target);
		FxPlayNetworked(chompCasterEffect, info.caster);
		Damage(damage).SetOriginPosition(info.caster.position).Dispatch(info.target);
		DoHeal(new HealData(GetValue(healAmount)), info.caster);
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
