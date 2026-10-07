using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Se_Mon_Special_BossMaw_ShadowOverdrive_Initial : StatusEffect
{
	public float maxSlowRatio;

	public float slowDuration;

	public float postDelay;

	public float armorBonus;

	public int missileCount;

	public Vector2 missileRange;

	public float targetRange;

	public float missileDelay;

	public float missileInterval;

	private float _baseSpeed;

	private float _targetSpeed;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(victim);
			_baseSpeed = ((Component)(object)DewResources.GetByType<Mon_Special_BossMaw>(ResourceLoadSettings.Light)).GetComponent<EntityControl>().baseAgentSpeed;
			_targetSpeed = _baseSpeed * (1f - maxSlowRatio);
			victim.Control.StartChannel(new Channel
			{
				blockedActions = (Channel.BlockedAction.Ability | Channel.BlockedAction.Attack),
				duration = slowDuration,
				onCancel = RpcRestoreBaseAgentSpeed,
				onComplete = RpcRestoreBaseAgentSpeed
			});
			DoArmorBoost(armorBonus);
			SetTimer(slowDuration);
			yield return null;
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (normalizedDuration.HasValue)
		{
			float t = 1f - normalizedDuration.Value;
			victim.Control.baseAgentSpeed = Mathf.Lerp(_baseSpeed, _targetSpeed, t);
			victim.Animation.model.walkAnimationSpeed = Mathf.Lerp(1f, 0.1f, t);
		}
	}

	[ClientRpc]
	private void RpcRestoreBaseAgentSpeed()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Se_Mon_Special_BossMaw_ShadowOverdrive_Initial::RpcRestoreBaseAgentSpeed()", -935634803, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !victim.IsNullInactiveDeadOrKnockedOut())
		{
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			victim.Control.StartChannel(new Channel
			{
				blockedActions = (Channel.BlockedAction.Ability | Channel.BlockedAction.Attack),
				duration = postDelay + missileInterval * (float)missileCount
			});
			for (int i = 0; i < missileCount; i++)
			{
				Vector3 vector = info.caster.agentPosition + Random.insideUnitCircle.normalized.ToXZ() * Random.Range(5f, missileRange.y);
				List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, info.caster.agentPosition, targetRange, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
				{
					sortComparer = CollisionCheckSettings.Random
				});
				if (list.Count > 0 && Random.value < 0.5f)
				{
					Entity entity = list[Random.Range(0, list.Count)];
					vector = AbilityTrigger.PredictPoint_Simple(victim, Random.Range(0.55f, 1f), entity, missileDelay) + Random.insideUnitCircle.ToXZ().normalized * Random.Range(0f, 3f);
					vector = Dew.GetValidAgentDestination_Closest(entity.agentPosition, vector);
					vector = Dew.GetPositionOnGround(vector);
					Vector3 vector2 = vector - info.caster.agentPosition;
					if (vector2.sqrMagnitude < missileRange.x * missileRange.x)
					{
						vector2 = vector2.normalized * missileRange.x;
						vector = info.caster.agentPosition + vector2;
						vector = Dew.GetValidAgentDestination_Closest(entity.agentPosition, vector);
						vector = Dew.GetPositionOnGround(vector);
					}
				}
				handle.Return();
				CreateAbilityInstance<Ai_Mon_Special_BossMaw_ShadowOverdrive_Projectile>(victim.position, null, new CastInfo(info.caster, vector));
				yield return new SI.WaitForSeconds(missileInterval);
			}
		}
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcRestoreBaseAgentSpeed()
	{
		victim.Control.baseAgentSpeed = _baseSpeed;
		victim.Animation.model.walkAnimationSpeed = 1f;
	}

	protected static void InvokeUserCode_RpcRestoreBaseAgentSpeed(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRestoreBaseAgentSpeed called on server.");
		}
		else
		{
			((Se_Mon_Special_BossMaw_ShadowOverdrive_Initial)(object)obj).UserCode_RpcRestoreBaseAgentSpeed();
		}
	}

	static Se_Mon_Special_BossMaw_ShadowOverdrive_Initial()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Se_Mon_Special_BossMaw_ShadowOverdrive_Initial), "System.Void Se_Mon_Special_BossMaw_ShadowOverdrive_Initial::RpcRestoreBaseAgentSpeed()", (RemoteCallDelegate)InvokeUserCode_RpcRestoreBaseAgentSpeed);
	}
}
