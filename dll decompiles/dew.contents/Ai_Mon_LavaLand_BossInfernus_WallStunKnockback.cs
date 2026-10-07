using System;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class Ai_Mon_LavaLand_BossInfernus_WallStunKnockback : AbilityInstance
{
	public DewEase ease;

	public DewEase easeOnHitWall;

	public float knockbackMaxDist;

	public float knockbackSpeed;

	public float wallStunDuration;

	public ScalingValue wallStunDmg;

	public GameObject fxWallStun;

	[NonSerialized]
	public float additionalKnockbackDist;

	[NonSerialized]
	public bool useCasterOrigin;

	[NonSerialized]
	private float _baseKnockbackMaxDist;

	[NonSerialized]
	private float _baseKnockbackSpeed;

	[NonSerialized]
	private ScalingValue _baseWallStunDmg;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_baseKnockbackMaxDist = knockbackMaxDist;
		_baseKnockbackSpeed = knockbackSpeed;
		_baseWallStunDmg = wallStunDmg;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		knockbackMaxDist *= Mathf.Max(0.25f, NetworkedManagerBase<GameManager>.instance.GetSpecialSkillChanceMultiplier());
		knockbackMaxDist += additionalKnockbackDist;
		Vector3 vector = info.target.agentPosition + ((Component)(object)this).transform.forward * knockbackMaxDist;
		if (useCasterOrigin)
		{
			vector = info.target.agentPosition + (info.target.agentPosition - info.caster.agentPosition).normalized * knockbackMaxDist;
		}
		NavMeshHit val = default;
		if (NavMesh.Raycast(info.target.agentPosition, vector, ref val, -1))
		{
			vector = val.position;
			info.target.Control.StartDisplacement(new DispByDestination
			{
				destination = vector,
				canGoOverTerrain = false,
				duration = Vector2.Distance(val.position.ToXY(), info.target.agentPosition.ToXY()) / knockbackSpeed,
				ease = easeOnHitWall,
				isCanceledByCC = false,
				isFriendly = false,
				onCancel = Destroy,
				onFinish = () =>
				{
					Damage(wallStunDmg).SetOriginPosition(info.caster.agentPosition).SetDirection(((Component)(object)this).transform.forward).Dispatch(info.target);
					CreateBasicEffect(info.target, new StunEffect(), wallStunDuration, "infernus_wallstun", DuplicateEffectBehavior.UsePrevious);
					FxPlayNetworked(fxWallStun, info.target);
					Destroy();
				}
			});
		}
		else
		{
			info.target.Control.StartDisplacement(new DispByDestination
			{
				destination = vector,
				canGoOverTerrain = false,
				duration = knockbackMaxDist / knockbackSpeed,
				ease = ease,
				isCanceledByCC = false,
				isFriendly = false
			});
			Destroy();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		knockbackMaxDist = _baseKnockbackMaxDist;
		knockbackSpeed = _baseKnockbackSpeed;
		wallStunDmg = _baseWallStunDmg;
		additionalKnockbackDist = 0f;
		useCasterOrigin = false;
	}

	private void MirrorProcessed()
	{
	}
}
