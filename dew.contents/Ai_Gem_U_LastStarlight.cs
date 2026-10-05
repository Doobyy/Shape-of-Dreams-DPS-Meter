using System;
using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Gem_U_LastStarlight : AbilityInstance
{
	public float attractionRadius;

	public float delay;

	public float duration;

	public float procCoefficient;

	public GameObject fxBlackholePrepare;

	public GameObject fxBlackholeStart;

	public GameObject fxBlackholeEnd;

	public GameObject fxHit;

	public float tickInterval;

	public float tickDamageRadius;

	public float attractStrength;

	public ScalingValue tickDamage;

	[SyncVar]
	private bool _isBlackholeOn;

	private float _lastTickTime;

	public override bool reuseInRoom => true;

	public bool Network_isBlackholeOn
	{
		get
		{
			return _isBlackholeOn;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref _isBlackholeOn, 64uL, (Action<bool, bool>)null);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Network_isBlackholeOn = false;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			FxPlayNetworked(fxBlackholePrepare, info.point, Quaternion.identity);
			yield return new SI.WaitForSeconds(delay);
			Network_isBlackholeOn = true;
			FxPlayNetworked(fxBlackholeStart, info.point, Quaternion.identity);
			yield return new SI.WaitForSeconds(duration);
			Network_isBlackholeOn = false;
			FxStopNetworked(fxBlackholeStart);
			FxPlayNetworked(fxBlackholeEnd, info.point, Quaternion.identity);
			Destroy();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxBlackholeStart);
			FxPlayNetworked(fxBlackholeEnd, info.point, Quaternion.identity);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!_isBlackholeOn)
		{
			return;
		}
		ListReturnHandle<Entity> handle;
		foreach (Entity item in DewPhysics.OverlapCircleAllEntities(out handle, info.point, attractionRadius, tvDefaultHarmfulEffectTargets))
		{
			if (item is Monster && !item.IsNullInactiveDeadOrKnockedOut() && item.Control.isLocalMovementProcessor && !item.Control.isDisplacing && !item.Status.hasCrowdControlImmunity)
			{
				float num = attractStrength;
				item.Control.SetAgentPosition(item.agentPosition + (position - item.agentPosition).normalized * (num * dt));
			}
		}
		handle.Return();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (gem.IsNullOrInactive() || gem.skill.IsNullOrInactive())
		{
			Destroy();
		}
		else
		{
			if (!(Time.time - _lastTickTime > tickInterval))
			{
				return;
			}
			_lastTickTime = Time.time;
			ListReturnHandle<Entity> handle2;
			foreach (Entity item2 in DewPhysics.OverlapCircleAllEntities(out handle2, info.point, tickDamageRadius, tvDefaultHarmfulEffectTargets))
			{
				if (item2 is Monster && !item2.IsNullInactiveDeadOrKnockedOut())
				{
					CreateDamage(DamageData.SourceType.Magic, tickDamage, procCoefficient).SetElemental(ElementalType.Light).SetOriginPosition(info.point).SetAttr(DamageAttribute.DamageOverTime)
						.Dispatch(item2);
					FxPlayNewNetworked(fxHit, item2);
				}
			}
			handle2.Return();
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
			NetworkWriterExtensions.WriteBool(writer, _isBlackholeOn);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, _isBlackholeOn);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isBlackholeOn, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isBlackholeOn, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
