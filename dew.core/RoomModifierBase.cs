using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

public class RoomModifierBase : Actor, IExcludeFromPool
{
	[FormerlySerializedAs("showOnTopRight")]
	public bool isMain;

	public Color mainColor = Color.white;

	public Sprite mapSprite;

	public float mapSpriteScale = 1f;

	public NodeModifierVisibility visibilityOnWorld = NodeModifierVisibility.OnRevealedFull;

	public bool hiddenOnVisitedNode;

	public string roomOverride;

	public bool removeModifierWhenOverrideOverwritten = true;

	public bool excludeFromPool;

	public ModifierSpawnType spawnType;

	public float chance = 0.01f;

	public ScaleWithDifficultyMode difficultyScaling;

	public string[] allowedZones;

	public Vector2Int zoneIndexRange = new Vector2Int(0, int.MaxValue);

	public bool spawnOncePerLoop;

	public bool disallowOtherModifiers;

	public bool modifiesRewards;

	public GameObject fxLoop;

	public bool disableOtherEnvParticles;

	public bool enableInnerDecorations;

	public DecorationSettings innerDecorations;

	public bool enableOuterDecorations;

	public DecorationSettings outerDecorations;

	[CompilerGenerated]
	[SyncVar]
	private int id__BackingField;

	public readonly List<GameObject> spawnedDecorations = new List<GameObject>();

	private Action<Entity> _onEntityAdd;

	private readonly List<Action> _onDestroyActor = new List<Action>();

	bool IExcludeFromPool.excludeFromPool => excludeFromPool;

	[SaveVar(SaveVarFlags.Default)]
	public int id
	{
		[CompilerGenerated]
		get
		{
			return id__BackingField;
		}
		[CompilerGenerated]
		internal set
		{
			Network_003Cid_003Ek__BackingField = value;
		}
	}

	public ModifierData modData => NetworkedManagerBase<ZoneManager>.instance.currentNode.modifiers.Find((ModifierData m) => m.id == id);

	public override bool isDestroyedOnRoomChange => false;

	public int Network_003Cid_003Ek__BackingField
	{
		get
		{
			return id__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref id__BackingField, 8uL, (Action<int, int>)null);
		}
	}

	private string GetZoneValidationMessage(string[] obj)
	{
		if (obj == null)
		{
			return null;
		}
		foreach (string text in obj)
		{
			if (!DewResources.database.nameToGuid.ContainsKey(text))
			{
				return "Zone of name '" + text + "' not found";
			}
		}
		return null;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		FxPlay(fxLoop);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoadStarted += new Action<EventInfoLoadRoom>(ClientEventOnRoomLoadStarted);
		NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnEntityAdd += new Action<Entity>(OnEntityAdd);
		if (spawnOncePerLoop)
		{
			string name = ((object)this).GetType().Name;
			if (!NetworkedManagerBase<ZoneManager>.instance.bannedRoomModifiersForCurrentLoop.Contains(name))
			{
				NetworkedManagerBase<ZoneManager>.instance.bannedRoomModifiersForCurrentLoop.Add(name);
			}
		}
	}

	private void OnEntityAdd(Entity obj)
	{
		try
		{
			_onEntityAdd?.Invoke(obj);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		FxStop(fxLoop);
		foreach (GameObject spawnedDecoration in spawnedDecorations)
		{
			if (spawnedDecoration != null)
			{
				UnityEngine.Object.Destroy(spawnedDecoration);
			}
		}
		spawnedDecorations.Clear();
		if (disableOtherEnvParticles)
		{
			foreach (EnvParticle instance in EnvParticle.instances)
			{
				if (!(instance == null))
				{
					instance.gameObject.SetActive(value: true);
				}
			}
		}
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null)
			{
				NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoadStarted -= new Action<EventInfoLoadRoom>(ClientEventOnRoomLoadStarted);
			}
			if ((UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance != null)
			{
				NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnEntityAdd -= new Action<Entity>(OnEntityAdd);
			}
		}
		foreach (Action item in _onDestroyActor)
		{
			try
			{
				item();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		_onDestroyActor.Clear();
	}

	[Server]
	public void ModifyEntities(Action<Entity> onStart, Action<Entity> onStop)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void RoomModifierBase::ModifyEntities(System.Action`1<Entity>,System.Action`1<Entity>)' called when server was not active");
			return;
		}
		foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
		{
			try
			{
				onStart?.Invoke(allEntity);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		_onEntityAdd = (Action<Entity>)Delegate.Combine(_onEntityAdd, onStart);
		_onDestroyActor.Add(() =>
		{
			foreach (Entity allEntity2 in NetworkedManagerBase<ActorManager>.instance.allEntities)
			{
				try
				{
					onStop?.Invoke(allEntity2);
				}
				catch (Exception exception2)
				{
					Debug.LogException(exception2);
				}
			}
		});
	}

	public override void OnStart()
	{
		base.OnStart();
		spawnedDecorations.Clear();
		if (enableInnerDecorations)
		{
			spawnedDecorations.AddRange(PlaceDecorations(innerDecorations, SingletonDewNetworkBehaviour<Room>.instance.map.mapData.innerPropNodeIndices));
		}
		if (enableOuterDecorations)
		{
			spawnedDecorations.AddRange(PlaceDecorations(outerDecorations, SingletonDewNetworkBehaviour<Room>.instance.map.mapData.outerPropNodeIndices));
		}
		if (enableInnerDecorations)
		{
			GameObject[] decorations = innerDecorations.decorations;
			for (int i = 0; i < decorations.Length; i++)
			{
				decorations[i].SetActive(value: false);
			}
		}
		if (enableOuterDecorations)
		{
			GameObject[] decorations = outerDecorations.decorations;
			for (int i = 0; i < decorations.Length; i++)
			{
				decorations[i].SetActive(value: false);
			}
		}
		if (!disableOtherEnvParticles)
		{
			return;
		}
		foreach (EnvParticle instance in EnvParticle.instances)
		{
			if (!(instance == null))
			{
				instance.gameObject.SetActive(value: false);
			}
		}
	}

	public static List<GameObject> PlaceDecorations(DecorationSettings s, IReadOnlyList<(int, int)> indices)
	{
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		List<GameObject> list = new List<GameObject>();
		float cellSize = SingletonDewNetworkBehaviour<Room>.instance.map.mapData.cells.cellSize;
		int num = Mathf.RoundToInt(cellSize * cellSize * (float)indices.Count * s.decoDensity);
		for (int i = 0; i < num; i++)
		{
			GameObject gameObject = s.decorations[UnityEngine.Random.Range(0, s.decorations.Length)];
			Vector3 vector = SingletonDewNetworkBehaviour<Room>.instance.map.mapData.cells.GetWorldPos(indices[UnityEngine.Random.Range(0, indices.Count)]).ToXZ();
			vector += UnityEngine.Random.insideUnitSphere * s.decoPositionRandomMag;
			vector = Dew.GetPositionOnGround(vector) + gameObject.transform.localPosition;
			RaycastHit[] array = DewPool.GetArray(out ArrayReturnHandle<RaycastHit> handle, 128);
			int num2 = Physics.RaycastNonAlloc(vector + Vector3.up * 5f, Vector3.down, array, 10f, LayerMasks.Ground);
			bool flag = false;
			for (int j = 0; j < num2; j++)
			{
				RaycastHit val = array[j];
				if (((Component)(object)val.collider).TryGetComponent(out Room_SurfaceOverride component) && component.data != null && component.data.banDecorations)
				{
					flag = true;
					break;
				}
			}
			handle.Return();
			if (!flag)
			{
				Quaternion quaternion = Quaternion.Euler(UnityEngine.Random.Range(s.decoRotationMin.x, s.decoRotationMax.x), UnityEngine.Random.Range(s.decoRotationMin.y, s.decoRotationMax.y), UnityEngine.Random.Range(s.decoRotationMin.z, s.decoRotationMax.z)) * gameObject.transform.localRotation;
				Vector3 zero = Vector3.zero;
				if (s.uniformScale)
				{
					float num3 = UnityEngine.Random.Range(s.decoScaleMin.x, s.decoScaleMax.x);
					zero = Vector3.Scale(gameObject.transform.localScale, new Vector3(num3, num3, num3));
				}
				else
				{
					zero = Vector3.Scale(gameObject.transform.localScale, new Vector3(UnityEngine.Random.Range(s.decoScaleMin.x, s.decoScaleMax.x), UnityEngine.Random.Range(s.decoScaleMin.y, s.decoScaleMax.y), UnityEngine.Random.Range(s.decoScaleMin.z, s.decoScaleMax.z)));
				}
				GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, vector, quaternion);
				gameObject2.transform.localScale = zero;
				list.Add(gameObject2);
			}
		}
		return list;
	}

	private void ClientEventOnRoomLoadStarted(EventInfoLoadRoom _)
	{
		NetworkedManagerBase<ZoneManager>.instance.modifierServerData[id].persistentData = DewPersistence.SerializeGameObject(((Component)(object)this).gameObject);
		DestroyIfActive();
	}

	public float GetScaledChance()
	{
		return difficultyScaling switch
		{
			ScaleWithDifficultyMode.None => chance, 
			ScaleWithDifficultyMode.Beneficial => chance * NetworkedManagerBase<GameManager>.instance.difficulty.beneficialNodeMultiplier, 
			ScaleWithDifficultyMode.Harmful => chance * NetworkedManagerBase<GameManager>.instance.difficulty.harmfulNodeMultiplier, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public virtual bool IsAvailableInGame()
	{
		return true;
	}

	public virtual bool CanSpawnAtNode(int nodeIndex)
	{
		return true;
	}

	public override bool ShouldBeSavedWithRoom()
	{
		return false;
	}

	[Server]
	public void RemoveModifier()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void RoomModifierBase::RemoveModifier()' called when server was not active");
		}
		else
		{
			NetworkedManagerBase<ZoneManager>.instance.RemoveModifier(NetworkedManagerBase<ZoneManager>.instance.currentNodeIndex, id);
		}
	}

	[Server]
	public void PlaceShrine<T>(PlaceShrineSettings settings) where T : Shrine
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void RoomModifierBase::PlaceShrine(PlaceShrineSettings)' called when server was not active");
			return;
		}
		DewRandom roomRandom = SingletonDewNetworkBehaviour<Room>.instance.GetRoomRandom(-10000 + id);
		Shrine shrine = null;
		if (!isNewInstance)
		{
			foreach (Actor allActor in NetworkedManagerBase<ActorManager>.instance.allActors)
			{
				if (allActor is Shrine shrine2 && shrine2.persistentData.TryGetData<string>("RoomModifierBase", "placeShrineId", out var value) && !(value != typeof(T).Name))
				{
					shrine = shrine2;
					break;
				}
			}
		}
		else
		{
			Vector3 value2;
			if (settings.customPosition.HasValue)
			{
				value2 = settings.customPosition.Value;
			}
			else if (settings.spawnOnLastSection)
			{
				SingletonDewNetworkBehaviour<Room>.instance.GetFinalSection().TryGetGoodNodePosition(out value2, roomRandom);
			}
			else
			{
				SingletonDewNetworkBehaviour<Room>.instance.props.TryGetGoodNodePosition(out value2, roomRandom);
			}
			shrine = Dew.CreateActor<T>(value2, null, this);
			shrine.persistentData.SetData("RoomModifierBase", "placeShrineId", typeof(T).Name);
			if (settings.lockedUntilCleared)
			{
				shrine.isLocked = true;
				if (SingletonDewNetworkBehaviour<Room>.instance.didClearRoom)
				{
					Listener();
				}
				else
				{
					SingletonDewNetworkBehaviour<Room>.instance.onRoomClear.AddListener(Listener);
				}
			}
		}
		if ((UnityEngine.Object)(object)shrine == null)
		{
			return;
		}
		Action<Entity> onSuccessfulUse = (Entity _) =>
		{
			if (shrine.maxTotalUseCount >= 0 && shrine.totalUseCount >= shrine.maxTotalUseCount && settings.removeModifierOnUse)
			{
				RemoveModifier();
			}
		};
		shrine.ClientEvent_OnSuccessfulUse += onSuccessfulUse;
		_onDestroyActor.Add(() =>
		{
			if ((UnityEngine.Object)(object)shrine != null)
			{
				shrine.ClientEvent_OnSuccessfulUse -= onSuccessfulUse;
			}
		});
		void Listener()
		{
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			while ((UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.instance != null && SingletonDewNetworkBehaviour<Room>.instance.monsters.isDoingHunterWelcomingSpawn)
			{
				yield return new WaitForSeconds(0.5f);
			}
			if ((UnityEngine.Object)(object)shrine != null)
			{
				shrine.isLocked = false;
			}
		}
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteInt(writer, id__BackingField);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 8L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, id__BackingField);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref id__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 8L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref id__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
	}
}
