using System.Collections;
using Mirror;

public class Se_Primus_Pizza_Fly : StatusEffect
{
	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			if (victim.Status.hasCrowdControlImmunity)
			{
				victim.Control.StartDisplacement(new DispByDestination
				{
					destination = info.point,
					ease = DewEase.EaseInOutQuad,
					duration = 0.5f,
					isFriendly = true,
					canGoOverTerrain = true
				});
				Destroy();
			}
			else
			{
				DoStun();
				DoUncollidable();
				PureDamage((victim.currentHealth + victim.Status.currentShield) * 0.5f).Dispatch(victim);
				victim.Control.StartDisplacement(new DispByDestination
				{
					destination = info.point,
					ease = DewEase.EaseOutQuart,
					duration = 1.5f,
					isFriendly = false,
					canGoOverTerrain = true,
					onFinish = DestroyIfActive,
					onCancel = DestroyIfActive,
					rotateForward = false
				});
				victim.Control.Rotate(victim.agentPosition - info.point, immediately: false);
			}
		}
		yield break;
	}

	private void MirrorProcessed()
	{
	}
}
