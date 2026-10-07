using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Se_E_Harvest_OnVictim : StatusEffect
{
	[NonSerialized]
	[SyncVar(hook = "OnEffectCountChanged")]
	public int effectCount;

	[NonSerialized]
	[SyncVar]
	public float totalStrength;

	public GameObject fxExplode;

	public GameObject fxHealSelf;

	public GameObject effectPrefab;

	public Transform effectParent;

	public ScalingValue baseDamage;

	public ScalingValue healOnKill;

	public float ampPerStrength;

	public float slowDuration = 1.5f;

	public float slowAmount = 60f;

	private List<GameObject> _effects = new List<GameObject>();

	public Action<int, int> _Mirror_SyncVarHookDelegate_effectCount;

	public override bool reuseInRoom => true;

	public int NetworkeffectCount
	{
		get
		{
			return effectCount;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref effectCount, 4096uL, _Mirror_SyncVarHookDelegate_effectCount);
		}
	}

	public float NetworktotalStrength
	{
		get
		{
			return totalStrength;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref totalStrength, 8192uL, (Action<float, float>)null);
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		effectPrefab.SetActive(value: false);
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && (info.caster.IsNullInactiveDeadOrKnockedOut() || !info.caster.CheckEnemyOrNeutral(victim)))
		{
			Destroy();
		}
	}

	[Server]
	public void Explode(Actor parent)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Se_E_Harvest_OnVictim::Explode(Actor)' called when server was not active");
			return;
		}
		parentActor = parent;
		for (int i = 0; i < 1 + Mathf.RoundToInt((float)effectCount / 3f); i++)
		{
			FxPlayNewNetworked(fxExplode, victim);
		}
		DamageData damageData = Damage(baseDamage).ApplyAmplification(ampPerStrength * Mathf.Max(totalStrength - 1f, 0f)).SetOriginPosition(info.caster.agentPosition);
		if (totalStrength >= 2.99f)
		{
			damageData.SetAttr(DamageAttribute.IsCrit);
		}
		damageData.Dispatch(victim);
		NetworktotalStrength = 0f;
		NetworkeffectCount = 0;
		if (!victim.IsNullInactiveDeadOrKnockedOut())
		{
			CreateBasicEffect(victim, new SlowEffect
			{
				strength = slowAmount
			}, slowDuration);
		}
		DestroyIfActive();
		LockDestroy();
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(0.3f);
			UnlockDestroy();
			if ((UnityEngine.Object)(object)victim != null && victim.isDead && !info.caster.IsNullInactiveDeadOrKnockedOut())
			{
				Heal(healOnKill).SetCanMerge().Dispatch(info.caster);
				FxPlayNetworked(fxHealSelf, info.caster);
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		Clear();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		NetworkeffectCount = 0;
		NetworktotalStrength = 0f;
	}

	private void OnEffectCountChanged(int prev, int newVal)
	{
		if (isActive)
		{
			while (_effects.Count > effectCount)
			{
				UnityEngine.Object.Destroy(_effects[_effects.Count - 1]);
				_effects.RemoveAt(_effects.Count - 1);
			}
			while (_effects.Count < effectCount)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(effectPrefab, effectParent);
				gameObject.SetActive(value: true);
				gameObject.transform.localPosition = UnityEngine.Random.insideUnitSphere * 0.15f;
				gameObject.transform.localRotation = UnityEngine.Random.rotation;
				_effects.Add(gameObject);
			}
		}
	}

	private void Clear()
	{
		while (_effects.Count > 0)
		{
			UnityEngine.Object.Destroy(_effects[_effects.Count - 1]);
			_effects.RemoveAt(_effects.Count - 1);
		}
	}

	public Se_E_Harvest_OnVictim()
	{
		_Mirror_SyncVarHookDelegate_effectCount = OnEffectCountChanged;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteInt(writer, effectCount);
			NetworkWriterExtensions.WriteFloat(writer, totalStrength);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x1000L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, effectCount);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x2000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, totalStrength);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref effectCount, _Mirror_SyncVarHookDelegate_effectCount, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref totalStrength, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x1000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref effectCount, _Mirror_SyncVarHookDelegate_effectCount, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x2000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref totalStrength, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
