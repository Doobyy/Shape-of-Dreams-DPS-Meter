using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Gem_C_Responsibility : Gem
{
	public float initDelay;

	public ScalingValue totalDamage;

	public AnimationCurve pitchCurve;

	public AnimationCurve volumeCurve;

	public AnimationCurve shakeAmpCurve;

	public GameObject fxActivateInit;

	public GameObject fxHit;

	protected override void OnCastCompleteBeforePrepare(EventInfoCast info)
	{
		base.OnCastCompleteBeforePrepare(info);
		if (IsReady())
		{
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			NotifyUse();
			StartCooldown();
			FxPlayNewNetworked(fxActivateInit, owner);
			info.instance.LockDestroy();
			List<Entity> ents = new List<Entity>();
			info.instance.ActorEvent_OnDealDamage += (Action<EventInfoDamage>)((EventInfoDamage dmg) =>
			{
				if (isValid && !ents.Contains(dmg.victim) && owner.CheckEnemyOrNeutral(dmg.victim))
				{
					ents.Add(dmg.victim);
				}
			});
			yield return new WaitForSeconds(initDelay);
			if (!isValid)
			{
				info.instance.UnlockDestroy();
			}
			else
			{
				for (int num = ents.Count - 1; num >= 0; num--)
				{
					if (ents[num].IsNullInactiveDeadOrKnockedOut())
					{
						ents.RemoveAt(num);
					}
				}
				if (ents.Count == 0)
				{
					ResetCooldown();
				}
				else
				{
					float strength = 1f / (float)ents.Count;
					float damagePerEnt = GetValue(totalDamage) * strength;
					Entity[] array = ents.ToArray();
					foreach (Entity entity in array)
					{
						PhysicalDamage(damagePerEnt).Dispatch(entity);
						RpcPlayHit(entity, strength);
						yield return new WaitForSeconds(0.25f / (float)ents.Count);
					}
				}
				info.instance.UnlockDestroy();
			}
		}
	}

	[ClientRpc]
	private void RpcPlayHit(Entity e, float strength)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)e);
		NetworkWriterExtensions.WriteFloat((NetworkWriter)(object)val, strength);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Gem_C_Responsibility::RpcPlayHit(Entity,System.Single)", 1761183003, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcPlayHit__Entity__Single(Entity e, float strength)
	{
		float pitchMultiplier = pitchCurve.Evaluate(strength);
		float num = volumeCurve.Evaluate(strength);
		float amplitude = shakeAmpCurve.Evaluate(strength);
		List<DewAudioSource> componentsInChildrenNonAlloc = fxHit.GetComponentsInChildrenNonAlloc(out ListReturnHandle<DewAudioSource> handle);
		List<FxCameraShake> componentsInChildrenNonAlloc2 = fxHit.GetComponentsInChildrenNonAlloc(out ListReturnHandle<FxCameraShake> handle2);
		foreach (DewAudioSource item in componentsInChildrenNonAlloc)
		{
			item.pitchMultiplier = pitchMultiplier;
			item.volumeMultiplier = num;
		}
		foreach (FxCameraShake item2 in componentsInChildrenNonAlloc2)
		{
			item2.amplitude = amplitude;
		}
		fxHit.transform.localScale = Vector3.one * num;
		handle.Return();
		handle2.Return();
		FxPlayNew(fxHit, e);
	}

	protected static void InvokeUserCode_RpcPlayHit__Entity__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayHit called on server.");
		}
		else
		{
			((Gem_C_Responsibility)(object)obj).UserCode_RpcPlayHit__Entity__Single(NetworkReaderExtensions.ReadNetworkBehaviour<Entity>(reader), NetworkReaderExtensions.ReadFloat(reader));
		}
	}

	static Gem_C_Responsibility()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Gem_C_Responsibility), "System.Void Gem_C_Responsibility::RpcPlayHit(Entity,System.Single)", (RemoteCallDelegate)InvokeUserCode_RpcPlayHit__Entity__Single);
	}
}
