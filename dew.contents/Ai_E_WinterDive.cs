using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Ai_E_WinterDive : AbilityInstance
{
	public DewCollider range;

	public KnockUpStrength knockUpStrength;

	public ScalingValue dmgFactor;

	public float castingDuration;

	public float ascendTime;

	public float ascendHeight;

	public float descendTime;

	public float postDelay;

	public float stunDuration;

	public DewAnimationClip landingAnim;

	public GameObject ascendEffect;

	public GameObject descendEffect;

	public GameObject landingEffect;

	public GameObject hitEffect;

	private EntityTransformModifier _entTransform;

	private bool _renderersDisabledLocal;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		_entTransform = info.caster.Visual.GetNewTransformModifier();
		if (((NetworkBehaviour)this).isServer)
		{
			info.caster.Control.StartDaze(castingDuration + ascendTime + descendTime);
			FxPlayNetworked(ascendEffect, info.caster);
			RpcAscend();
			CreateBasicEffect(info.caster, new UncollidableEffect(), castingDuration + descendTime + postDelay);
			CreateBasicEffect(info.caster, new InvulnerableEffect(), castingDuration + descendTime + postDelay);
			CreateBasicEffect(info.caster, new UntargetableEffect(), castingDuration + descendTime + postDelay);
			CreateBasicEffect(info.caster, new InvisibleEffect
			{
				ignoreReveal = true
			}, castingDuration + descendTime + postDelay);
			yield return new SI.WaitForSeconds(castingDuration / 2f);
			FxStopNetworked(ascendEffect);
			Vector3 dest = Dew.GetValidAgentDestination_Closest(info.caster.agentPosition, info.point);
			info.caster.Control.Teleport(dest);
			info.caster.Animation.PlayAbilityAnimation(landingAnim);
			range.transform.position = dest;
			yield return new SI.WaitForSeconds(castingDuration / 2f);
			FxPlayNetworked(descendEffect, info.caster);
			RpcDescend();
			info.caster.Control.StartDaze(postDelay);
			yield return new SI.WaitForSeconds(descendTime);
			FxPlayNetworked(landingEffect, dest, Quaternion.identity);
			List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
			for (int i = 0; i < entities.Count; i++)
			{
				Entity entity = entities[i];
				FxPlayNewNetworked(hitEffect, entity);
				entity.Visual.KnockUp(knockUpStrength, isFriendly: false);
				Damage(dmgFactor).SetElemental(ElementalType.Cold).Dispatch(entity);
				CreateBasicEffect(entity, new StunEffect(), stunDuration, "winterdive_stun");
			}
			handle.Return();
			FxStopNetworked(descendEffect);
			Destroy();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (_renderersDisabledLocal)
		{
			_renderersDisabledLocal = false;
			if ((Object)(object)info.caster != null)
			{
				info.caster.Visual.EnableRenderersLocal();
			}
		}
		if (_entTransform != null)
		{
			_entTransform.Stop();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_renderersDisabledLocal = false;
	}

	[ClientRpc]
	private void RpcAscend()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Ai_E_WinterDive::RpcAscend()", 1646478346, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcDescend()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Ai_E_WinterDive::RpcDescend()", 994226764, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcAscend()
	{
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			for (float t = 0f; t < ascendTime; t += Time.deltaTime)
			{
				float num = t / ascendTime;
				_entTransform.worldOffset = Vector3.up * (num * ascendHeight);
				_entTransform.scaleMultiplier = Vector3.one * (1f - num);
				yield return null;
			}
			_entTransform.worldOffset = Vector3.up * ascendHeight;
			_entTransform.scaleMultiplier = Vector3.zero;
			info.caster.Visual.DisableRenderersLocal();
			_renderersDisabledLocal = true;
		}
	}

	protected static void InvokeUserCode_RpcAscend(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcAscend called on server.");
		}
		else
		{
			((Ai_E_WinterDive)(object)obj).UserCode_RpcAscend();
		}
	}

	protected void UserCode_RpcDescend()
	{
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			info.caster.Visual.EnableRenderersLocal();
			_renderersDisabledLocal = false;
			for (float t = 0f; t < descendTime; t += Time.deltaTime)
			{
				float num = t / descendTime;
				_entTransform.worldOffset = Vector3.up * ((1f - num) * ascendHeight);
				_entTransform.scaleMultiplier = Vector3.one * num;
				yield return null;
			}
			_entTransform.worldOffset = Vector3.zero;
			_entTransform.scaleMultiplier = Vector3.one;
			yield return null;
			info.caster.Visual.FixTailsAndClothes();
		}
	}

	protected static void InvokeUserCode_RpcDescend(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDescend called on server.");
		}
		else
		{
			((Ai_E_WinterDive)(object)obj).UserCode_RpcDescend();
		}
	}

	static Ai_E_WinterDive()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Ai_E_WinterDive), "System.Void Ai_E_WinterDive::RpcAscend()", (RemoteCallDelegate)InvokeUserCode_RpcAscend);
		RemoteProcedureCalls.RegisterRpc(typeof(Ai_E_WinterDive), "System.Void Ai_E_WinterDive::RpcDescend()", (RemoteCallDelegate)InvokeUserCode_RpcDescend);
	}
}
