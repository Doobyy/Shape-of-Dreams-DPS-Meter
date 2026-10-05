using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossMaw_DarkSpear : DashAttackInstance
{
	public float stunDuration;

	public float displaceDuration;

	public float randomMagnitude;

	public DewEase ease;

	private Vector3 _dashDest;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			_dashDest = info.caster.agentPosition + info.forward * dash.distance;
		}
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		if (((NetworkBehaviour)this).isServer && !entity.Status.hasCrowdControlImmunity)
		{
			CreateBasicEffect(entity, new StunEffect(), stunDuration, "stun_bossmawspear");
			float num = Vector3.Dot(entity.agentPosition - _dashDest, info.forward);
			Vector3 end = _dashDest + info.forward * num + Random.insideUnitCircle.ToXZ() * randomMagnitude;
			end = Dew.GetValidAgentDestination_Closest(_dashDest, end);
			entity.Control.StartDisplacement(new DispByDestination
			{
				canGoOverTerrain = false,
				affectedByMovementSpeed = false,
				destination = end,
				duration = displaceDuration,
				ease = ease,
				isFriendly = false,
				rotateForward = false,
				isCanceledByCC = false
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
