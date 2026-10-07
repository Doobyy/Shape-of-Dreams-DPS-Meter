using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Mon_Ink_BossWhiteNight : BossMonster, IPrewarmMonsterContributor
{
	public float firstPhaseHealthThreshold = 0.65f;

	public float secondPhaseHealthThreshold = 0.35f;

	[Space(15f)]
	public GameObject fxDuoDeath;

	public float cutsceneDuration = 5f;

	[Space(10f)]
	public int onDeathProjectileCount = 10;

	[NonSerialized]
	public GameObject vfxObject;

	[NonSerialized]
	public List<Mon_Ink_BossWhiteNightHallucination> _Hallucinations;

	internal Mon_Ink_BossDarkMoon _bossDarkMoon;

	[SyncVar]
	internal bool _isSolo;

	[SyncVar]
	internal bool _isRage;

	internal bool _isNextSpecialAttackDarkMoonMain;

	private bool _canSpawnDarkMoon = true;

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

	public void ContributeMonsterPrewarm(Dictionary<Monster, int> counts, int instanceCount)
	{
		if (Ge_Shrine_Anitya.IsDoubleSpawn())
		{
			Mon_Ink_BossDarkMoon byType = DewResources.GetByType<Mon_Ink_BossDarkMoon>(default(ResourceLoadSettings));
			if (!((UnityEngine.Object)(object)byType == null))
			{
				counts.TryGetValue(byType, out var value);
				counts[byType] = value + instanceCount;
			}
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		Visual.ClientEvent_OnRendererEnabledChanged += new Action<bool>(OnRendererEnableChanged);
		ClientEntityEvent_OnStatusEffectAdded += new Action<EventInfoStatusEffect>(OnStatusEffectAdded);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		Network_isRage = false;
		_canSpawnDarkMoon = true;
		canUseSpecialAttack = false;
		_isNextSpecialAttackDarkMoonMain = false;
		_bossDarkMoon = null;
		firstPhaseHealthThreshold = 0.65f;
		secondPhaseHealthThreshold = 0.35f;
		_Hallucinations = new List<Mon_Ink_BossWhiteNightHallucination>();
		for (int i = 0; i < 3; i++)
		{
			Mon_Ink_BossWhiteNightHallucination item = SpawnEntity(Vector3.zero, null, DewPlayer.creep, level, (Mon_Ink_BossWhiteNightHallucination b) =>
			{
				b.Visual.DisableRenderers();
			});
			_Hallucinations.Add(item);
		}
		Network_isSolo = !Ge_Shrine_Anitya.IsDoubleSpawn();
		if (!_isSolo)
		{
			firstPhaseHealthThreshold = 0.5f;
			secondPhaseHealthThreshold = 0.4f;
		}
		CreateStatusEffect(this, new CastInfo(this), (Se_Mon_Ink_BossDeathInterrupt b) =>
		{
		});
		EntityEvent_OnDeath += new Action<EventInfoKill>(EntityEventOnDeath);
		EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(EntityEventOnTakeDamage);
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			CreateBasicEffect(this, new UnstoppableEffect(), float.PositiveInfinity);
			float num = Visual.spawnDuration + Visual.dazeAfterSpawnDuration;
			Control.StartDaze(num + 3f);
			yield return new WaitForSeconds(num);
			CreateAbilityInstance<Ai_Mon_Ink_BossWhiteNight_DestructionWave>(position, null, new CastInfo(this, Dew.SelectRandomAliveHero(fallbackToDead: true, skipStealthed: true)));
		}
	}

	public override void OnModelLoaded()
	{
		base.OnModelLoaded();
		vfxObject = Visual.model.GetCustomMapping<GameObject>("vfxObject");
		Visual.model.fxDeath = fxDuoDeath;
	}

	private void OnStatusEffectAdded(EventInfoStatusEffect obj)
	{
		if (!(obj.effect is Se_Mon_Ink_Boss_Rage) || _isRage)
		{
			return;
		}
		Animation.model.walkAnimationSpeed = 1.8f;
		if (((NetworkBehaviour)this).isServer)
		{
			Network_isRage = true;
			if (normalizedHealth < 0.35f)
			{
				Status.SetHealth(maxHealth * 0.35f);
			}
			if (Ability.TryGetAbility<At_Mon_Ink_BossWhiteNight_OneInchPunch>(out var trigger))
			{
				trigger.configs[0].maxCharges = 2;
				trigger.configs[0].addedCharges = 2;
				ResetCooldown(trigger);
			}
			if (Ability.TryGetAbility<At_Mon_Ink_BossWhiteNight_EnergyWave>(out var trigger2))
			{
				trigger2.configs[0].maxCharges = 2;
				trigger2.configs[0].addedCharges = 2;
				ResetCooldown(trigger);
			}
		}
	}

	private void EntityEventOnTakeDamage(EventInfoDamage obj)
	{
		if (Status.HasStatusEffect<Se_Mon_Ink_Boss_PhaseChange>())
		{
			return;
		}
		if (_isSolo)
		{
			if (!canUseSpecialAttack && normalizedHealth < firstPhaseHealthThreshold)
			{
				StartSpecialAttackPhase();
				Dew.CallDelayed(() =>
				{
					Status.SetHealth(firstPhaseHealthThreshold * maxHealth);
				});
			}
			else if (!_isRage && normalizedHealth < secondPhaseHealthThreshold)
			{
				CreateStatusEffect(this, new CastInfo(this), (Se_Mon_Ink_Boss_PhaseChange b) =>
				{
					b.enableRageMode = true;
				});
				Network_isRage = true;
				Dew.CallDelayed(() =>
				{
					Status.SetHealth(secondPhaseHealthThreshold * maxHealth);
				});
			}
		}
		else if (_canSpawnDarkMoon && normalizedHealth < firstPhaseHealthThreshold)
		{
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		else if (!_canSpawnDarkMoon && !canUseSpecialAttack && normalizedHealth < secondPhaseHealthThreshold)
		{
			StartSpecialAttackPhase();
		}
		IEnumerator Routine()
		{
			_canSpawnDarkMoon = false;
			CreateStatusEffect(this, new CastInfo(this), (Se_Mon_Ink_Boss_PhaseChange b) =>
			{
				b.staggerDuration = cutsceneDuration;
			});
			Dew.CallDelayed(() =>
			{
				Status.SetHealth(firstPhaseHealthThreshold * maxHealth);
			});
			yield return new WaitForSeconds(3f);
			_bossDarkMoon = SpawnEntity(SingletonBehaviour<Ink_BossTeleportPosition>.instance.transform.position, Quaternion.AngleAxis(ManagerBase<CameraManager>.instance.entityCamAngle + 180f, Vector3.up), DewPlayer.creep, level, (Mon_Ink_BossDarkMoon b) =>
			{
				b.Network_isSolo = false;
				b.Visual.spawnDuration = 6f;
				b._bossWhiteNight = this;
				b.Visual.ClientEvent_OnModelLoaded += (Action)(() =>
				{
					b.Visual.model.fxDeath = fxDuoDeath;
				});
			});
			_bossDarkMoon.EntityEvent_OnDeath += new Action<EventInfoKill>(OnDarkMoonDeath);
			_bossDarkMoon.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(OnDarkMoonTakeDamage);
		}
	}

	private void EntityEventOnDeath(EventInfoKill obj)
	{
		if (_isSolo)
		{
			FxPlayNetworked(fxDuoDeath, this);
		}
		if (!_bossDarkMoon.IsNullInactiveDeadOrKnockedOut() && !_bossDarkMoon._isRage && !_bossDarkMoon._isSolo)
		{
			CreateStatusEffect(_bossDarkMoon, new CastInfo(_bossDarkMoon), (Se_Mon_Ink_Boss_PhaseChange b) =>
			{
				b.enableRageMode = true;
			});
			if (!_bossDarkMoon.canUseSpecialAttack)
			{
				_bossDarkMoon.StartSpecialAttackPhase();
			}
			_bossDarkMoon._bossWhiteNight = null;
			_bossDarkMoon.AI.disableAI = false;
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			for (int i = 0; i < onDeathProjectileCount; i++)
			{
				CreateAbilityInstance<Ai_Mon_Ink_Boss_OnDeath>(position, null, new CastInfo(this, _bossDarkMoon));
				yield return new WaitForSeconds(0.2f);
			}
			_bossDarkMoon.EntityEvent_OnDeath -= new Action<EventInfoKill>(OnDarkMoonDeath);
			_bossDarkMoon.EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(OnDarkMoonTakeDamage);
		}
	}

	private void OnDarkMoonTakeDamage(EventInfoDamage obj)
	{
		if (!_bossDarkMoon.IsNullInactiveDeadOrKnockedOut() && !canUseSpecialAttack && _bossDarkMoon.normalizedHealth < secondPhaseHealthThreshold)
		{
			StartSpecialAttackPhase();
			_isNextSpecialAttackDarkMoonMain = true;
		}
	}

	private void OnDarkMoonDeath(EventInfoKill obj)
	{
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		CreateStatusEffect(this, new CastInfo(this), (Se_Mon_Ink_Boss_PhaseChange b) =>
		{
			b.enableRageMode = true;
		});
		canUseSpecialAttack = true;
		_isNextSpecialAttackDarkMoonMain = false;
		_bossDarkMoon = null;
		IEnumerator Routine()
		{
			for (int i = 0; i < onDeathProjectileCount; i++)
			{
				CreateAbilityInstance<Ai_Mon_Ink_Boss_OnDeath>(obj.victim.position, null, new CastInfo(obj.victim, this));
				yield return new WaitForSeconds(0.2f);
			}
		}
	}

	protected override bool PoolAIUpdate(ref EntityAIContext context, PoolAbilityInfo abilityInfo)
	{
		if ((UnityEngine.Object)(object)context.targetEnemy == null)
		{
			return false;
		}
		AbilityTrigger triggerInstance = abilityInfo.triggerInstance;
		if (!(triggerInstance is At_Mon_Special_BossMaw_Catacylsm))
		{
			if (triggerInstance is At_Mon_Ink_BossWhiteNight_RageInstance_RockSpawner)
			{
				if (_isRage && AI.Helper_CanBeCast<At_Mon_Ink_BossWhiteNight_RageInstance_RockSpawner>())
				{
					AI.Helper_CastAbilityAuto<At_Mon_Ink_BossWhiteNight_RageInstance_RockSpawner>();
					return true;
				}
				return false;
			}
			return base.PoolAIUpdate(ref context, abilityInfo);
		}
		if (canUseSpecialAttack && AI.Helper_CanBeCast<At_Mon_Ink_BossWhiteNight_Cataclysm>())
		{
			AI.Helper_CastAbilityAuto<At_Mon_Ink_BossWhiteNight_Cataclysm>();
			if (!_bossDarkMoon.IsNullOrInactive())
			{
				_bossDarkMoon.AI.disableAI = true;
				_isNextSpecialAttackDarkMoonMain = !_isNextSpecialAttackDarkMoonMain;
			}
			return true;
		}
		return false;
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if (!((UnityEngine.Object)(object)context.targetEnemy == null))
		{
			if (Status.HasStatusEffect<Se_Mon_Ink_BossWhiteNight_Cataclysm>())
			{
				Control.Attack(null, doChase: false);
			}
			else
			{
				AI.Helper_ChaseTarget();
			}
		}
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

	public void StartSpecialAttackPhase()
	{
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			CreateStatusEffect<Se_Mon_Ink_Boss_PhaseChange>(this, new CastInfo(this));
			if (!_isSolo && !_bossDarkMoon.IsNullInactiveDeadOrKnockedOut())
			{
				CreateStatusEffect<Se_Mon_Ink_Boss_PhaseChange>(_bossDarkMoon, new CastInfo(_bossDarkMoon));
				Dew.CallDelayed(() =>
				{
					if (normalizedHealth < secondPhaseHealthThreshold)
					{
						Status.SetHealth(secondPhaseHealthThreshold * maxHealth);
					}
					if (_bossDarkMoon.normalizedHealth < secondPhaseHealthThreshold)
					{
						_bossDarkMoon.Status.SetHealth(secondPhaseHealthThreshold * maxHealth);
					}
				});
			}
			canUseSpecialAttack = true;
			CreateStatusEffect<Se_Mon_Ink_BossWhiteNight_Cataclysm>(this, new CastInfo(this));
			if (Ability.TryGetAbility<At_Mon_Ink_BossWhiteNight_Cataclysm>(out var trigger))
			{
				trigger.configs[0].maxCharges = 1;
				ResetCooldown(trigger);
			}
			if (!_isSolo && !_bossDarkMoon.IsNullOrInactive() && !_canSpawnDarkMoon)
			{
				_bossDarkMoon.AI.disableAI = true;
				CreateStatusEffect<Se_Mon_Ink_BossWhiteNight_Cataclysm>(_bossDarkMoon, new CastInfo(_bossDarkMoon)).DestroyOnDeath(this);
				yield return new WaitForSeconds(3f);
				if (Status.TryGetStatusEffect<Se_Mon_Ink_BossDeathInterrupt>(out var effect))
				{
					effect.Destroy();
				}
				if (_bossDarkMoon.Status.TryGetStatusEffect<Se_Mon_Ink_BossDeathInterrupt>(out var effect2))
				{
					effect2.Destroy();
				}
			}
		}
	}

	public override Type GetUniqueReward()
	{
		return typeof(St_U_BeamOfBalance);
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteBool(writer, _isSolo);
			NetworkWriterExtensions.WriteBool(writer, _isRage);
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
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isSolo, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isRage, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
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
	}
}
