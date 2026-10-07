using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_R_DancingBlades : TickDamageInstance
{
	public ScalingValue apDamage;

	public ScalingValue adDamage;

	public float attractSpeed;

	public override bool reuseInRoom => true;

	protected override void OnPrepare()
	{
		base.OnPrepare();
		position = info.point;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			CreateAbilityInstance<Ai_R_DancingBlades_MockProjectile>(info.caster.position, null, new CastInfo(info.caster, info.point));
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, position, 10f, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
		{
			sortComparer = CollisionCheckSettings.DistanceFromCenter
		});
		if (list.Count > 0)
		{
			Vector3 v = list[0].position;
			Vector2 vector = v.ToXY() - position.ToXY();
			float magnitude = vector.magnitude;
			if (magnitude <= attractSpeed * dt)
			{
				position = v;
			}
			else
			{
				Vector2 v2 = vector / magnitude;
				position += v2.ToXZ() * (attractSpeed * dt);
			}
		}
		handle.Return();
	}

	protected override void OnCollisionCheck()
	{
		float value = GetValue(apDamage);
		float value2 = GetValue(adDamage);
		if (value > value2)
		{
			dmgFactor = apDamage;
			type = DamageData.SourceType.Magic;
		}
		else
		{
			dmgFactor = adDamage;
			type = DamageData.SourceType.Physical;
		}
		base.OnCollisionCheck();
	}

	private void MirrorProcessed()
	{
	}
}
