using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Se_D_ConvergencePoint : StatusEffect
{
	public ScalingValue damageBonus;

	public DewCollider otherTargetRange;

	public int chainedTargets;

	public bool allowMovementSkill;

	public float empowerDuration;

	public DewBeamRenderer beamPrefab;

	public GameObject hitEffect;

	public float chainedStrength = 1f;

	private StatBonus _bonus;

	[SyncVar(hook = "OnDurationChanged")]
	private float _empowerRemainingDuration;

	[SyncVar]
	private bool _shouldBeam;

	private DewBeamRenderer[] _beamRenderers;

	private readonly SyncList<Entity> _targets = new SyncList<Entity>();

	private OnScreenTimerHandle _handle;

	private AbilityTrigger _trigger;

	public Action<float, float> _Mirror_SyncVarHookDelegate__empowerRemainingDuration;

	public Hero heroVictim => victim as Hero;

	public bool isEmpowered => _empowerRemainingDuration > 0f;

	public float Network_empowerRemainingDuration
	{
		get
		{
			return _empowerRemainingDuration;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _empowerRemainingDuration, 4096uL, _Mirror_SyncVarHookDelegate__empowerRemainingDuration);
		}
	}

	public bool Network_shouldBeam
	{
		get
		{
			return _shouldBeam;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref _shouldBeam, 8192uL, (Action<bool, bool>)null);
		}
	}

	protected override void OnPrepare()
	{
		base.OnPrepare();
		for (int i = 0; i < chainedTargets + 1; i++)
		{
			_targets.Add((Entity)null);
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		_trigger = firstTrigger;
		_beamRenderers = new DewBeamRenderer[chainedTargets];
		for (int i = 0; i < chainedTargets; i++)
		{
			_beamRenderers[i] = UnityEngine.Object.Instantiate(beamPrefab, ((Component)(object)this).transform);
		}
		if (((NetworkBehaviour)this).isServer)
		{
			_bonus = new StatBonus();
			heroVictim.Status.AddStatBonus(_bonus);
			heroVictim.EntityEvent_OnAttackFiredBeforePrepare += new Action<EventInfoAttackFired>(EntityEventOnAttackFiredBeforePrepare);
			heroVictim.ClientHeroEvent_OnSkillUse += new Action<EventInfoSkillUse>(HeroEventOnSkillUse);
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		if (_targets.Count == 0)
		{
			return;
		}
		for (int i = 1; i < _targets.Count; i++)
		{
			if (_targets[i].IsNullOrInactive() || _targets[i - 1].IsNullOrInactive())
			{
				_beamRenderers[i - 1].enabled = false;
				continue;
			}
			_beamRenderers[i - 1].SetPoints(_targets[i - 1].Visual.GetCenterPosition(), _targets[i].Visual.GetCenterPosition());
			_beamRenderers[i - 1].enabled = true;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (_handle != null)
		{
			HideOnScreenTimerLocally(_handle);
			_handle = null;
		}
		if (_beamRenderers != null)
		{
			for (int i = 0; i < _beamRenderers.Length; i++)
			{
				_beamRenderers[i].enabled = false;
			}
		}
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)heroVictim == null))
		{
			victim.EntityEvent_OnAttackFiredBeforePrepare -= new Action<EventInfoAttackFired>(EntityEventOnAttackFiredBeforePrepare);
			heroVictim.ClientHeroEvent_OnSkillUse -= new Action<EventInfoSkillUse>(HeroEventOnSkillUse);
			if ((UnityEngine.Object)(object)firstTrigger != null)
			{
				firstTrigger.fillAmount = 0f;
			}
			if (_bonus != null)
			{
				heroVictim.Status.RemoveStatBonus(_bonus);
			}
		}
	}

	private void HeroEventOnSkillUse(EventInfoSkillUse obj)
	{
		for (int i = 0; i < _beamRenderers.Length; i++)
		{
			_beamRenderers[i].enabled = false;
		}
		for (int j = 0; j < _targets.Count; j++)
		{
			_targets[j] = null;
		}
		if (obj.type == HeroSkillLocation.Q || obj.type == HeroSkillLocation.W || obj.type == HeroSkillLocation.E || obj.type == HeroSkillLocation.R || (allowMovementSkill && obj.type == HeroSkillLocation.Movement))
		{
			Network_empowerRemainingDuration = empowerDuration;
			ResetCooldown(victim.Ability.attackAbility);
		}
	}

	private void OnDurationChanged(float oldVal, float newVal)
	{
		if ((UnityEngine.Object)(object)victim == null || !isActive)
		{
			return;
		}
		if (((NetworkBehaviour)victim).isOwned)
		{
			if (newVal <= 0f && _handle != null)
			{
				HideOnScreenTimerLocally(_handle);
				_handle = null;
			}
			else if (newVal > 0f && _handle == null)
			{
				_handle = ShowOnScreenTimerLocally(new OnScreenTimerHandle
				{
					fillAmountGetter = () => _empowerRemainingDuration / empowerDuration
				});
			}
		}
		if (((NetworkBehaviour)this).isServer)
		{
			if (newVal <= 0f && _bonus.attackDamageFlat > 0f)
			{
				_bonus.attackDamageFlat = 0f;
			}
			else if (newVal > 0f && _bonus.attackDamageFlat <= 0f)
			{
				_bonus.attackDamageFlat = GetValue(damageBonus);
			}
			if ((UnityEngine.Object)(object)_trigger != null)
			{
				_trigger.fillAmount = _empowerRemainingDuration / empowerDuration;
			}
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer)
		{
			Network_empowerRemainingDuration = Mathf.MoveTowards(_empowerRemainingDuration, 0f, dt);
			Network_shouldBeam = isEmpowered && (UnityEngine.Object)(object)victim.Control.attackTarget != null && victim.Ability.attackAbility.IsTargetInRange(victim.Control.attackTarget);
		}
		if (_shouldBeam)
		{
			return;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			for (int i = 0; i < _targets.Count; i++)
			{
				_targets[i] = null;
			}
		}
		DewBeamRenderer[] beamRenderers = _beamRenderers;
		for (int j = 0; j < beamRenderers.Length; j++)
		{
			beamRenderers[j].enabled = false;
		}
	}

	private void EntityEventOnAttackFiredBeforePrepare(EventInfoAttackFired obj)
	{
		if (!isEmpowered)
		{
			return;
		}
		int num = 0;
		_targets[0] = obj.info.target;
		FxPlayNewNetworked(hitEffect, obj.info.target);
		for (int i = 0; i < chainedTargets; i++)
		{
			otherTargetRange.transform.position = _targets[i].agentPosition;
			List<Entity> entities = otherTargetRange.GetEntities(out var handle, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
			{
				sortComparer = CollisionCheckSettings.DistanceFromCenter
			});
			Entity entity = null;
			foreach (Entity item in entities)
			{
				bool flag = false;
				for (int j = 0; j <= i; j++)
				{
					if ((UnityEngine.Object)(object)_targets[j] == (UnityEngine.Object)(object)item)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					entity = item;
					break;
				}
			}
			handle.Return();
			if ((UnityEngine.Object)(object)entity == null)
			{
				break;
			}
			_targets[i + 1] = entity;
			FxPlayNewNetworked(hitEffect, entity);
			Ai_Atk_YubarStardust ai_Atk_YubarStardust = (Ai_Atk_YubarStardust)obj.instance;
			ai_Atk_YubarStardust.chainStrength = chainedStrength;
			if (i + 1 == 1)
			{
				ai_Atk_YubarStardust.chainTarget0 = entity;
			}
			else if (i + 1 == 2)
			{
				ai_Atk_YubarStardust.chainTarget1 = entity;
			}
			else if (i + 1 == 3)
			{
				ai_Atk_YubarStardust.chainTarget2 = entity;
			}
			num++;
		}
		for (int k = num + 1; k < chainedTargets + 1; k++)
		{
			_targets[k] = null;
		}
	}

	public Se_D_ConvergencePoint()
	{
		((NetworkBehaviour)this).InitSyncObject((SyncObject)(object)_targets);
		_Mirror_SyncVarHookDelegate__empowerRemainingDuration = OnDurationChanged;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteFloat(writer, _empowerRemainingDuration);
			NetworkWriterExtensions.WriteBool(writer, _shouldBeam);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x1000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _empowerRemainingDuration);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x2000L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, _shouldBeam);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _empowerRemainingDuration, _Mirror_SyncVarHookDelegate__empowerRemainingDuration, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _shouldBeam, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x1000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _empowerRemainingDuration, _Mirror_SyncVarHookDelegate__empowerRemainingDuration, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x2000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _shouldBeam, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
