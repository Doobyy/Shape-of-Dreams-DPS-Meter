using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Ai_Mon_Special_BossMaw_AnnihilationStance : AbilityInstance
{
	public int spawnCount;

	public float spawnInterval;

	public float castDuration;

	public float startDelay;

	public float postDelay;

	public float range;

	public GameObject fxUse;

	public GameObject fxTransform;

	public GameObject fxTelegraph;

	public DewAnimationClip castClip;

	public DewAnimationClip animLeft;

	public DewAnimationClip animRight;

	public EntityModel heroModel;

	private Ai_Mon_Special_BossMaw_AnnihilationStance_Instance _prefab;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		_prefab = DewResources.GetByType<Ai_Mon_Special_BossMaw_AnnihilationStance_Instance>(default(ResourceLoadSettings));
		CreateBasicEffect(info.caster, new UnstoppableEffect(), float.PositiveInfinity).DestroyOnDestroy(this);
		ChangeModelLocal();
		float timer = Time.time;
		List<float> angleList = DewPool.GetList(out ListReturnHandle<float> handle);
		info.caster.Animation.PlayAbilityAnimation(castClip);
		info.caster.Control.StartChannel(new Channel
		{
			blockedActions = Channel.BlockedAction.Everything,
			duration = castDuration,
			onCancel = DestroyIfActive,
			onComplete = () =>
			{
				((MonoBehaviour)(object)this).StartCoroutine(Routine());
			}
		});
		yield return new SI.WaitForSeconds(startDelay);
		for (int i = 0; i < spawnCount; i++)
		{
			List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle2, info.caster.agentPosition, range, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
			{
				sortComparer = CollisionCheckSettings.DistanceFromCenter
			});
			Entity entity = ((list.Count <= 0) ? Dew.GetClosestAliveHero(info.caster.agentPosition, fallbackToDead: true, info.caster) : list[0]);
			handle2.Return();
			if (!((Object)(object)entity == null))
			{
				float num = 15f;
				float num2 = ((Random.value > 0.5f) ? CastInfo.GetAngle(Random.insideUnitCircle.ToXZ().normalized) : AbilityTrigger.PredictAngle_SpeedAcceleration(info.caster, Random.Range(0.5f, 1f), entity, info.caster.agentPosition, castDuration - (Time.time - timer) - spawnInterval * (float)i, _prefab.startInFrontDistance, _prefab.initialSpeed, _prefab.targetSpeed, _prefab.acceleration));
				num2 += Random.Range(0f - num, num);
				Quaternion value = Quaternion.AngleAxis(num2, Vector3.up);
				angleList.Add(num2);
				FxPlayNewNetworked(fxTelegraph, info.caster.agentPosition, value);
				yield return new SI.WaitForSeconds(spawnInterval);
			}
		}
		IEnumerator Routine()
		{
			info.caster.Control.StartDaze(postDelay);
			bool isLeft = true;
			for (int j = 0; j < spawnCount; j++)
			{
				DewAnimationClip clip = (isLeft ? animLeft : animRight);
				float angle = angleList[j];
				info.caster.Animation.PlayAbilityAnimation(clip);
				info.caster.Control.Rotate(angle, immediately: false, spawnInterval);
				RpcPlayUse(isLeft, angle);
				CreateAbilityInstance(info.caster.agentPosition, null, new CastInfo(info.caster, angle), (Ai_Mon_Special_BossMaw_AnnihilationStance_Instance b) =>
				{
					b.NetworkisLeft = isLeft;
				});
				isLeft = !isLeft;
				yield return new WaitForSeconds(spawnInterval);
			}
			handle.Return();
			yield return new WaitForSeconds(postDelay - spawnInterval * (float)spawnCount);
			DestroyIfActive();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		FxPlay(fxTransform, info.caster);
		info.caster.Visual.LoadModelDefaultLocal();
	}

	[ClientRpc]
	private void RpcPlayUse(bool isLeft, float angle)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteBool((NetworkWriter)(object)val, isLeft);
		NetworkWriterExtensions.WriteFloat((NetworkWriter)(object)val, angle);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Ai_Mon_Special_BossMaw_AnnihilationStance::RpcPlayUse(System.Boolean,System.Single)", 1893355202, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void ChangeModelLocal()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Ai_Mon_Special_BossMaw_AnnihilationStance::ChangeModelLocal()", 595096298, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcPlayUse__Boolean__Single(bool isLeft, float angle)
	{
		Vector3 localScale = fxUse.transform.localScale;
		if (isLeft)
		{
			fxUse.transform.localScale = localScale.WithX(0f - localScale.x);
		}
		FxPlayNew(fxUse, info.caster, info.caster.agentPosition, Quaternion.Euler(0f, angle, 0f));
		fxUse.transform.localScale = localScale;
	}

	protected static void InvokeUserCode_RpcPlayUse__Boolean__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayUse called on server.");
		}
		else
		{
			((Ai_Mon_Special_BossMaw_AnnihilationStance)(object)obj).UserCode_RpcPlayUse__Boolean__Single(NetworkReaderExtensions.ReadBool(reader), NetworkReaderExtensions.ReadFloat(reader));
		}
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
			((Ai_Mon_Special_BossMaw_AnnihilationStance)(object)obj).UserCode_ChangeModelLocal();
		}
	}

	static Ai_Mon_Special_BossMaw_AnnihilationStance()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Ai_Mon_Special_BossMaw_AnnihilationStance), "System.Void Ai_Mon_Special_BossMaw_AnnihilationStance::RpcPlayUse(System.Boolean,System.Single)", (RemoteCallDelegate)InvokeUserCode_RpcPlayUse__Boolean__Single);
		RemoteProcedureCalls.RegisterRpc(typeof(Ai_Mon_Special_BossMaw_AnnihilationStance), "System.Void Ai_Mon_Special_BossMaw_AnnihilationStance::ChangeModelLocal()", (RemoteCallDelegate)InvokeUserCode_ChangeModelLocal);
	}
}
