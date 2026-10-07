using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_R_NaturesWhisper : AbilityInstance
{
	public float targetedAttackTargetSearchRadius = 3f;

	public GameObject fxAttackTarget;

	public GameObject fxMove;

	[NonSerialized]
	public bool doTeleport;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		doTeleport = false;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		((Component)(object)this).transform.position = info.point;
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (info.caster is Hero hero)
		{
			Entity entity = null;
			List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, info.point, targetedAttackTargetSearchRadius, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
			{
				sortComparer = CollisionCheckSettings.DistanceFromCenter
			});
			if (list.Count > 0)
			{
				entity = list[0];
			}
			handle.Return();
			if (hero.Status.TryGetStatusEffect<Se_R_NaturesWhisper_Buff>(out var effect))
			{
				effect.Destroy();
			}
			CreateStatusEffect<Se_R_NaturesWhisper_Buff>(hero);
			foreach (Summon summon in hero.summons)
			{
				if (summon.Status.TryGetStatusEffect<Se_R_NaturesWhisper_Buff>(out var effect2))
				{
					effect2.Destroy();
				}
				CreateStatusEffect<Se_R_NaturesWhisper_Buff>(summon);
				if ((UnityEngine.Object)(object)entity != null)
				{
					summon.SummonCommand_AttackTarget(entity);
				}
				else
				{
					summon.SummonCommand_MoveToDestination(info.point);
				}
				if (doTeleport)
				{
					Vector3 pos = Dew.GetValidAgentDestination_LinearSweep(info.point, Dew.GetPositionOnGround(info.point + (UnityEngine.Random.onUnitSphere * 3f).Flattened()));
					pos = Dew.GetValidAgentDestination_Closest(info.caster.position, pos);
					summon.Control.CancelOngoingChannels();
					summon.Control.Stop();
					CreateStatusEffect(summon, new CastInfo(summon), (Se_BarrierPassThrough se) =>
					{
						se.destination = pos;
						se.duration = 0.5f;
					});
				}
			}
			if ((UnityEngine.Object)(object)entity == null)
			{
				FxPlayNetworked(fxMove);
				CreateAbilityInstance(info.caster.position, null, new CastInfo(info.caster, info.point), (Ai_R_NaturesWhisper_MockProjectile p) =>
				{
					p.mode = Projectile.ProjectileMode.Point;
				});
			}
			else
			{
				FxPlayNetworked(fxAttackTarget, entity);
				CreateAbilityInstance(info.caster.position, null, new CastInfo(info.caster, entity), (Ai_R_NaturesWhisper_MockProjectile p) =>
				{
					p.mode = Projectile.ProjectileMode.Target;
				});
			}
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
