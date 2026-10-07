using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossPolaris_Monster_Dash : AbilityInstance
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			CreateBasicEffect(info.caster, new UnstoppableEffect(), 3600f).DestroyOnDestroy(this);
			info.caster.Control.StartDisplacement(new DispByDestination
			{
				duration = Mathf.Clamp(Vector3.Distance(info.caster.agentPosition, info.point) / 20f, 0.2f, 0.5f) * 0.75f,
				affectedByMovementSpeed = false,
				canGoOverTerrain = true,
				destination = info.point,
				ease = DewEase.EaseInOutQuad,
				isFriendly = true,
				isCanceledByCC = false,
				onCancel = DestroyIfActive,
				onFinish = DestroyIfActive,
				rotateForward = true,
				rotateSmoothly = true
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
