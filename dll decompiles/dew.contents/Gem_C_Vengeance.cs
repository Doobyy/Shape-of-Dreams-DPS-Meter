using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Gem_C_Vengeance : Gem
{
	public int shootCount = 2;

	public float shootInterval = 0.1f;

	public float empowerDuration = 5f;

	public ScalingValue ampAmount;

	public float targetRadius;

	public GameObject fxEmpowered;

	[SyncVar(hook = "OnDamageAmplifiedChanged")]
	private bool _isAmpActive;

	private float _lastHitTime;

	private WaitForSeconds _shootWait;

	public Action<bool, bool> _Mirror_SyncVarHookDelegate__isAmpActive;

	public bool Network_isAmpActive
	{
		get
		{
			return _isAmpActive;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref _isAmpActive, 262144uL, _Mirror_SyncVarHookDelegate__isAmpActive);
		}
	}

	public override void OnEquipGem(Hero newOwner)
	{
		base.OnEquipGem(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			owner.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(EntityEventOnTakeDamage);
		}
	}

	public override void OnUnequipGem(Hero oldOwner)
	{
		base.OnUnequipGem(oldOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)oldOwner != null)
			{
				oldOwner.EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(EntityEventOnTakeDamage);
			}
			Network_isAmpActive = false;
		}
	}

	public override void OnEquipSkill(SkillTrigger newSkill)
	{
		base.OnEquipSkill(newSkill);
		if (((NetworkBehaviour)this).isServer)
		{
			newSkill.dealtDamageProcessor.Add(AmplifyDamage);
			newSkill.dealtHealProcessor.Add(AmplifyHeal);
		}
	}

	public override void OnUnequipSkill(SkillTrigger oldSkill)
	{
		base.OnUnequipSkill(oldSkill);
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)oldSkill != null)
			{
				oldSkill.dealtDamageProcessor.Remove(AmplifyDamage);
				oldSkill.dealtHealProcessor.Remove(AmplifyHeal);
			}
			Network_isAmpActive = false;
		}
	}

	private void EntityEventOnTakeDamage(EventInfoDamage obj)
	{
		StartCooldown();
		Network_isAmpActive = true;
		_lastHitTime = Time.time;
		if (_shootWait == null)
		{
			_shootWait = new WaitForSeconds(shootInterval);
		}
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			for (int i = 0; i < shootCount; i++)
			{
				if (!isValid)
				{
					break;
				}
				if (!LaunchProjectile())
				{
					break;
				}
				NotifyUse();
				yield return _shootWait;
			}
		}
	}

	private bool LaunchProjectile()
	{
		if (!isValid)
		{
			return false;
		}
		if (isNotReadyByRateLimit)
		{
			return false;
		}
		List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, owner.position, targetRadius, tvDefaultHarmfulEffectTargets);
		if (list.Count > 0)
		{
			CreateAbilityInstance<Ai_Gem_C_Vengence_Projectile>(owner.position, null, new CastInfo(owner, list[UnityEngine.Random.Range(0, list.Count)]));
			handle.Return();
			return true;
		}
		handle.Return();
		return false;
	}

	private void AmplifyHeal(ref HealData data, Actor actor, Entity target)
	{
		if (isValid && _isAmpActive && !data.IsAmountModifiedBy(this))
		{
			data.SetCrit();
			data.SetAmountModifiedBy(this);
			data.ApplyAmplification(GetValue(ampAmount));
		}
	}

	private void AmplifyDamage(ref DamageData data, Actor actor, Entity target)
	{
		if (isValid && _isAmpActive && !data.IsAmountModifiedBy(this) && owner.CheckEnemyOrNeutral(target))
		{
			data.SetAttr(DamageAttribute.IsCrit);
			data.SetAmountModifiedBy(this);
			data.ApplyAmplification(GetValue(ampAmount));
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && _isAmpActive && Time.time - _lastHitTime > empowerDuration)
		{
			Network_isAmpActive = false;
		}
	}

	private void OnDamageAmplifiedChanged(bool oldVal, bool newVal)
	{
		if (newVal)
		{
			FxPlay(fxEmpowered, owner);
		}
		else
		{
			FxStop(fxEmpowered);
		}
	}

	public Gem_C_Vengeance()
	{
		_Mirror_SyncVarHookDelegate__isAmpActive = OnDamageAmplifiedChanged;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteBool(writer, _isAmpActive);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40000L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, _isAmpActive);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isAmpActive, _Mirror_SyncVarHookDelegate__isAmpActive, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isAmpActive, _Mirror_SyncVarHookDelegate__isAmpActive, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
