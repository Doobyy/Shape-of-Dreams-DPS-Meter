using Mirror;
using UnityEngine;

public class Se_Mon_Special_BossPolaris_Holy_Dash : StatusEffect
{
	public DewEase ease = DewEase.EaseOutQuad;

	public float speed = 20f;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoUnstoppable();
			victim.Visual.DisableRenderers();
			Vector3 validAgentPosition = Dew.GetValidAgentPosition(Dew.GetPositionOnGround(info.point));
			victim.Control.StartDisplacement(new DispByDestination
			{
				destination = validAgentPosition,
				affectedByMovementSpeed = false,
				canGoOverTerrain = true,
				duration = Vector3.Distance(validAgentPosition, info.caster.agentPosition) / speed,
				ease = ease,
				isFriendly = true,
				rotateForward = false,
				rotateSmoothly = false,
				isCanceledByCC = false,
				onCancel = DestroyIfActive,
				onFinish = DestroyIfActive
			});
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (Object)(object)victim != null)
		{
			victim.Visual.EnableRenderers();
		}
	}

	private void MirrorProcessed()
	{
	}
}
