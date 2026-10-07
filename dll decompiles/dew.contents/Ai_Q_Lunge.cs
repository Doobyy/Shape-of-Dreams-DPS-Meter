using System;
using System.Collections.Generic;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Ai_Q_Lunge : AbilityInstance
{
	public DewCollider targetRange;

	public DewCollider stabRange;

	public GameObject stabEffect;

	public GameObject stabHitEffect;

	public bool resetAttackCooldown;

	public bool doCalibration;

	public float maxDeviationAngle = 20f;

	public int calibrationSteps = 10;

	public bool lookAtTarget;

	public float overrideRotDuration = 1f;

	public DewAnimationClip endAnimation;

	public ScalingValue dmgFactor;

	public float speed = 10f;

	public float dodgeCooldownReductionRatioOnHit = 0.5f;

	[NonSerialized]
	public bool singleTargetOnly;

	private ActorRef<StatusEffect> _unstoppable;

	private ActorRef<StatusEffect> _uncollidable;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		singleTargetOnly = false;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		Vector3 validAgentDestination_LinearSweep = Dew.GetValidAgentDestination_LinearSweep(info.caster.agentPosition, info.point);
		((Component)(object)this).transform.position = info.caster.Visual.GetCenterPosition();
		((Component)(object)this).transform.rotation = Quaternion.LookRotation(validAgentDestination_LinearSweep - ((Component)(object)info.caster).transform.position);
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			info.caster.Control.StartDisplacement(new DispByDestination
			{
				canGoOverTerrain = true,
				destination = validAgentDestination_LinearSweep,
				duration = Vector3.Distance(((Component)(object)info.caster).transform.position, validAgentDestination_LinearSweep) / speed,
				ease = DewEase.Linear,
				isCanceledByCC = true,
				isFriendly = true,
				rotateForward = true,
				onFinish = FinishDash,
				onCancel = Destroy
			});
			if (resetAttackCooldown)
			{
				ResetCooldown(info.caster.Ability.attackAbility);
			}
			_unstoppable = CreateBasicEffect(info.caster, new UnstoppableEffect(), 2f, "lunge_unstoppable");
			_uncollidable = CreateBasicEffect(info.caster, new UncollidableEffect(), 2f, "lunge_uncollidable");
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if (!_unstoppable.IsNullOrInactive())
			{
				_unstoppable.Get().Destroy();
			}
			if (!_uncollidable.IsNullOrInactive())
			{
				_uncollidable.Get().Destroy();
			}
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		((Component)(object)this).transform.position = info.caster.Visual.GetCenterPosition();
	}

	private void FinishDash()
	{
		List<Entity> entities = targetRange.GetEntities(out var handle, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
		{
			sortComparer = CollisionCheckSettings.DistanceFromCenter
		});
		if (entities.Count == 0)
		{
			handle.Return();
			info.caster.Animation.StopAbilityAnimation();
			Destroy();
			return;
		}
		Entity entity = entities[0];
		Quaternion rot = Quaternion.LookRotation(entity.position - ((Component)(object)this).transform.position).Flattened();
		if (doCalibration)
		{
			float num = float.NegativeInfinity;
			float y = rot.eulerAngles.y;
			for (int i = 0; i < calibrationSteps; i++)
			{
				float num2 = Mathf.Lerp(0f - maxDeviationAngle, maxDeviationAngle, (float)i / (float)(calibrationSteps - 1));
				((Component)(object)this).transform.rotation = Quaternion.Euler(0f, y + num2, 0f);
				List<Entity> entities2 = stabRange.GetEntities(out var handle2, tvDefaultHarmfulEffectTargets);
				float num3 = (float)entities2.Count - Mathf.Abs(num2 / maxDeviationAngle) * 0.5f;
				if (entities2.Contains(entity))
				{
					num3 += 2f;
				}
				if (num3 > num)
				{
					rot = ((Component)(object)this).transform.rotation;
					num = num3;
				}
				handle2.Return();
			}
		}
		((Component)(object)this).transform.rotation = rot;
		RpcDoStabEffects(rot);
		if (lookAtTarget)
		{
			info.caster.Control.RotateTowards(entity, immediately: true, overrideRotDuration);
		}
		else
		{
			info.caster.Control.Rotate(rot, immediately: true, overrideRotDuration);
		}
		info.caster.Animation.PlayAbilityAnimation(endAnimation);
		List<Entity> entities3 = stabRange.GetEntities(out var handle3, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
		{
			sortComparer = CollisionCheckSettings.DistanceFromCenter
		});
		foreach (Entity item in entities3)
		{
			FxPlayNewNetworked(stabHitEffect, item);
			Damage(dmgFactor).DoAttackEffect(AttackEffectType.Others).SetOriginPosition(info.caster.position).Dispatch(item);
			if (singleTargetOnly)
			{
				break;
			}
		}
		if (entities3.Count > 0 && info.caster is Hero hero && (bool)(UnityEngine.Object)(object)hero.Skill.Movement)
		{
			ApplyCooldownReductionByRatio(hero.Skill.Movement, new CooldownReductionByRatioSettings
			{
				ratio = dodgeCooldownReductionRatioOnHit,
				ignoreCanReceiveCooldown = true
			});
		}
		handle3.Return();
		Destroy();
	}

	[ClientRpc]
	private void RpcDoStabEffects(Quaternion rot)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteQuaternion((NetworkWriter)(object)val, rot);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Ai_Q_Lunge::RpcDoStabEffects(UnityEngine.Quaternion)", 277725726, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcDoStabEffects__Quaternion(Quaternion rot)
	{
		((Component)(object)this).transform.rotation = rot;
		FxPlay(stabEffect);
	}

	protected static void InvokeUserCode_RpcDoStabEffects__Quaternion(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDoStabEffects called on server.");
		}
		else
		{
			((Ai_Q_Lunge)(object)obj).UserCode_RpcDoStabEffects__Quaternion(NetworkReaderExtensions.ReadQuaternion(reader));
		}
	}

	static Ai_Q_Lunge()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Ai_Q_Lunge), "System.Void Ai_Q_Lunge::RpcDoStabEffects(UnityEngine.Quaternion)", (RemoteCallDelegate)InvokeUserCode_RpcDoStabEffects__Quaternion);
	}
}
