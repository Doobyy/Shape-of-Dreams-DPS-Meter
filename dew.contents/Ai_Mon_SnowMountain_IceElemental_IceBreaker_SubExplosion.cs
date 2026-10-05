using System;
using UnityEngine;

public class Ai_Mon_SnowMountain_IceElemental_IceBreaker_SubExplosion : InstantDamageInstance
{
	[NonSerialized]
	public Vector3 pullDirection;

	public float knockupStrength;

	public float pullDistance;

	public float pullDuration;

	public DewEase pullEase;

	public override bool reuseInRoom => true;

	protected override void OnHit(Entity entity)
	{
		entity.Visual.KnockUp(knockupStrength, isFriendly: false);
		base.OnHit(entity);
		Vector3 validAgentDestination_LinearSweep = Dew.GetValidAgentDestination_LinearSweep(entity.agentPosition, entity.agentPosition + pullDirection * pullDistance);
		entity.Control.StartDisplacement(new DispByDestination
		{
			destination = validAgentDestination_LinearSweep,
			affectedByMovementSpeed = false,
			canGoOverTerrain = false,
			duration = pullDuration,
			ease = pullEase,
			isCanceledByCC = false,
			isFriendly = false,
			rotateForward = false
		});
	}

	private void MirrorProcessed()
	{
	}
}
