using System;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Mon_Primus_BossPrimusAeron : BossMonster
{
	[Serializable]
	public class AnimEntries
	{
		public AnimationClip idle;

		public AnimationClip runForward;

		public AnimationClip runForwardRight;

		public AnimationClip runRight;

		public AnimationClip runBackwardRight;

		public AnimationClip runBackward;

		public AnimationClip runBackwardLeft;

		public AnimationClip runLeft;

		public AnimationClip runForwardLeft;
	}

	public enum WeaponType
	{
		None,
		GreatSword,
		Spells,
		DoubleSword
	}

	public enum PhaseType
	{
		Force = 0,
		Adapt = 1,
		Rage = 2,
		InTransition = -1
	}

	[NonSerialized]
	public GameObject[] fxYesArmor;

	[NonSerialized]
	public GameObject[] fxNoArmor;

	[NonSerialized]
	public GameObject[] fxGreatSword;

	[NonSerialized]
	public GameObject[] fxSpells;

	[NonSerialized]
	public GameObject[] fxDoubleSword;

	public AnimEntries[] animsByPhase;

	[CompilerGenerated]
	[SyncVar(hook = "OnIsArmorBrokenChanged")]
	private bool isArmorBroken__BackingField;

	[CompilerGenerated]
	[SyncVar(hook = "OnWeaponChanged")]
	private WeaponType weapon__BackingField = WeaponType.GreatSword;

	[CompilerGenerated]
	[SyncVar(hook = "OnPhaseChanged")]
	private PhaseType phase__BackingField;

	public Action<bool, bool> _Mirror_SyncVarHookDelegate__003CisArmorBroken_003Ek__BackingField;

	public Action<WeaponType, WeaponType> _Mirror_SyncVarHookDelegate__003Cweapon_003Ek__BackingField;

	public Action<PhaseType, PhaseType> _Mirror_SyncVarHookDelegate__003Cphase_003Ek__BackingField;

	public bool isArmorBroken
	{
		[CompilerGenerated]
		get
		{
			return isArmorBroken__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CisArmorBroken_003Ek__BackingField = value;
		}
	}

	public WeaponType weapon
	{
		[CompilerGenerated]
		get
		{
			return weapon__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003Cweapon_003Ek__BackingField = value;
		}
	}

	public PhaseType phase
	{
		[CompilerGenerated]
		get
		{
			return phase__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003Cphase_003Ek__BackingField = value;
		}
	}

	public bool Network_003CisArmorBroken_003Ek__BackingField
	{
		get
		{
			return isArmorBroken__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref isArmorBroken__BackingField, 512uL, _Mirror_SyncVarHookDelegate__003CisArmorBroken_003Ek__BackingField);
		}
	}

	public WeaponType Network_003Cweapon_003Ek__BackingField
	{
		get
		{
			return weapon__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<WeaponType>(value, ref weapon__BackingField, 1024uL, _Mirror_SyncVarHookDelegate__003Cweapon_003Ek__BackingField);
		}
	}

	public PhaseType Network_003Cphase_003Ek__BackingField
	{
		get
		{
			return phase__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<PhaseType>(value, ref phase__BackingField, 2048uL, _Mirror_SyncVarHookDelegate__003Cphase_003Ek__BackingField);
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			Network_003CisArmorBroken_003Ek__BackingField = false;
			Network_003Cweapon_003Ek__BackingField = WeaponType.GreatSword;
			Network_003Cphase_003Ek__BackingField = PhaseType.Force;
			CreateBasicEffect(this, new UnstoppableEffect(), float.PositiveInfinity);
			skipBossSoulFlow = true;
			CreateStatusEffect<Se_Mon_Primus_BossPrimusAeron_PhaseSwitcher>(this, new CastInfo(this));
			CreateStatusEffect<Se_Mon_Primus_BossPrimusAeron_Adaptation>(this, new CastInfo(this));
			if (!Visual.skipSpawning)
			{
				CreateAbilityInstance<Ai_Mon_Primus_SpawnAttack>(Dew.GetPositionOnGround(((Component)(object)this).transform.position + ((Component)(object)this).transform.forward * 3f), null, new CastInfo(this));
			}
		}
	}

	public override void OnModelLoaded()
	{
		base.OnModelLoaded();
		fxYesArmor = Visual.model.GetCustomMappingArray<GameObject>("fxYesArmor");
		fxNoArmor = Visual.model.GetCustomMappingArray<GameObject>("fxNoArmor");
		fxGreatSword = Visual.model.GetCustomMappingArray<GameObject>("fxGreatSword");
		fxSpells = Visual.model.GetCustomMappingArray<GameObject>("fxSpells");
		fxDoubleSword = Visual.model.GetCustomMappingArray<GameObject>("fxDoubleSword");
		OnIsArmorBrokenChanged(oldVal: false, isArmorBroken);
		OnWeaponChanged(WeaponType.None, weapon);
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if (phase != PhaseType.InTransition && !((UnityEngine.Object)(object)context.targetEnemy == null))
		{
			if (phase == PhaseType.Force)
			{
				AIUpdate_Force(ref context);
			}
			else if (phase == PhaseType.Adapt)
			{
				AIUpdate_Adapt(ref context);
			}
			else if (phase == PhaseType.Rage)
			{
				AIUpdate_Rage(ref context);
			}
		}
	}

	private void AIUpdate_Force(ref EntityAIContext context)
	{
		if (normalizedHealth < 0.35f && AI.Helper_CanBeCast<At_Mon_Primus_BossPrimusAeron_Force_GoldRain>() && AI.Helper_IsTargetInRange<At_Mon_Primus_BossPrimusAeron_Force_GoldRain>())
		{
			AI.Helper_CastAbilityAuto<At_Mon_Primus_BossPrimusAeron_Force_GoldRain>();
			return;
		}
		if (normalizedHealth < 0.8f && UnityEngine.Random.value < 0.2f && AI.Helper_CanBeCast<At_Mon_Primus_BossPrimusAeron_Force_DropGiantSword>())
		{
			Primus_Pizza0 closestPizza = null;
			float num = float.PositiveInfinity;
			int num2 = 0;
			foreach (Primus_Pizza0 instance in Primus_Pizza0.instances)
			{
				if (!instance.isBroken)
				{
					num2++;
					float num3 = Vector3.Distance(context.targetEnemy.GetAIAgentPosition(this), instance.centerPoint.position);
					if (num3 < num)
					{
						closestPizza = instance;
						num = num3;
					}
				}
			}
			if ((UnityEngine.Object)(object)closestPizza != null && num2 > 4)
			{
				Debug.Log($"Breaking a pizza in AIUpdate. Had {num2} remaining pizzas");
				Vector3 positionOnGround = Dew.GetPositionOnGround(closestPizza.centerPoint.position);
				CreateAbilityInstance(positionOnGround, null, new CastInfo(this, positionOnGround), (Ai_Mon_Primus_BossPrimusAeron_Force_DropGiantSword ai) =>
				{
					ai.pizza = closestPizza;
				});
				Ability.GetAbility<At_Mon_Primus_BossPrimusAeron_Force_DropGiantSword>().SetCharge(0, 0);
			}
			else
			{
				AI.Helper_CastAbilityAuto<At_Mon_Primus_BossPrimusAeron_Force_DropGiantSword>();
			}
		}
		if (normalizedHealth < 0.65f && UnityEngine.Random.value < 0.35f && AI.Helper_CanBeCast<At_Mon_Primus_BossPrimusAeron_Force_JumpAttack>() && AI.Helper_IsTargetInRange<At_Mon_Primus_BossPrimusAeron_Force_JumpAttack>())
		{
			AI.Helper_CastAbilityAuto<At_Mon_Primus_BossPrimusAeron_Force_JumpAttack>();
		}
		else if (UnityEngine.Random.value < 0.4f && AI.Helper_CanBeCast<At_Mon_Primus_BossPrimusAeron_Force_Swipe>() && AI.Helper_IsTargetInRange<At_Mon_Primus_BossPrimusAeron_Force_Swipe>())
		{
			AI.Helper_CastAbilityAuto<At_Mon_Primus_BossPrimusAeron_Force_Swipe>();
		}
		else if (AI.Helper_CanBeCast<At_Mon_Primus_BossPrimusAeron_Dash>() && Vector3.Distance(agentPosition, context.targetEnemy.GetAIAgentPosition(this)) > 4f)
		{
			AI.Helper_CastAbilityAuto<At_Mon_Primus_BossPrimusAeron_Dash>();
		}
		else if (normalizedHealth < 0.8f && UnityEngine.Random.value < 0.4f && AI.Helper_CanBeCast<At_Mon_Primus_BossPrimusAeron_Force_Grab>() && AI.Helper_IsTargetInRange<At_Mon_Primus_BossPrimusAeron_Force_Grab>())
		{
			AI.Helper_CastAbilityAuto<At_Mon_Primus_BossPrimusAeron_Force_Grab>();
		}
		else
		{
			AI.Helper_ChaseTarget();
		}
	}

	private void AIUpdate_Adapt(ref EntityAIContext context)
	{
		if (normalizedHealth < 0.5f && AI.Helper_CanBeCast<At_Mon_Primus_BossPrimusAeron_Adapt_Doom>())
		{
			AI.Helper_CastAbilityAuto<At_Mon_Primus_BossPrimusAeron_Adapt_Doom>();
		}
		else if (normalizedHealth < 0.75f && UnityEngine.Random.value < 0.2f && AI.Helper_CanBeCast<At_Mon_Primus_BossPrimusAeron_Adapt_SmiteStorm>())
		{
			AI.Helper_CastAbilityAuto<At_Mon_Primus_BossPrimusAeron_Adapt_SmiteStorm>();
		}
		else if (UnityEngine.Random.value < 0.4f && AI.Helper_CanBeCast<At_Mon_Primus_BossPrimusAeron_Adapt_Starfall>())
		{
			AI.Helper_CastAbilityAuto<At_Mon_Primus_BossPrimusAeron_Adapt_Starfall>();
		}
		else if (normalizedHealth < 0.9f && UnityEngine.Random.value < 0.4f && AI.Helper_CanBeCast<At_Mon_Primus_BossPrimusAeron_Adapt_IceBlock>())
		{
			AI.Helper_CastAbilityAuto<At_Mon_Primus_BossPrimusAeron_Adapt_IceBlock>();
		}
		else if (normalizedHealth < 0.8f && UnityEngine.Random.value < 0.4f && AI.Helper_CanBeCast<At_Mon_Primus_BossPrimusAeron_Adapt_Arbalest>() && AI.Helper_IsTargetInRange<At_Mon_Primus_BossPrimusAeron_Adapt_Arbalest>())
		{
			AI.Helper_CastAbilityAuto<At_Mon_Primus_BossPrimusAeron_Adapt_Arbalest>();
		}
		else if (UnityEngine.Random.value < 0.4f && AI.Helper_CanBeCast<At_Mon_Primus_BossPrimusAeron_Dash>())
		{
			AI.Helper_CastAbilityAuto<At_Mon_Primus_BossPrimusAeron_Dash>();
		}
		else
		{
			AI.Helper_ChaseTarget();
		}
	}

	private void AIUpdate_Rage(ref EntityAIContext context)
	{
		float num = (currentHealth + Status.currentShield) / maxHealth;
		if (num < 0.4f && AI.Helper_CanBeCast<At_Mon_Primus_BossPrimusAeron_Rage_MassSilence>())
		{
			AI.Helper_CastAbilityAuto<At_Mon_Primus_BossPrimusAeron_Rage_MassSilence>();
		}
		else if (num < 0.5f && AI.Helper_CanBeCast<At_Mon_Primus_BossPrimusAeron_Rage_MassDelusion>())
		{
			AI.Helper_CastAbilityAuto<At_Mon_Primus_BossPrimusAeron_Rage_MassDelusion>();
		}
		else if (num < 0.75f && UnityEngine.Random.value < 0.4f && AI.Helper_CanBeCast<At_Mon_Primus_BossPrimusAeron_Rage_DashAttack>())
		{
			AI.Helper_CastAbilityAuto<At_Mon_Primus_BossPrimusAeron_Rage_DashAttack>();
		}
		else if (UnityEngine.Random.value < 0.4f && AI.Helper_CanBeCast<At_Mon_Primus_BossPrimusAeron_Dash>())
		{
			AI.Helper_CastAbilityAuto<At_Mon_Primus_BossPrimusAeron_Dash>();
		}
		else
		{
			AI.Helper_ChaseTarget();
		}
	}

	private void OnIsArmorBrokenChanged(bool oldVal, bool newVal)
	{
		if (newVal)
		{
			GameObject[] array = fxNoArmor;
			foreach (GameObject effect in array)
			{
				FxPlay(effect);
			}
			array = fxYesArmor;
			foreach (GameObject effect2 in array)
			{
				FxStop(effect2);
			}
		}
		else
		{
			GameObject[] array = fxYesArmor;
			foreach (GameObject effect3 in array)
			{
				FxPlay(effect3);
			}
			array = fxNoArmor;
			foreach (GameObject effect4 in array)
			{
				FxStop(effect4);
			}
		}
	}

	private void OnWeaponChanged(WeaponType oldMode, WeaponType newMode)
	{
		GameObject[] array = fxGreatSword;
		foreach (GameObject effect in array)
		{
			if (newMode == WeaponType.GreatSword)
			{
				FxPlay(effect, this);
			}
			else
			{
				FxStop(effect);
			}
		}
		array = fxSpells;
		foreach (GameObject effect2 in array)
		{
			if (newMode == WeaponType.Spells)
			{
				FxPlay(effect2, this);
			}
			else
			{
				FxStop(effect2);
			}
		}
		array = fxDoubleSword;
		foreach (GameObject effect3 in array)
		{
			if (newMode == WeaponType.DoubleSword)
			{
				FxPlay(effect3, this);
			}
			else
			{
				FxStop(effect3);
			}
		}
	}

	private void OnPhaseChanged(PhaseType oldPhase, PhaseType newPhase)
	{
		if (newPhase >= PhaseType.Force && (int)newPhase < animsByPhase.Length)
		{
			AnimEntries animEntries = animsByPhase[(int)newPhase];
			Animation.ReplaceAnimationLocal(EntityAnimation.ReplaceableAnimationType.Idle, animEntries.idle);
			Animation.ReplaceAnimationLocal(EntityAnimation.ReplaceableAnimationType.RunForward, animEntries.runForward);
			Animation.ReplaceAnimationLocal(EntityAnimation.ReplaceableAnimationType.RunForwardRight, animEntries.runForwardRight);
			Animation.ReplaceAnimationLocal(EntityAnimation.ReplaceableAnimationType.RunRight, animEntries.runRight);
			Animation.ReplaceAnimationLocal(EntityAnimation.ReplaceableAnimationType.RunBackwardRight, animEntries.runBackwardRight);
			Animation.ReplaceAnimationLocal(EntityAnimation.ReplaceableAnimationType.RunBackward, animEntries.runBackward);
			Animation.ReplaceAnimationLocal(EntityAnimation.ReplaceableAnimationType.RunBackwardLeft, animEntries.runBackwardLeft);
			Animation.ReplaceAnimationLocal(EntityAnimation.ReplaceableAnimationType.RunLeft, animEntries.runLeft);
			Animation.ReplaceAnimationLocal(EntityAnimation.ReplaceableAnimationType.RunForwardLeft, animEntries.runForwardLeft);
			if ((UnityEngine.Object)(object)animEntries.runBackwardLeft != null)
			{
				Animation.animator.SetInteger("locomotionType", 2);
			}
			else
			{
				Animation.animator.SetInteger("locomotionType", 1);
			}
		}
		if (((NetworkBehaviour)this).isServer)
		{
			switch (newPhase)
			{
			case PhaseType.Force:
			{
				At_Mon_Primus_BossPrimusAeron_Force_Atk attackAbility5 = Dew.CreateAbilityTrigger<At_Mon_Primus_BossPrimusAeron_Force_Atk>();
				AbilityTrigger attackAbility6 = Ability.attackAbility;
				Ability.SetAttackAbility(attackAbility5);
				attackAbility6.Destroy();
				break;
			}
			case PhaseType.Adapt:
			{
				At_Mon_Primus_BossPrimusAeron_Adapt_Atk attackAbility3 = Dew.CreateAbilityTrigger<At_Mon_Primus_BossPrimusAeron_Adapt_Atk>();
				AbilityTrigger attackAbility4 = Ability.attackAbility;
				Ability.SetAttackAbility(attackAbility3);
				attackAbility4.Destroy();
				break;
			}
			case PhaseType.Rage:
			{
				At_Mon_Primus_BossPrimusAeron_Rage_Atk attackAbility = Dew.CreateAbilityTrigger<At_Mon_Primus_BossPrimusAeron_Rage_Atk>();
				AbilityTrigger attackAbility2 = Ability.attackAbility;
				Ability.SetAttackAbility(attackAbility);
				attackAbility2.Destroy();
				break;
			}
			}
		}
	}

	protected override void OnDeath(EventInfoKill info)
	{
		base.OnDeath(info);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		Control.Rotate(ManagerBase<CameraManager>.instance.entityCamAngle + 180f, immediately: true, 1f);
		GameObject[] array = fxDoubleSword;
		foreach (GameObject effect in array)
		{
			FxStopNetworked(effect);
		}
		if (weapon == WeaponType.DoubleSword)
		{
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		DewGameplayExperienceSettings ges = NetworkedManagerBase<GameManager>.instance.ges;
		int num = UnityEngine.Random.Range(ges.stardustBossSoulAmount.x, ges.stardustBossSoulAmount.y + 1);
		num += NetworkedManagerBase<GameManager>.instance.difficulty.bossKillBonusStardust;
		foreach (Actor allActor in NetworkedManagerBase<ActorManager>.instance.allActors)
		{
			if (allActor is LucidDream { type: LucidDreamType.Evil })
			{
				num += 3;
			}
		}
		foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
		{
			gamePlayer.GiveStardust(num);
		}
		if ((bool)(UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Primus_Ending>.instance)
		{
			SingletonDewNetworkBehaviour<Primus_Ending>.instance.StartPrimusDeath();
		}
		IEnumerator Routine()
		{
			for (int s = 0; s < 2; s++)
			{
				Vector3 vector = ((s == 0) ? new Vector3(-4f, -2f, 0f) : new Vector3(4f, -2f, 0f));
				Vector3 landPos = agentPosition;
				for (int j = 0; j < 10; j++)
				{
					if (Vector2.Distance(agentPosition.ToXY(), landPos.ToXY()) > 4f)
					{
						break;
					}
					landPos = Dew.GetValidAgentDestination_LinearSweep(agentPosition, agentPosition + vector + UnityEngine.Random.onUnitSphere.Flattened().normalized * j * 0.5f);
				}
				CreateAbilityInstance(position, Quaternion.LookRotation(landPos - agentPosition), new CastInfo(this, landPos), (Ai_Mon_Primus_BossPrimusAeron_PhaseSwitcher_DualSword ai) =>
				{
					ai.initialSpeed = Vector2.Distance(agentPosition.ToXY(), landPos.ToXY()) / 1.5f;
				});
				yield return new WaitForSeconds(0.2f);
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		Actor[] array = NetworkedManagerBase<ActorManager>.instance.allActors.ToArray();
		foreach (Actor actor in array)
		{
			if (!actor.IsNullOrInactive())
			{
				if (actor is Se_MirageSkin_Delusion_Delusional)
				{
					actor.Destroy();
				}
				if (actor is Se_MirageSkin_Oblivion_Silenced)
				{
					actor.Destroy();
				}
				if (actor is ElementalStatusEffect)
				{
					actor.Destroy();
				}
			}
		}
	}

	public Mon_Primus_BossPrimusAeron()
	{
		_Mirror_SyncVarHookDelegate__003CisArmorBroken_003Ek__BackingField = OnIsArmorBrokenChanged;
		_Mirror_SyncVarHookDelegate__003Cweapon_003Ek__BackingField = OnWeaponChanged;
		_Mirror_SyncVarHookDelegate__003Cphase_003Ek__BackingField = OnPhaseChanged;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteBool(writer, isArmorBroken__BackingField);
			GeneratedNetworkCode._Write_Mon_Primus_BossPrimusAeron_002FWeaponType(writer, weapon__BackingField);
			GeneratedNetworkCode._Write_Mon_Primus_BossPrimusAeron_002FPhaseType(writer, phase__BackingField);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x200L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, isArmorBroken__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x400L) != 0L)
		{
			GeneratedNetworkCode._Write_Mon_Primus_BossPrimusAeron_002FWeaponType(writer, weapon__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x800L) != 0L)
		{
			GeneratedNetworkCode._Write_Mon_Primus_BossPrimusAeron_002FPhaseType(writer, phase__BackingField);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isArmorBroken__BackingField, _Mirror_SyncVarHookDelegate__003CisArmorBroken_003Ek__BackingField, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<WeaponType>(ref weapon__BackingField, _Mirror_SyncVarHookDelegate__003Cweapon_003Ek__BackingField, GeneratedNetworkCode._Read_Mon_Primus_BossPrimusAeron_002FWeaponType(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<PhaseType>(ref phase__BackingField, _Mirror_SyncVarHookDelegate__003Cphase_003Ek__BackingField, GeneratedNetworkCode._Read_Mon_Primus_BossPrimusAeron_002FPhaseType(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x200L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isArmorBroken__BackingField, _Mirror_SyncVarHookDelegate__003CisArmorBroken_003Ek__BackingField, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x400L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<WeaponType>(ref weapon__BackingField, _Mirror_SyncVarHookDelegate__003Cweapon_003Ek__BackingField, GeneratedNetworkCode._Read_Mon_Primus_BossPrimusAeron_002FWeaponType(reader));
		}
		if ((num & 0x800L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<PhaseType>(ref phase__BackingField, _Mirror_SyncVarHookDelegate__003Cphase_003Ek__BackingField, GeneratedNetworkCode._Read_Mon_Primus_BossPrimusAeron_002FPhaseType(reader));
		}
	}
}
