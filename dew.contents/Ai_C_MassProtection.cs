using Mirror;
using UnityEngine;

public class Ai_C_MassProtection : AbilityInstance
{
	public float postDaze = 0.25f;

	public float radius = 10f;

	public GameObject fxHit;

	public ScalingValue shieldAmount;

	public float shieldDuration;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		info.caster.Control.StartChannel(new Channel
		{
			duration = postDaze,
			blockedActions = (Channel.BlockedAction.Ability | Channel.BlockedAction.Attack)
		});
		ListReturnHandle<Entity> handle;
		foreach (Entity item in DewPhysics.OverlapCircleAllEntities(out handle, position, radius, tvDefaultUsefulEffectTargets, new CollisionCheckSettings
		{
			includeUncollidable = true
		}))
		{
			if (!item.IsNullInactiveDeadOrKnockedOut())
			{
				if ((Object)(object)item != (Object)(object)info.caster)
				{
					FxPlayNewNetworked(fxHit, item, item.Visual.GetCenterPosition(), Quaternion.LookRotation(item.agentPosition - info.caster.position).Flattened());
				}
				GiveShield(item, GetValue(shieldAmount), shieldDuration, isDecay: true);
			}
		}
		handle.Return();
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
