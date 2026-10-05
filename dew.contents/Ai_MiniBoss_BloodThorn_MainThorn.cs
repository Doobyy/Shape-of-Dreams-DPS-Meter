using System;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(GenericTransformSync))]
public class Ai_MiniBoss_BloodThorn_MainThorn : InstantDamageInstance
{
	public float speed = 3f;

	public int subThornMinCount = 3;

	public float subThornRandomRange;

	[NonSerialized]
	public bool isTargetMode = true;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		isTargetMode = true;
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			Vector3 current = position;
			if (!isTargetMode)
			{
				current = Vector3.MoveTowards(current, info.point, dt * speed);
				position = current;
			}
			else if (!info.target.IsNullInactiveDeadOrKnockedOut())
			{
				current = Vector3.MoveTowards(current, info.target.agentPosition, dt * speed);
				position = current;
			}
		}
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		if (entity.Status.TryGetStatusEffect<Se_MiniBoss_BloodThorn_Bleeding>(out var effect))
		{
			effect.ResetTimer();
		}
		else
		{
			CreateStatusEffect<Se_MiniBoss_BloodThorn_Bleeding>(entity, new CastInfo(info.caster, entity));
		}
	}

	protected override void OnAfterDelay()
	{
		base.OnAfterDelay();
		int num = Mathf.RoundToInt(NetworkedManagerBase<GameManager>.instance.GetSpecialSkillChanceMultiplier()) + subThornMinCount;
		float minInclusive = range.radius + 1.5f;
		for (int i = 0; i < num; i++)
		{
			if (info.caster.IsNullInactiveDeadOrKnockedOut())
			{
				break;
			}
			float num2 = UnityEngine.Random.Range(minInclusive, subThornRandomRange);
			Vector3 vector = position + UnityEngine.Random.insideUnitCircle.ToXZ() * num2;
			vector = Dew.GetPositionOnGround(vector);
			CreateAbilityInstance<Ai_MiniBoss_BloodThorn_Projectile>(position, null, new CastInfo(info.caster, vector));
		}
	}

	private void MirrorProcessed()
	{
	}
}
