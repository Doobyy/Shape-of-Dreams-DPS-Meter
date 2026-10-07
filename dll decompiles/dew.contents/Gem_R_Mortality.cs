using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Gem_R_Mortality : Gem
{
	public ScalingValue killChance;

	public bool useProcChance;

	public float procChanceMultiplier;

	public GameObject fxKill;

	protected override void OnDealDamage(EventInfoDamage info)
	{
		base.OnDealDamage(info);
		if ((Object)(object)info.actor == null || (Object)(object)owner == null || info.chain.DidReact(this) || !owner.CheckEnemyOrNeutral(info.victim) || info.victim.IsNullInactiveDeadOrKnockedOut() || (useProcChance && Random.value > info.damage.procCoefficient * procChanceMultiplier) || !IsReady() || Random.Range(0f, 100f) >= GetValue(killChance) * 100f || !(info.victim is Monster) || info.victim is BossMonster || info.victim.Status.HasStatusEffect<MiniBossEffect>())
		{
			return;
		}
		FxPlayNewNetworked(fxKill, info.victim);
		Dew.CallDelayed(() =>
		{
			if (!info.victim.IsNullInactiveDeadOrKnockedOut() && !info.victim.Status.hasDamageImmunity)
			{
				RpcClearDeathEffects(info.victim);
				CreateDamage(DamageData.SourceType.Pure, 9999999f - info.damage.amount).SetActor(this).SetAmountOrigin(info.damage).SetAttr(DamageAttribute.IsCrit)
					.SetAttr(DamageAttribute.NoTracking)
					.SetAmountModifiedBy(this)
					.Dispatch(info.victim, info.chain.New(this));
			}
		});
		NotifyUse();
	}

	[ClientRpc]
	private void RpcClearDeathEffects(Entity e)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)e);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Gem_R_Mortality::RpcClearDeathEffects(Entity)", 1997197298, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcClearDeathEffects__Entity(Entity e)
	{
		e.Visual.model.deathBehavior = EntityVisual.EntityDeathBehavior.HideModel;
		e.Visual.model.fxDeath = null;
	}

	protected static void InvokeUserCode_RpcClearDeathEffects__Entity(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcClearDeathEffects called on server.");
		}
		else
		{
			((Gem_R_Mortality)(object)obj).UserCode_RpcClearDeathEffects__Entity(NetworkReaderExtensions.ReadNetworkBehaviour<Entity>(reader));
		}
	}

	static Gem_R_Mortality()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Gem_R_Mortality), "System.Void Gem_R_Mortality::RpcClearDeathEffects(Entity)", (RemoteCallDelegate)InvokeUserCode_RpcClearDeathEffects__Entity);
	}
}
