using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using TMPro;
using UnityEngine;

public class Mon_RedGiant : Monster
{
	public GameObject deathEffects;

	public TextMeshPro dpsText;

	[SyncVar]
	private float _totalDamage;

	[SyncVar]
	private float _dps;

	[SyncVar]
	private float _maxSingleDmg;

	private float _lastDamageTime;

	private Queue<(float, float)> _damages = new Queue<(float, float)>();

	public float Network_totalDamage
	{
		get
		{
			return _totalDamage;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _totalDamage, 256uL, (Action<float, float>)null);
		}
	}

	public float Network_dps
	{
		get
		{
			return _dps;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _dps, 512uL, (Action<float, float>)null);
		}
	}

	public float Network_maxSingleDmg
	{
		get
		{
			return _maxSingleDmg;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _maxSingleDmg, 1024uL, (Action<float, float>)null);
		}
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		EntityEvent_OnTakeDamage += (Action<EventInfoDamage>)((EventInfoDamage info) =>
		{
			float num = info.damage.amount + info.damage.discardedAmount;
			_lastDamageTime = Time.time;
			Network_maxSingleDmg = Mathf.Max(_maxSingleDmg, num);
			Network_totalDamage = _totalDamage + num;
			_damages.Enqueue((Time.time, num));
		});
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer)
		{
			if (Time.time - _lastDamageTime > 5f)
			{
				Network_totalDamage = 0f;
				Network_maxSingleDmg = 0f;
			}
			(float, float) tuple = default;
			while (_damages.TryPeek(ref tuple) && Time.time - tuple.Item1 > 3f)
			{
				_damages.Dequeue();
			}
			float num = 0f;
			foreach (var damage in _damages)
			{
				num += damage.Item2;
			}
			Network_dps = num / 3f;
		}
		((TMP_Text)dpsText).text = $"Total: {_totalDamage:#,##0}\r\nDPS: {_dps:#,##0}\r\nMax Single Dmg: {_maxSingleDmg:#,##0}";
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		if ((UnityEngine.Object)(object)context.targetEnemy != null)
		{
			Control.RotateTowards(context.targetEnemy.GetAIPosition(this), immediately: false);
		}
	}

	protected override void OnDeath(EventInfoKill info)
	{
		base.OnDeath(info);
		Visual.DisableRenderersLocal();
		if ((UnityEngine.Object)(object)info.actor != null && (UnityEngine.Object)(object)info.actor.firstEntity != null)
		{
			rotation = Quaternion.LookRotation(((Component)(object)this).transform.position - ((Component)(object)info.actor.firstEntity).transform.position).Flattened();
		}
		deathEffects.transform.rotation = rotation;
		deathEffects.transform.parent = null;
		deathEffects.SetActive(value: true);
		Rigidbody[] componentsInChildren = deathEffects.GetComponentsInChildren<Rigidbody>();
		foreach (Rigidbody obj in componentsInChildren)
		{
			obj.AddForce(UnityEngine.Random.Range(0f, 1f) * deathEffects.transform.forward, (ForceMode)2);
			((Component)(object)obj).transform.localScale = Vector3.one * UnityEngine.Random.Range(0.1f, 0.2f);
			((Component)(object)obj).transform.rotation = UnityEngine.Random.rotation;
		}
		UnityEngine.Object.Destroy(deathEffects, 3f);
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteFloat(writer, _totalDamage);
			NetworkWriterExtensions.WriteFloat(writer, _dps);
			NetworkWriterExtensions.WriteFloat(writer, _maxSingleDmg);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _totalDamage);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x200L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _dps);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x400L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _maxSingleDmg);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _totalDamage, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _dps, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _maxSingleDmg, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x100L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _totalDamage, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x200L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _dps, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x400L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _maxSingleDmg, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
