using Mirror;
using UnityEngine;

public class Ai_Mon_LavaLand_FireElemental_BackDash : AbilityInstance
{
	public float dashDuration;

	public float dashDistance;

	public float finishDelay;

	public GameObject dashEffect;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		CreateBasicEffect(info.caster, new UnstoppableEffect(), dashDuration);
		FxPlayNetworked(dashEffect, info.caster);
		Vector3 vector = info.caster.position + (info.caster.position - info.target.position).normalized * dashDistance;
		info.caster.Control.Rotate(info.caster.position - vector, immediately: false, dashDuration);
		info.caster.Control.StartDisplacement(new DispByDestination
		{
			affectedByMovementSpeed = true,
			destination = vector,
			duration = dashDuration,
			ease = DewEase.EaseOutQuad,
			isFriendly = true,
			onFinish = () =>
			{
				if (isActive)
				{
					info.caster.Control.StartDaze(finishDelay);
					Destroy();
				}
			},
			onCancel = Destroy,
			rotateForward = false,
			canGoOverTerrain = false,
			isCanceledByCC = false
		});
	}

	private void MirrorProcessed()
	{
	}
}
