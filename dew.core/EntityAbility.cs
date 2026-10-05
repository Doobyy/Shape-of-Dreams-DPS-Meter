using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class EntityAbility : EntityComponent, ICleanup
{
	public const int AttackAbilityIndex = 63;

	public SafeAction<int, AbilityTrigger> ClientEvent_OnAbilityAdded;

	public SafeAction<int, AbilityTrigger> ClientEvent_OnAbilityRemoved;

	[SyncVar]
	private AttackTrigger _originalAttackAbility;

	[SyncVar]
	private AbilityTrigger _overridenAttackAbility;

	internal readonly List<AbilityTrigger> _attackAbilityOverrides = new List<AbilityTrigger>();

	private NetworkBehaviourDictionaryWrapper<int, AbilityTrigger> _abilities;

	private Action<Actor> _cachedOnAbilityTriggerDestroyed;

	private readonly SyncDictionary<int, SyncedNetworkBehaviour> _synedAbilities = new SyncDictionary<int, SyncedNetworkBehaviour>();

	public AssetRef<AttackTrigger> attackAbilityPreset;

	public AssetRef<AbilityTrigger>[] abilityPreset;

	private List<AbilityLockHandle> _handles = new List<AbilityLockHandle>();

	[SyncVar]
	private ulong _abilityCastLockBitmap;

	[SyncVar]
	private ulong _abilityEditLockBitmap;

	[SyncVar]
	private ulong _showLockIconBitmap;

	protected NetworkBehaviourSyncVar ____originalAttackAbilityNetId;

	protected NetworkBehaviourSyncVar ____overridenAttackAbilityNetId;

	public AbilityTrigger attackAbility
	{
		get
		{
			if (!((UnityEngine.Object)(object)overridenAttackAbility != null))
			{
				return originalAttackAbility;
			}
			return overridenAttackAbility;
		}
	}

	public AttackTrigger originalAttackAbility
	{
		get
		{
			if (!abilities.ContainsKey(63))
			{
				return null;
			}
			return abilities[63] as AttackTrigger;
		}
	}

	public AbilityTrigger overridenAttackAbility
	{
		get
		{
			return Network_overridenAttackAbility;
		}
		private set
		{
			Network_overridenAttackAbility = value;
		}
	}

	public IReadOnlyDictionary<int, AbilityTrigger> abilities => _abilities;

	bool ICleanup.canDestroy => true;

	public AttackTrigger Network_originalAttackAbility
	{
		get
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return ((NetworkBehaviour)this).GetSyncVarNetworkBehaviour<AttackTrigger>(____originalAttackAbilityNetId, ref _originalAttackAbility);
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter_NetworkBehaviour<AttackTrigger>(value, ref _originalAttackAbility, 1uL, (Action<AttackTrigger, AttackTrigger>)null, ref ____originalAttackAbilityNetId);
		}
	}

	public AbilityTrigger Network_overridenAttackAbility
	{
		get
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return ((NetworkBehaviour)this).GetSyncVarNetworkBehaviour<AbilityTrigger>(____overridenAttackAbilityNetId, ref _overridenAttackAbility);
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter_NetworkBehaviour<AbilityTrigger>(value, ref _overridenAttackAbility, 2uL, (Action<AbilityTrigger, AbilityTrigger>)null, ref ____overridenAttackAbilityNetId);
		}
	}

	public ulong Network_abilityCastLockBitmap
	{
		get
		{
			return _abilityCastLockBitmap;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<ulong>(value, ref _abilityCastLockBitmap, 4uL, (Action<ulong, ulong>)null);
		}
	}

	public ulong Network_abilityEditLockBitmap
	{
		get
		{
			return _abilityEditLockBitmap;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<ulong>(value, ref _abilityEditLockBitmap, 8uL, (Action<ulong, ulong>)null);
		}
	}

	public ulong Network_showLockIconBitmap
	{
		get
		{
			return _showLockIconBitmap;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<ulong>(value, ref _showLockIconBitmap, 16uL, (Action<ulong, ulong>)null);
		}
	}

	public override void ClearPooledEventsAndProcessors()
	{
		base.ClearPooledEventsAndProcessors();
		ClientEvent_OnAbilityAdded?.Clear();
		ClientEvent_OnAbilityRemoved?.Clear();
	}

	protected override void Awake()
	{
		base.Awake();
		_abilities = new NetworkBehaviourDictionaryWrapper<int, AbilityTrigger>((IDictionary<int, SyncedNetworkBehaviour>)_synedAbilities);
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		if ((UnityEngine.Object)(object)attackAbilityPreset.asset != null)
		{
			SetAttackAbility(Dew.CreateAbilityTrigger(attackAbilityPreset.asset));
		}
		if (abilityPreset == null)
		{
			return;
		}
		for (int i = 0; i < abilityPreset.Length; i++)
		{
			AbilityTrigger asset = abilityPreset[i].asset;
			if (!((UnityEngine.Object)(object)asset == null))
			{
				AbilityTrigger trigger = ((!(asset is SkillTrigger trigger2)) ? Dew.CreateAbilityTrigger(asset) : Dew.CreateSkillTrigger(trigger2, ((Component)(object)this).transform.position, 1));
				SetAbility(i, trigger);
			}
		}
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		((SyncIDictionary<int, SyncedNetworkBehaviour>)(object)_synedAbilities).Callback += OnAbilityChanged;
		foreach (KeyValuePair<int, SyncedNetworkBehaviour> synedAbility in _synedAbilities)
		{
			OnAbilityChanged((Operation<int, SyncedNetworkBehaviour>)0, synedAbility.Key, synedAbility.Value);
		}
	}

	private void OnAbilityChanged(Operation<int, SyncedNetworkBehaviour> op, int key, SyncedNetworkBehaviour item)
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Invalid comparison between Unknown and I4
		HeroSkillLocation b;
		switch (key)
		{
		default:
			return;
		case 0:
			b = HeroSkillLocation.Q;
			break;
		case 1:
			b = HeroSkillLocation.W;
			break;
		case 2:
			b = HeroSkillLocation.E;
			break;
		case 3:
			b = HeroSkillLocation.R;
			break;
		case 4:
			b = HeroSkillLocation.Identity;
			break;
		case 5:
			b = HeroSkillLocation.Movement;
			break;
		}
		if ((UnityEngine.Object)(object)DewPlayer.local != null && (UnityEngine.Object)(object)entity == (UnityEngine.Object)(object)DewPlayer.local.hero)
		{
			NetworkedManagerBase<ClientEventManager>.instance.OnLocalHeroAbilityChanged?.Invoke((Hero)entity, b);
		}
		if ((int)op == 0)
		{
			ClientEvent_OnAbilityAdded?.Invoke(key, (AbilityTrigger)(object)(NetworkBehaviour)item);
		}
		if ((int)op == 2)
		{
			ClientEvent_OnAbilityRemoved?.Invoke(key, (AbilityTrigger)(object)(NetworkBehaviour)item);
		}
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (!entity.isSleeping && ((NetworkBehaviour)this).isServer)
		{
			if (_attackAbilityOverrides.Count == 0 && (UnityEngine.Object)(object)overridenAttackAbility != null)
			{
				overridenAttackAbility = null;
			}
			else if (_attackAbilityOverrides.Count > 0 && (UnityEngine.Object)(object)overridenAttackAbility != (UnityEngine.Object)(object)_attackAbilityOverrides[_attackAbilityOverrides.Count - 1])
			{
				overridenAttackAbility = _attackAbilityOverrides[_attackAbilityOverrides.Count - 1];
			}
		}
	}

	[Server]
	public void AddAbility(AbilityTrigger trigger)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityAbility::AddAbility(AbilityTrigger)' called when server was not active");
			return;
		}
		if ((UnityEngine.Object)(object)trigger == null)
		{
			throw new ArgumentNullException("trigger");
		}
		for (int i = 0; i < 128; i++)
		{
			if (!_abilities.ContainsKey(i))
			{
				SetAbility(i, trigger);
				return;
			}
		}
		throw new InvalidOperationException("Abilities full: " + trigger.GetActorReadableName() + " -> " + entity.GetActorReadableName());
	}

	[Server]
	public T AddAbility<T>() where T : AbilityTrigger
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'T EntityAbility::AddAbility()' called when server was not active");
			return null;
		}
		T val = Dew.CreateAbilityTrigger<T>();
		if ((UnityEngine.Object)(object)val == null)
		{
			throw new ArgumentNullException("trigger");
		}
		for (int i = 0; i < 128; i++)
		{
			if (!_abilities.ContainsKey(i))
			{
				SetAbility(i, val);
				return val;
			}
		}
		throw new InvalidOperationException("Abilities full: " + val.GetActorReadableName() + " -> " + entity.GetActorReadableName());
	}

	[Server]
	public void SetAbility(int index, AbilityTrigger trigger)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityAbility::SetAbility(System.Int32,AbilityTrigger)' called when server was not active");
			return;
		}
		if ((UnityEngine.Object)(object)trigger == null)
		{
			throw new ArgumentNullException("trigger");
		}
		if (_abilities.ContainsKey(index))
		{
			RemoveAbility(index);
		}
		_abilities.Add(index, trigger);
		trigger.owner = entity;
		trigger.parentActor = entity;
		trigger.abilityIndex = index;
		trigger.ClientActorEvent_OnDestroyed += new Action<Actor>(OnAbilityTriggerDestroyed);
	}

	[Server]
	public AbilityTrigger RemoveAbility(int index)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'AbilityTrigger EntityAbility::RemoveAbility(System.Int32)' called when server was not active");
			return null;
		}
		if (!_abilities.TryGetValue(index, out var value))
		{
			throw new Exception($"This entity({this}) has no ability at the given index {index}");
		}
		value.owner = null;
		value.ClientActorEvent_OnDestroyed -= _cachedOnAbilityTriggerDestroyed;
		if (value.isActive)
		{
			value.parentActor = null;
		}
		_abilities.Remove(index);
		return value;
	}

	private void OnAbilityTriggerDestroyed(Actor obj)
	{
		if (!entity.IsNullOrInactive() && obj is AbilityTrigger abilityTrigger && (UnityEngine.Object)(object)abilityTrigger.owner == (UnityEngine.Object)(object)entity && abilities.ContainsKey(abilityTrigger.abilityIndex) && (UnityEngine.Object)(object)abilities[abilityTrigger.abilityIndex] == (UnityEngine.Object)(object)abilityTrigger)
		{
			RemoveAbility(abilityTrigger.abilityIndex);
		}
	}

	public T GetAbility<T>() where T : AbilityTrigger
	{
		foreach (KeyValuePair<int, SyncedNetworkBehaviour> synedAbility in _synedAbilities)
		{
			if ((NetworkBehaviour)synedAbility.Value is T result)
			{
				return result;
			}
		}
		return null;
	}

	public bool TryGetAbility<T>(out T trigger) where T : AbilityTrigger
	{
		foreach (KeyValuePair<int, SyncedNetworkBehaviour> synedAbility in _synedAbilities)
		{
			if ((NetworkBehaviour)synedAbility.Value is T val)
			{
				trigger = val;
				return true;
			}
		}
		trigger = null;
		return false;
	}

	[Server]
	public void SetAttackAbility(AttackTrigger trigger)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityAbility::SetAttackAbility(AttackTrigger)' called when server was not active");
		}
		else
		{
			this.SetAttackAbility<AttackTrigger>(trigger);
		}
	}

	[Server]
	public void SetAttackAbility<T>(T trigger) where T : AttackTrigger
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityAbility::SetAttackAbility(T)' called when server was not active");
		}
		else
		{
			SetAbility(63, trigger);
		}
	}

	[Server]
	public AbilityTrigger RemoveAttackAbility()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'AbilityTrigger EntityAbility::RemoveAttackAbility()' called when server was not active");
			return null;
		}
		return RemoveAbility(63);
	}

	void ICleanup.OnCleanup()
	{
		foreach (AbilityTrigger value in _abilities.Values)
		{
			Dew.Destroy(((Component)(object)value).gameObject);
		}
	}

	public bool IsAbilityCastLocked(int index)
	{
		if (index < 0)
		{
			return false;
		}
		if (index > 64)
		{
			throw new InvalidOperationException("Ability index out of bounds");
		}
		if ((_abilityCastLockBitmap & (ulong)(1L << index)) != 0L)
		{
			if (abilities.ContainsKey(index))
			{
				return !abilities[index].currentConfig.ignoreAbilityLock;
			}
			return true;
		}
		return false;
	}

	public bool IsAbilityEditLocked(int index)
	{
		if (index < 0)
		{
			return false;
		}
		if (index > 64)
		{
			throw new InvalidOperationException("Ability index out of bounds");
		}
		return (_abilityEditLockBitmap & (ulong)(1L << index)) != 0;
	}

	public bool ShouldShowAbilityLockIcon(int index)
	{
		if (index < 0)
		{
			return false;
		}
		if (index > 64)
		{
			throw new InvalidOperationException("Ability index out of bounds");
		}
		return (_showLockIconBitmap & (ulong)(1L << index)) != 0;
	}

	public bool IsAttackAbilityLocked(int index)
	{
		return IsAbilityCastLocked(63);
	}

	[Server]
	public AbilityLockHandle GetNewAbilityLockHandle(bool shouldShowLockIcon = false)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'AbilityLockHandle EntityAbility::GetNewAbilityLockHandle(System.Boolean)' called when server was not active");
			return null;
		}
		AbilityLockHandle abilityLockHandle = new AbilityLockHandle();
		_handles.Add(abilityLockHandle);
		abilityLockHandle._parent = this;
		abilityLockHandle.shouldShowLockEffect = shouldShowLockIcon;
		return abilityLockHandle;
	}

	[Server]
	internal void UnlockAbility(AbilityLockHandle handle)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityAbility::UnlockAbility(AbilityLockHandle)' called when server was not active");
			return;
		}
		_handles.Remove(handle);
		CalculateAbilityLockBitmap();
	}

	[Server]
	internal void CalculateAbilityLockBitmap()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityAbility::CalculateAbilityLockBitmap()' called when server was not active");
			return;
		}
		Network_abilityCastLockBitmap = 0uL;
		Network_abilityEditLockBitmap = 0uL;
		Network_showLockIconBitmap = 0uL;
		foreach (AbilityLockHandle handle in _handles)
		{
			Network_abilityCastLockBitmap = _abilityCastLockBitmap | handle._castLockBitmap;
			Network_abilityEditLockBitmap = _abilityEditLockBitmap | handle._editLockBitmap;
			if (handle.shouldShowLockEffect)
			{
				Network_showLockIconBitmap = _showLockIconBitmap | handle._castLockBitmap;
			}
		}
	}

	public EntityAbility()
	{
		((NetworkBehaviour)this).InitSyncObject((SyncObject)(object)_synedAbilities);
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		((NetworkBehaviour)this).SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_originalAttackAbility);
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_overridenAttackAbility);
			NetworkWriterExtensions.WriteULong(writer, _abilityCastLockBitmap);
			NetworkWriterExtensions.WriteULong(writer, _abilityEditLockBitmap);
			NetworkWriterExtensions.WriteULong(writer, _showLockIconBitmap);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 1L) != 0L)
		{
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_originalAttackAbility);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 2L) != 0L)
		{
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_overridenAttackAbility);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 4L) != 0L)
		{
			NetworkWriterExtensions.WriteULong(writer, _abilityCastLockBitmap);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 8L) != 0L)
		{
			NetworkWriterExtensions.WriteULong(writer, _abilityEditLockBitmap);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x10L) != 0L)
		{
			NetworkWriterExtensions.WriteULong(writer, _showLockIconBitmap);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		((NetworkBehaviour)this).DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<AttackTrigger>(ref _originalAttackAbility, (Action<AttackTrigger, AttackTrigger>)null, reader, ref ____originalAttackAbilityNetId);
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<AbilityTrigger>(ref _overridenAttackAbility, (Action<AbilityTrigger, AbilityTrigger>)null, reader, ref ____overridenAttackAbilityNetId);
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<ulong>(ref _abilityCastLockBitmap, (Action<ulong, ulong>)null, NetworkReaderExtensions.ReadULong(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<ulong>(ref _abilityEditLockBitmap, (Action<ulong, ulong>)null, NetworkReaderExtensions.ReadULong(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<ulong>(ref _showLockIconBitmap, (Action<ulong, ulong>)null, NetworkReaderExtensions.ReadULong(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 1L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<AttackTrigger>(ref _originalAttackAbility, (Action<AttackTrigger, AttackTrigger>)null, reader, ref ____originalAttackAbilityNetId);
		}
		if ((num & 2L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<AbilityTrigger>(ref _overridenAttackAbility, (Action<AbilityTrigger, AbilityTrigger>)null, reader, ref ____overridenAttackAbilityNetId);
		}
		if ((num & 4L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<ulong>(ref _abilityCastLockBitmap, (Action<ulong, ulong>)null, NetworkReaderExtensions.ReadULong(reader));
		}
		if ((num & 8L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<ulong>(ref _abilityEditLockBitmap, (Action<ulong, ulong>)null, NetworkReaderExtensions.ReadULong(reader));
		}
		if ((num & 0x10L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<ulong>(ref _showLockIconBitmap, (Action<ulong, ulong>)null, NetworkReaderExtensions.ReadULong(reader));
		}
	}
}
