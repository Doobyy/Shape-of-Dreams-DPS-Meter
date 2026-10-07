using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class HeroSkill : HeroComponent, ICleanup
{
	private const float UnequipMaxDistance = 1.5f;

	public SafeAction<SkillTrigger> ClientHeroEvent_OnSkillEquip;

	public SafeAction<SkillTrigger> ClientHeroEvent_OnSkillUnequip;

	public SafeAction<SkillTrigger> ClientHeroEvent_OnSkillPickup;

	public SafeAction<SkillTrigger> ClientHeroEvent_OnSkillDrop;

	public SafeAction<HeroSkillLocation, HeroSkillLocation> ClientHeroEvent_OnSkillSwap;

	public SafeAction<SkillTrigger, int, int> ClientHeroEvent_OnSkillLevelChanged;

	public AssetRef<SkillTrigger>[] loadoutQ;

	public AssetRef<SkillTrigger>[] loadoutR;

	public AssetRef<SkillTrigger>[] loadoutTrait;

	public AssetRef<SkillTrigger>[] loadoutMovement;

	[CompilerGenerated]
	[SyncVar(hook = "OnHoldingObjectChanged")]
	private IItem holdingObject__BackingField;

	[NonSerialized]
	[SyncVar]
	[SaveVar(SaveVarFlags.Default)]
	public float starNormalizedStrength = -1f;

	private NetworkBehaviourDictionaryWrapper<GemLocation, Gem> _gems;

	private readonly SyncDictionary<GemLocation, SyncedNetworkBehaviour> _syncedGems = new SyncDictionary<GemLocation, SyncedNetworkBehaviour>();

	public SafeAction<Gem, int, int> ClientHeroEvent_OnGemQualityChanged;

	public SafeAction<Gem> ClientHeroEvent_OnGemEquip;

	public SafeAction<Gem> ClientHeroEvent_OnGemUnequip;

	public SafeAction<Gem> ClientHeroEvent_OnGemPickup;

	public SafeAction<Gem> ClientHeroEvent_OnGemDrop;

	public SafeAction<GemLocation, GemLocation> ClientHeroEvent_OnGemSwap;

	public SafeAction<Gem> ClientHeroEvent_OnOnlyGemNotificationRequested;

	[CompilerGenerated]
	[SyncVar]
	private int maxGemCountQ__BackingField = 3;

	[CompilerGenerated]
	[SyncVar]
	private int maxGemCountW__BackingField = 3;

	[CompilerGenerated]
	[SyncVar]
	private int maxGemCountE__BackingField = 3;

	[CompilerGenerated]
	[SyncVar]
	private int maxGemCountR__BackingField = 3;

	[CompilerGenerated]
	[SyncVar]
	private int maxGemCountIdentity__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private int maxGemCountMovement__BackingField;

	public Action<IItem, IItem> _Mirror_SyncVarHookDelegate__003CholdingObject_003Ek__BackingField;

	public SkillTrigger Q
	{
		get
		{
			if (!hero.Ability.abilities.TryGetValue(0, out var value))
			{
				return null;
			}
			return value as SkillTrigger;
		}
		set
		{
			hero.Ability.SetAbility(0, value);
		}
	}

	public SkillTrigger W
	{
		get
		{
			if (!hero.Ability.abilities.TryGetValue(1, out var value))
			{
				return null;
			}
			return value as SkillTrigger;
		}
		set
		{
			hero.Ability.SetAbility(1, value);
		}
	}

	public SkillTrigger E
	{
		get
		{
			if (!hero.Ability.abilities.TryGetValue(2, out var value))
			{
				return null;
			}
			return value as SkillTrigger;
		}
		set
		{
			hero.Ability.SetAbility(2, value);
		}
	}

	public SkillTrigger R
	{
		get
		{
			if (!hero.Ability.abilities.TryGetValue(3, out var value))
			{
				return null;
			}
			return value as SkillTrigger;
		}
		set
		{
			hero.Ability.SetAbility(3, value);
		}
	}

	public SkillTrigger Identity
	{
		get
		{
			if (!hero.Ability.abilities.TryGetValue(4, out var value))
			{
				return null;
			}
			return value as SkillTrigger;
		}
		set
		{
			hero.Ability.SetAbility(4, value);
		}
	}

	public SkillTrigger Movement
	{
		get
		{
			if (!hero.Ability.abilities.TryGetValue(5, out var value))
			{
				return null;
			}
			return value as SkillTrigger;
		}
		set
		{
			hero.Ability.SetAbility(5, value);
		}
	}

	public IItem holdingObject
	{
		[CompilerGenerated]
		get
		{
			return holdingObject__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CholdingObject_003Ek__BackingField = value;
		}
	}

	bool ICleanup.canDestroy => true;

	public IReadOnlyDictionary<GemLocation, Gem> gems => _gems;

	public int maxGemCountQ
	{
		[CompilerGenerated]
		get
		{
			return maxGemCountQ__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CmaxGemCountQ_003Ek__BackingField = value;
		}
	}

	public int maxGemCountW
	{
		[CompilerGenerated]
		get
		{
			return maxGemCountW__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CmaxGemCountW_003Ek__BackingField = value;
		}
	}

	public int maxGemCountE
	{
		[CompilerGenerated]
		get
		{
			return maxGemCountE__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CmaxGemCountE_003Ek__BackingField = value;
		}
	}

	public int maxGemCountR
	{
		[CompilerGenerated]
		get
		{
			return maxGemCountR__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CmaxGemCountR_003Ek__BackingField = value;
		}
	}

	public int maxGemCountIdentity
	{
		[CompilerGenerated]
		get
		{
			return maxGemCountIdentity__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CmaxGemCountIdentity_003Ek__BackingField = value;
		}
	}

	public int maxGemCountMovement
	{
		[CompilerGenerated]
		get
		{
			return maxGemCountMovement__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CmaxGemCountMovement_003Ek__BackingField = value;
		}
	}

	public IItem Network_003CholdingObject_003Ek__BackingField
	{
		get
		{
			return holdingObject__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<IItem>(value, ref holdingObject__BackingField, 1uL, _Mirror_SyncVarHookDelegate__003CholdingObject_003Ek__BackingField);
		}
	}

	public float NetworkstarNormalizedStrength
	{
		get
		{
			return starNormalizedStrength;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref starNormalizedStrength, 2uL, (Action<float, float>)null);
		}
	}

	public int Network_003CmaxGemCountQ_003Ek__BackingField
	{
		get
		{
			return maxGemCountQ__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref maxGemCountQ__BackingField, 4uL, (Action<int, int>)null);
		}
	}

	public int Network_003CmaxGemCountW_003Ek__BackingField
	{
		get
		{
			return maxGemCountW__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref maxGemCountW__BackingField, 8uL, (Action<int, int>)null);
		}
	}

	public int Network_003CmaxGemCountE_003Ek__BackingField
	{
		get
		{
			return maxGemCountE__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref maxGemCountE__BackingField, 16uL, (Action<int, int>)null);
		}
	}

	public int Network_003CmaxGemCountR_003Ek__BackingField
	{
		get
		{
			return maxGemCountR__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref maxGemCountR__BackingField, 32uL, (Action<int, int>)null);
		}
	}

	public int Network_003CmaxGemCountIdentity_003Ek__BackingField
	{
		get
		{
			return maxGemCountIdentity__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref maxGemCountIdentity__BackingField, 64uL, (Action<int, int>)null);
		}
	}

	public int Network_003CmaxGemCountMovement_003Ek__BackingField
	{
		get
		{
			return maxGemCountMovement__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref maxGemCountMovement__BackingField, 128uL, (Action<int, int>)null);
		}
	}

	public override void ClearPooledEventsAndProcessors()
	{
		base.ClearPooledEventsAndProcessors();
		ClientHeroEvent_OnSkillEquip?.Clear();
		ClientHeroEvent_OnSkillUnequip?.Clear();
		ClientHeroEvent_OnSkillPickup?.Clear();
		ClientHeroEvent_OnSkillDrop?.Clear();
		ClientHeroEvent_OnSkillSwap?.Clear();
		ClientHeroEvent_OnSkillLevelChanged?.Clear();
		ClientHeroEvent_OnGemQualityChanged?.Clear();
		ClientHeroEvent_OnGemEquip?.Clear();
		ClientHeroEvent_OnGemUnequip?.Clear();
		ClientHeroEvent_OnGemPickup?.Clear();
		ClientHeroEvent_OnGemDrop?.Clear();
		ClientHeroEvent_OnGemSwap?.Clear();
		ClientHeroEvent_OnOnlyGemNotificationRequested?.Clear();
	}

	protected override void Awake()
	{
		base.Awake();
		_gems = new NetworkBehaviourDictionaryWrapper<GemLocation, Gem>((IDictionary<GemLocation, SyncedNetworkBehaviour>)_syncedGems);
	}

	public SkillTrigger[] GetLoadoutSkills(HeroSkillLocation type, DewGameContentSettings content = null)
	{
		if (content == null)
		{
			content = DewBuildProfile.current.content;
		}
		return type switch
		{
			HeroSkillLocation.Q => loadoutQ.Where((AssetRef<SkillTrigger> asset) => Dew.IsSkillIncludedInGame(asset.typeName, content)).Select((Func<AssetRef<SkillTrigger>, SkillTrigger>)((AssetRef<SkillTrigger> s) => s)).ToArray(), 
			HeroSkillLocation.R => loadoutR.Where((AssetRef<SkillTrigger> asset) => Dew.IsSkillIncludedInGame(asset.typeName, content)).Select((Func<AssetRef<SkillTrigger>, SkillTrigger>)((AssetRef<SkillTrigger> s) => s)).ToArray(), 
			HeroSkillLocation.Identity => loadoutTrait.Where((AssetRef<SkillTrigger> asset) => Dew.IsSkillIncludedInGame(asset.typeName, content)).Select((Func<AssetRef<SkillTrigger>, SkillTrigger>)((AssetRef<SkillTrigger> s) => s)).ToArray(), 
			HeroSkillLocation.Movement => loadoutMovement.Where((AssetRef<SkillTrigger> asset) => Dew.IsSkillIncludedInGame(asset.typeName, content)).Select((Func<AssetRef<SkillTrigger>, SkillTrigger>)((AssetRef<SkillTrigger> s) => s)).ToArray(), 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}

	public override void OnLateStartServer()
	{
		base.OnLateStartServer();
		HandleLoadout(HeroSkillLocation.Q);
		HandleLoadout(HeroSkillLocation.R);
		HandleLoadout(HeroSkillLocation.Identity);
		HandleLoadout(HeroSkillLocation.Movement);
		if (DewBuildProfile.current.buildType != BuildType.DemoLite)
		{
			HandleConstellations();
		}
		void HandleLoadout(HeroSkillLocation type)
		{
			int skill = hero.loadout.GetSkill(type);
			if (skill >= 0)
			{
				SkillTrigger[] loadoutSkills = GetLoadoutSkills(type);
				if (loadoutSkills != null && loadoutSkills.Length != 0)
				{
					int num = Mathf.Clamp(skill, 0, loadoutSkills.Length);
					SkillTrigger skillTrigger = Dew.CreateSkillTrigger(loadoutSkills[num], ((Component)(object)this).transform.position, 1);
					skillTrigger.skillType = type;
					hero.Ability.SetAbility((int)type, skillTrigger);
				}
			}
		}
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		((SyncIDictionary<GemLocation, SyncedNetworkBehaviour>)(object)_syncedGems).Callback += OnGemChanged;
		foreach (KeyValuePair<GemLocation, SyncedNetworkBehaviour> syncedGem in _syncedGems)
		{
			OnGemChanged((Operation<GemLocation, SyncedNetworkBehaviour>)0, syncedGem.Key, syncedGem.Value);
		}
	}

	public SkillTrigger GetSkill(HeroSkillLocation type)
	{
		return type switch
		{
			HeroSkillLocation.Q => Q, 
			HeroSkillLocation.W => W, 
			HeroSkillLocation.E => E, 
			HeroSkillLocation.R => R, 
			HeroSkillLocation.Identity => Identity, 
			HeroSkillLocation.Movement => Movement, 
			_ => null, 
		};
	}

	public bool TryGetSkill(HeroSkillLocation type, out SkillTrigger skill)
	{
		skill = null;
		switch (type)
		{
		case HeroSkillLocation.Q:
			skill = Q;
			break;
		case HeroSkillLocation.W:
			skill = W;
			break;
		case HeroSkillLocation.E:
			skill = E;
			break;
		case HeroSkillLocation.R:
			skill = R;
			break;
		case HeroSkillLocation.Identity:
			skill = Identity;
			break;
		case HeroSkillLocation.Movement:
			skill = Movement;
			break;
		}
		return (UnityEngine.Object)(object)skill != null;
	}

	[Server]
	public SkillTrigger UnequipSkill(HeroSkillLocation type, Vector3 position, bool ignoreCanReplace = false)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'SkillTrigger HeroSkill::UnequipSkill(HeroSkillLocation,UnityEngine.Vector3,System.Boolean)' called when server was not active");
			return null;
		}
		position = hero.position + Vector3.ClampMagnitude(position - hero.position, 1.5f);
		if (!ignoreCanReplace && !CanReplaceSkill(type))
		{
			return null;
		}
		if (!hero.Ability.abilities.ContainsKey((int)type))
		{
			return null;
		}
		foreach (Gem item in GetGemsInSkill(type))
		{
			item.skill = null;
			item.parentActor = hero;
		}
		SkillTrigger skillTrigger = (SkillTrigger)hero.Ability.RemoveAbility((int)type);
		skillTrigger.tempOwner = GetDroppedItemTempOwner();
		Vector3 validAgentDestination_LinearSweep = Dew.GetValidAgentDestination_LinearSweep(hero.agentPosition, position);
		validAgentDestination_LinearSweep = Dew.GetPositionOnGround(validAgentDestination_LinearSweep);
		skillTrigger.RpcSetPositionAndRotation(validAgentDestination_LinearSweep, ManagerBase<CameraManager>.instance.entityCamAngleRotation);
		RpcInvokeOnSkillUnequip(skillTrigger);
		return skillTrigger;
	}

	public void CmdUnequipSkill(HeroSkillLocation type, Vector3 position)
	{
		ManagerBase<EditSkillManager>.instance.SetClientState_SetSkillSlot(type, null);
		CmdUnequipSkill_Internal(type, position);
	}

	[Command]
	private void CmdUnequipSkill_Internal(HeroSkillLocation type, Vector3 position)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_HeroSkillLocation((NetworkWriter)(object)val, type);
		NetworkWriterExtensions.WriteVector3((NetworkWriter)(object)val, position);
		((NetworkBehaviour)this).SendCommandInternal("System.Void HeroSkill::CmdUnequipSkill_Internal(HeroSkillLocation,UnityEngine.Vector3)", -1886966275, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Server]
	public void EquipSkill(HeroSkillLocation type, SkillTrigger skill, bool ignoreCanReplace = false)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void HeroSkill::EquipSkill(HeroSkillLocation,SkillTrigger,System.Boolean)' called when server was not active");
		}
		else
		{
			if ((!ignoreCanReplace && !CanReplaceSkill(type)) || skill.IsNullOrInactive() || (UnityEngine.Object)(object)skill.owner != null || ((UnityEngine.Object)(object)skill.handOwner != null && (UnityEngine.Object)(object)skill.handOwner != (UnityEngine.Object)(object)hero) || (!ignoreCanReplace && skill.isCharacterSkill && !string.IsNullOrEmpty(skill.characterSkillOwner) && skill.characterSkillOwner != hero.owner.guid))
			{
				return;
			}
			if (holdingObject == skill)
			{
				Network_003CholdingObject_003Ek__BackingField = null;
			}
			if (hero.Ability.abilities.ContainsKey((int)type))
			{
				UnequipSkill(type, hero.position, ignoreCanReplace);
			}
			skill.handOwner = null;
			skill.skillType = type;
			hero.Ability.SetAbility((int)type, skill);
			foreach (Gem item in GetGemsInSkill(type))
			{
				item.skill = skill;
				item.parentActor = skill;
			}
			RpcInvokeOnSkillEquip(skill);
		}
	}

	public void CmdEquipSkill(HeroSkillLocation type, SkillTrigger skill)
	{
		ManagerBase<EditSkillManager>.instance.SetClientState_SetSkillSlot(type, skill);
		CmdEquipSkill_Internal(type, skill);
	}

	[Command]
	private void CmdEquipSkill_Internal(HeroSkillLocation type, SkillTrigger skill)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_HeroSkillLocation((NetworkWriter)(object)val, type);
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)skill);
		((NetworkBehaviour)this).SendCommandInternal("System.Void HeroSkill::CmdEquipSkill_Internal(HeroSkillLocation,SkillTrigger)", -101434386, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public void CmdSwapSlotSkill(HeroSkillLocation a, HeroSkillLocation b)
	{
		SkillTrigger skill = GetSkill(a);
		SkillTrigger skill2 = GetSkill(b);
		ManagerBase<EditSkillManager>.instance.SetClientState_SetSkillSlot(a, skill2);
		ManagerBase<EditSkillManager>.instance.SetClientState_SetSkillSlot(b, skill);
		CmdSwapSlotSkill_Internal(a, b);
	}

	[Command]
	private void CmdSwapSlotSkill_Internal(HeroSkillLocation a, HeroSkillLocation b)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_HeroSkillLocation((NetworkWriter)(object)val, a);
		GeneratedNetworkCode._Write_HeroSkillLocation((NetworkWriter)(object)val, b);
		((NetworkBehaviour)this).SendCommandInternal("System.Void HeroSkill::CmdSwapSlotSkill_Internal(HeroSkillLocation,HeroSkillLocation)", -36094078, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public bool TryGetSkillLocation(SkillTrigger skill, out HeroSkillLocation type)
	{
		if ((UnityEngine.Object)(object)GetSkill(HeroSkillLocation.Q) == (UnityEngine.Object)(object)skill)
		{
			type = HeroSkillLocation.Q;
			return true;
		}
		if ((UnityEngine.Object)(object)GetSkill(HeroSkillLocation.W) == (UnityEngine.Object)(object)skill)
		{
			type = HeroSkillLocation.W;
			return true;
		}
		if ((UnityEngine.Object)(object)GetSkill(HeroSkillLocation.E) == (UnityEngine.Object)(object)skill)
		{
			type = HeroSkillLocation.E;
			return true;
		}
		if ((UnityEngine.Object)(object)GetSkill(HeroSkillLocation.R) == (UnityEngine.Object)(object)skill)
		{
			type = HeroSkillLocation.R;
			return true;
		}
		if ((UnityEngine.Object)(object)GetSkill(HeroSkillLocation.Identity) == (UnityEngine.Object)(object)skill)
		{
			type = HeroSkillLocation.Identity;
			return true;
		}
		if ((UnityEngine.Object)(object)GetSkill(HeroSkillLocation.Movement) == (UnityEngine.Object)(object)skill)
		{
			type = HeroSkillLocation.Movement;
			return true;
		}
		type = HeroSkillLocation.Q;
		return false;
	}

	public void CmdMoveSkill(SkillTrigger skill, Vector3 position)
	{
		position = hero.position + Vector3.ClampMagnitude(position - hero.agentPosition, 1.5f);
		position = Dew.GetValidAgentDestination_LinearSweep(hero.agentPosition, position);
		position = Dew.GetPositionOnGround(position);
		skill.position = position;
		CmdMoveSkill_Internal(skill, position);
	}

	[Command]
	private void CmdMoveSkill_Internal(SkillTrigger skill, Vector3 position)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)skill);
		NetworkWriterExtensions.WriteVector3((NetworkWriter)(object)val, position);
		((NetworkBehaviour)this).SendCommandInternal("System.Void HeroSkill::CmdMoveSkill_Internal(SkillTrigger,UnityEngine.Vector3)", -1687722992, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public bool CanReplaceSkill(HeroSkillLocation type)
	{
		if (entity.Ability.IsAbilityEditLocked((int)type))
		{
			return false;
		}
		if (type != HeroSkillLocation.Identity)
		{
			return type != HeroSkillLocation.Movement;
		}
		return false;
	}

	[ClientRpc]
	private void RpcInvokeOnSkillEquip(SkillTrigger skill)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)skill);
		((NetworkBehaviour)this).SendRPCInternal("System.Void HeroSkill::RpcInvokeOnSkillEquip(SkillTrigger)", 438388514, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcInvokeOnSkillUnequip(SkillTrigger skill)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)skill);
		((NetworkBehaviour)this).SendRPCInternal("System.Void HeroSkill::RpcInvokeOnSkillUnequip(SkillTrigger)", 1228677609, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	internal void RpcInvokeOnSkillPickup(SkillTrigger skill)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)skill);
		((NetworkBehaviour)this).SendRPCInternal("System.Void HeroSkill::RpcInvokeOnSkillPickup(SkillTrigger)", -889243458, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	internal void RpcInvokeOnSkillDrop(SkillTrigger skill)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)skill);
		((NetworkBehaviour)this).SendRPCInternal("System.Void HeroSkill::RpcInvokeOnSkillDrop(SkillTrigger)", -352525839, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	internal void RpcInvokeOnSkillSwap(HeroSkillLocation a, HeroSkillLocation b)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_HeroSkillLocation((NetworkWriter)(object)val, a);
		GeneratedNetworkCode._Write_HeroSkillLocation((NetworkWriter)(object)val, b);
		((NetworkBehaviour)this).SendRPCInternal("System.Void HeroSkill::RpcInvokeOnSkillSwap(HeroSkillLocation,HeroSkillLocation)", 832442570, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Server]
	public void HoldInHand(IItem holdable)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void HeroSkill::HoldInHand(IItem)' called when server was not active");
		}
		else if (!((UnityEngine.Object)(object)holdable.handOwner != null) && !((UnityEngine.Object)(object)holdable.owner != null) && (!(holdable is SkillTrigger { isCharacterSkill: not false } skillTrigger) || string.IsNullOrEmpty(skillTrigger.characterSkillOwner) || !(skillTrigger.characterSkillOwner != hero.owner.guid)))
		{
			if (!holdingObject.IsHoldableObjectNullOrInactive())
			{
				StopHoldInHand();
			}
			Network_003CholdingObject_003Ek__BackingField = holdable;
			if (holdable is SkillTrigger skill)
			{
				RpcInvokeOnSkillPickup(skill);
			}
			if (holdable is Gem gem)
			{
				RpcInvokeOnGemPickup(gem);
			}
		}
	}

	public void CmdStopHoldInHand()
	{
		CmdStopHoldInHand_Internal();
	}

	[Command]
	private void CmdStopHoldInHand_Internal()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendCommandInternal("System.Void HeroSkill::CmdStopHoldInHand_Internal()", -792606367, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Server]
	public void StopHoldInHand()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void HeroSkill::StopHoldInHand()' called when server was not active");
			return;
		}
		if (holdingObject.IsHoldableObjectNullOrInactive())
		{
			Network_003CholdingObject_003Ek__BackingField = null;
			return;
		}
		if (((UnityEngine.Object)(object)holdingObject.owner != null && (UnityEngine.Object)(object)holdingObject.owner != (UnityEngine.Object)(object)hero) || (UnityEngine.Object)(object)holdingObject.handOwner != (UnityEngine.Object)(object)hero)
		{
			Network_003CholdingObject_003Ek__BackingField = null;
			return;
		}
		Vector3 validAgentDestination_LinearSweep = Dew.GetValidAgentDestination_LinearSweep(hero.agentPosition, hero.agentPosition + UnityEngine.Random.insideUnitSphere.Flattened() * 1.5f);
		validAgentDestination_LinearSweep = Dew.GetPositionOnGround(validAgentDestination_LinearSweep);
		if (holdingObject is SkillTrigger skillTrigger)
		{
			if ((UnityEngine.Object)(object)skillTrigger.owner == null)
			{
				skillTrigger.RpcSetPositionAndRotation(validAgentDestination_LinearSweep, ManagerBase<CameraManager>.instance.entityCamAngleRotation);
			}
			skillTrigger.handOwner = null;
		}
		else if (holdingObject is Gem gem)
		{
			if ((UnityEngine.Object)(object)gem.owner == null)
			{
				gem.RpcSetPositionAndRotation(validAgentDestination_LinearSweep, ManagerBase<CameraManager>.instance.entityCamAngleRotation);
			}
			gem.handOwner = null;
		}
		if (holdingObject is SkillTrigger skill)
		{
			RpcInvokeOnSkillDrop(skill);
		}
		if (holdingObject is Gem gem2)
		{
			RpcInvokeOnGemDrop(gem2);
		}
		Network_003CholdingObject_003Ek__BackingField = null;
	}

	public DewPlayer GetDroppedItemTempOwner()
	{
		if (!((UnityEngine.Object)(object)hero.owner != null) || !hero.owner.shareItemsWhenDropped)
		{
			return hero.owner;
		}
		return null;
	}

	private void OnHoldingObjectChanged(IItem oldObject, IItem newObject)
	{
		if (((NetworkBehaviour)this).isServer)
		{
			if (oldObject != null)
			{
				oldObject.handOwner = null;
				oldObject.tempOwner = GetDroppedItemTempOwner();
			}
			if (newObject != null)
			{
				newObject.handOwner = hero;
			}
		}
		if (((NetworkBehaviour)this).isOwned)
		{
			if (newObject is SkillTrigger skill)
			{
				ManagerBase<EditSkillManager>.instance.StartEquipSkill(skill);
			}
			if (newObject is Gem gem)
			{
				ManagerBase<EditSkillManager>.instance.StartEquipGem(gem);
			}
		}
	}

	void ICleanup.OnCleanup()
	{
		foreach (Gem item in new List<Gem>(gems.Values))
		{
			Dew.Destroy(((Component)(object)item).gameObject);
		}
	}

	private void HandleConstellations()
	{
		float maxStrength;
		float currStrength;
		if (DewBuildProfile.current.buildType != BuildType.DemoLite && hero.loadout != null)
		{
			maxStrength = 0f;
			currStrength = 0f;
			HandleStarType(StarType.Destruction);
			HandleStarType(StarType.Life);
			HandleStarType(StarType.Imagination);
			HandleStarType(StarType.Flexible);
			if (starNormalizedStrength < 0f)
			{
				NetworkstarNormalizedStrength = currStrength / maxStrength;
			}
		}
		void HandleStarType(StarType type)
		{
			maxStrength += hero.GetConstellationSettings(type).maxCount;
			foreach (LoadoutStarItem s in hero.loadout.GetStarList(type))
			{
				if (!string.IsNullOrEmpty(s.name) && s.level != 0)
				{
					StarEffect byShortTypeName = DewResources.GetByShortTypeName<StarEffect>(s.name, default(ResourceLoadSettings));
					if (!((UnityEngine.Object)(object)byShortTypeName == null))
					{
						float strength;
						if (type == StarType.Flexible || byShortTypeName.type == StarType.Flexible || type == byShortTypeName.type)
						{
							strength = 1f;
						}
						else
						{
							strength = 0.5f;
						}
						currStrength += strength * Mathf.Clamp01(0.4f + (float)s.level / (float)byShortTypeName.maxStarLevel * 0.6f);
						hero.CreateStatusEffect(byShortTypeName, hero, new CastInfo(hero), (StarEffect se) =>
						{
							se.skillLevel = s.level;
							se.strength = strength;
						});
					}
				}
			}
		}
	}

	private void OnGemChanged(Operation<GemLocation, SyncedNetworkBehaviour> op, GemLocation key, SyncedNetworkBehaviour item)
	{
		if (!((UnityEngine.Object)(object)DewPlayer.local == null) && !((UnityEngine.Object)(object)hero != (UnityEngine.Object)(object)DewPlayer.local.hero))
		{
			NetworkedManagerBase<ClientEventManager>.instance.OnLocalHeroGemChanged?.Invoke(hero, key);
		}
	}

	[Server]
	public void EquipGem(GemLocation loc, Gem gem)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void HeroSkill::EquipGem(GemLocation,Gem)' called when server was not active");
			return;
		}
		if ((UnityEngine.Object)(object)gem == null)
		{
			throw new ArgumentNullException("gem");
		}
		if (TryGetEquippedGemOfSameType(((object)gem).GetType(), out var _, out var _))
		{
			throw new InvalidOperationException("Tried to equip more than one of same type of gem");
		}
		if ((UnityEngine.Object)(object)gem.owner != null || (UnityEngine.Object)(object)gem.skill != null || ((UnityEngine.Object)(object)gem.handOwner != null && (UnityEngine.Object)(object)gem.handOwner != (UnityEngine.Object)(object)hero))
		{
			throw new InvalidOperationException("Gem already belongs to someone else");
		}
		if (gems.ContainsKey(loc))
		{
			throw new InvalidOperationException("Another gem already equipped to specified slot");
		}
		if (holdingObject == gem)
		{
			Network_003CholdingObject_003Ek__BackingField = null;
		}
		gem.owner = hero;
		gem.location = loc;
		if (TryGetSkill(loc.skill, out var skill))
		{
			gem.skill = skill;
			gem.parentActor = skill;
		}
		else
		{
			gem.parentActor = hero;
		}
		_gems.Add(loc, gem);
		gem.ClientActorEvent_OnDestroyed += new Action<Actor>(OnGemDestroyed);
		gem.RpcSetPositionAndRotation(hero.position, Quaternion.identity);
		RpcInvokeOnGemEquip(gem);
	}

	private void OnGemDestroyed(Actor obj)
	{
		if (!entity.IsNullOrInactive() && obj is Gem gem && (UnityEngine.Object)(object)gem.owner == (UnityEngine.Object)(object)entity && gems.ContainsKey(gem.location) && (UnityEngine.Object)(object)gems[gem.location] == (UnityEngine.Object)(object)gem)
		{
			UnequipGem(gem.location, entity.agentPosition);
		}
	}

	public void CmdEquipGem(GemLocation loc, Gem gem)
	{
		if (!ManagerBase<ControlManager>.instance.gemLocationConstraint.HasValue || ManagerBase<ControlManager>.instance.gemLocationConstraint.Value == loc.skill)
		{
			ManagerBase<EditSkillManager>.instance.SetClientState_SetGemSlot(loc, gem);
			CmdEquipGem_Internal(loc, gem);
		}
	}

	[Command]
	private void CmdEquipGem_Internal(GemLocation loc, Gem gem)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_GemLocation((NetworkWriter)(object)val, loc);
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)gem);
		((NetworkBehaviour)this).SendCommandInternal("System.Void HeroSkill::CmdEquipGem_Internal(GemLocation,Gem)", -1094464876, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public Gem UnequipGem(GemLocation loc, Vector3 position)
	{
		position = hero.agentPosition + Vector3.ClampMagnitude(position - hero.agentPosition, 1.5f);
		Gem gem = gems[loc];
		_gems.Remove(loc);
		gem.skill = null;
		gem.owner = null;
		gem.tempOwner = GetDroppedItemTempOwner();
		gem.parentActor = null;
		gem.ClientActorEvent_OnDestroyed -= new Action<Actor>(OnGemDestroyed);
		Vector3 validAgentDestination_LinearSweep = Dew.GetValidAgentDestination_LinearSweep(hero.agentPosition, position);
		validAgentDestination_LinearSweep = Dew.GetPositionOnGround(validAgentDestination_LinearSweep);
		gem.RpcSetPositionAndRotation(validAgentDestination_LinearSweep, ManagerBase<CameraManager>.instance.entityCamAngleRotation);
		RpcInvokeOnGemUnequip(gem);
		return gem;
	}

	[Server]
	public void DropItemsOnOwnerDisconnect()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void HeroSkill::DropItemsOnOwnerDisconnect()' called when server was not active");
			return;
		}
		if (!holdingObject.IsHoldableObjectNullOrInactive())
		{
			IItem item = holdingObject;
			StopHoldInHand();
			if ((UnityEngine.Object)(object)item.owner == null && (UnityEngine.Object)(object)item.handOwner == null)
			{
				item.tempOwner = null;
			}
		}
		List<GemLocation> list = null;
		foreach (KeyValuePair<GemLocation, Gem> gem2 in gems)
		{
			if (gem2.Value.isDroppedOnOwnerDisconnect)
			{
				if (list == null)
				{
					list = new List<GemLocation>();
				}
				list.Add(gem2.Key);
			}
		}
		if (list == null)
		{
			return;
		}
		foreach (GemLocation item2 in list)
		{
			Gem gem = UnequipGem(item2, hero.agentPosition);
			gem.tempOwner = null;
			RpcInvokeOnGemDrop(gem);
		}
	}

	public bool TryGetGemLocation(Gem gem, out GemLocation location)
	{
		foreach (KeyValuePair<GemLocation, Gem> gem2 in gems)
		{
			if ((UnityEngine.Object)(object)gem2.Value == (UnityEngine.Object)(object)gem)
			{
				location = gem2.Key;
				return true;
			}
		}
		location = default;
		return false;
	}

	[Server]
	public void MergeGem(Gem victim, Gem receivingGem)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void HeroSkill::MergeGem(Gem,Gem)' called when server was not active");
			return;
		}
		if ((UnityEngine.Object)(object)victim == null)
		{
			throw new ArgumentNullException("victim");
		}
		if ((UnityEngine.Object)(object)receivingGem == null)
		{
			throw new ArgumentNullException("receivingGem");
		}
		if ((UnityEngine.Object)(object)victim.owner != null)
		{
			throw new InvalidOperationException("Victim gem is equipped");
		}
		if ((UnityEngine.Object)(object)receivingGem.owner != (UnityEngine.Object)(object)hero)
		{
			throw new InvalidOperationException("Receiving gem is not owned");
		}
		if (((object)victim).GetType() != ((object)receivingGem).GetType())
		{
			throw new InvalidOperationException("Merge gem type is different");
		}
		receivingGem.quality = Gem.GetMergedQuality(victim.quality, receivingGem.quality);
		victim.Destroy();
		NetworkedManagerBase<ClientEventManager>.instance.InvokeOnGemMergeUpgraded(hero, receivingGem);
	}

	public void CmdUnequipGem(GemLocation loc, Vector3 position)
	{
		ManagerBase<EditSkillManager>.instance.SetClientState_SetGemSlot(loc, null);
		CmdUnequipGem_Internal(loc, position);
	}

	[Command]
	private void CmdUnequipGem_Internal(GemLocation loc, Vector3 position)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_GemLocation((NetworkWriter)(object)val, loc);
		NetworkWriterExtensions.WriteVector3((NetworkWriter)(object)val, position);
		((NetworkBehaviour)this).SendCommandInternal("System.Void HeroSkill::CmdUnequipGem_Internal(GemLocation,UnityEngine.Vector3)", 896427363, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public void CmdMoveGem(Gem gem, Vector3 position)
	{
		position = hero.agentPosition + Vector3.ClampMagnitude(position - hero.agentPosition, 1.5f);
		position = Dew.GetValidAgentDestination_LinearSweep(hero.agentPosition, position);
		position = Dew.GetPositionOnGround(position);
		gem.position = position;
		CmdMoveGem_Internal(gem, position);
	}

	[Command]
	private void CmdMoveGem_Internal(Gem gem, Vector3 position)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)gem);
		NetworkWriterExtensions.WriteVector3((NetworkWriter)(object)val, position);
		((NetworkBehaviour)this).SendCommandInternal("System.Void HeroSkill::CmdMoveGem_Internal(Gem,UnityEngine.Vector3)", -794189872, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public bool HasGemOfType(string type)
	{
		foreach (KeyValuePair<GemLocation, Gem> gem in gems)
		{
			if (((object)gem.Value).GetType().Name == type)
			{
				return true;
			}
		}
		return false;
	}

	public bool TryGetEquippedGemOfSameType(Type type, out GemLocation loc, out Gem gem)
	{
		foreach (KeyValuePair<GemLocation, Gem> gem2 in gems)
		{
			if (((object)gem2.Value).GetType() == type)
			{
				loc = gem2.Key;
				gem = gem2.Value;
				return true;
			}
		}
		loc = default;
		gem = null;
		return false;
	}

	public int GetMaxGemCount(HeroSkillLocation type)
	{
		return type switch
		{
			HeroSkillLocation.Q => maxGemCountQ, 
			HeroSkillLocation.W => maxGemCountW, 
			HeroSkillLocation.E => maxGemCountE, 
			HeroSkillLocation.R => maxGemCountR, 
			HeroSkillLocation.Identity => maxGemCountIdentity, 
			HeroSkillLocation.Movement => maxGemCountMovement, 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}

	public void SetMaxGemCount(HeroSkillLocation type, int count)
	{
		switch (type)
		{
		case HeroSkillLocation.Q:
			Network_003CmaxGemCountQ_003Ek__BackingField = count;
			break;
		case HeroSkillLocation.W:
			Network_003CmaxGemCountW_003Ek__BackingField = count;
			break;
		case HeroSkillLocation.E:
			Network_003CmaxGemCountE_003Ek__BackingField = count;
			break;
		case HeroSkillLocation.R:
			Network_003CmaxGemCountR_003Ek__BackingField = count;
			break;
		case HeroSkillLocation.Identity:
			Network_003CmaxGemCountIdentity_003Ek__BackingField = count;
			break;
		case HeroSkillLocation.Movement:
			Network_003CmaxGemCountMovement_003Ek__BackingField = count;
			break;
		default:
			throw new ArgumentOutOfRangeException("type", type, null);
		}
	}

	public int GetCurrentGemCount(HeroSkillLocation type)
	{
		int num = 0;
		foreach (Gem item in GetGemsInSkill(type))
		{
			_ = item;
			num++;
		}
		return num;
	}

	public int GetEmptyGemSlot(HeroSkillLocation type)
	{
		int maxGemCount = GetMaxGemCount(type);
		if (maxGemCount <= 0)
		{
			return -1;
		}
		for (int i = 0; i < maxGemCount; i++)
		{
			if (!gems.ContainsKey(new GemLocation
			{
				index = i,
				skill = type
			}))
			{
				return i;
			}
		}
		return -1;
	}

	public Gem GetFirstGem(HeroSkillLocation type)
	{
		if (GetMaxGemCount(type) <= 0)
		{
			return null;
		}
		GemLocation gemLocation = default;
		Gem gem = null;
		foreach (KeyValuePair<GemLocation, Gem> item in GetGemsPairInSkill(type))
		{
			if ((UnityEngine.Object)(object)gem == null || gemLocation.index > item.Key.index)
			{
				gemLocation = item.Key;
				gem = item.Value;
			}
		}
		return gem;
	}

	public Gem GetGem(GemLocation loc)
	{
		gems.TryGetValue(loc, out var value);
		return value;
	}

	public bool TryGetGem(GemLocation loc, out Gem gem)
	{
		return gems.TryGetValue(loc, out gem);
	}

	public IEnumerable<Gem> GetGemsInSkill(HeroSkillLocation type)
	{
		if (GetMaxGemCount(type) <= 0)
		{
			yield break;
		}
		foreach (KeyValuePair<GemLocation, Gem> gem in gems)
		{
			if (gem.Key.skill == type)
			{
				yield return gem.Value;
			}
		}
	}

	public IEnumerable<KeyValuePair<GemLocation, Gem>> GetGemsPairInSkill(HeroSkillLocation type)
	{
		foreach (KeyValuePair<GemLocation, Gem> gem in gems)
		{
			if (gem.Key.skill == type)
			{
				yield return gem;
			}
		}
	}

	public void CmdSwapSlotGem(GemLocation a, GemLocation b)
	{
		Gem gem = GetGem(a);
		Gem gem2 = GetGem(b);
		ManagerBase<EditSkillManager>.instance.SetClientState_SetGemSlot(a, gem2);
		ManagerBase<EditSkillManager>.instance.SetClientState_SetGemSlot(b, gem);
		CmdSwapSlotGem_Internal(a, b);
	}

	[Command]
	private void CmdSwapSlotGem_Internal(GemLocation a, GemLocation b)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_GemLocation((NetworkWriter)(object)val, a);
		GeneratedNetworkCode._Write_GemLocation((NetworkWriter)(object)val, b);
		((NetworkBehaviour)this).SendCommandInternal("System.Void HeroSkill::CmdSwapSlotGem_Internal(GemLocation,GemLocation)", -330993968, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcInvokeOnGemEquip(Gem gem)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)gem);
		((NetworkBehaviour)this).SendRPCInternal("System.Void HeroSkill::RpcInvokeOnGemEquip(Gem)", 279202744, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcInvokeOnGemUnequip(Gem gem)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)gem);
		((NetworkBehaviour)this).SendRPCInternal("System.Void HeroSkill::RpcInvokeOnGemUnequip(Gem)", 1260293521, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	internal void RpcInvokeOnGemPickup(Gem gem)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)gem);
		((NetworkBehaviour)this).SendRPCInternal("System.Void HeroSkill::RpcInvokeOnGemPickup(Gem)", -1409720352, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	internal void RpcInvokeOnGemDrop(Gem gem)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)gem);
		((NetworkBehaviour)this).SendRPCInternal("System.Void HeroSkill::RpcInvokeOnGemDrop(Gem)", 724416973, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	internal void RpcInvokeOnGemSwap(GemLocation a, GemLocation b)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_GemLocation((NetworkWriter)(object)val, a);
		GeneratedNetworkCode._Write_GemLocation((NetworkWriter)(object)val, b);
		((NetworkBehaviour)this).SendRPCInternal("System.Void HeroSkill::RpcInvokeOnGemSwap(GemLocation,GemLocation)", -1010508132, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public void RequestOnlyGemNotification(Gem gem)
	{
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)gem == null) && ((NetworkBehaviour)this).netIdentity.connectionToClient != null)
		{
			TpcOnlyGemNotification(gem);
		}
	}

	[TargetRpc]
	private void TpcOnlyGemNotification(Gem gem)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)gem);
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)null, "System.Void HeroSkill::TpcOnlyGemNotification(Gem)", -87490424, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	public HeroSkill()
	{
		((NetworkBehaviour)this).InitSyncObject((SyncObject)(object)_syncedGems);
		_Mirror_SyncVarHookDelegate__003CholdingObject_003Ek__BackingField = OnHoldingObjectChanged;
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_CmdUnequipSkill_Internal__HeroSkillLocation__Vector3(HeroSkillLocation type, Vector3 position)
	{
		try
		{
			SkillTrigger skillTrigger = UnequipSkill(type, position);
			if ((UnityEngine.Object)(object)skillTrigger != null)
			{
				RpcInvokeOnSkillDrop(skillTrigger);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected static void InvokeUserCode_CmdUnequipSkill_Internal__HeroSkillLocation__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdUnequipSkill_Internal called on client.");
		}
		else
		{
			((HeroSkill)(object)obj).UserCode_CmdUnequipSkill_Internal__HeroSkillLocation__Vector3(GeneratedNetworkCode._Read_HeroSkillLocation(reader), NetworkReaderExtensions.ReadVector3(reader));
		}
	}

	protected void UserCode_CmdEquipSkill_Internal__HeroSkillLocation__SkillTrigger(HeroSkillLocation type, SkillTrigger skill)
	{
		try
		{
			EquipSkill(type, skill);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected static void InvokeUserCode_CmdEquipSkill_Internal__HeroSkillLocation__SkillTrigger(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdEquipSkill_Internal called on client.");
		}
		else
		{
			((HeroSkill)(object)obj).UserCode_CmdEquipSkill_Internal__HeroSkillLocation__SkillTrigger(GeneratedNetworkCode._Read_HeroSkillLocation(reader), NetworkReaderExtensions.ReadNetworkBehaviour<SkillTrigger>(reader));
		}
	}

	protected void UserCode_CmdSwapSlotSkill_Internal__HeroSkillLocation__HeroSkillLocation(HeroSkillLocation a, HeroSkillLocation b)
	{
		try
		{
			SkillTrigger skill = GetSkill(a);
			SkillTrigger skill2 = GetSkill(b);
			if ((UnityEngine.Object)(object)skill != null)
			{
				UnequipSkill(a, hero.position);
			}
			if ((UnityEngine.Object)(object)skill2 != null)
			{
				UnequipSkill(b, hero.position);
			}
			if ((UnityEngine.Object)(object)skill != null)
			{
				EquipSkill(b, skill);
			}
			if ((UnityEngine.Object)(object)skill2 != null)
			{
				EquipSkill(a, skill2);
			}
			RpcInvokeOnSkillSwap(a, b);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected static void InvokeUserCode_CmdSwapSlotSkill_Internal__HeroSkillLocation__HeroSkillLocation(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSwapSlotSkill_Internal called on client.");
		}
		else
		{
			((HeroSkill)(object)obj).UserCode_CmdSwapSlotSkill_Internal__HeroSkillLocation__HeroSkillLocation(GeneratedNetworkCode._Read_HeroSkillLocation(reader), GeneratedNetworkCode._Read_HeroSkillLocation(reader));
		}
	}

	protected void UserCode_CmdMoveSkill_Internal__SkillTrigger__Vector3(SkillTrigger skill, Vector3 position)
	{
		if (!((UnityEngine.Object)(object)skill.owner != null))
		{
			position = hero.agentPosition + Vector3.ClampMagnitude(position - hero.agentPosition, 1.5f);
			position = Dew.GetValidAgentDestination_LinearSweep(hero.agentPosition, position);
			position = Dew.GetPositionOnGround(position);
			skill.RpcSetPositionAndRotation(position, ManagerBase<CameraManager>.instance.entityCamAngleRotation);
		}
	}

	protected static void InvokeUserCode_CmdMoveSkill_Internal__SkillTrigger__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdMoveSkill_Internal called on client.");
		}
		else
		{
			((HeroSkill)(object)obj).UserCode_CmdMoveSkill_Internal__SkillTrigger__Vector3(NetworkReaderExtensions.ReadNetworkBehaviour<SkillTrigger>(reader), NetworkReaderExtensions.ReadVector3(reader));
		}
	}

	protected void UserCode_RpcInvokeOnSkillEquip__SkillTrigger(SkillTrigger skill)
	{
		if (!((UnityEngine.Object)(object)skill == null))
		{
			ClientHeroEvent_OnSkillEquip?.Invoke(skill);
		}
	}

	protected static void InvokeUserCode_RpcInvokeOnSkillEquip__SkillTrigger(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeOnSkillEquip called on server.");
		}
		else
		{
			((HeroSkill)(object)obj).UserCode_RpcInvokeOnSkillEquip__SkillTrigger(NetworkReaderExtensions.ReadNetworkBehaviour<SkillTrigger>(reader));
		}
	}

	protected void UserCode_RpcInvokeOnSkillUnequip__SkillTrigger(SkillTrigger skill)
	{
		if (!((UnityEngine.Object)(object)skill == null))
		{
			ClientHeroEvent_OnSkillUnequip?.Invoke(skill);
		}
	}

	protected static void InvokeUserCode_RpcInvokeOnSkillUnequip__SkillTrigger(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeOnSkillUnequip called on server.");
		}
		else
		{
			((HeroSkill)(object)obj).UserCode_RpcInvokeOnSkillUnequip__SkillTrigger(NetworkReaderExtensions.ReadNetworkBehaviour<SkillTrigger>(reader));
		}
	}

	protected void UserCode_RpcInvokeOnSkillPickup__SkillTrigger(SkillTrigger skill)
	{
		if (!((UnityEngine.Object)(object)skill == null))
		{
			ClientHeroEvent_OnSkillPickup?.Invoke(skill);
		}
	}

	protected static void InvokeUserCode_RpcInvokeOnSkillPickup__SkillTrigger(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeOnSkillPickup called on server.");
		}
		else
		{
			((HeroSkill)(object)obj).UserCode_RpcInvokeOnSkillPickup__SkillTrigger(NetworkReaderExtensions.ReadNetworkBehaviour<SkillTrigger>(reader));
		}
	}

	protected void UserCode_RpcInvokeOnSkillDrop__SkillTrigger(SkillTrigger skill)
	{
		if (!((UnityEngine.Object)(object)skill == null))
		{
			ClientHeroEvent_OnSkillDrop?.Invoke(skill);
		}
	}

	protected static void InvokeUserCode_RpcInvokeOnSkillDrop__SkillTrigger(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeOnSkillDrop called on server.");
		}
		else
		{
			((HeroSkill)(object)obj).UserCode_RpcInvokeOnSkillDrop__SkillTrigger(NetworkReaderExtensions.ReadNetworkBehaviour<SkillTrigger>(reader));
		}
	}

	protected void UserCode_RpcInvokeOnSkillSwap__HeroSkillLocation__HeroSkillLocation(HeroSkillLocation a, HeroSkillLocation b)
	{
		ClientHeroEvent_OnSkillSwap?.Invoke(a, b);
	}

	protected static void InvokeUserCode_RpcInvokeOnSkillSwap__HeroSkillLocation__HeroSkillLocation(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeOnSkillSwap called on server.");
		}
		else
		{
			((HeroSkill)(object)obj).UserCode_RpcInvokeOnSkillSwap__HeroSkillLocation__HeroSkillLocation(GeneratedNetworkCode._Read_HeroSkillLocation(reader), GeneratedNetworkCode._Read_HeroSkillLocation(reader));
		}
	}

	protected void UserCode_CmdStopHoldInHand_Internal()
	{
		try
		{
			StopHoldInHand();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected static void InvokeUserCode_CmdStopHoldInHand_Internal(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdStopHoldInHand_Internal called on client.");
		}
		else
		{
			((HeroSkill)(object)obj).UserCode_CmdStopHoldInHand_Internal();
		}
	}

	protected void UserCode_CmdEquipGem_Internal__GemLocation__Gem(GemLocation loc, Gem gem)
	{
		try
		{
			EquipGem(loc, gem);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected static void InvokeUserCode_CmdEquipGem_Internal__GemLocation__Gem(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdEquipGem_Internal called on client.");
		}
		else
		{
			((HeroSkill)(object)obj).UserCode_CmdEquipGem_Internal__GemLocation__Gem(GeneratedNetworkCode._Read_GemLocation(reader), NetworkReaderExtensions.ReadNetworkBehaviour<Gem>(reader));
		}
	}

	protected void UserCode_CmdUnequipGem_Internal__GemLocation__Vector3(GemLocation loc, Vector3 position)
	{
		try
		{
			Gem gem = UnequipGem(loc, position);
			RpcInvokeOnGemDrop(gem);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected static void InvokeUserCode_CmdUnequipGem_Internal__GemLocation__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdUnequipGem_Internal called on client.");
		}
		else
		{
			((HeroSkill)(object)obj).UserCode_CmdUnequipGem_Internal__GemLocation__Vector3(GeneratedNetworkCode._Read_GemLocation(reader), NetworkReaderExtensions.ReadVector3(reader));
		}
	}

	protected void UserCode_CmdMoveGem_Internal__Gem__Vector3(Gem gem, Vector3 position)
	{
		if (!((UnityEngine.Object)(object)gem.owner != null))
		{
			position = hero.agentPosition + Vector3.ClampMagnitude(position - hero.agentPosition, 1.5f);
			position = Dew.GetValidAgentDestination_LinearSweep(hero.agentPosition, position);
			position = Dew.GetPositionOnGround(position);
			gem.RpcSetPositionAndRotation(position, ManagerBase<CameraManager>.instance.entityCamAngleRotation);
		}
	}

	protected static void InvokeUserCode_CmdMoveGem_Internal__Gem__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdMoveGem_Internal called on client.");
		}
		else
		{
			((HeroSkill)(object)obj).UserCode_CmdMoveGem_Internal__Gem__Vector3(NetworkReaderExtensions.ReadNetworkBehaviour<Gem>(reader), NetworkReaderExtensions.ReadVector3(reader));
		}
	}

	protected void UserCode_CmdSwapSlotGem_Internal__GemLocation__GemLocation(GemLocation a, GemLocation b)
	{
		try
		{
			Gem gem = GetGem(a);
			Gem gem2 = GetGem(b);
			if ((UnityEngine.Object)(object)gem != null)
			{
				UnequipGem(a, hero.position);
			}
			if ((UnityEngine.Object)(object)gem2 != null)
			{
				UnequipGem(b, hero.position);
			}
			if ((UnityEngine.Object)(object)gem != null)
			{
				EquipGem(b, gem);
			}
			if ((UnityEngine.Object)(object)gem2 != null)
			{
				EquipGem(a, gem2);
			}
			RpcInvokeOnGemSwap(a, b);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected static void InvokeUserCode_CmdSwapSlotGem_Internal__GemLocation__GemLocation(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSwapSlotGem_Internal called on client.");
		}
		else
		{
			((HeroSkill)(object)obj).UserCode_CmdSwapSlotGem_Internal__GemLocation__GemLocation(GeneratedNetworkCode._Read_GemLocation(reader), GeneratedNetworkCode._Read_GemLocation(reader));
		}
	}

	protected void UserCode_RpcInvokeOnGemEquip__Gem(Gem gem)
	{
		if (!((UnityEngine.Object)(object)gem == null))
		{
			ClientHeroEvent_OnGemEquip?.Invoke(gem);
		}
	}

	protected static void InvokeUserCode_RpcInvokeOnGemEquip__Gem(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeOnGemEquip called on server.");
		}
		else
		{
			((HeroSkill)(object)obj).UserCode_RpcInvokeOnGemEquip__Gem(NetworkReaderExtensions.ReadNetworkBehaviour<Gem>(reader));
		}
	}

	protected void UserCode_RpcInvokeOnGemUnequip__Gem(Gem gem)
	{
		if (!((UnityEngine.Object)(object)gem == null))
		{
			ClientHeroEvent_OnGemUnequip?.Invoke(gem);
		}
	}

	protected static void InvokeUserCode_RpcInvokeOnGemUnequip__Gem(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeOnGemUnequip called on server.");
		}
		else
		{
			((HeroSkill)(object)obj).UserCode_RpcInvokeOnGemUnequip__Gem(NetworkReaderExtensions.ReadNetworkBehaviour<Gem>(reader));
		}
	}

	protected void UserCode_RpcInvokeOnGemPickup__Gem(Gem gem)
	{
		if (!((UnityEngine.Object)(object)gem == null))
		{
			ClientHeroEvent_OnGemPickup?.Invoke(gem);
		}
	}

	protected static void InvokeUserCode_RpcInvokeOnGemPickup__Gem(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeOnGemPickup called on server.");
		}
		else
		{
			((HeroSkill)(object)obj).UserCode_RpcInvokeOnGemPickup__Gem(NetworkReaderExtensions.ReadNetworkBehaviour<Gem>(reader));
		}
	}

	protected void UserCode_RpcInvokeOnGemDrop__Gem(Gem gem)
	{
		if (!((UnityEngine.Object)(object)gem == null))
		{
			ClientHeroEvent_OnGemDrop?.Invoke(gem);
		}
	}

	protected static void InvokeUserCode_RpcInvokeOnGemDrop__Gem(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeOnGemDrop called on server.");
		}
		else
		{
			((HeroSkill)(object)obj).UserCode_RpcInvokeOnGemDrop__Gem(NetworkReaderExtensions.ReadNetworkBehaviour<Gem>(reader));
		}
	}

	protected void UserCode_RpcInvokeOnGemSwap__GemLocation__GemLocation(GemLocation a, GemLocation b)
	{
		ClientHeroEvent_OnGemSwap?.Invoke(a, b);
	}

	protected static void InvokeUserCode_RpcInvokeOnGemSwap__GemLocation__GemLocation(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeOnGemSwap called on server.");
		}
		else
		{
			((HeroSkill)(object)obj).UserCode_RpcInvokeOnGemSwap__GemLocation__GemLocation(GeneratedNetworkCode._Read_GemLocation(reader), GeneratedNetworkCode._Read_GemLocation(reader));
		}
	}

	protected void UserCode_TpcOnlyGemNotification__Gem(Gem gem)
	{
		if (!((UnityEngine.Object)(object)gem == null))
		{
			ClientHeroEvent_OnOnlyGemNotificationRequested?.Invoke(gem);
		}
	}

	protected static void InvokeUserCode_TpcOnlyGemNotification__Gem(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcOnlyGemNotification called on server.");
		}
		else
		{
			((HeroSkill)(object)obj).UserCode_TpcOnlyGemNotification__Gem(NetworkReaderExtensions.ReadNetworkBehaviour<Gem>(reader));
		}
	}

	static HeroSkill()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected Obj, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected Obj, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected Obj, but got Unknown
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Expected Obj, but got Unknown
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected Obj, but got Unknown
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Expected Obj, but got Unknown
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Expected Obj, but got Unknown
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Expected Obj, but got Unknown
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Expected Obj, but got Unknown
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Expected Obj, but got Unknown
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Expected Obj, but got Unknown
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Expected Obj, but got Unknown
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Expected Obj, but got Unknown
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Expected Obj, but got Unknown
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Expected Obj, but got Unknown
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Expected Obj, but got Unknown
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Expected Obj, but got Unknown
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Expected Obj, but got Unknown
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Expected Obj, but got Unknown
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterCommand(typeof(HeroSkill), "System.Void HeroSkill::CmdUnequipSkill_Internal(HeroSkillLocation,UnityEngine.Vector3)", (RemoteCallDelegate)InvokeUserCode_CmdUnequipSkill_Internal__HeroSkillLocation__Vector3, true);
		RemoteProcedureCalls.RegisterCommand(typeof(HeroSkill), "System.Void HeroSkill::CmdEquipSkill_Internal(HeroSkillLocation,SkillTrigger)", (RemoteCallDelegate)InvokeUserCode_CmdEquipSkill_Internal__HeroSkillLocation__SkillTrigger, true);
		RemoteProcedureCalls.RegisterCommand(typeof(HeroSkill), "System.Void HeroSkill::CmdSwapSlotSkill_Internal(HeroSkillLocation,HeroSkillLocation)", (RemoteCallDelegate)InvokeUserCode_CmdSwapSlotSkill_Internal__HeroSkillLocation__HeroSkillLocation, true);
		RemoteProcedureCalls.RegisterCommand(typeof(HeroSkill), "System.Void HeroSkill::CmdMoveSkill_Internal(SkillTrigger,UnityEngine.Vector3)", (RemoteCallDelegate)InvokeUserCode_CmdMoveSkill_Internal__SkillTrigger__Vector3, true);
		RemoteProcedureCalls.RegisterCommand(typeof(HeroSkill), "System.Void HeroSkill::CmdStopHoldInHand_Internal()", (RemoteCallDelegate)InvokeUserCode_CmdStopHoldInHand_Internal, true);
		RemoteProcedureCalls.RegisterCommand(typeof(HeroSkill), "System.Void HeroSkill::CmdEquipGem_Internal(GemLocation,Gem)", (RemoteCallDelegate)InvokeUserCode_CmdEquipGem_Internal__GemLocation__Gem, true);
		RemoteProcedureCalls.RegisterCommand(typeof(HeroSkill), "System.Void HeroSkill::CmdUnequipGem_Internal(GemLocation,UnityEngine.Vector3)", (RemoteCallDelegate)InvokeUserCode_CmdUnequipGem_Internal__GemLocation__Vector3, true);
		RemoteProcedureCalls.RegisterCommand(typeof(HeroSkill), "System.Void HeroSkill::CmdMoveGem_Internal(Gem,UnityEngine.Vector3)", (RemoteCallDelegate)InvokeUserCode_CmdMoveGem_Internal__Gem__Vector3, true);
		RemoteProcedureCalls.RegisterCommand(typeof(HeroSkill), "System.Void HeroSkill::CmdSwapSlotGem_Internal(GemLocation,GemLocation)", (RemoteCallDelegate)InvokeUserCode_CmdSwapSlotGem_Internal__GemLocation__GemLocation, true);
		RemoteProcedureCalls.RegisterRpc(typeof(HeroSkill), "System.Void HeroSkill::RpcInvokeOnSkillEquip(SkillTrigger)", (RemoteCallDelegate)InvokeUserCode_RpcInvokeOnSkillEquip__SkillTrigger);
		RemoteProcedureCalls.RegisterRpc(typeof(HeroSkill), "System.Void HeroSkill::RpcInvokeOnSkillUnequip(SkillTrigger)", (RemoteCallDelegate)InvokeUserCode_RpcInvokeOnSkillUnequip__SkillTrigger);
		RemoteProcedureCalls.RegisterRpc(typeof(HeroSkill), "System.Void HeroSkill::RpcInvokeOnSkillPickup(SkillTrigger)", (RemoteCallDelegate)InvokeUserCode_RpcInvokeOnSkillPickup__SkillTrigger);
		RemoteProcedureCalls.RegisterRpc(typeof(HeroSkill), "System.Void HeroSkill::RpcInvokeOnSkillDrop(SkillTrigger)", (RemoteCallDelegate)InvokeUserCode_RpcInvokeOnSkillDrop__SkillTrigger);
		RemoteProcedureCalls.RegisterRpc(typeof(HeroSkill), "System.Void HeroSkill::RpcInvokeOnSkillSwap(HeroSkillLocation,HeroSkillLocation)", (RemoteCallDelegate)InvokeUserCode_RpcInvokeOnSkillSwap__HeroSkillLocation__HeroSkillLocation);
		RemoteProcedureCalls.RegisterRpc(typeof(HeroSkill), "System.Void HeroSkill::RpcInvokeOnGemEquip(Gem)", (RemoteCallDelegate)InvokeUserCode_RpcInvokeOnGemEquip__Gem);
		RemoteProcedureCalls.RegisterRpc(typeof(HeroSkill), "System.Void HeroSkill::RpcInvokeOnGemUnequip(Gem)", (RemoteCallDelegate)InvokeUserCode_RpcInvokeOnGemUnequip__Gem);
		RemoteProcedureCalls.RegisterRpc(typeof(HeroSkill), "System.Void HeroSkill::RpcInvokeOnGemPickup(Gem)", (RemoteCallDelegate)InvokeUserCode_RpcInvokeOnGemPickup__Gem);
		RemoteProcedureCalls.RegisterRpc(typeof(HeroSkill), "System.Void HeroSkill::RpcInvokeOnGemDrop(Gem)", (RemoteCallDelegate)InvokeUserCode_RpcInvokeOnGemDrop__Gem);
		RemoteProcedureCalls.RegisterRpc(typeof(HeroSkill), "System.Void HeroSkill::RpcInvokeOnGemSwap(GemLocation,GemLocation)", (RemoteCallDelegate)InvokeUserCode_RpcInvokeOnGemSwap__GemLocation__GemLocation);
		RemoteProcedureCalls.RegisterRpc(typeof(HeroSkill), "System.Void HeroSkill::TpcOnlyGemNotification(Gem)", (RemoteCallDelegate)InvokeUserCode_TpcOnlyGemNotification__Gem);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		((NetworkBehaviour)this).SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteIHoldableInHand(holdingObject__BackingField);
			NetworkWriterExtensions.WriteFloat(writer, starNormalizedStrength);
			NetworkWriterExtensions.WriteInt(writer, maxGemCountQ__BackingField);
			NetworkWriterExtensions.WriteInt(writer, maxGemCountW__BackingField);
			NetworkWriterExtensions.WriteInt(writer, maxGemCountE__BackingField);
			NetworkWriterExtensions.WriteInt(writer, maxGemCountR__BackingField);
			NetworkWriterExtensions.WriteInt(writer, maxGemCountIdentity__BackingField);
			NetworkWriterExtensions.WriteInt(writer, maxGemCountMovement__BackingField);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteIHoldableInHand(holdingObject__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 2L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, starNormalizedStrength);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 4L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, maxGemCountQ__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 8L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, maxGemCountW__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x10L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, maxGemCountE__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x20L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, maxGemCountR__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, maxGemCountIdentity__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, maxGemCountMovement__BackingField);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		((NetworkBehaviour)this).DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<IItem>(ref holdingObject__BackingField, _Mirror_SyncVarHookDelegate__003CholdingObject_003Ek__BackingField, reader.ReadIHoldableInHand());
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref starNormalizedStrength, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref maxGemCountQ__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref maxGemCountW__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref maxGemCountE__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref maxGemCountR__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref maxGemCountIdentity__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref maxGemCountMovement__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 1L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<IItem>(ref holdingObject__BackingField, _Mirror_SyncVarHookDelegate__003CholdingObject_003Ek__BackingField, reader.ReadIHoldableInHand());
		}
		if ((num & 2L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref starNormalizedStrength, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 4L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref maxGemCountQ__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 8L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref maxGemCountW__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x10L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref maxGemCountE__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x20L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref maxGemCountR__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref maxGemCountIdentity__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref maxGemCountMovement__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
	}
}
