using Mirror;
using UnityEngine;

public class Ai_Mon_Despair_Displacer_Blink : AbilityInstance
{
	public float minDistance;

	public Vector2 distanceFromTargetRange;

	public float speed;

	public bool doInvulnerable;

	public bool doUncollidable;

	public float postDelay;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			info.caster.Visual.DisableRenderers();
			DestroyOnDeath(info.caster);
			Vector3 dest = AbilityTrigger.PredictPoint_SpeedAcceleration(info.caster, Random.value, info.target, info.caster.agentPosition, 0f, 0f, speed, speed, 0f);
			dest += Random.insideUnitSphere.Flattened().normalized * Random.Range(distanceFromTargetRange.x, distanceFromTargetRange.y);
			if (dest.magnitude < minDistance)
			{
				dest = dest.normalized * minDistance;
			}
			dest = Dew.GetPositionOnGround(dest);
			dest = Dew.GetValidAgentDestination_Closest(info.target.agentPosition, dest);
			float num = Vector2.Distance(dest.ToXY(), info.caster.agentPosition.ToXY()) / speed;
			info.caster.Control.StartDaze(num + postDelay);
			if (doInvulnerable)
			{
				CreateBasicEffect(info.caster, new InvulnerableEffect(), num, "displacer_blink_invul");
			}
			if (doUncollidable)
			{
				CreateBasicEffect(info.caster, new UncollidableEffect(), num, "displacer_blink_uncol");
			}
			info.caster.Control.StartDisplacement(new DispByDestination
			{
				affectedByMovementSpeed = false,
				canGoOverTerrain = true,
				destination = dest,
				duration = num,
				ease = DewEase.EaseOutQuad,
				isCanceledByCC = false,
				isFriendly = true,
				onCancel = DestroyIfActive,
				onFinish = () =>
				{
					CreateAbilityInstance<Ai_Mon_Despair_Displacer_SpawnEgg>(dest, null, new CastInfo(info.caster));
					DestroyIfActive();
				},
				rotateForward = false
			});
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (Object)(object)info.caster != null)
		{
			if ((Object)(object)info.target != null)
			{
				info.caster.Control.RotateTowards(info.target, immediately: true);
			}
			info.caster.Visual.EnableRenderers();
		}
	}

	private void MirrorProcessed()
	{
	}
}
