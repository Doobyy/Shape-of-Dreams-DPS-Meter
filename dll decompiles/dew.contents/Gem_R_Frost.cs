using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Gem_R_Frost : Gem
{
	private struct Ad_FrostImmunity
	{
		public Dictionary<Entity, float> applyTime;
	}

	public ScalingValue damageRatio;

	public Vector2 delay;

	public float stunDuration;

	public float perEnemyCooldown;

	public ScalingValue gainedMaxHp;

	public GameObject activateEffect;

	[NonSerialized]
	[SyncVar]
	public float gainedHp;

	private float _reducedCooldownTime;

	public float NetworkgainedHp
	{
		get
		{
			return gainedHp;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref gainedHp, 262144uL, (Action<float, float>)null);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		ClientGemEvent_OnCooldownReduced += new Action<float>(ClientGemEventOnCooldownReduced);
		ClientGemEvent_OnCooldownReducedByRatio += new Action<float>(ClientGemEventOnCooldownReducedByRatio);
	}

	private void ClientGemEventOnCooldownReduced(float obj)
	{
		_reducedCooldownTime += obj;
	}

	private void ClientGemEventOnCooldownReducedByRatio(float obj)
	{
		_reducedCooldownTime += obj * perEnemyCooldown;
	}

	public override void OnEquipGem(Hero newOwner)
	{
		base.OnEquipGem(newOwner);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (!newOwner.Status.TryGetStatusEffect<Se_Gem_R_Frost_Stat>(out var effect))
		{
			effect = newOwner.CreateStatusEffect<Se_Gem_R_Frost_Stat>(newOwner, new CastInfo(newOwner));
		}
		Dew.CallDelayed(() =>
		{
			if (!((UnityEngine.Object)(object)newOwner == null) && newOwner.Status.TryGetStatusEffect<Se_Gem_R_Frost_Stat>(out var effect2))
			{
				NetworkgainedHp = effect2.bonus.maxHealthFlat;
			}
		});
	}

	protected override void OnDealDamage(EventInfoDamage obj)
	{
		base.OnDealDamage(obj);
		if (isValid && obj.damage.elemental == ElementalType.Cold)
		{
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			obj.actor.LockDestroy();
			yield return new WaitForSeconds(UnityEngine.Random.Range(delay.x, delay.y));
			if (!isValid || !owner.CheckEnemyOrNeutral(obj.victim) || (UnityEngine.Object)(object)obj.victim == null)
			{
				obj.actor.UnlockDestroy();
			}
			else
			{
				if (obj.victim.TryGetData<Ad_FrostImmunity>(out var data))
				{
					if (data.applyTime.TryGetValue(owner, out var value) && Time.time + _reducedCooldownTime - value < perEnemyCooldown)
					{
						obj.actor.UnlockDestroy();
						yield break;
					}
					data.applyTime[owner] = Time.time + _reducedCooldownTime;
				}
				else
				{
					obj.victim.AddData(new Ad_FrostImmunity
					{
						applyTime = new Dictionary<Entity, float> { 
						{
							owner,
							Time.time + _reducedCooldownTime
						} }
					});
				}
				FxPlayNewNetworked(activateEffect, obj.victim);
				CreateBasicEffect(obj.victim, new StunEffect(), stunDuration, "frost_stun");
				float maxHealth = owner.Status.maxHealth;
				obj.actor.DefaultDamage(maxHealth * GetValue(damageRatio)).SetElemental(ElementalType.Cold).Dispatch(obj.victim);
				if (!owner.Status.TryGetStatusEffect<Se_Gem_R_Frost_Stat>(out var effect))
				{
					effect = owner.CreateStatusEffect<Se_Gem_R_Frost_Stat>(owner, new CastInfo(owner));
				}
				effect.bonus.maxHealthFlat += Mathf.RoundToInt(GetValue(gainedMaxHp));
				NetworkgainedHp = effect.bonus.maxHealthFlat;
				NotifyUse();
				obj.actor.UnlockDestroy();
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
			NetworkWriterExtensions.WriteFloat(writer, gainedHp);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, gainedHp);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref gainedHp, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref gainedHp, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
