using Mirror;
using UnityEngine;

public class Ai_E_Blink_Back : AbilityInstance
{
	public GameObject teleportDestEffect;

	public GameObject prepareEffect;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		Vector3 validAgentDestination_Closest = Dew.GetValidAgentDestination_Closest(info.caster.agentPosition, ((St_E_Blink)firstTrigger).backLocation);
		position = info.caster.agentPosition;
		Vector3 vector = validAgentDestination_Closest - position;
		rotation = Quaternion.LookRotation(vector).Flattened();
		FxPlay(prepareEffect);
		if (((NetworkBehaviour)this).isServer)
		{
			info.caster.Control.Rotate(-vector, immediately: true);
			Teleport(info.caster, validAgentDestination_Closest);
			FxPlayNewNetworked(teleportDestEffect, validAgentDestination_Closest, Quaternion.identity);
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
