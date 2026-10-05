using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Mon_Ink_BossDarkMoon : BossMonster, IPrewarmMonsterContributor
{
	[Serializable]
	public class AnimEntries
	{
		public AnimationClip idle;

		public AnimationClip runForward;
	}

	public float firstPhaseHealthThreshold = 0.65f;

	public float secondPhaseHealthThreshold = 0.35f;

	[Space(15f)]
	public float hammerChance;

	public float spearChance;

	public float dashChance;

	[Space(10f)]
	public float runSpeed;

	public float runDirRefreshTime = 1f;

	public Vector2 runDuration;

	public float runCoolDownTime = 3f;

	public float maxDistance;

	public AnimationClip runClip;

	[Space(10f)]
	public float maxWhiteNightRange;

	[Space(15f)]
	public float hammerMinDis;

	[Space(10f)]
	public Transform[] swordPivots;

	public float animDuration;

	public float animAmp;

	[NonSerialized]
	public GameObject vfxObject;

	[NonSerialized]
	public GameObject fxSpearSpawn;

	[NonSerialized]
	public GameObject fxSpearDespawn;

	public AnimEntries spearAnims;

	internal Mon_Ink_BossWhiteNight _bossWhiteNight;

	internal AbilityInstance[] _swords;

	[SyncVar]
	internal bool _isSolo;

	[SyncVar]
	internal bool _isRage;

	private bool _isSwordSpawned;

	private float _baseRunSpeed;

	private float _currentRunDuration;

	private float _runStartTime;

	private float _runEndTime;

	private float _lastRunDirectionTime;

	private Vector3 _dir;

	private bool _isRunning;

	private AnimationClip _originRunClip;

	private AbilityTrigger _dashTrigger;

	[SyncVar]
	private bool _enableTempChase;

	public bool canUseSpecialAttack { get; private set; }

	public bool Network_isSolo
	{
		get
		{
			return _isSolo;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref _isSolo, 512uL, (Action<bool, bool>)null);
		}
	}

	public bool Network_isRage
	{
		get
		{
			return _isRage;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref _isRage, 1024uL, (Action<bool, bool>)null);
		}
	}

	public bool Network_enableTempChase
	{
		get
		{
			return _enableTempChase;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref _enableTempChase, 2048uL, (Action<bool, bool>)null);
		}
	}

	public void ContributeMonsterPrewarm(Dictionary<Monster, int> counts, int instanceCount)
	{
		Mon_Ink_BossDarkMoonHallucination byType = DewResources.GetByType<Mon_Ink_BossDarkMoonHallucination>(default(ResourceLoadSettings));
		if (!((UnityEngine.Object)(object)byType == null))
		{
			int num = 0;
			Ai_Mon_Ink_BossDarkMoon_Eclipse byType2 = DewResources.GetByType<Ai_Mon_Ink_BossDarkMoon_Eclipse>(default(ResourceLoadSettings));
			if ((UnityEngine.Object)(object)byType2 != null)
			{
				num += byType2.spawnCount * 2;
			}
			Ai_Mon_Ink_BossDarkMoon_Spear byType3 = DewResources.GetByType<Ai_Mon_Ink_BossDarkMoon_Spear>(default(ResourceLoadSettings));
			if ((UnityEngine.Object)(object)byType3 != null && byType3.rageSpawnInterval > 0.001f)
			{
				int num2 = Mathf.CeilToInt(byType3.rageSpawnDuration / byType3.rageSpawnInterval);
				num += num2 * 3;
			}
			Ai_Mon_Ink_BossDarkMoon_Blade_Instance byType4 = DewResources.GetByType<Ai_Mon_Ink_BossDarkMoon_Blade_Instance>(default(ResourceLoadSettings));
			if ((UnityEngine.Object)(object)byType4 != null)
			{
				num += Mathf.CeilToInt(byType4.rageSpawnChance * 10f);
			}
			counts.TryGetValue(byType, out var value);
			counts[byType] = value + num * instanceCount;
		}
	}

	public void StartSpecialAttackPhase()
	{
		canUseSpecialAttack = true;
		CreateStatusEffect<Se_Mon_Ink_BossDarkMoon_Eclipse>(this, new CastInfo(this));
		if (Ability.TryGetAbility<At_Mon_Ink_BossDarkMoon_Eclipse>(out var trigger))
		{
			trigger.configs[0].maxCharges = 1;
			ResetCooldown(trigger);
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		Visual.ClientEvent_OnRendererEnabledChanged += new Action<bool>(OnRendererEnableChanged);
		if (((NetworkBehaviour)this).isServer)
		{
			Network_isRage = false;
			_isSwordSpawned = false;
			canUseSpecialAttack = false;
			Network_enableTempChase = false;
			_bossWhiteNight = null;
			Network_isSolo = !Ge_Shrine_Anitya.IsDoubleSpawn();
			CreateStatusEffect<Se_Mon_Ink_BossDeathInterrupt>(this, new CastInfo(this));
			_ = _isSolo;
		}
		if (!_isSolo && Ability.TryGetAbility<At_Mon_Ink_BossDarkMoon_Blade>(out var trigger))
		{
			trigger.configs[0].cooldownTime = 8.5f - NetworkedManagerBase<GameManager>.instance.GetSpecialSkillChanceMultiplier() * 4.5f;
		}
		if (Ability.TryGetAbility<At_Mon_Ink_BossDarkMoon_ShortDash>(out var trigger2))
		{
			_dashTrigger = trigger2;
			if (_isSolo)
			{
				trigger2.configs[0].cooldownTime = 8f;
				trigger2.configs[0].channel.duration = 0.05f;
				trigger2.configs[0].castMethod._range = 9f;
			}
		}
		ClientEntityEvent_OnStatusEffectAdded += new Action<EventInfoStatusEffect>(OnStatusEffectAdded);
		ClientEntityEvent_OnStatusEffectRemoved += new Action<EventInfoStatusEffect>(OnStatusEffectRemoved);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		_baseRunSpeed = Control.baseAgentSpeed;
		_runEndTime = 0f;
		if (!_isSolo)
		{
			if (_bossWhiteNight.IsNullOrInactive())
			{
				foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
				{
					if (allEntity is Mon_Ink_BossWhiteNight)
					{
						_bossWhiteNight = (Mon_Ink_BossWhiteNight)allEntity;
						_bossWhiteNight.Network_isSolo = false;
					}
				}
			}
			Visual.HideGroundMarker();
		}
		EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(EntityEventOnTakeDamage);
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			if (!_isSolo)
			{
				if (!_bossWhiteNight.IsNullOrInactive())
				{
					_bossWhiteNight.Status.SetHealth(_bossWhiteNight.maxHealth * 0.72f);
				}
				Status.SetHealth(maxHealth * 0.72f);
			}
			_swords = new AbilityInstance[swordPivots.Length];
			CreateBasicEffect(this, new UnstoppableEffect(), float.PositiveInfinity);
			float seconds = Visual.spawnDuration + Visual.dazeAfterSpawnDuration;
			yield return new WaitForSeconds(seconds);
			if (Visual.isGroundMarkerHidden)
			{
				Visual.ShowGroundMarker();
			}
			Control.StartDaze(1f);
			if (currentPoolIndex == 1)
			{
				Dew.CallDelayed(ApplySecondSkillPoolEffect, 10);
				CreateAbilityInstance<Ai_Mon_Ink_BossDarkMoon_TeleportDash>(agentPosition, null, new CastInfo(this, Dew.SelectRandomAliveHero(fallbackToDead: true, skipStealthed: true)));
			}
			else if (_isSolo)
			{
				CreateAbilityInstance<Ai_Mon_Ink_BossDarkMoon_Blade>(position, null, new CastInfo(this, Dew.SelectRandomAliveHero(fallbackToDead: true, skipStealthed: true)));
			}
		}
	}

	public override void OnModelLoaded()
	{
		base.OnModelLoaded();
		vfxObject = Visual.model.GetCustomMapping<GameObject>("vfxObject");
		fxSpearSpawn = Visual.model.GetCustomMapping<GameObject>("fxSpearSpawn");
		fxSpearDespawn = Visual.model.GetCustomMapping<GameObject>("fxSpearDespawn");
		_originRunClip = Animation.model.runForwardClip;
	}

	private void OnStatusEffectAdded(EventInfoStatusEffect obj)
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (obj.effect is Se_Mon_Ink_BossWhiteNight_Cataclysm)
		{
			Network_enableTempChase = true;
		}
		if (obj.effect is Se_Mon_Ink_Boss_Rage)
		{
			Network_isRage = true;
			if (Ability.TryGetAbility<At_Mon_Ink_BossDarkMoon_Blade>(out var trigger))
			{
				trigger.configs[0].cooldownTime = 4f;
				trigger.configs[0].channel.duration = 0.4f;
				trigger.configs[0].castMethod._range = 12f;
			}
			if (Ability.TryGetAbility<At_Mon_Ink_BossDarkMoon_ShortDash>(out var trigger2))
			{
				trigger2.configs[0].cooldownTime = 8f;
				trigger2.configs[0].channel.duration = 0.05f;
				trigger2.configs[0].castMethod._range = 9f;
			}
			if (normalizedHealth < 0.35f)
			{
				Status.SetHealth(maxHealth * 0.35f);
			}
			if (Ability.TryGetAbility<At_Mon_Ink_BossDarkMoon_SpawnEgoSword>(out var trigger3))
			{
				trigger3.configs[0].maxCharges = 1;
			}
			for (int i = 0; i < swordPivots.Length; i++)
			{
				Transform transform = swordPivots[i];
				_swords[i] = CreateAbilityInstance<Ai_Mon_Ink_BossDarkMoon_SpawnEgoSword>(transform.position, rotation, new CastInfo(this));
			}
			_isSwordSpawned = true;
		}
	}

	private void OnStatusEffectRemoved(EventInfoStatusEffect obj)
	{
		if (((NetworkBehaviour)this).isServer && obj.effect is Se_Mon_Ink_BossWhiteNight_Cataclysm)
		{
			Network_enableTempChase = false;
		}
	}

	private void EntityEventOnTakeDamage(EventInfoDamage obj)
	{
		if (!_isSolo || Status.HasStatusEffect<Se_Mon_Ink_Boss_PhaseChange>())
		{
			return;
		}
		if (!canUseSpecialAttack && normalizedHealth < firstPhaseHealthThreshold)
		{
			CreateStatusEffect<Se_Mon_Ink_Boss_PhaseChange>(this, new CastInfo(this));
			Dew.CallDelayed(() =>
			{
				Status.SetHealth(firstPhaseHealthThreshold * maxHealth);
			});
			StartSpecialAttackPhase();
		}
		else if (!_isRage && normalizedHealth < secondPhaseHealthThreshold)
		{
			CreateStatusEffect(this, new CastInfo(this), (Se_Mon_Ink_Boss_PhaseChange b) =>
			{
				b.enableRageMode = true;
			});
			Dew.CallDelayed(() =>
			{
				Status.SetHealth(secondPhaseHealthThreshold * maxHealth);
			});
		}
	}

	protected override bool PoolAIUpdate(ref EntityAIContext context, PoolAbilityInfo abilityInfo)
	{
		if ((UnityEngine.Object)(object)context.targetEnemy == null)
		{
			return false;
		}
		if (Status.HasStatusEffect<Se_Mon_Ink_Boss_PhaseChange>())
		{
			return true;
		}
		AbilityTrigger triggerInstance = abilityInfo.triggerInstance;
		if (!(triggerInstance is At_Mon_Ink_BossDarkMoon_Eclipse))
		{
			if (!(triggerInstance is At_Mon_Ink_BossDarkMoon_ShortDash))
			{
				if (!(triggerInstance is At_Mon_Ink_BossDarkMoon_Spear))
				{
					if (!(triggerInstance is At_Mon_Ink_BossDarkMoon_Hammer))
					{
						if (triggerInstance is At_Mon_Ink_BossDarkMoon_Blade)
						{
							if (!_enableTempChase && !_isRage && !_isSolo && AI.Helper_CanBeCast<At_Mon_Ink_BossDarkMoon_Blade>())
							{
								AI.Helper_CastAbilityAuto<At_Mon_Ink_BossDarkMoon_Blade>();
								if (!_isSolo && !_isRage)
								{
									ResetCooldown(_dashTrigger);
								}
								return true;
							}
							return false;
						}
						return base.PoolAIUpdate(ref context, abilityInfo);
					}
					if (UnityEngine.Random.value < abilityInfo.chance * context.deltaTime && AI.Helper_IsTargetInRange<At_Mon_Ink_BossDarkMoon_Hammer>() && (context.targetEnemy.GetAIAgentPosition(this) - agentPosition).sqrMagnitude > hammerMinDis * hammerMinDis && AI.Helper_CanBeCast<At_Mon_Ink_BossDarkMoon_Hammer>())
					{
						AI.Helper_CastAbilityAuto<At_Mon_Ink_BossDarkMoon_Hammer>();
						if (!_isSolo && !_isRage)
						{
							ResetCooldown(_dashTrigger);
						}
						return true;
					}
					return false;
				}
				if (UnityEngine.Random.value < abilityInfo.chance * context.deltaTime && AI.Helper_CanBeCast<At_Mon_Ink_BossDarkMoon_Spear>())
				{
					AI.Helper_CastAbilityAuto<At_Mon_Ink_BossDarkMoon_Spear>();
					if (!_isSolo && !_isRage)
					{
						ResetCooldown(_dashTrigger);
					}
					return true;
				}
				return false;
			}
			if (!_enableTempChase && UnityEngine.Random.value < abilityInfo.chance * context.deltaTime && AI.Helper_CanBeCast<At_Mon_Ink_BossDarkMoon_ShortDash>() && AI.Helper_IsTargetInRange<At_Mon_Ink_BossDarkMoon_ShortDash>())
			{
				AI.Helper_CastAbilityAuto<At_Mon_Ink_BossDarkMoon_ShortDash>();
				return true;
			}
			return false;
		}
		if (canUseSpecialAttack && AI.Helper_CanBeCast<At_Mon_Ink_BossDarkMoon_Eclipse>())
		{
			AI.Helper_CastAbilityAuto<At_Mon_Ink_BossDarkMoon_Eclipse>();
			return true;
		}
		return false;
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if ((UnityEngine.Object)(object)context.targetEnemy == null)
		{
			return;
		}
		if (Status.HasStatusEffect<Se_Mon_Ink_BossWhiteNight_Cataclysm>() || Status.HasStatusEffect<Se_Mon_Ink_BossDarkMoon_Eclipse>())
		{
			Control.Attack(null, doChase: false);
			return;
		}
		if (!_isRage && !_isSolo && !_bossWhiteNight.IsNullInactiveDeadOrKnockedOut() && (_bossWhiteNight.position - position).sqrMagnitude > maxWhiteNightRange * maxWhiteNightRange)
		{
			if (_isRunning)
			{
				StopRunning();
			}
			Control.MoveToDestination(_bossWhiteNight.position, immediately: false);
		}
		if (_isRunning && Time.time - _runStartTime < _currentRunDuration)
		{
			if ((UnityEngine.Object)(object)context.targetEnemy == null || !context.targetEnemy.isActive || Vector2.Distance(context.targetEnemy.GetAIPosition(this).ToXY(), position.ToXY()) > maxDistance)
			{
				StopRunning();
			}
			else if (Time.time - _lastRunDirectionTime > runDirRefreshTime)
			{
				_lastRunDirectionTime = Time.time;
				_dir = GetRunFromTargetDestination(context.targetEnemy);
				Control.MoveToDestination(_dir, immediately: false);
			}
		}
		else if (!_enableTempChase && !_isRage && !_isSolo && !_isRunning && Time.time - _runEndTime > UnityEngine.Random.Range(runCoolDownTime, runCoolDownTime * 2f))
		{
			if (Control.IsActionBlocked(EntityControl.BlockableAction.Move) != EntityControl.BlockStatus.Blocked)
			{
				StartRunning();
			}
		}
		else
		{
			if (_isRunning)
			{
				StopRunning();
			}
			if (_enableTempChase || _isSolo || _isRage)
			{
				AI.Helper_ChaseTarget();
			}
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		if (((NetworkBehaviour)this).isServer && _isSwordSpawned)
		{
			for (int i = 0; i < _swords.Length; i++)
			{
				AbilityInstance obj = _swords[i];
				float num = Mathf.Sin(Time.time / animDuration * 2f * (float)Math.PI);
				Vector3 vector = Vector3.up * (num * animAmp);
				obj.position = swordPivots[i].position + vector;
				obj.rotation = rotation;
			}
		}
	}

	public void StartRunning()
	{
		Animation.model.runForwardClip = runClip;
		Animation.model.walkAnimationSpeed = 0.7f;
		Control.baseAgentSpeed = runSpeed;
		_runStartTime = Time.time;
		_currentRunDuration = UnityEngine.Random.Range(runDuration.x, runDuration.y);
		_isRunning = true;
	}

	public void StopRunning()
	{
		_isRunning = false;
		_runEndTime = Time.time;
		Animation.model.walkAnimationSpeed = 1f;
		Control.baseAgentSpeed = _baseRunSpeed;
		Animation.model.runForwardClip = _originRunClip;
		Control.RotateTowards(AI.context.targetEnemy, immediately: true);
		_runStartTime = float.NegativeInfinity;
		if ((UnityEngine.Object)(object)AI.context.targetEnemy != null)
		{
			Control.Attack(AI.context.targetEnemy, doChase: true);
		}
	}

	private Vector3 GetRunFromTargetDestination(Entity target)
	{
		if ((UnityEngine.Object)(object)target == null)
		{
			target = Dew.GetClosestAliveHero(position, fallbackToDead: true, this);
		}
		Control.RotateTowards(target, immediately: true, runDirRefreshTime + 0.5f);
		float num = 5f;
		Vector3 normalized = (agentPosition - target.GetAIAgentPosition(this)).normalized;
		Vector3 vector = Quaternion.AngleAxis(UnityEngine.Random.Range(-45f, 45f), Vector3.up) * normalized;
		Vector3 vector2 = agentPosition + vector * num;
		Vector3 validAgentDestination_LinearSweep = Dew.GetValidAgentDestination_LinearSweep(agentPosition, vector2);
		if (Vector3.SqrMagnitude(vector2) > Vector3.SqrMagnitude(validAgentDestination_LinearSweep))
		{
			validAgentDestination_LinearSweep = Dew.GetValidAgentDestination_LinearSweep(agentPosition, Vector3.Reflect(-validAgentDestination_LinearSweep, Vector3.forward));
		}
		return validAgentDestination_LinearSweep;
	}

	private void OnRendererEnableChanged(bool obj)
	{
		if ((bool)vfxObject)
		{
			if (Visual.isRendererOff)
			{
				vfxObject.SetActive(value: false);
			}
			else
			{
				vfxObject.SetActive(value: true);
			}
		}
	}

	[ClientRpc]
	private void ApplySecondSkillPoolEffect()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Mon_Ink_BossDarkMoon::ApplySecondSkillPoolEffect()", 912305585, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	public void PlaySpawnSpearEffectNetworked()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Mon_Ink_BossDarkMoon::PlaySpawnSpearEffectNetworked()", 720514391, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void PlaySpawnSpearEffect()
	{
		FxPlay(fxSpearSpawn);
		FxStop(fxSpearDespawn);
	}

	[ClientRpc]
	public void StopSpearEffectNetworked()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Mon_Ink_BossDarkMoon::StopSpearEffectNetworked()", -1420299494, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void StopSpearEffect()
	{
		FxStop(fxSpearSpawn);
		FxPlay(fxSpearDespawn);
	}

	protected override void OnDeath(EventInfoKill info)
	{
		base.OnDeath(info);
		vfxObject.SetActive(value: false);
		fxSpearSpawn.SetActive(value: false);
		fxSpearDespawn.SetActive(value: false);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		FxStop(fxSpearDespawn);
		FxStop(fxSpearSpawn);
	}

	public override Type GetUniqueReward()
	{
		return typeof(St_U_BeamOfBalance);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_ApplySecondSkillPoolEffect()
	{
		AnimEntries animEntries = spearAnims;
		Animation.ReplaceAnimationLocal(EntityAnimation.ReplaceableAnimationType.Idle, animEntries.idle);
		Animation.ReplaceAnimationLocal(EntityAnimation.ReplaceableAnimationType.RunForward, animEntries.runForward);
		PlaySpawnSpearEffect();
	}

	protected static void InvokeUserCode_ApplySecondSkillPoolEffect(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC ApplySecondSkillPoolEffect called on server.");
		}
		else
		{
			((Mon_Ink_BossDarkMoon)(object)obj).UserCode_ApplySecondSkillPoolEffect();
		}
	}

	protected void UserCode_PlaySpawnSpearEffectNetworked()
	{
		PlaySpawnSpearEffect();
	}

	protected static void InvokeUserCode_PlaySpawnSpearEffectNetworked(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC PlaySpawnSpearEffectNetworked called on server.");
		}
		else
		{
			((Mon_Ink_BossDarkMoon)(object)obj).UserCode_PlaySpawnSpearEffectNetworked();
		}
	}

	protected void UserCode_StopSpearEffectNetworked()
	{
		StopSpearEffect();
	}

	protected static void InvokeUserCode_StopSpearEffectNetworked(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC StopSpearEffectNetworked called on server.");
		}
		else
		{
			((Mon_Ink_BossDarkMoon)(object)obj).UserCode_StopSpearEffectNetworked();
		}
	}

	static Mon_Ink_BossDarkMoon()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected Obj, but got Unknown
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Mon_Ink_BossDarkMoon), "System.Void Mon_Ink_BossDarkMoon::ApplySecondSkillPoolEffect()", (RemoteCallDelegate)InvokeUserCode_ApplySecondSkillPoolEffect);
		RemoteProcedureCalls.RegisterRpc(typeof(Mon_Ink_BossDarkMoon), "System.Void Mon_Ink_BossDarkMoon::PlaySpawnSpearEffectNetworked()", (RemoteCallDelegate)InvokeUserCode_PlaySpawnSpearEffectNetworked);
		RemoteProcedureCalls.RegisterRpc(typeof(Mon_Ink_BossDarkMoon), "System.Void Mon_Ink_BossDarkMoon::StopSpearEffectNetworked()", (RemoteCallDelegate)InvokeUserCode_StopSpearEffectNetworked);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteBool(writer, _isSolo);
			NetworkWriterExtensions.WriteBool(writer, _isRage);
			NetworkWriterExtensions.WriteBool(writer, _enableTempChase);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x200L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, _isSolo);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x400L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, _isRage);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x800L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, _enableTempChase);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isSolo, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isRage, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _enableTempChase, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x200L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isSolo, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x400L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isRage, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x800L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _enableTempChase, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
