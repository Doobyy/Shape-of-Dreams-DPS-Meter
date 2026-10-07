using System;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class St_D_CircleOfLife : SkillTrigger
{
	public GameObject fxEffectOnSummon;

	public ScalingValue summonHealRatio;

	public ScalingValue summonAddedDuration;

	protected override void OnEquip(Entity newOwner)
	{
		base.OnEquip(newOwner);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		newOwner.EntityEvent_OnAttackEffectTriggered += new Action<EventInfoAttackEffect>(EntityEventOnAttackEffectTriggered);
		newOwner.ClientActorEvent_OnCreate += new Action<Actor>(ClientActorEventOnCreate);
		foreach (Summon summon in ((Hero)newOwner).summons)
		{
			CreateStatusEffect<Se_D_CircleOfLife_Summon>(summon, new CastInfo(newOwner));
		}
	}

	private void ClientActorEventOnCreate(Actor obj)
	{
		if (obj is Summon victim)
		{
			CreateStatusEffect<Se_D_CircleOfLife_Summon>(victim, new CastInfo(owner));
		}
	}

	private void EntityEventOnAttackEffectTriggered(EventInfoAttackEffect obj)
	{
		if (obj.type == AttackEffectType.Others)
		{
			return;
		}
		Hero hero = owner;
		if (hero == null || hero.summons.Count == 0)
		{
			return;
		}
		foreach (Summon summon in hero.summons)
		{
			FxPlayNewNetworked(fxEffectOnSummon, summon);
			Heal(GetValue(summonHealRatio) * summon.maxHealth).Dispatch(summon);
			if (!float.IsPositiveInfinity(summon.maxDuration))
			{
				summon.AddDuration(GetValue(summonAddedDuration));
				TpcShowSummonDurationIncrease(((NetworkBehaviour)owner).connectionToClient, summon);
			}
		}
	}

	protected override void OnUnequip(Entity formerOwner)
	{
		base.OnUnequip(formerOwner);
		if (!((NetworkBehaviour)this).isServer || !((UnityEngine.Object)(object)formerOwner != null))
		{
			return;
		}
		formerOwner.EntityEvent_OnAttackEffectTriggered -= new Action<EventInfoAttackEffect>(EntityEventOnAttackEffectTriggered);
		formerOwner.ClientActorEvent_OnCreate -= new Action<Actor>(ClientActorEventOnCreate);
		foreach (Summon summon in ((Hero)formerOwner).summons)
		{
			if (summon.Status.TryGetStatusEffect<Se_D_CircleOfLife_Summon>(out var effect))
			{
				effect.Destroy();
			}
		}
	}

	[TargetRpc]
	private void TpcShowSummonDurationIncrease(NetworkConnectionToClient target, Summon s)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)s);
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)(object)target, "System.Void St_D_CircleOfLife::TpcShowSummonDurationIncrease(Mirror.NetworkConnectionToClient,Summon)", 2048588388, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_TpcShowSummonDurationIncrease__NetworkConnectionToClient__Summon(NetworkConnectionToClient target, Summon s)
	{
		float value = GetValue(summonAddedDuration);
		InGameUIManager.instance.ShowWorldPopMessage(new WorldMessageSetting
		{
			color = Color.white,
			rawText = $"<size=80%>+{value:#,##0.#}",
			worldPosGetter = () => s.agentPosition,
			popOffset = new Vector2(0f, 0f)
		});
	}

	protected static void InvokeUserCode_TpcShowSummonDurationIncrease__NetworkConnectionToClient__Summon(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcShowSummonDurationIncrease called on server.");
		}
		else
		{
			((St_D_CircleOfLife)(object)obj).UserCode_TpcShowSummonDurationIncrease__NetworkConnectionToClient__Summon((NetworkConnectionToClient)(object)NetworkClient.connection, NetworkReaderExtensions.ReadNetworkBehaviour<Summon>(reader));
		}
	}

	static St_D_CircleOfLife()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(St_D_CircleOfLife), "System.Void St_D_CircleOfLife::TpcShowSummonDurationIncrease(Mirror.NetworkConnectionToClient,Summon)", (RemoteCallDelegate)InvokeUserCode_TpcShowSummonDurationIncrease__NetworkConnectionToClient__Summon);
	}
}
