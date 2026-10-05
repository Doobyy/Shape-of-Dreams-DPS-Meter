using Mirror;
using UnityEngine;

public class Ai_Mon_Forest_BossDemon_Blink : AbilityInstance
{
	public DewEase ease;

	public float duration;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			Vector3 validAgentDestination_LinearSweep = Dew.GetValidAgentDestination_LinearSweep(info.caster.agentPosition, info.point);
			info.caster.Control.StartDisplacement(new DispByDestination
			{
				destination = validAgentDestination_LinearSweep,
				canGoOverTerrain = false,
				duration = duration,
				ease = ease,
				isCanceledByCC = false,
				isFriendly = true,
				onCancel = Destroy,
				onFinish = Destroy,
				rotateForward = true
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
