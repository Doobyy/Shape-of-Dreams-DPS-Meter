using System.Collections;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Se_Shrine_Despair_Teleport : StatusEffect
{
	public float delay;

	public GameObject displacingEffect;

	public Vector2 displaceDuration;

	public DewEase ease;

	public bool turnOffRenderer;

	public float heightMultiplier;

	public float minHeight;

	private float _displaceDuration;

	private EntityTransformModifier _entTransform;

	[SerializeField]
	private AnimationCurve _heightCurve;

	protected override IEnumerator OnCreateSequenced()
	{
		_entTransform = info.caster.Visual.GetNewTransformModifier();
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		victim.Status.DisableSectionTriggering();
		DoUncollidable();
		DoInvulnerable();
		DoUntargetable();
		DoInvisible(ignoreReveal: true);
		DestroyOnDestroy(victim);
		_displaceDuration = Random.Range(displaceDuration.x, displaceDuration.y);
		victim.Control.CancelOngoingChannels();
		victim.Control.CancelOngoingDisplacement();
		victim.Control.StartDaze(delay + _displaceDuration);
		if (victim is Hero hero)
		{
			foreach (Summon summon in hero.summons)
			{
				Summon sum = summon;
				((MonoBehaviour)(object)this).StartCoroutine(Routine());
				IEnumerator Routine()
				{
					yield return new WaitForSeconds(Random.Range(0.1f, 0.35f));
					if (!sum.IsNullInactiveDeadOrKnockedOut() && !sum.Status.HasStatusEffect<Se_Shrine_Despair_Teleport>())
					{
						sum.CreateStatusEffect<Se_Shrine_Despair_Teleport>(sum, new CastInfo(sum, info.point));
					}
				}
			}
			foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
			{
				Entity ent = allEntity;
				if (ent is Monster && (!(Vector3.Distance(ent.agentPosition, victim.agentPosition) > 4f) || !((Object)(object)ent.AI.context.targetEnemy != (Object)(object)victim)) && !((Object)(object)ent.owner != (Object)(object)DewPlayer.creep))
				{
					((MonoBehaviour)(object)this).StartCoroutine(Routine2());
				}
				IEnumerator Routine2()
				{
					yield return new WaitForSeconds(Random.Range(0.1f, 0.35f));
					if (!ent.IsNullInactiveDeadOrKnockedOut() && !ent.Status.HasStatusEffect<Se_Shrine_Despair_Teleport>())
					{
						ent.CreateStatusEffect<Se_Shrine_Despair_Teleport>(ent, new CastInfo(ent, info.point));
					}
				}
			}
		}
		yield return new SI.WaitForSeconds(delay);
		FxPlayNetworked(displacingEffect, victim);
		Vector3 positionOnGround = Dew.GetPositionOnGround(info.point);
		RpcDisplacement(positionOnGround);
		victim.Control.StartDisplacement(new DispByDestination
		{
			affectedByMovementSpeed = false,
			canGoOverTerrain = true,
			destination = positionOnGround,
			duration = _displaceDuration,
			ease = ease,
			isCanceledByCC = false,
			isFriendly = true,
			rotateForward = true,
			onCancel = DestroyIfActive,
			onFinish = DestroyIfActive
		});
		if (turnOffRenderer)
		{
			victim.Visual.DisableRenderers();
		}
	}

	[ClientRpc]
	private void RpcDisplacement(Vector3 dest)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteVector3((NetworkWriter)(object)val, dest);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Se_Shrine_Despair_Teleport::RpcDisplacement(UnityEngine.Vector3)", 998316557, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		displacingEffect.transform.rotation = victim.rotation;
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (_entTransform != null)
		{
			_entTransform.Stop();
		}
		if (((NetworkBehaviour)this).isServer)
		{
			if ((Object)(object)victim != null)
			{
				victim.Status.EnableSectionTriggering();
			}
			FxStopNetworked(displacingEffect);
			if (turnOffRenderer && (Object)(object)victim != null)
			{
				victim.Visual.EnableRenderers();
			}
		}
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcDisplacement__Vector3(Vector3 dest)
	{
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			float baseHeight = _entTransform.worldOffset.y;
			float distance = Vector3.Distance(dest, info.caster.agentPosition);
			for (float t = 0f; t < _displaceDuration; t += Time.deltaTime)
			{
				if (!info.caster.Control.isDisplacing)
				{
					break;
				}
				float time = t / _displaceDuration;
				float num = _heightCurve.Evaluate(time);
				float num2 = Mathf.Max(distance * heightMultiplier, minHeight);
				float num3 = baseHeight + num2 * num;
				_entTransform.worldOffset = Vector3.up * num3;
				yield return null;
			}
			_entTransform.worldOffset = Vector3.up * baseHeight;
		}
	}

	protected static void InvokeUserCode_RpcDisplacement__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDisplacement called on server.");
		}
		else
		{
			((Se_Shrine_Despair_Teleport)(object)obj).UserCode_RpcDisplacement__Vector3(NetworkReaderExtensions.ReadVector3(reader));
		}
	}

	static Se_Shrine_Despair_Teleport()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Se_Shrine_Despair_Teleport), "System.Void Se_Shrine_Despair_Teleport::RpcDisplacement(UnityEngine.Vector3)", (RemoteCallDelegate)InvokeUserCode_RpcDisplacement__Vector3);
	}
}
