using Mirror;
using UnityEngine;

public class Ai_Star_Bismuth_F_IT_SmallerRingAddDisplacement_Dash : AbilityInstance
{
	public float dashDistance = 4f;

	public float dashSpeed = 30f;

	public DewEase ease = DewEase.EaseOutQuad;

	public bool unstoppableWhileDashing = true;

	public GameObject fxDash;

	public DewAnimationClip dashAnim;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		Entity caster = info.caster;
		Vector3 forward = info.forward;
		Vector3 end = caster.agentPosition + forward * dashDistance;
		end = Dew.GetValidAgentDestination_Closest(caster.agentPosition, end);
		float num = Vector3.Distance(end, caster.agentPosition);
		if (num < 0.1f)
		{
			Destroy();
			return;
		}
		if (unstoppableWhileDashing)
		{
			CreateBasicEffect(caster, new UnstoppableEffect(), num / dashSpeed, "it_dash_unstoppable");
		}
		FxPlayNetworked(fxDash, caster);
		if (dashAnim != null)
		{
			caster.Animation.PlayAbilityAnimation(dashAnim);
		}
		caster.Control.StartDisplacement(new DispByDestination
		{
			affectedByMovementSpeed = false,
			destination = end,
			duration = num / dashSpeed,
			ease = ease,
			isCanceledByCC = false,
			isFriendly = true,
			rotateForward = true,
			onFinish = Destroy,
			onCancel = Destroy
		});
	}

	private void MirrorProcessed()
	{
	}
}
