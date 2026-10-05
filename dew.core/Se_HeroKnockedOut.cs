using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Se_HeroKnockedOut : StatusEffect, IOtherPlayersTonedDownDisable
{
	public float reviveHealthMultiplier = 0.5f;

	public GameObject goldExplosion;

	public GameObject blueExplosion;

	[NonSerialized]
	public bool disableQuest;

	private bool _didAddQuest;

	protected override void OnCreate()
	{
		base.OnCreate();
		Hero hero = (Hero)victim;
		if (isNewInstance)
		{
			FxPlay(hero.Visual.model.hasGoldDissolve ? goldExplosion : blueExplosion, hero);
			if (hero.Visual.model.fxDeath != null)
			{
				FxGibs[] componentsInChildren = hero.Visual.model.fxDeath.GetComponentsInChildren<FxGibs>(includeInactive: true);
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].info = new GibInfo
					{
						normalizedCurrentDamage = Vector3.up * 0.25f,
						velocity = hero.AI.estimatedVelocityUnclamped,
						yVelocity = hero.Visual.currentYVelocity
					};
				}
				FxPlayNew(hero.Visual.model.fxDeath, hero.Visual.entity);
			}
		}
		if (((NetworkBehaviour)this).isServer)
		{
			DoStun();
			DoSilence();
			DoInvulnerable();
			DoUntargetable();
			DoUncollidable();
			DoInvisible(ignoreReveal: true);
			victim.Status.DisableSectionTriggering();
			hero.isKnockedOut = true;
			EventInfoKill kill = new EventInfoKill
			{
				actor = hero._lastAttacker,
				victim = hero
			};
			RpcInvokeKnockedOut(kill);
			hero.Control.freeMovement = true;
			CheckAndAddHeroSoul();
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
		}
	}

	private void ClientEventOnRoomLoaded(EventInfoLoadRoom obj)
	{
		CheckAndAddHeroSoul();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if ((UnityEngine.Object)(object)victim != null)
		{
			victim.Status.EnableSectionTriggering();
		}
		Hero hero = victim as Hero;
		if ((UnityEngine.Object)(object)hero != null && hero.isActive)
		{
			hero.isKnockedOut = false;
			hero.Control.freeMovement = false;
			RpcInvokeRevive();
		}
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded -= new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
		}
		for (int i = 0; i < NetworkedManagerBase<ZoneManager>.instance.nodes.Count; i++)
		{
			WorldNodeData worldNodeData = NetworkedManagerBase<ZoneManager>.instance.nodes[i];
			int num = worldNodeData.modifiers.FindIndex((ModifierData m) => m.type == "RoomMod_HeroSoul" && m.clientData == victim.owner.guid);
			if (num >= 0)
			{
				NetworkedManagerBase<ZoneManager>.instance.RemoveModifier(i, worldNodeData.modifiers[num].id);
			}
		}
	}

	[ClientRpc]
	private void RpcInvokeKnockedOut(EventInfoKill kill)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_EventInfoKill((NetworkWriter)(object)val, kill);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Se_HeroKnockedOut::RpcInvokeKnockedOut(EventInfoKill)", -1360231535, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcInvokeRevive()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Se_HeroKnockedOut::RpcInvokeRevive()", 194098549, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public void Revive(float reviveHealthMult = -1f)
	{
		if (reviveHealthMult < 0f)
		{
			reviveHealthMult = reviveHealthMultiplier;
		}
		victim.Status.SetHealth(victim.maxHealth * reviveHealthMult);
		Destroy();
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (!_didAddQuest && Time.time - creationTime > 1f && (UnityEngine.Object)(object)Dew.SelectRandomAliveHero(fallbackToDead: false) != null && NetworkedManagerBase<ZoneManager>.instance.currentNode.type != WorldNodeType.ExitBoss && !disableQuest)
		{
			_didAddQuest = true;
			NetworkedManagerBase<QuestManager>.instance.StartQuest((Quest_LostSoul s) =>
			{
				s.NetworktargetHero = (Hero)victim;
			});
		}
		victim.Status.SetHealth(0.01f);
	}

	public void CheckAndAddHeroSoul()
	{
		if (!disableQuest && NetworkedManagerBase<ZoneManager>.instance.currentNode.type != WorldNodeType.ExitBoss && !((IEnumerable<WorldNodeData>)NetworkedManagerBase<ZoneManager>.instance.nodes).Any((WorldNodeData n) => n.modifiers.Any((ModifierData m) => m.type == "RoomMod_HeroSoul" && m.clientData == victim.owner.guid)) && Dew.GetAliveHeroCount() != 0)
		{
			NetworkedManagerBase<ZoneManager>.instance.TryGetNodeIndexForNextGoal(new GetNodeIndexSettings
			{
				desiredDistance = NetworkedManagerBase<GameManager>.instance.difficulty.lostSoulDistance,
				avoidMainModifier = false,
				preferCloserToExit = true
			}, out var nodeIndex);
			NetworkedManagerBase<ZoneManager>.instance.AddModifier<RoomMod_HeroSoul>(nodeIndex, victim.owner.guid);
		}
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcInvokeKnockedOut__EventInfoKill(EventInfoKill kill)
	{
		try
		{
			((Hero)victim).ClientHeroEvent_OnKnockedOut?.Invoke(kill);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		try
		{
			NetworkedManagerBase<ClientEventManager>.instance.OnHeroKnockedOut?.Invoke((Hero)victim);
		}
		catch (Exception exception2)
		{
			Debug.LogException(exception2);
		}
	}

	protected static void InvokeUserCode_RpcInvokeKnockedOut__EventInfoKill(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeKnockedOut called on server.");
		}
		else
		{
			((Se_HeroKnockedOut)(object)obj).UserCode_RpcInvokeKnockedOut__EventInfoKill(GeneratedNetworkCode._Read_EventInfoKill(reader));
		}
	}

	protected void UserCode_RpcInvokeRevive()
	{
		try
		{
			((Hero)victim).ClientHeroEvent_OnRevive?.Invoke((Hero)victim);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		try
		{
			NetworkedManagerBase<ClientEventManager>.instance.OnHeroRevive?.Invoke((Hero)victim);
		}
		catch (Exception exception2)
		{
			Debug.LogException(exception2);
		}
	}

	protected static void InvokeUserCode_RpcInvokeRevive(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeRevive called on server.");
		}
		else
		{
			((Se_HeroKnockedOut)(object)obj).UserCode_RpcInvokeRevive();
		}
	}

	static Se_HeroKnockedOut()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Se_HeroKnockedOut), "System.Void Se_HeroKnockedOut::RpcInvokeKnockedOut(EventInfoKill)", (RemoteCallDelegate)InvokeUserCode_RpcInvokeKnockedOut__EventInfoKill);
		RemoteProcedureCalls.RegisterRpc(typeof(Se_HeroKnockedOut), "System.Void Se_HeroKnockedOut::RpcInvokeRevive()", (RemoteCallDelegate)InvokeUserCode_RpcInvokeRevive);
	}
}
