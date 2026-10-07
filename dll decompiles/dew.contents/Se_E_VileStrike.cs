using Mirror;
using UnityEngine;

public class Se_E_VileStrike : StatusEffect
{
	public float dashDuration;

	public DewEase dashEase;

	private Vector3 _dest;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoUntargetable();
			DoInvulnerable();
			DoUncollidable();
			victim.Visual.DisableRenderers();
			_dest = Dew.GetValidAgentDestination_Closest(victim.agentPosition, info.point);
			victim.Control.StartDisplacement(new DispByDestination
			{
				duration = dashDuration,
				ease = dashEase,
				destination = _dest,
				isFriendly = true,
				onCancel = DestroyIfActive,
				onFinish = Finish,
				rotateForward = true,
				canGoOverTerrain = true,
				isCanceledByCC = false
			});
		}
	}

	private void Finish()
	{
		CreateAbilityInstance<Ai_E_VileStrike_Damage>(_dest, null, new CastInfo(info.caster));
		victim.Control.Rotate(ManagerBase<CameraManager>.instance.entityCamAngle - 50f, immediately: true);
		victim.Control.StartDaze(0.1f);
		DestroyIfActive();
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
