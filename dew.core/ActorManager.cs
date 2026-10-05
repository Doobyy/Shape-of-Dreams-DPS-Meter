using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class ActorManager : NetworkedManagerBase<ActorManager>
{
	public static bool enableUsefulActorName;

	public SafeAction<Actor> ClientEvent_OnActorAdd;

	public SafeAction<Actor> ClientEvent_OnActorRemove;

	public SafeAction<Entity> ClientEvent_OnEntityAdd;

	public SafeAction<Entity> ClientEvent_OnEntityRemove;

	public SafeAction<Entity> ClientEvent_OnAwakeEntityAdd;

	public SafeAction<Entity> ClientEvent_OnAwakeEntityRemove;

	public SafeAction<Hero> ClientEvent_OnHeroAdd;

	public SafeAction<Hero> ClientEvent_OnHeroRemove;

	public SafeAction<Hero> ClientEvent_OnLocalHeroAdd;

	public SafeAction<Hero> ClientEvent_OnLocalHeroRemove;

	public SafeAction<Actor> onActorBeforePrepare;

	[CompilerGenerated]
	[SyncVar]
	private Actor serverActor__BackingField;

	public HashSet<Actor> allActors = new HashSet<Actor>();

	public HashSet<Actor> allActorsBeingDestroyed = new HashSet<Actor>();

	public HashSet<Entity> allEntities = new HashSet<Entity>();

	public List<Hero> allHeroes = new List<Hero>();

	private float _lastNullEntryCheckTime;

	private const float SleepTickInterval = 1f;

	private const float HeroWakeEntitiesSqrDistance = 1600f;

	private const float TimeTakenToStartSleeping = 4f;

	private const float HeroTeleportMinDistanceForWakeRecalculation = 5f;

	private float _nextSleepCheck = float.PositiveInfinity;

	private const float StuckCheckInterval = 0.1f;

	private const int StuckCheckStrikes = 3;

	private int _currentIndex;

	private int _stuckCheckVersion;

	private float _nextStuckCheckTime;

	protected NetworkBehaviourSyncVar ____003CserverActor_003Ek__BackingFieldNetId;

	public Actor serverActor
	{
		[CompilerGenerated]
		get
		{
			return Network_003CserverActor_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CserverActor_003Ek__BackingField = value;
		}
	}

	public int numOfActors => allActors.Count;

	public int numOfEntities => allEntities.Count;

	public int numOfHeroes => allHeroes.Count;

	public Actor Network_003CserverActor_003Ek__BackingField
	{
		get
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return ((NetworkBehaviour)this).GetSyncVarNetworkBehaviour<Actor>(____003CserverActor_003Ek__BackingFieldNetId, ref serverActor__BackingField);
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter_NetworkBehaviour<Actor>(value, ref serverActor__BackingField, 1uL, (Action<Actor, Actor>)null, ref ____003CserverActor_003Ek__BackingFieldNetId);
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Init()
	{
		enableUsefulActorName = false;
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		Network_003CserverActor_003Ek__BackingField = Dew.CreateActor<ServerActor>(Vector3.zero, Quaternion.identity);
		DoSleepOnStartServer();
	}

	public override void OnStart()
	{
		base.OnStart();
		DewResources.AddPreloadRule((MonoBehaviour)(object)this, (PreloadInterface preload) =>
		{
			foreach (Actor item in allActors.Concat(allActorsBeingDestroyed))
			{
				if (!item.IsNullOrInactive())
				{
					preload.AddType(((object)item).GetType().Name);
				}
			}
		});
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		DoSleepLogicUpdate(dt);
		DoStuckCheckLogicUpdate(dt);
		DoCollectionsLogicUpdate(dt);
	}

	[Server]
	public void ClearSceneIdOfAllActors()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ActorManager::ClearSceneIdOfAllActors()' called when server was not active");
			return;
		}
		Actor[] array = UnityEngine.Object.FindObjectsOfType<Actor>(true);
		for (int i = 0; i < array.Length; i++)
		{
			if (((Component)(object)array[i]).TryGetComponent(out NetworkIdentity component))
			{
				component.sceneId = 0uL;
			}
		}
		RpcClearSceneIdOfAllActors();
	}

	[ClientRpc]
	private void RpcClearSceneIdOfAllActors()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void ActorManager::RpcClearSceneIdOfAllActors()", -1762359386, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public static void CleanupActorsBeforeShutdown()
	{
		Actor[] array = UnityEngine.Object.FindObjectsOfType<Actor>(true);
		foreach (Actor actor in array)
		{
			if (!((UnityEngine.Object)(object)actor == null) && (!(DewResources.variantsParent != null) || !(((Component)(object)actor).transform.parent == DewResources.variantsParent)) && (!(ManagerBase<SpawnManager>.softInstance != null) || !(((Component)(object)actor).transform.parent == ManagerBase<SpawnManager>.softInstance.transform)))
			{
				UnityEngine.Object.Destroy(((Component)(object)actor).gameObject);
			}
		}
	}

	internal void AddActor(Actor actor)
	{
		allActors.Add(actor);
		ClientEvent_OnActorAdd?.Invoke(actor);
		if (actor is Entity entity)
		{
			allEntities.Add(entity);
			if (NetworkServer.active)
			{
				RegisterSleepEvents(entity);
			}
			ClientEvent_OnEntityAdd?.Invoke(entity);
			ClientEvent_OnAwakeEntityAdd?.Invoke(entity);
		}
		if (actor is Hero hero)
		{
			allHeroes.Add(hero);
			ClientEvent_OnHeroAdd?.Invoke(hero);
		}
	}

	internal void RemoveActor(Actor actor)
	{
		if (allActors.Remove(actor))
		{
			ClientEvent_OnActorRemove?.Invoke(actor);
		}
		if (actor is Entity entity && allEntities.Remove(entity))
		{
			ClientEvent_OnEntityRemove?.Invoke(entity);
		}
		if (actor is Hero hero && allHeroes.Remove(hero))
		{
			ClientEvent_OnHeroRemove?.Invoke(hero);
		}
	}

	internal void AddActorBeingDestroyed(Actor actor)
	{
		if (actor != null)
		{
			allActorsBeingDestroyed.Add(actor);
		}
	}

	internal void RemoveActorBeingDestroyed(Actor actor)
	{
		if (actor != null)
		{
			allActorsBeingDestroyed.Remove(actor);
		}
	}

	private void DoCollectionsLogicUpdate(float dt)
	{
		if (Time.time - _lastNullEntryCheckTime < 3f)
		{
			return;
		}
		_lastNullEntryCheckTime = Time.time;
		if (!((UnityEngine.Object)(object)DewPlayer.local != null) || !((UnityEngine.Object)(object)DewPlayer.local.hero != null) || !DewPlayer.local.hero.isInCombat)
		{
			allActors.RemoveWhere((Actor a) => a.IsNullOrInactive());
			allEntities.RemoveWhere((Entity a) => a.IsNullOrInactive());
			allHeroes.FilterInPlace((Hero a) => !a.IsNullOrInactive());
			allActorsBeingDestroyed.RemoveWhere((Actor a) => a.IsNullOrInactive());
		}
	}

	private void DoSleepOnStartServer()
	{
		GameManager.CallOnReady(() =>
		{
			_nextSleepCheck = Time.time + 1f;
		});
	}

	private void RegisterSleepEvents(Entity obj)
	{
		if (obj is Hero { Control: var control })
		{
			control.ClientEvent_OnTeleport += (Action<Vector3, Vector3>)((Vector3 from, Vector3 to) =>
			{
				if (Vector2.Distance(from.ToXY(), to.ToXY()) > 5f)
				{
					CalculateWakeImmediately();
				}
			});
		}
		obj.ActorEvent_OnDealDamage += (Action<EventInfoDamage>)((EventInfoDamage _) =>
		{
			obj.WakeUp();
		});
		obj.EntityEvent_OnTakeDamage += (Action<EventInfoDamage>)((EventInfoDamage _) =>
		{
			obj.WakeUp();
		});
		obj.EntityEvent_OnTakeHeal += (Action<EventInfoHeal>)((EventInfoHeal _) =>
		{
			obj.WakeUp();
		});
	}

	public void DoSleepLogicUpdate(float dt)
	{
		if (((NetworkBehaviour)this).isServer && Time.time > _nextSleepCheck)
		{
			_nextSleepCheck = Time.time + 1f;
			DoSleepTick();
		}
	}

	private void DoSleepTick()
	{
		foreach (Entity allEntity in allEntities)
		{
			if ((UnityEngine.Object)(object)allEntity == null || !allEntity.isActive)
			{
				continue;
			}
			bool flag = CanEntitySleep(allEntity);
			if (!flag)
			{
				allEntity._accumulatedSleepTime = 0f;
				allEntity.WakeUp();
			}
			else if (!allEntity.isSleeping & flag)
			{
				allEntity._accumulatedSleepTime++;
				if (allEntity._accumulatedSleepTime > 4f)
				{
					allEntity.Sleep();
				}
			}
		}
	}

	private void CalculateWakeImmediately()
	{
		foreach (Entity allEntity in allEntities)
		{
			if (!((UnityEngine.Object)(object)allEntity == null) && allEntity.isActive && !CanEntitySleep(allEntity))
			{
				allEntity._accumulatedSleepTime = 0f;
				allEntity.WakeUp();
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool CanEntitySleep(Entity e)
	{
		try
		{
			if (e is Hero)
			{
				return false;
			}
			if (e.IsAnyBoss())
			{
				return false;
			}
			if (!e.CanSleep())
			{
				return false;
			}
			foreach (Hero allHero in allHeroes)
			{
				if (Vector2.SqrMagnitude(allHero.position.ToXY() - e.position.ToXY()) < 1600f)
				{
					return false;
				}
			}
			return true;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return false;
		}
	}

	private void DoStuckCheckLogicUpdate(float dt)
	{
		if (!NetworkServer.active || NetworkedManagerBase<GameManager>.instance.disableStuckCheck || Time.time < _nextStuckCheckTime)
		{
			return;
		}
		if (NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition)
		{
			_stuckCheckVersion++;
			return;
		}
		_currentIndex++;
		if (_currentIndex >= allEntities.Count)
		{
			_currentIndex = 0;
		}
		if (allEntities.Count == 0)
		{
			return;
		}
		Entity entity = null;
		int num = 0;
		foreach (Entity allEntity in allEntities)
		{
			if (num++ == _currentIndex)
			{
				entity = allEntity;
				break;
			}
		}
		if (entity.IsNullInactiveDeadOrKnockedOut() || entity.Control.isDisplacing || !((Behaviour)(object)entity.Control._agent).enabled || entity.Visual.isSpawning)
		{
			_nextStuckCheckTime = 0f;
			return;
		}
		_nextStuckCheckTime = Time.time + 0.1f;
		if (entity._stuckCheckVersion != _stuckCheckVersion)
		{
			entity._stuckCheckVersion = _stuckCheckVersion;
			entity._wasStuckCounter = 0;
		}
		if (!Dew.IsOkay(entity.agentPosition) || CheckIfStuck(entity))
		{
			entity._wasStuckCounter++;
			if (entity._wasStuckCounter < 3)
			{
				return;
			}
			if (entity._wasStuckCounter == 3)
			{
				Debug.Log("Entity seems to be stuck: " + entity.GetActorReadableName());
			}
			if (entity.isSleeping)
			{
				entity.WakeUp();
			}
			if (entity is Monster monster && !monster.IsAnyBoss())
			{
				monster.PureDamage(monster.maxHealth * 0.1f * (float)(entity._wasStuckCounter - 3 + 1), 0f).SetAttr(DamageAttribute.DamageOverTime).SetAttr(DamageAttribute.IgnoreDamageImmunity)
					.Dispatch(monster);
				return;
			}
			Vector3 curr = entity.agentPosition;
			Dew.FilterNonOkayValues(ref curr);
			Vector3 start = Dew.SelectBestWithScore((IList<Vector3>)SingletonDewNetworkBehaviour<Room>.instance.playerPathablePoints, (Func<Vector3, int, float>)((Vector3 point, int _) => 0f - Vector3.Distance(point, curr)), 0f, (DewRandom)null);
			BossMonster bossMonster = Dew.FindActorOfType<BossMonster>();
			if ((UnityEngine.Object)(object)bossMonster != null && !CheckIfStuck(bossMonster))
			{
				start = bossMonster.agentPosition;
			}
			start = Dew.GetValidAgentDestination_Closest(start, curr);
			entity.Control.StartDisplacement(new DispByDestination
			{
				destination = start,
				duration = 0.2f,
				ease = DewEase.EaseOutQuad,
				canGoOverTerrain = true,
				isFriendly = true,
				rotateForward = false,
				rotateSmoothly = false,
				affectedByMovementSpeed = false,
				isCanceledByCC = false
			});
		}
		else
		{
			entity._wasStuckCounter = 0;
		}
	}

	private bool CheckIfStuck(Entity entity)
	{
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		if ((UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.softInstance == null)
		{
			return false;
		}
		Vector3 agentPosition = entity.agentPosition;
		if (NetworkedManagerBase<ZoneManager>.instance.currentNode.type == WorldNodeType.ExitBoss && entity is Hero)
		{
			BossMonster bossMonster = Dew.FindActorOfType<BossMonster>();
			if (!bossMonster.IsNullInactiveDeadOrKnockedOut() && !bossMonster.Control.isDisplacing && ((Behaviour)(object)bossMonster.Control._agent).enabled && !bossMonster.Visual.isSpawning && !CheckIfStuck(bossMonster) && (int)Dew.GetNavMeshPathStatus(agentPosition, bossMonster.agentPosition) != 0)
			{
				return true;
			}
		}
		if (SingletonDewNetworkBehaviour<Room>.softInstance.playerPathablePoints.Count == 0)
		{
			return false;
		}
		foreach (Vector3 playerPathablePoint in SingletonDewNetworkBehaviour<Room>.softInstance.playerPathablePoints)
		{
			if ((int)Dew.GetNavMeshPathStatus(Dew.GetPositionOnGround(playerPathablePoint), agentPosition) == 0)
			{
				return false;
			}
		}
		return true;
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcClearSceneIdOfAllActors()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			Actor[] array = UnityEngine.Object.FindObjectsOfType<Actor>(true);
			for (int i = 0; i < array.Length; i++)
			{
				((NetworkBehaviour)array[i]).netIdentity.sceneId = 0uL;
			}
		}
	}

	protected static void InvokeUserCode_RpcClearSceneIdOfAllActors(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcClearSceneIdOfAllActors called on server.");
		}
		else
		{
			((ActorManager)(object)obj).UserCode_RpcClearSceneIdOfAllActors();
		}
	}

	static ActorManager()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(ActorManager), "System.Void ActorManager::RpcClearSceneIdOfAllActors()", (RemoteCallDelegate)InvokeUserCode_RpcClearSceneIdOfAllActors);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		((NetworkBehaviour)this).SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_003CserverActor_003Ek__BackingField);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 1L) != 0L)
		{
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_003CserverActor_003Ek__BackingField);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		((NetworkBehaviour)this).DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<Actor>(ref serverActor__BackingField, (Action<Actor, Actor>)null, reader, ref ____003CserverActor_003Ek__BackingFieldNetId);
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 1L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<Actor>(ref serverActor__BackingField, (Action<Actor, Actor>)null, reader, ref ____003CserverActor_003Ek__BackingFieldNetId);
		}
	}
}
