using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Ai_E_AntiGravity : AbilityInstance
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct Ad_CheckFloating
	{
	}

	public DewCollider range;

	public ScalingValue dmgFactor;

	public float procCoefficient;

	public DewEase ascendEase;

	public GameObject gravityEffect;

	public GameObject gravityEntEffect;

	public GameObject impactEffect;

	public GameObject descendEffect;

	public GameObject hitEffect;

	public float sustainTime;

	public float ascendTime;

	public float descendTime;

	public float dmgDelay;

	public float ascendHeight;

	public float bindPostDuration;

	private float _bindDuration;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		_bindDuration = ascendTime + sustainTime + descendTime;
		range.transform.position = info.point;
		FxPlayNetworked(gravityEffect, info.point, Quaternion.identity);
		List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
		for (int i = 0; i < entities.Count; i++)
		{
			Entity entity = entities[i];
			if (!entity.Status.hasCrowdControlImmunity && !entity.IsNullInactiveDeadOrKnockedOut())
			{
				CreateBasicEffect(entity, new StunEffect(), _bindDuration + bindPostDuration, "stun_antigravity");
				if (!entity.HasData<Ad_CheckFloating>())
				{
					entity.AddData<Ad_CheckFloating>(default);
					FxPlayNewNetworked(gravityEntEffect, entity);
					RpcGravitySequence(entity);
				}
			}
		}
		handle.Return();
		yield return new SI.WaitForSeconds(_bindDuration + dmgDelay);
		FxPlayNetworked(impactEffect, info.point, Quaternion.identity);
		List<Entity> entities2 = range.GetEntities(out var handle2, tvDefaultHarmfulEffectTargets);
		for (int j = 0; j < entities2.Count; j++)
		{
			Entity entity2 = entities2[j];
			FxPlayNewNetworked(hitEffect, entity2);
			Damage(dmgFactor, procCoefficient).Dispatch(entity2);
			if (entity2.HasData<Ad_CheckFloating>())
			{
				entity2.RemoveData<Ad_CheckFloating>();
			}
		}
		handle2.Return();
		Destroy();
	}

	[ClientRpc]
	private void RpcGravitySequence(Entity e)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)e);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Ai_E_AntiGravity::RpcGravitySequence(Entity)", 975538599, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcGravitySequence__Entity(Entity e)
	{
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			EaseFunction easeFunc = EasingFunction.GetEasingFunction(ascendEase);
			EntityTransformModifier entT = e.Visual.GetNewTransformModifier();
			try
			{
				for (float t = 0f; t < ascendTime; t += Time.deltaTime)
				{
					if (e.IsNullInactiveDeadOrKnockedOut())
					{
						yield break;
					}
					float v = t / ascendTime;
					v = easeFunc(0f, 1f, v);
					entT.worldOffset = Vector3.up * (v * ascendHeight);
					yield return null;
				}
				entT.worldOffset = Vector3.up * ascendHeight;
				yield return new WaitForSeconds(sustainTime);
				FxPlayNew(descendEffect, e);
				for (float t = 0f; t < descendTime; t += Time.deltaTime)
				{
					if (e.IsNullInactiveDeadOrKnockedOut())
					{
						yield break;
					}
					float num = t / descendTime;
					entT.worldOffset = Vector3.up * ((1f - num) * ascendHeight);
					yield return null;
				}
			}
			finally
			{
				if ((Object)(object)e != null)
				{
					entT.worldOffset = Vector3.zero;
					entT.Stop();
				}
			}
			yield return null;
			if (!e.IsNullInactiveDeadOrKnockedOut())
			{
				e.Visual.FixTailsAndClothes();
			}
		}
	}

	protected static void InvokeUserCode_RpcGravitySequence__Entity(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcGravitySequence called on server.");
		}
		else
		{
			((Ai_E_AntiGravity)(object)obj).UserCode_RpcGravitySequence__Entity(NetworkReaderExtensions.ReadNetworkBehaviour<Entity>(reader));
		}
	}

	static Ai_E_AntiGravity()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Ai_E_AntiGravity), "System.Void Ai_E_AntiGravity::RpcGravitySequence(Entity)", (RemoteCallDelegate)InvokeUserCode_RpcGravitySequence__Entity);
	}
}
