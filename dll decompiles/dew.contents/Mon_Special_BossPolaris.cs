using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using FIMSpace.FTail;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Mon_Special_BossPolaris : BossMonster
{
	public enum MainPhase
	{
		Holy = 0,
		Monster = 1,
		InTransition = -1
	}

	[NonSerialized]
	[SyncVar]
	public MainPhase mainPhase;

	public EntityModel monsterFormModel;

	public float adaptiveDamageMaxHp = 0.04f;

	public float adaptiveDamageCurrentHealth = 0.08f;

	public float adaptiveDamageCurrentShield = 0.2f;

	private Animator _wings;

	private TailAnimator2[] _leftWingTailAnimators;

	private TailAnimator2[] _rightWingTailAnimators;

	private Transform _wingSpinRoot;

	private float _lastImportantSpellCastTime = float.NegativeInfinity;

	public MainPhase NetworkmainPhase
	{
		get
		{
			return mainPhase;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<MainPhase>(value, ref mainPhase, 512uL, (Action<MainPhase, MainPhase>)null);
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (mainPhase != MainPhase.Holy)
		{
			NetworkmainPhase = MainPhase.Holy;
			Control.rotationSmoothTime = 0.1f;
			Control.rotateSpeed = 720f;
			RpcResetToHolyForm();
		}
		_lastImportantSpellCastTime = float.NegativeInfinity;
		EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(GrantOneTimeDecayingArmorsOnFirstHit);
		dealtDamageProcessor.Add(Processor);
		CreateBasicEffect(this, new UnstoppableEffect(), float.PositiveInfinity);
		CreateStatusEffect<Se_Mon_Special_BossPolaris_Holy_PhaseShifter>(this, new CastInfo(this));
		CreateStatusEffect<Se_Mon_Special_BossPolaris_Adaptation>(this, new CastInfo(this));
		Dew.CallDelayed(() =>
		{
			if (!this.IsNullOrInactive())
			{
				GiveShield(this, maxHealth * 0.5f, 36000f);
			}
		});
		skipBossSoulFlow = true;
		foreach (Hero allHero in NetworkedManagerBase<ActorManager>.instance.allHeroes)
		{
			allHero.EntityEvent_OnCastStart += new Action<EventInfoCast>(EntityEventOnCastStart);
		}
	}

	private void GrantOneTimeDecayingArmorsOnFirstHit(EventInfoDamage obj)
	{
		EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(GrantOneTimeDecayingArmorsOnFirstHit);
		int num = ((mainPhase == MainPhase.Holy) ? 60 : 50);
		CreateBasicEffect(this, new ArmorBoostEffect
		{
			strength = num
		}, 32f);
		CreateBasicEffect(this, new ArmorBoostEffect
		{
			strength = num
		}, 16f);
		CreateBasicEffect(this, new ArmorBoostEffect
		{
			strength = num
		}, 8f);
	}

	public void SetMonsterPhase()
	{
		RpcSetMonsterPhase();
		NetworkmainPhase = MainPhase.Monster;
		Ability.SetAttackAbility(Dew.CreateAbilityTrigger<At_Mon_Special_BossPolaris_Monster_Atk>());
		Control.rotationSmoothTime = 0.25f;
		Control.rotateSpeed = 360f;
		EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(GrantOneTimeDecayingArmorsOnFirstHit);
	}

	[ClientRpc]
	private void RpcSetMonsterPhase()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Mon_Special_BossPolaris::RpcSetMonsterPhase()", -1574050179, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcResetToHolyForm()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Mon_Special_BossPolaris::RpcResetToHolyForm()", 1338675580, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public override void OnModelLoaded()
	{
		base.OnModelLoaded();
		_wings = Visual.model.GetCustomMapping<Animator>("wings");
		if ((UnityEngine.Object)(object)_wings != null)
		{
			_wingSpinRoot = Visual.model.GetCustomMapping<Transform>("wingSpinRoot");
			GameObject customMapping = Visual.model.GetCustomMapping<GameObject>("leftWing");
			GameObject customMapping2 = Visual.model.GetCustomMapping<GameObject>("rightWing");
			if (_leftWingTailAnimators != null)
			{
				DOTween.Kill((object)_leftWingTailAnimators, false);
			}
			if (_rightWingTailAnimators != null)
			{
				DOTween.Kill((object)_rightWingTailAnimators, false);
			}
			_leftWingTailAnimators = customMapping.GetComponentsInChildren<TailAnimator2>();
			_rightWingTailAnimators = customMapping2.GetComponentsInChildren<TailAnimator2>();
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		Wings_LogicUpdate();
	}

	private void Wings_LogicUpdate()
	{
		if ((bool)(UnityEngine.Object)(object)_wings)
		{
			_wings.SetBool("isWalking", Control.isWalking || (Control.ongoingDisplacement != null && Control.ongoingDisplacement.isFriendly && Control.ongoingDisplacement.rotateForward));
		}
	}

	private Tween Wings_TweenTailAnimatorAmount(bool isRightWing, float duration, float endValue)
	{
		TailAnimator2[] list = (isRightWing ? _rightWingTailAnimators : _leftWingTailAnimators);
		return (Tween)(object)DOTween.To((DOGetter<float>)(() => list[0].TailAnimatorAmount), (DOSetter<float>)((float x) =>
		{
			TailAnimator2[] array = list;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].TailAnimatorAmount = x;
			}
		}), endValue, duration);
	}

	public void Wings_OpenWings()
	{
		if ((bool)(UnityEngine.Object)(object)_wings)
		{
			_wings.SetTrigger("Open");
			Wings_AdjustBothWingsTailAnimator();
		}
	}

	public void Wings_CloseWings()
	{
		if ((bool)(UnityEngine.Object)(object)_wings)
		{
			_wings.SetTrigger("Close");
			Wings_AdjustBothWingsTailAnimator();
		}
	}

	private void Wings_AdjustBothWingsTailAnimator()
	{
		if (_leftWingTailAnimators != null)
		{
			DOTween.Kill((object)_leftWingTailAnimators, false);
		}
		if (_rightWingTailAnimators != null)
		{
			DOTween.Kill((object)_rightWingTailAnimators, false);
		}
		TweenSettingsExtensions.Append(TweenSettingsExtensions.AppendInterval(TweenSettingsExtensions.Append(TweenSettingsExtensions.SetId<Sequence>(DOTween.Sequence(), (object)_leftWingTailAnimators), Wings_TweenTailAnimatorAmount(isRightWing: false, 0.05f, 0f)), 0.1f), Wings_TweenTailAnimatorAmount(isRightWing: false, 0.3f, 1f));
		TweenSettingsExtensions.Append(TweenSettingsExtensions.AppendInterval(TweenSettingsExtensions.Append(TweenSettingsExtensions.SetId<Sequence>(DOTween.Sequence(), (object)_rightWingTailAnimators), Wings_TweenTailAnimatorAmount(isRightWing: true, 0.05f, 0f)), 0.1f), Wings_TweenTailAnimatorAmount(isRightWing: true, 0.3f, 1f));
	}

	public void Wings_PrepareLeft()
	{
		Wings_Prepare(isRightWing: false);
	}

	public void Wings_PrepareRight()
	{
		Wings_Prepare(isRightWing: true);
	}

	private void Wings_Prepare(bool isRightWing)
	{
		if ((bool)(UnityEngine.Object)(object)_wings)
		{
			_wings.SetTrigger(isRightWing ? "PrepareRight" : "PrepareLeft");
			TailAnimator2[] array = (isRightWing ? _rightWingTailAnimators : _leftWingTailAnimators);
			if (array != null)
			{
				DOTween.Kill((object)array, false);
			}
			TweenSettingsExtensions.Append(TweenSettingsExtensions.AppendInterval(TweenSettingsExtensions.Append(TweenSettingsExtensions.SetId<Sequence>(DOTween.Sequence(), (object)array), Wings_TweenTailAnimatorAmount(isRightWing, 0.4f, 0f)), 1f), Wings_TweenTailAnimatorAmount(isRightWing, 1f, 1f));
			TweenSettingsExtensions.Append(TweenSettingsExtensions.Append(TweenSettingsExtensions.SetId<Sequence>(DOTween.Sequence(), (object)array), (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Quaternion, Vector3, QuaternionOptions>>(ShortcutExtensions.DOLocalRotate(_wingSpinRoot, new Vector3(270f, isRightWing ? 110f : (-110f), 0f), 0.75f, (RotateMode)0), (Ease)6)), (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Quaternion, Vector3, QuaternionOptions>>(ShortcutExtensions.DOLocalRotate(_wingSpinRoot, new Vector3(270f, 0f, 0f), 1.5f, (RotateMode)0), (Ease)6));
		}
	}

	public void Wings_SwingLeft()
	{
		Wings_Swing(isRightWing: false);
	}

	public void Wings_SwingRight()
	{
		Wings_Swing(isRightWing: true);
	}

	private void Wings_Swing(bool isRightWing)
	{
		if ((bool)(UnityEngine.Object)(object)_wings)
		{
			_wings.SetTrigger(isRightWing ? "SwingRight" : "SwingLeft");
			TailAnimator2[] array = (isRightWing ? _rightWingTailAnimators : _leftWingTailAnimators);
			if (array != null)
			{
				DOTween.Kill((object)array, false);
			}
			TweenSettingsExtensions.Append(TweenSettingsExtensions.AppendInterval(TweenSettingsExtensions.Append(TweenSettingsExtensions.SetId<Sequence>(DOTween.Sequence(), (object)array), Wings_TweenTailAnimatorAmount(isRightWing, 0.05f, 0f)), 0.1f), Wings_TweenTailAnimatorAmount(isRightWing, 0.3f, 1f));
			TweenSettingsExtensions.Append(TweenSettingsExtensions.Append(TweenSettingsExtensions.SetId<Sequence>(DOTween.Sequence(), (object)array), (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Quaternion, Vector3, QuaternionOptions>>(ShortcutExtensions.DOLocalRotate(_wingSpinRoot, new Vector3(270f, isRightWing ? (-70f) : 70f, 0f), 0.15f, (RotateMode)0), (Ease)6)), (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Quaternion, Vector3, QuaternionOptions>>(ShortcutExtensions.DOLocalRotate(_wingSpinRoot, new Vector3(270f, 0f, 0f), 1.35f, (RotateMode)0), (Ease)7));
		}
	}

	private void EntityEventOnCastStart(EventInfoCast obj)
	{
		float num = 0.1f;
		TriggerConfig triggerConfig = obj.trigger.configs[obj.configIndex];
		num += triggerConfig.cooldownTime * 0.05f + triggerConfig.channel.duration * 0.5f;
		if (UnityEngine.Random.value < num)
		{
			_lastImportantSpellCastTime = Time.time;
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
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer || !(UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance)
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
				if (actor is Se_Mon_Special_BossPolaris_CleansingFlame)
				{
					actor.Destroy();
				}
				if (actor is Se_MiniBoss_BloodThorn_Bleeding)
				{
					actor.Destroy();
				}
				if (actor is ElementalStatusEffect)
				{
					actor.Destroy();
				}
			}
		}
		foreach (Hero allHero in NetworkedManagerBase<ActorManager>.instance.allHeroes)
		{
			allHero.EntityEvent_OnCastStart -= new Action<EventInfoCast>(EntityEventOnCastStart);
		}
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if (!((UnityEngine.Object)(object)context.targetEnemy == null))
		{
			if (mainPhase == MainPhase.Holy)
			{
				AIUpdate_Holy(ref context);
			}
			else if (mainPhase == MainPhase.Monster)
			{
				AIUpdate_Monster(ref context);
			}
		}
	}

	public bool Holy_CanCastPhase2Abilities()
	{
		return Status.currentShield < 0.0001f;
	}

	public bool Holy_CanCastPhase3Abilities()
	{
		return normalizedHealth < 0.5f;
	}

	private void AIUpdate_Holy(ref EntityAIContext context)
	{
		if (UnityEngine.Random.value < 0.75f * context.deltaTime && AI.Helper_CanBeCast<At_Mon_Special_BossPolaris_Holy_Dash>())
		{
			AI.Helper_CastAbility<At_Mon_Special_BossPolaris_Holy_Dash>(new CastInfo(this, Holy_GetGoodDashPosition()));
			return;
		}
		if (Holy_CanCastPhase3Abilities() && AI.Helper_CanBeCast<At_Mon_Special_BossPolaris_Holy_RainFire>())
		{
			AI.Helper_CastAbilityAuto<At_Mon_Special_BossPolaris_Holy_RainFire>();
			return;
		}
		if (Holy_CanCastPhase2Abilities())
		{
			if (AI.Helper_CanBeCast<At_Mon_Special_BossPolaris_Holy_Summon>())
			{
				AI.Helper_CastAbilityAuto<At_Mon_Special_BossPolaris_Holy_Summon>();
				return;
			}
			if (UnityEngine.Random.value < 0.5f * context.deltaTime && AI.Helper_CanBeCast<At_Mon_Special_BossPolaris_Holy_CounterSpell>() && AI.Helper_IsTargetInRange<At_Mon_Special_BossPolaris_Holy_CounterSpell>())
			{
				AI.Helper_CastAbilityAuto<At_Mon_Special_BossPolaris_Holy_CounterSpell>();
				return;
			}
			if (UnityEngine.Random.value < 0.5f * context.deltaTime && AI.Helper_CanBeCast<At_Mon_Special_BossPolaris_Holy_GoldenSpear>() && AI.Helper_IsTargetInRange<At_Mon_Special_BossPolaris_Holy_GoldenSpear>())
			{
				AI.Helper_CastAbilityAuto<At_Mon_Special_BossPolaris_Holy_GoldenSpear>();
				return;
			}
			if (AI.Helper_CanBeCast<At_Mon_Special_BossPolaris_Holy_SelfCleanse>())
			{
				int num = Status.fireStack + Status.lightStack + Status.darkStack;
				if (Status.hasCold)
				{
					num += 2;
				}
				if (num > 8)
				{
					AI.Helper_CastAbilityAuto<At_Mon_Special_BossPolaris_Holy_SelfCleanse>();
					return;
				}
			}
		}
		if (UnityEngine.Random.value < 0.5f * context.deltaTime && AI.Helper_CanBeCast<At_Mon_Special_BossPolaris_Holy_Purgatory>() && AI.Helper_IsTargetInRange<At_Mon_Special_BossPolaris_Holy_Purgatory>())
		{
			AI.Helper_CastAbilityAuto<At_Mon_Special_BossPolaris_Holy_Purgatory>();
		}
		else if (UnityEngine.Random.value < 0.5f * context.deltaTime && AI.Helper_CanBeCast<At_Mon_Special_BossPolaris_Holy_ScatterFire>() && AI.Helper_IsTargetInRange<At_Mon_Special_BossPolaris_Holy_ScatterFire>())
		{
			AI.Helper_CastAbilityAuto<At_Mon_Special_BossPolaris_Holy_ScatterFire>();
		}
		else
		{
			AI.Helper_ChaseTarget();
		}
	}

	public Vector3 Holy_GetGoodDashPosition()
	{
		float num = float.NegativeInfinity;
		Vector3 result = Vector3.zero;
		for (float num2 = 0f; num2 < 360f; num2 += 25f)
		{
			Vector3 vector = SingletonBehaviour<Room_BossArena>.instance.center + Quaternion.Euler(0f, num2, 0f) * Vector3.forward * (SingletonBehaviour<Room_BossArena>.instance.radius * UnityEngine.Random.Range(0.1f, 0.7f));
			vector = agentPosition + Vector3.ClampMagnitude(vector - agentPosition, 8.5f);
			float num3 = 10f;
			List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, vector, 8f);
			num3 -= (float)list.Count;
			if (Vector2.Distance(vector.ToXY(), agentPosition.ToXY()) < 5f)
			{
				num3 -= 5f;
			}
			handle.Return();
			num3 *= UnityEngine.Random.Range(0.9f, 1.1f);
			if (num3 > num)
			{
				num = num3;
				result = vector;
			}
		}
		return result;
	}

	private void AIUpdate_Monster(ref EntityAIContext context)
	{
		bool flag = Status.HasStatusEffect<Se_Mon_Special_BossPolaris_Monster_PizzaLightning_Rage>();
		if (normalizedHealth < 0.6f && AI.Helper_CanBeCast<At_Mon_Special_BossPolaris_Monster_Scream>())
		{
			AI.Helper_CastAbilityAuto<At_Mon_Special_BossPolaris_Monster_Scream>();
		}
		else if (normalizedHealth < 0.4f && AI.Helper_CanBeCast<At_Mon_Special_BossPolaris_Monster_PizzaLightning>())
		{
			AI.Helper_CastAbilityAuto<At_Mon_Special_BossPolaris_Monster_PizzaLightning>();
		}
		else if (normalizedHealth < 0.75f && UnityEngine.Random.value < 0.75f * context.deltaTime && AI.Helper_CanBeCast<At_Mon_Special_BossPolaris_Monster_JumpStomp>() && AI.Helper_IsTargetInRange<At_Mon_Special_BossPolaris_Monster_JumpStomp>())
		{
			AI.Helper_CastAbilityAuto<At_Mon_Special_BossPolaris_Monster_JumpStomp>();
		}
		else if (normalizedHealth < 0.75f && ((UnityEngine.Random.value < 0.75f * context.deltaTime) | flag) && AI.Helper_CanBeCast<At_Mon_Special_BossPolaris_Monster_SlashDash>() && AI.Helper_IsTargetInRange<At_Mon_Special_BossPolaris_Monster_SlashDash>())
		{
			AI.Helper_CastAbilityAuto<At_Mon_Special_BossPolaris_Monster_SlashDash>();
		}
		else if (((UnityEngine.Random.value < 0.75f * context.deltaTime) | flag) && AI.Helper_CanBeCast<At_Mon_Special_BossPolaris_Monster_Dash>() && AI.Helper_IsTargetInRange<At_Mon_Special_BossPolaris_Monster_Dash>() && Vector3.Distance(context.targetEnemy.GetAIAgentPosition(this), agentPosition) > 4f)
		{
			AI.Helper_CastAbilityAuto<At_Mon_Special_BossPolaris_Monster_Dash>();
		}
		else if (UnityEngine.Random.value < 0.75f * context.deltaTime && AI.Helper_CanBeCast<At_Mon_Special_BossPolaris_Monster_Smite>() && AI.Helper_IsTargetInRange<At_Mon_Special_BossPolaris_Monster_Smite>())
		{
			AI.Helper_CastAbilityAuto<At_Mon_Special_BossPolaris_Monster_Smite>();
		}
		else if (AI.Helper_CanBeCast<At_Mon_Special_BossPolaris_Monster_CircularSwipe>() && AI.Helper_IsTargetInRange<At_Mon_Special_BossPolaris_Monster_CircularSwipe>())
		{
			AI.Helper_CastAbilityAuto<At_Mon_Special_BossPolaris_Monster_CircularSwipe>();
		}
		else
		{
			AI.Helper_ChaseTarget();
		}
	}

	private void Processor(ref DamageData data, Actor actor, Entity target)
	{
		if (CheckEnemyOrNeutral(target) && !data.IsAmountModifiedBy(this))
		{
			data.SetAmountModifiedBy(this);
			if (mainPhase == MainPhase.InTransition)
			{
				data.ApplyRawMultiplier(0f);
			}
			else if (!(actor is ElementalStatusEffect) && !(actor is Se_Mon_Special_BossPolaris_CleansingFlame) && !(actor is Mon_Special_BossPolaris))
			{
				float num = data.originalAmount / Status.abilityPower;
				data.AddFlatAmount((target.maxHealth * adaptiveDamageMaxHp + target.currentHealth * adaptiveDamageCurrentHealth + target.Status.currentShield * adaptiveDamageCurrentShield) * num);
			}
		}
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcSetMonsterPhase()
	{
		Control.baseAgentSpeed = 7.5f;
		Visual.LoadModelLocal(monsterFormModel);
	}

	protected static void InvokeUserCode_RpcSetMonsterPhase(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetMonsterPhase called on server.");
		}
		else
		{
			((Mon_Special_BossPolaris)(object)obj).UserCode_RpcSetMonsterPhase();
		}
	}

	protected void UserCode_RpcResetToHolyForm()
	{
		Control.baseAgentSpeed = 5f;
		Visual.LoadModelDefaultLocal();
	}

	protected static void InvokeUserCode_RpcResetToHolyForm(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcResetToHolyForm called on server.");
		}
		else
		{
			((Mon_Special_BossPolaris)(object)obj).UserCode_RpcResetToHolyForm();
		}
	}

	static Mon_Special_BossPolaris()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Mon_Special_BossPolaris), "System.Void Mon_Special_BossPolaris::RpcSetMonsterPhase()", (RemoteCallDelegate)InvokeUserCode_RpcSetMonsterPhase);
		RemoteProcedureCalls.RegisterRpc(typeof(Mon_Special_BossPolaris), "System.Void Mon_Special_BossPolaris::RpcResetToHolyForm()", (RemoteCallDelegate)InvokeUserCode_RpcResetToHolyForm);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			GeneratedNetworkCode._Write_Mon_Special_BossPolaris_002FMainPhase(writer, mainPhase);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x200L) != 0L)
		{
			GeneratedNetworkCode._Write_Mon_Special_BossPolaris_002FMainPhase(writer, mainPhase);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<MainPhase>(ref mainPhase, (Action<MainPhase, MainPhase>)null, GeneratedNetworkCode._Read_Mon_Special_BossPolaris_002FMainPhase(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x200L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<MainPhase>(ref mainPhase, (Action<MainPhase, MainPhase>)null, GeneratedNetworkCode._Read_Mon_Special_BossPolaris_002FMainPhase(reader));
		}
	}
}
