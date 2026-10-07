using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_QR_ValiantHeart : AbilityInstance
{
	public DewCollider range;

	public ScalingValue dmgFactor;

	public Knockback knockback;

	public GameObject fxHit;

	public float damageDelay = 0.1f;

	[NonSerialized]
	public int targetLimitCount = int.MaxValue;

	[SyncVar]
	private Quaternion _rotation;

	private bool _didHit;

	public override bool reuseInRoom => true;

	public Quaternion Network_rotation
	{
		get
		{
			return _rotation;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<Quaternion>(value, ref _rotation, 64uL, (Action<Quaternion, Quaternion>)null);
		}
	}

	protected override void OnPrepare()
	{
		base.OnPrepare();
		List<Entity> targetEntities = Hero_Bismuth.GetTargetEntities(out var handle, info.caster, canBeNeutral: true, 6.5f);
		if (targetEntities.Count > 0)
		{
			Network_rotation = Quaternion.LookRotation(targetEntities[0].agentPosition - position).Flattened();
		}
		else
		{
			Network_rotation = Quaternion.LookRotation(info.caster.owner.cursorWorldPos - position).Flattened();
		}
		handle.Return();
	}

	protected override IEnumerator OnCreateSequenced()
	{
		((Component)(object)this).transform.rotation = _rotation;
		startEffectNoStop.transform.localScale = startEffectNoStop.transform.localScale.WithX((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f);
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		if (info.caster is Hero_Bismuth hero_Bismuth)
		{
			hero_Bismuth.SpendAttack();
			hero_Bismuth.book.RpcBookCast(_rotation);
		}
		yield return new SI.WaitForSeconds(damageDelay);
		List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
		{
			sortComparer = CollisionCheckSettings.DistanceFromCenter
		});
		for (int i = 0; i < Mathf.Min(targetLimitCount, entities.Count); i++)
		{
			Entity entity = entities[i];
			CreateDamage(DamageData.SourceType.Physical, dmgFactor).SetOriginPosition(info.caster.agentPosition).Dispatch(entity);
			knockback.ApplyWithOrigin(info.caster.agentPosition, entity);
			FxPlayNetworked(fxHit, entity);
			if (!_didHit)
			{
				_didHit = true;
				CreateStatusEffect<Se_QR_ValiantHeart_Speed>(info.caster);
			}
		}
		handle.Return();
		Destroy();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_didHit = false;
		targetLimitCount = int.MaxValue;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteQuaternion(writer, _rotation);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteQuaternion(writer, _rotation);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Quaternion>(ref _rotation, (Action<Quaternion, Quaternion>)null, NetworkReaderExtensions.ReadQuaternion(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Quaternion>(ref _rotation, (Action<Quaternion, Quaternion>)null, NetworkReaderExtensions.ReadQuaternion(reader));
		}
	}
}
