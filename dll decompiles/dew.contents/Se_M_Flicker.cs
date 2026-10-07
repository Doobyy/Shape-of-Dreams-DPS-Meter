using System.Collections;
using Mirror;
using UnityEngine;

public class Se_M_Flicker : StatusEffect
{
	public float disappearTime;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		info.caster.Visual.DisableRenderersLocal();
		if (((NetworkBehaviour)this).isServer)
		{
			DoInvulnerable();
			DoUncollidable();
			Vector3 dest = Dew.GetValidAgentDestination_Closest(info.caster.agentPosition, info.point);
			info.caster.Control.StartDaze(disappearTime);
			info.caster.Control.StartDisplacement(new DispByDestination
			{
				canGoOverTerrain = true,
				destination = dest,
				duration = disappearTime,
				ease = DewEase.EaseOutQuad,
				isCanceledByCC = false,
				isFriendly = true,
				isDodging = true,
				rotateForward = false
			});
			yield return new SI.WaitForSeconds(disappearTime);
			FxPlayNewNetworked(endEffect, dest, info.caster.rotation);
			Destroy();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		info.caster.Visual.EnableRenderersLocal();
	}

	private void MirrorProcessed()
	{
	}
}
