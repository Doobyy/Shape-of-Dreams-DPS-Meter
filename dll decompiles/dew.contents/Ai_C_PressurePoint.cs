using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class Ai_C_PressurePoint : AbilityInstance
{
	public DewCollider range;

	public ScalingValue dmgFactor;

	public float stunDmgRatio;

	public GameObject punchEffect;

	public GameObject hitEffect;

	public GameObject trailEffect;

	public GameObject stunEffect;

	public float stunDuration;

	public float knockbackMaxDist;

	public float knockbackSpeed;

	public DewEase easeOnHitWall;

	public DewEase ease;

	private int _generation;

	public float wallStunDmg => GetValue(dmgFactor) * stunDmgRatio;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		_generation++;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		int gen = _generation;
		DestroyOnDeath(info.caster);
		FxPlayNetworked(punchEffect);
		ListReturnHandle<Entity> handle;
		NavMeshHit val = default;
		foreach (Entity ent in range.GetEntities(out handle, tvDefaultHarmfulEffectTargets))
		{
			FxPlayNewNetworked(hitEffect, ent);
			FxPlayNewNetworked(trailEffect, ent);
			Damage(dmgFactor).SetDirection(rotation).Dispatch(ent);
			if (ent.Status.hasCrowdControlImmunity)
			{
				Damage(dmgFactor).ApplyRawMultiplier(stunDmgRatio).SetOriginPosition(info.caster.agentPosition).SetDirection(info.forward)
					.Dispatch(ent);
				FxPlayNetworked(stunEffect, ent);
				continue;
			}
			CreateBasicEffect(ent, new StunEffect(), stunDuration, "pressurepoint_stun", DuplicateEffectBehavior.UsePrevious);
			ent.Control.Rotate(-info.forward, immediately: true);
			Vector3 vector = ent.agentPosition + info.forward * knockbackMaxDist;
			if (NavMesh.Raycast(ent.agentPosition, vector, ref val, -1))
			{
				vector = val.position;
				ent.Control.StartDisplacement(new DispByDestination
				{
					destination = vector,
					canGoOverTerrain = false,
					duration = Vector2.Distance(val.position.ToXY(), ent.agentPosition.ToXY()) / knockbackSpeed,
					ease = easeOnHitWall,
					isCanceledByCC = false,
					isFriendly = false,
					rotateForward = false,
					onFinish = () =>
					{
						if (gen == _generation && isActive)
						{
							Damage(dmgFactor).ApplyRawMultiplier(stunDmgRatio).SetOriginPosition(info.caster.agentPosition).SetDirection(info.forward)
								.Dispatch(ent);
							FxPlayNetworked(stunEffect, ent);
						}
					}
				});
			}
			else
			{
				ent.Control.StartDisplacement(new DispByDestination
				{
					destination = vector,
					canGoOverTerrain = false,
					duration = knockbackMaxDist / knockbackSpeed,
					ease = ease,
					rotateForward = false,
					isCanceledByCC = false,
					isFriendly = false
				});
			}
		}
		handle.Return();
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
