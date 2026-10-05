using System;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class ClientEventManager : NetworkedManagerBase<ClientEventManager>
{
	public SafeAction<OnScreenTimerHandle> OnShowOnScreenTimer;

	public SafeAction<OnScreenTimerHandle> OnHideOnScreenTimer;

	public SafeAction<EventInfoHeal> OnTakeManaHeal;

	public SafeAction<EventInfoHeal> OnTakeHeal;

	public SafeAction<EventInfoDamage> OnTakeDamage;

	public SafeAction<EventInfoShield> OnTakeShield;

	public SafeAction<EventInfoKill> OnDeath;

	public SafeAction<EventInfoSpentMana> OnGetManaSpent;

	public SafeAction<EventInfoAttackMissed> OnAttackMissed;

	public SafeAction<EventInfoDamageNegatedByImmunity> OnDamageNegated;

	public SafeAction<EventInfoDamageNegatedByShield> OnDamageNegatedByShield;

	public SafeAction<EventInfoApplyElemental> OnApplyElemental;

	public SafeAction<EventInfoCast> OnCastComplete;

	public SafeAction<EventInfoAttackHit> OnAttackHit;

	public SafeAction<Hero, NetworkBehaviour> OnItemSold;

	public SafeAction<Hero, NetworkBehaviour> OnItemBought;

	public SafeAction<Hero, Gem> OnGemMergeUpgraded;

	public SafeAction<Hero, NetworkBehaviour> OnItemUpgraded;

	public SafeAction<Hero, HeroSkillLocation> OnLocalHeroAbilityChanged;

	public SafeAction<Hero, GemLocation> OnLocalHeroGemChanged;

	public SafeAction<Hero, NetworkBehaviour> OnDismantled;

	public SafeAction<Hero, NetworkBehaviour> OnItemCleansed;

	public SafeAction<Hero> OnHeroKnockedOut;

	public SafeAction<Hero> OnHeroRevive;

	public SafeAction<DewPlayer, DewPlayer, int, int> OnGiveCurrency;

	public SafeAction<DewPlayer> OnChaosUsed;

	public SafeAction<Entity> OnIgnoreCC;

	public SafeAction<Entity> OnRefreshEntityHealthbar;

	private Action<EventInfoHeal> _invokeOnTakeManaHeal;

	private Action<EventInfoHeal> _invokeOnTakeHeal;

	private Action<EventInfoDamage> _invokeOnTakeDamage;

	private Action<EventInfoShield> _invokeOnTakeShield;

	private Action<EventInfoKill> _invokeOnDeath;

	private Action<EventInfoSpentMana> _invokeOnGetManaSpent;

	private Action<EventInfoAttackMissed> _invokeOnAttackMissed;

	private Action<EventInfoDamageNegatedByImmunity> _invokeOnDamageNegated;

	private Action<EventInfoDamageNegatedByShield> _invokeOnDamageNegatedByShield;

	private Action<EventInfoAttackHit> _invokeOnAttackHit;

	private Action<EventInfoApplyElemental> _invokeOnApplyElemental;

	public override void OnStart()
	{
		base.OnStart();
		_invokeOnTakeManaHeal = InvokeOnTakeManaHeal;
		_invokeOnTakeHeal = InvokeOnTakeHeal;
		_invokeOnTakeDamage = InvokeOnTakeDamage;
		_invokeOnTakeShield = InvokeOnTakeShield;
		_invokeOnDeath = InvokeOnDeath;
		_invokeOnGetManaSpent = InvokeOnGetManaSpent;
		_invokeOnAttackMissed = InvokeOnAttackMissed;
		_invokeOnDamageNegated = InvokeOnDamageNegated;
		_invokeOnDamageNegatedByShield = InvokeOnDamageNegatedByShield;
		_invokeOnAttackHit = InvokeOnAttackHit;
		_invokeOnApplyElemental = InvokeOnApplyElemental;
		NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd += new Action<Actor>(AddEvents);
		NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorRemove += new Action<Actor>(RemoveEvents);
		foreach (Actor allActor in NetworkedManagerBase<ActorManager>.instance.allActors)
		{
			AddEvents(allActor);
		}
		DewPlayer.onGamePlayerAdded += new Action<DewPlayer>(OnGamePlayerAdd);
		DewPlayer.onGamePlayerRemoved += new Action<DewPlayer>(OnGamePlayerRemove);
		foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
		{
			OnGamePlayerAdd(gamePlayer);
		}
		OnGiveCurrency += (Action<DewPlayer, DewPlayer, int, int>)((DewPlayer from, DewPlayer to, int gold, int dreamDust) =>
		{
			string content = ((gold > 0 && dreamDust > 0) ? "Chat_Notice_GiveCurrency_GoldAndDreamDust" : ((gold <= 0) ? "Chat_Notice_GiveCurrency_DreamDust" : "Chat_Notice_GiveCurrency_Gold"));
			NetworkedManagerBase<ChatManager>.instance.ShowMessageLocally(new ChatManager.Message
			{
				type = ChatManager.MessageType.Notice,
				content = content,
				args = new string[4]
				{
					ChatManager.GetColoredDescribedPlayerName(from),
					ChatManager.GetColoredDescribedPlayerName(to),
					gold.ToString("#,##0"),
					dreamDust.ToString("#,##0")
				}
			});
		});
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		DewPlayer.onGamePlayerAdded -= new Action<DewPlayer>(OnGamePlayerAdd);
		DewPlayer.onGamePlayerRemoved -= new Action<DewPlayer>(OnGamePlayerRemove);
	}

	private void OnGamePlayerAdd(DewPlayer obj)
	{
		DewPlayer from = obj;
		obj.ClientEvent_OnGiveCurrency += (Action<int, int, DewPlayer>)((int gold, int dreamDust, DewPlayer to) =>
		{
			OnGiveCurrency?.Invoke(from, to, gold, dreamDust);
		});
	}

	private void OnGamePlayerRemove(DewPlayer obj)
	{
	}

	public virtual void AddEvents(Actor actor)
	{
		if (((NetworkBehaviour)this).isServer)
		{
			if (actor is Entity entity)
			{
				entity.EntityEvent_OnTakeManaHeal += _invokeOnTakeManaHeal;
				entity.EntityEvent_OnTakeHeal += _invokeOnTakeHeal;
				entity.EntityEvent_OnTakeDamage += _invokeOnTakeDamage;
				entity.EntityEvent_OnTakeShield += _invokeOnTakeShield;
				entity.EntityEvent_OnDeath += _invokeOnDeath;
				entity.EntityEvent_OnGetManaSpent += _invokeOnGetManaSpent;
				entity.EntityEvent_OnAttackMissed += _invokeOnAttackMissed;
				entity.EntityEvent_OnDamageNegatedByImmunity += _invokeOnDamageNegated;
				entity.EntityEvent_OnDamageNegatedByShield += _invokeOnDamageNegatedByShield;
				entity.EntityEvent_OnAttackHit += _invokeOnAttackHit;
			}
			actor.ActorEvent_OnApplyElemental += _invokeOnApplyElemental;
		}
	}

	public virtual void RemoveEvents(Actor actor)
	{
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)actor == null))
		{
			if (actor is Entity entity)
			{
				entity.EntityEvent_OnTakeManaHeal -= _invokeOnTakeManaHeal;
				entity.EntityEvent_OnTakeHeal -= _invokeOnTakeHeal;
				entity.EntityEvent_OnTakeDamage -= _invokeOnTakeDamage;
				entity.EntityEvent_OnTakeShield -= _invokeOnTakeShield;
				entity.EntityEvent_OnDeath -= _invokeOnDeath;
				entity.EntityEvent_OnGetManaSpent -= _invokeOnGetManaSpent;
				entity.EntityEvent_OnAttackMissed -= _invokeOnAttackMissed;
				entity.EntityEvent_OnDamageNegatedByImmunity -= _invokeOnDamageNegated;
				entity.EntityEvent_OnDamageNegatedByShield -= _invokeOnDamageNegatedByShield;
				entity.EntityEvent_OnAttackHit -= _invokeOnAttackHit;
			}
			actor.ActorEvent_OnApplyElemental -= _invokeOnApplyElemental;
		}
	}

	[ClientRpc]
	private void InvokeOnTakeManaHeal(EventInfoHeal info)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_EventInfoHeal((NetworkWriter)(object)val, info);
		((NetworkBehaviour)this).SendRPCInternal("System.Void ClientEventManager::InvokeOnTakeManaHeal(EventInfoHeal)", -1327707822, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void InvokeOnTakeHeal(EventInfoHeal info)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_EventInfoHeal((NetworkWriter)(object)val, info);
		((NetworkBehaviour)this).SendRPCInternal("System.Void ClientEventManager::InvokeOnTakeHeal(EventInfoHeal)", 977163033, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void InvokeOnTakeDamage(EventInfoDamage info)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_EventInfoDamage((NetworkWriter)(object)val, info);
		((NetworkBehaviour)this).SendRPCInternal("System.Void ClientEventManager::InvokeOnTakeDamage(EventInfoDamage)", 292245963, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void InvokeOnTakeShield(EventInfoShield info)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_EventInfoShield((NetworkWriter)(object)val, info);
		((NetworkBehaviour)this).SendRPCInternal("System.Void ClientEventManager::InvokeOnTakeShield(EventInfoShield)", -384181161, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void InvokeOnDeath(EventInfoKill info)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_EventInfoKill((NetworkWriter)(object)val, info);
		((NetworkBehaviour)this).SendRPCInternal("System.Void ClientEventManager::InvokeOnDeath(EventInfoKill)", 1130950576, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void InvokeOnGetManaSpent(EventInfoSpentMana info)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_EventInfoSpentMana((NetworkWriter)(object)val, info);
		((NetworkBehaviour)this).SendRPCInternal("System.Void ClientEventManager::InvokeOnGetManaSpent(EventInfoSpentMana)", -19905218, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void InvokeOnAttackMissed(EventInfoAttackMissed info)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_EventInfoAttackMissed((NetworkWriter)(object)val, info);
		((NetworkBehaviour)this).SendRPCInternal("System.Void ClientEventManager::InvokeOnAttackMissed(EventInfoAttackMissed)", -1821054102, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void InvokeOnDamageNegated(EventInfoDamageNegatedByImmunity info)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_EventInfoDamageNegatedByImmunity((NetworkWriter)(object)val, info);
		((NetworkBehaviour)this).SendRPCInternal("System.Void ClientEventManager::InvokeOnDamageNegated(EventInfoDamageNegatedByImmunity)", 337155917, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void InvokeOnDamageNegatedByShield(EventInfoDamageNegatedByShield info)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_EventInfoDamageNegatedByShield((NetworkWriter)(object)val, info);
		((NetworkBehaviour)this).SendRPCInternal("System.Void ClientEventManager::InvokeOnDamageNegatedByShield(EventInfoDamageNegatedByShield)", -590220912, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void InvokeOnAttackHit(EventInfoAttackHit info)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_EventInfoAttackHit((NetworkWriter)(object)val, info);
		((NetworkBehaviour)this).SendRPCInternal("System.Void ClientEventManager::InvokeOnAttackHit(EventInfoAttackHit)", 1032775184, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	public void InvokeOnItemSold(Hero h, NetworkBehaviour nb)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)h);
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, nb);
		((NetworkBehaviour)this).SendRPCInternal("System.Void ClientEventManager::InvokeOnItemSold(Hero,Mirror.NetworkBehaviour)", -1877842699, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	public void InvokeOnItemCleansed(Hero h, NetworkBehaviour nb)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)h);
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, nb);
		((NetworkBehaviour)this).SendRPCInternal("System.Void ClientEventManager::InvokeOnItemCleansed(Hero,Mirror.NetworkBehaviour)", -62337846, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	public void InvokeOnItemBought(Hero h, NetworkBehaviour nb)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)h);
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, nb);
		((NetworkBehaviour)this).SendRPCInternal("System.Void ClientEventManager::InvokeOnItemBought(Hero,Mirror.NetworkBehaviour)", 1103697868, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	public void InvokeOnGemMergeUpgraded(Hero h, Gem g)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)h);
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)g);
		((NetworkBehaviour)this).SendRPCInternal("System.Void ClientEventManager::InvokeOnGemMergeUpgraded(Hero,Gem)", 1730456686, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	public void InvokeOnItemUpgraded(Hero h, NetworkBehaviour nb)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)h);
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, nb);
		((NetworkBehaviour)this).SendRPCInternal("System.Void ClientEventManager::InvokeOnItemUpgraded(Hero,Mirror.NetworkBehaviour)", 1700352233, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	public void InvokeOnChaosUsed(DewPlayer p)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)p);
		((NetworkBehaviour)this).SendRPCInternal("System.Void ClientEventManager::InvokeOnChaosUsed(DewPlayer)", 808351382, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	public void InvokeOnDismantled(Hero h, NetworkBehaviour nb)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)h);
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, nb);
		((NetworkBehaviour)this).SendRPCInternal("System.Void ClientEventManager::InvokeOnDismantled(Hero,Mirror.NetworkBehaviour)", 997592145, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	public void InvokeOnIgnoreCC(Entity ent)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)ent);
		((NetworkBehaviour)this).SendRPCInternal("System.Void ClientEventManager::InvokeOnIgnoreCC(Entity)", -997650711, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	public void InvokeOnApplyElemental(EventInfoApplyElemental info)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_EventInfoApplyElemental((NetworkWriter)(object)val, info);
		((NetworkBehaviour)this).SendRPCInternal("System.Void ClientEventManager::InvokeOnApplyElemental(EventInfoApplyElemental)", 2129906046, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	public void InvokeOnRefreshEntityHealthbar(Entity e)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)e);
		((NetworkBehaviour)this).SendRPCInternal("System.Void ClientEventManager::InvokeOnRefreshEntityHealthbar(Entity)", -1092666960, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	public void InvokeOnCastComplete(EventInfoCast eventInfo)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_EventInfoCast((NetworkWriter)(object)val, eventInfo);
		((NetworkBehaviour)this).SendRPCInternal("System.Void ClientEventManager::InvokeOnCastComplete(EventInfoCast)", -2136773767, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_InvokeOnTakeManaHeal__EventInfoHeal(EventInfoHeal info)
	{
		if (!((UnityEngine.Object)(object)info.target == null))
		{
			OnTakeManaHeal?.Invoke(info);
		}
	}

	protected static void InvokeUserCode_InvokeOnTakeManaHeal__EventInfoHeal(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnTakeManaHeal called on server.");
		}
		else
		{
			((ClientEventManager)(object)obj).UserCode_InvokeOnTakeManaHeal__EventInfoHeal(GeneratedNetworkCode._Read_EventInfoHeal(reader));
		}
	}

	protected void UserCode_InvokeOnTakeHeal__EventInfoHeal(EventInfoHeal info)
	{
		if (!((UnityEngine.Object)(object)info.target == null))
		{
			OnTakeHeal?.Invoke(info);
		}
	}

	protected static void InvokeUserCode_InvokeOnTakeHeal__EventInfoHeal(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnTakeHeal called on server.");
		}
		else
		{
			((ClientEventManager)(object)obj).UserCode_InvokeOnTakeHeal__EventInfoHeal(GeneratedNetworkCode._Read_EventInfoHeal(reader));
		}
	}

	protected void UserCode_InvokeOnTakeDamage__EventInfoDamage(EventInfoDamage info)
	{
		if (!((UnityEngine.Object)(object)info.victim == null))
		{
			OnTakeDamage?.Invoke(info);
		}
	}

	protected static void InvokeUserCode_InvokeOnTakeDamage__EventInfoDamage(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnTakeDamage called on server.");
		}
		else
		{
			((ClientEventManager)(object)obj).UserCode_InvokeOnTakeDamage__EventInfoDamage(GeneratedNetworkCode._Read_EventInfoDamage(reader));
		}
	}

	protected void UserCode_InvokeOnTakeShield__EventInfoShield(EventInfoShield info)
	{
		if (!((UnityEngine.Object)(object)info.target == null) && !((UnityEngine.Object)(object)info.statusEffect == null))
		{
			OnTakeShield?.Invoke(info);
		}
	}

	protected static void InvokeUserCode_InvokeOnTakeShield__EventInfoShield(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnTakeShield called on server.");
		}
		else
		{
			((ClientEventManager)(object)obj).UserCode_InvokeOnTakeShield__EventInfoShield(GeneratedNetworkCode._Read_EventInfoShield(reader));
		}
	}

	protected void UserCode_InvokeOnDeath__EventInfoKill(EventInfoKill info)
	{
		if (!((UnityEngine.Object)(object)info.victim == null))
		{
			OnDeath?.Invoke(info);
		}
	}

	protected static void InvokeUserCode_InvokeOnDeath__EventInfoKill(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnDeath called on server.");
		}
		else
		{
			((ClientEventManager)(object)obj).UserCode_InvokeOnDeath__EventInfoKill(GeneratedNetworkCode._Read_EventInfoKill(reader));
		}
	}

	protected void UserCode_InvokeOnGetManaSpent__EventInfoSpentMana(EventInfoSpentMana info)
	{
		if (!((UnityEngine.Object)(object)info.entity == null))
		{
			OnGetManaSpent?.Invoke(info);
		}
	}

	protected static void InvokeUserCode_InvokeOnGetManaSpent__EventInfoSpentMana(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnGetManaSpent called on server.");
		}
		else
		{
			((ClientEventManager)(object)obj).UserCode_InvokeOnGetManaSpent__EventInfoSpentMana(GeneratedNetworkCode._Read_EventInfoSpentMana(reader));
		}
	}

	protected void UserCode_InvokeOnAttackMissed__EventInfoAttackMissed(EventInfoAttackMissed info)
	{
		if (!((UnityEngine.Object)(object)info.victim == null))
		{
			OnAttackMissed?.Invoke(info);
		}
	}

	protected static void InvokeUserCode_InvokeOnAttackMissed__EventInfoAttackMissed(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnAttackMissed called on server.");
		}
		else
		{
			((ClientEventManager)(object)obj).UserCode_InvokeOnAttackMissed__EventInfoAttackMissed(GeneratedNetworkCode._Read_EventInfoAttackMissed(reader));
		}
	}

	protected void UserCode_InvokeOnDamageNegated__EventInfoDamageNegatedByImmunity(EventInfoDamageNegatedByImmunity info)
	{
		if (!((UnityEngine.Object)(object)info.victim == null))
		{
			OnDamageNegated?.Invoke(info);
		}
	}

	protected static void InvokeUserCode_InvokeOnDamageNegated__EventInfoDamageNegatedByImmunity(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnDamageNegated called on server.");
		}
		else
		{
			((ClientEventManager)(object)obj).UserCode_InvokeOnDamageNegated__EventInfoDamageNegatedByImmunity(GeneratedNetworkCode._Read_EventInfoDamageNegatedByImmunity(reader));
		}
	}

	protected void UserCode_InvokeOnDamageNegatedByShield__EventInfoDamageNegatedByShield(EventInfoDamageNegatedByShield info)
	{
		if (!((UnityEngine.Object)(object)info.victim == null))
		{
			OnDamageNegatedByShield?.Invoke(info);
		}
	}

	protected static void InvokeUserCode_InvokeOnDamageNegatedByShield__EventInfoDamageNegatedByShield(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnDamageNegatedByShield called on server.");
		}
		else
		{
			((ClientEventManager)(object)obj).UserCode_InvokeOnDamageNegatedByShield__EventInfoDamageNegatedByShield(GeneratedNetworkCode._Read_EventInfoDamageNegatedByShield(reader));
		}
	}

	protected void UserCode_InvokeOnAttackHit__EventInfoAttackHit(EventInfoAttackHit info)
	{
		if (!((UnityEngine.Object)(object)info.victim == null))
		{
			OnAttackHit?.Invoke(info);
		}
	}

	protected static void InvokeUserCode_InvokeOnAttackHit__EventInfoAttackHit(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnAttackHit called on server.");
		}
		else
		{
			((ClientEventManager)(object)obj).UserCode_InvokeOnAttackHit__EventInfoAttackHit(GeneratedNetworkCode._Read_EventInfoAttackHit(reader));
		}
	}

	protected void UserCode_InvokeOnItemSold__Hero__NetworkBehaviour(Hero h, NetworkBehaviour nb)
	{
		if (!((UnityEngine.Object)(object)h == null) && !((UnityEngine.Object)(object)nb == null))
		{
			OnItemSold?.Invoke(h, nb);
		}
	}

	protected static void InvokeUserCode_InvokeOnItemSold__Hero__NetworkBehaviour(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnItemSold called on server.");
		}
		else
		{
			((ClientEventManager)(object)obj).UserCode_InvokeOnItemSold__Hero__NetworkBehaviour(NetworkReaderExtensions.ReadNetworkBehaviour<Hero>(reader), NetworkReaderExtensions.ReadNetworkBehaviour(reader));
		}
	}

	protected void UserCode_InvokeOnItemCleansed__Hero__NetworkBehaviour(Hero h, NetworkBehaviour nb)
	{
		if (!((UnityEngine.Object)(object)h == null) && !((UnityEngine.Object)(object)nb == null))
		{
			OnItemCleansed?.Invoke(h, nb);
		}
	}

	protected static void InvokeUserCode_InvokeOnItemCleansed__Hero__NetworkBehaviour(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnItemCleansed called on server.");
		}
		else
		{
			((ClientEventManager)(object)obj).UserCode_InvokeOnItemCleansed__Hero__NetworkBehaviour(NetworkReaderExtensions.ReadNetworkBehaviour<Hero>(reader), NetworkReaderExtensions.ReadNetworkBehaviour(reader));
		}
	}

	protected void UserCode_InvokeOnItemBought__Hero__NetworkBehaviour(Hero h, NetworkBehaviour nb)
	{
		if (!((UnityEngine.Object)(object)h == null) && !((UnityEngine.Object)(object)nb == null))
		{
			OnItemBought?.Invoke(h, nb);
		}
	}

	protected static void InvokeUserCode_InvokeOnItemBought__Hero__NetworkBehaviour(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnItemBought called on server.");
		}
		else
		{
			((ClientEventManager)(object)obj).UserCode_InvokeOnItemBought__Hero__NetworkBehaviour(NetworkReaderExtensions.ReadNetworkBehaviour<Hero>(reader), NetworkReaderExtensions.ReadNetworkBehaviour(reader));
		}
	}

	protected void UserCode_InvokeOnGemMergeUpgraded__Hero__Gem(Hero h, Gem g)
	{
		if (!((UnityEngine.Object)(object)h == null) && !((UnityEngine.Object)(object)g == null))
		{
			OnGemMergeUpgraded?.Invoke(h, g);
		}
	}

	protected static void InvokeUserCode_InvokeOnGemMergeUpgraded__Hero__Gem(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnGemMergeUpgraded called on server.");
		}
		else
		{
			((ClientEventManager)(object)obj).UserCode_InvokeOnGemMergeUpgraded__Hero__Gem(NetworkReaderExtensions.ReadNetworkBehaviour<Hero>(reader), NetworkReaderExtensions.ReadNetworkBehaviour<Gem>(reader));
		}
	}

	protected void UserCode_InvokeOnItemUpgraded__Hero__NetworkBehaviour(Hero h, NetworkBehaviour nb)
	{
		if (!((UnityEngine.Object)(object)h == null) && !((UnityEngine.Object)(object)nb == null))
		{
			OnItemUpgraded?.Invoke(h, nb);
		}
	}

	protected static void InvokeUserCode_InvokeOnItemUpgraded__Hero__NetworkBehaviour(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnItemUpgraded called on server.");
		}
		else
		{
			((ClientEventManager)(object)obj).UserCode_InvokeOnItemUpgraded__Hero__NetworkBehaviour(NetworkReaderExtensions.ReadNetworkBehaviour<Hero>(reader), NetworkReaderExtensions.ReadNetworkBehaviour(reader));
		}
	}

	protected void UserCode_InvokeOnChaosUsed__DewPlayer(DewPlayer p)
	{
		OnChaosUsed?.Invoke(p);
	}

	protected static void InvokeUserCode_InvokeOnChaosUsed__DewPlayer(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnChaosUsed called on server.");
		}
		else
		{
			((ClientEventManager)(object)obj).UserCode_InvokeOnChaosUsed__DewPlayer(NetworkReaderExtensions.ReadNetworkBehaviour<DewPlayer>(reader));
		}
	}

	protected void UserCode_InvokeOnDismantled__Hero__NetworkBehaviour(Hero h, NetworkBehaviour nb)
	{
		if (!((UnityEngine.Object)(object)h == null) && !((UnityEngine.Object)(object)nb == null))
		{
			OnDismantled?.Invoke(h, nb);
		}
	}

	protected static void InvokeUserCode_InvokeOnDismantled__Hero__NetworkBehaviour(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnDismantled called on server.");
		}
		else
		{
			((ClientEventManager)(object)obj).UserCode_InvokeOnDismantled__Hero__NetworkBehaviour(NetworkReaderExtensions.ReadNetworkBehaviour<Hero>(reader), NetworkReaderExtensions.ReadNetworkBehaviour(reader));
		}
	}

	protected void UserCode_InvokeOnIgnoreCC__Entity(Entity ent)
	{
		if (!((UnityEngine.Object)(object)ent == null))
		{
			OnIgnoreCC?.Invoke(ent);
		}
	}

	protected static void InvokeUserCode_InvokeOnIgnoreCC__Entity(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnIgnoreCC called on server.");
		}
		else
		{
			((ClientEventManager)(object)obj).UserCode_InvokeOnIgnoreCC__Entity(NetworkReaderExtensions.ReadNetworkBehaviour<Entity>(reader));
		}
	}

	protected void UserCode_InvokeOnApplyElemental__EventInfoApplyElemental(EventInfoApplyElemental info)
	{
		if (!((UnityEngine.Object)(object)info.victim == null) && !((UnityEngine.Object)(object)info.actor == null))
		{
			OnApplyElemental?.Invoke(info);
		}
	}

	protected static void InvokeUserCode_InvokeOnApplyElemental__EventInfoApplyElemental(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnApplyElemental called on server.");
		}
		else
		{
			((ClientEventManager)(object)obj).UserCode_InvokeOnApplyElemental__EventInfoApplyElemental(GeneratedNetworkCode._Read_EventInfoApplyElemental(reader));
		}
	}

	protected void UserCode_InvokeOnRefreshEntityHealthbar__Entity(Entity e)
	{
		if (!((UnityEngine.Object)(object)e == null))
		{
			OnRefreshEntityHealthbar?.Invoke(e);
		}
	}

	protected static void InvokeUserCode_InvokeOnRefreshEntityHealthbar__Entity(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnRefreshEntityHealthbar called on server.");
		}
		else
		{
			((ClientEventManager)(object)obj).UserCode_InvokeOnRefreshEntityHealthbar__Entity(NetworkReaderExtensions.ReadNetworkBehaviour<Entity>(reader));
		}
	}

	protected void UserCode_InvokeOnCastComplete__EventInfoCast(EventInfoCast eventInfo)
	{
		if (!((UnityEngine.Object)(object)eventInfo.instance == null) && !((UnityEngine.Object)(object)eventInfo.trigger == null))
		{
			OnCastComplete?.Invoke(eventInfo);
		}
	}

	protected static void InvokeUserCode_InvokeOnCastComplete__EventInfoCast(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnCastComplete called on server.");
		}
		else
		{
			((ClientEventManager)(object)obj).UserCode_InvokeOnCastComplete__EventInfoCast(GeneratedNetworkCode._Read_EventInfoCast(reader));
		}
	}

	static ClientEventManager()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected Obj, but got Unknown
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected Obj, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected Obj, but got Unknown
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected Obj, but got Unknown
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected Obj, but got Unknown
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Expected Obj, but got Unknown
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Expected Obj, but got Unknown
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Expected Obj, but got Unknown
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Expected Obj, but got Unknown
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Expected Obj, but got Unknown
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Expected Obj, but got Unknown
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Expected Obj, but got Unknown
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Expected Obj, but got Unknown
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Expected Obj, but got Unknown
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Expected Obj, but got Unknown
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Expected Obj, but got Unknown
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Expected Obj, but got Unknown
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Expected Obj, but got Unknown
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Expected Obj, but got Unknown
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(ClientEventManager), "System.Void ClientEventManager::InvokeOnTakeManaHeal(EventInfoHeal)", (RemoteCallDelegate)InvokeUserCode_InvokeOnTakeManaHeal__EventInfoHeal);
		RemoteProcedureCalls.RegisterRpc(typeof(ClientEventManager), "System.Void ClientEventManager::InvokeOnTakeHeal(EventInfoHeal)", (RemoteCallDelegate)InvokeUserCode_InvokeOnTakeHeal__EventInfoHeal);
		RemoteProcedureCalls.RegisterRpc(typeof(ClientEventManager), "System.Void ClientEventManager::InvokeOnTakeDamage(EventInfoDamage)", (RemoteCallDelegate)InvokeUserCode_InvokeOnTakeDamage__EventInfoDamage);
		RemoteProcedureCalls.RegisterRpc(typeof(ClientEventManager), "System.Void ClientEventManager::InvokeOnTakeShield(EventInfoShield)", (RemoteCallDelegate)InvokeUserCode_InvokeOnTakeShield__EventInfoShield);
		RemoteProcedureCalls.RegisterRpc(typeof(ClientEventManager), "System.Void ClientEventManager::InvokeOnDeath(EventInfoKill)", (RemoteCallDelegate)InvokeUserCode_InvokeOnDeath__EventInfoKill);
		RemoteProcedureCalls.RegisterRpc(typeof(ClientEventManager), "System.Void ClientEventManager::InvokeOnGetManaSpent(EventInfoSpentMana)", (RemoteCallDelegate)InvokeUserCode_InvokeOnGetManaSpent__EventInfoSpentMana);
		RemoteProcedureCalls.RegisterRpc(typeof(ClientEventManager), "System.Void ClientEventManager::InvokeOnAttackMissed(EventInfoAttackMissed)", (RemoteCallDelegate)InvokeUserCode_InvokeOnAttackMissed__EventInfoAttackMissed);
		RemoteProcedureCalls.RegisterRpc(typeof(ClientEventManager), "System.Void ClientEventManager::InvokeOnDamageNegated(EventInfoDamageNegatedByImmunity)", (RemoteCallDelegate)InvokeUserCode_InvokeOnDamageNegated__EventInfoDamageNegatedByImmunity);
		RemoteProcedureCalls.RegisterRpc(typeof(ClientEventManager), "System.Void ClientEventManager::InvokeOnDamageNegatedByShield(EventInfoDamageNegatedByShield)", (RemoteCallDelegate)InvokeUserCode_InvokeOnDamageNegatedByShield__EventInfoDamageNegatedByShield);
		RemoteProcedureCalls.RegisterRpc(typeof(ClientEventManager), "System.Void ClientEventManager::InvokeOnAttackHit(EventInfoAttackHit)", (RemoteCallDelegate)InvokeUserCode_InvokeOnAttackHit__EventInfoAttackHit);
		RemoteProcedureCalls.RegisterRpc(typeof(ClientEventManager), "System.Void ClientEventManager::InvokeOnItemSold(Hero,Mirror.NetworkBehaviour)", (RemoteCallDelegate)InvokeUserCode_InvokeOnItemSold__Hero__NetworkBehaviour);
		RemoteProcedureCalls.RegisterRpc(typeof(ClientEventManager), "System.Void ClientEventManager::InvokeOnItemCleansed(Hero,Mirror.NetworkBehaviour)", (RemoteCallDelegate)InvokeUserCode_InvokeOnItemCleansed__Hero__NetworkBehaviour);
		RemoteProcedureCalls.RegisterRpc(typeof(ClientEventManager), "System.Void ClientEventManager::InvokeOnItemBought(Hero,Mirror.NetworkBehaviour)", (RemoteCallDelegate)InvokeUserCode_InvokeOnItemBought__Hero__NetworkBehaviour);
		RemoteProcedureCalls.RegisterRpc(typeof(ClientEventManager), "System.Void ClientEventManager::InvokeOnGemMergeUpgraded(Hero,Gem)", (RemoteCallDelegate)InvokeUserCode_InvokeOnGemMergeUpgraded__Hero__Gem);
		RemoteProcedureCalls.RegisterRpc(typeof(ClientEventManager), "System.Void ClientEventManager::InvokeOnItemUpgraded(Hero,Mirror.NetworkBehaviour)", (RemoteCallDelegate)InvokeUserCode_InvokeOnItemUpgraded__Hero__NetworkBehaviour);
		RemoteProcedureCalls.RegisterRpc(typeof(ClientEventManager), "System.Void ClientEventManager::InvokeOnChaosUsed(DewPlayer)", (RemoteCallDelegate)InvokeUserCode_InvokeOnChaosUsed__DewPlayer);
		RemoteProcedureCalls.RegisterRpc(typeof(ClientEventManager), "System.Void ClientEventManager::InvokeOnDismantled(Hero,Mirror.NetworkBehaviour)", (RemoteCallDelegate)InvokeUserCode_InvokeOnDismantled__Hero__NetworkBehaviour);
		RemoteProcedureCalls.RegisterRpc(typeof(ClientEventManager), "System.Void ClientEventManager::InvokeOnIgnoreCC(Entity)", (RemoteCallDelegate)InvokeUserCode_InvokeOnIgnoreCC__Entity);
		RemoteProcedureCalls.RegisterRpc(typeof(ClientEventManager), "System.Void ClientEventManager::InvokeOnApplyElemental(EventInfoApplyElemental)", (RemoteCallDelegate)InvokeUserCode_InvokeOnApplyElemental__EventInfoApplyElemental);
		RemoteProcedureCalls.RegisterRpc(typeof(ClientEventManager), "System.Void ClientEventManager::InvokeOnRefreshEntityHealthbar(Entity)", (RemoteCallDelegate)InvokeUserCode_InvokeOnRefreshEntityHealthbar__Entity);
		RemoteProcedureCalls.RegisterRpc(typeof(ClientEventManager), "System.Void ClientEventManager::InvokeOnCastComplete(EventInfoCast)", (RemoteCallDelegate)InvokeUserCode_InvokeOnCastComplete__EventInfoCast);
	}
}
