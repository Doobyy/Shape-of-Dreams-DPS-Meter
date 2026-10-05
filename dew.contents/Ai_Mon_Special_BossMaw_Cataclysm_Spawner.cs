using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Ai_Mon_Special_BossMaw_Cataclysm_Spawner : AbilityInstance
{
	public float slowRatio;

	public int spawnCount;

	public float maxRange;

	public float spawnInterval;

	public float spawnMaxDeviation;

	public float dmgDelay;

	public float castDuration;

	public float delayAfterTransform;

	public float delayAfterMeteor;

	public float postDelay;

	public GameObject fxTransform;

	public DewAnimationClip castClip;

	public EntityModel heroModel;

	private float _baseSpeed;

	protected override IEnumerator OnCreateSequenced()
	{
		_baseSpeed = ((Component)(object)DewResources.GetByType<Mon_Special_BossMaw>(ResourceLoadSettings.Light)).GetComponent<EntityControl>().baseAgentSpeed;
		info.caster.Control.baseAgentSpeed = _baseSpeed * (1f - slowRatio);
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		info.caster.Control.StartChannel(new Channel
		{
			blockedActions = (Channel.BlockedAction.Ability | Channel.BlockedAction.Attack),
			duration = castDuration + delayAfterTransform + delayAfterMeteor
		});
		ChangeModelLocal();
		info.caster.Animation.PlayAbilityAnimation(castClip);
		CreateStatusEffect<Se_Mon_Special_BossMaw_Cataclysm>(info.caster, new CastInfo(info.caster));
		yield return new SI.WaitForSeconds(delayAfterTransform);
		for (int i = 0; i < spawnCount; i++)
		{
			Vector3 end = info.caster.agentPosition + Random.insideUnitCircle.ToXZ().normalized * Random.Range(0f, maxRange);
			end = Dew.GetValidAgentDestination_Closest(info.caster.agentPosition, end);
			List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, info.caster.agentPosition, maxRange, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
			{
				sortComparer = CollisionCheckSettings.DistanceFromCenter
			});
			if (list.Count > 0)
			{
				Entity entity = list[0];
				end = AbilityTrigger.PredictPoint_Simple(info.caster, NetworkedManagerBase<GameManager>.instance.GetPredictionStrength(), entity, dmgDelay);
				end += Random.insideUnitCircle.ToXZ().normalized * Random.Range(0f, spawnMaxDeviation);
				end = Dew.GetValidAgentDestination_Closest(entity.agentPosition, end);
			}
			end = Dew.GetPositionOnGround(end);
			handle.Return();
			CreateAbilityInstance<Ai_Mon_Special_BossMaw_Cataclysm_Meteor>(end, Quaternion.Euler(0f, Random.Range(0, 360), 0f), new CastInfo(info.caster, end));
			yield return new SI.WaitForSeconds(spawnInterval);
		}
		yield return new SI.WaitForSeconds(delayAfterMeteor);
		info.caster.Control.StartDaze(postDelay);
		DestroyIfActive();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		FxPlay(fxTransform, info.caster);
		info.caster.Visual.LoadModelDefaultLocal();
		info.caster.Control.baseAgentSpeed = _baseSpeed;
	}

	[ClientRpc]
	private void ChangeModelLocal()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Ai_Mon_Special_BossMaw_Cataclysm_Spawner::ChangeModelLocal()", 729749318, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_ChangeModelLocal()
	{
		FxPlay(fxTransform, info.caster);
		info.caster.Visual.LoadModelLocal(heroModel);
	}

	protected static void InvokeUserCode_ChangeModelLocal(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC ChangeModelLocal called on server.");
		}
		else
		{
			((Ai_Mon_Special_BossMaw_Cataclysm_Spawner)(object)obj).UserCode_ChangeModelLocal();
		}
	}

	static Ai_Mon_Special_BossMaw_Cataclysm_Spawner()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Ai_Mon_Special_BossMaw_Cataclysm_Spawner), "System.Void Ai_Mon_Special_BossMaw_Cataclysm_Spawner::ChangeModelLocal()", (RemoteCallDelegate)InvokeUserCode_ChangeModelLocal);
	}
}
