using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Q_IncendiaryRounds_Attack : AbilityInstance
{
	public ScalingValue damage;

	public GameObject hitEffect;

	public GameObject addedFlyEffect;

	public GameObject aoeEffect;

	public DewCollider aoeRange;

	[NonSerialized]
	[SyncVar]
	public AbilityInstance attackInstance;

	[NonSerialized]
	public bool doAreaOfEffectDamage;

	private Action<EventInfoAttackHit> _cachedOnAttackHit;

	protected NetworkBehaviourSyncVar ___attackInstanceNetId;

	public override bool reuseInRoom => true;

	public AbilityInstance NetworkattackInstance
	{
		get
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return ((NetworkBehaviour)this).GetSyncVarNetworkBehaviour<AbilityInstance>(___attackInstanceNetId, ref attackInstance);
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter_NetworkBehaviour<AbilityInstance>(value, ref attackInstance, 64uL, (Action<AbilityInstance, AbilityInstance>)null, ref ___attackInstanceNetId);
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (NetworkattackInstance is Projectile projectile)
		{
			projectile.AddEffect(Projectile.AddEffectTarget.Fly, addedFlyEffect);
		}
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkattackInstance.ActorEvent_OnAttackHit += new Action<EventInfoAttackHit>(ActorEventOnAttackHit);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)NetworkattackInstance != null)
		{
			NetworkattackInstance.ActorEvent_OnAttackHit -= _cachedOnAttackHit;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		doAreaOfEffectDamage = false;
	}

	private void ActorEventOnAttackHit(EventInfoAttackHit obj)
	{
		Damage(damage).SetElemental(ElementalType.Fire).Dispatch(obj.victim);
		FxPlayNewNetworked(hitEffect, obj.victim);
		if (doAreaOfEffectDamage)
		{
			aoeRange.transform.position = obj.victim.position;
			FxPlayNewNetworked(aoeEffect, obj.victim);
			ListReturnHandle<Entity> handle;
			foreach (Entity entity in aoeRange.GetEntities(out handle, tvDefaultHarmfulEffectTargets))
			{
				if (!((UnityEngine.Object)(object)entity == (UnityEngine.Object)(object)obj.victim))
				{
					Damage(damage).SetElemental(ElementalType.Fire).Dispatch(entity);
				}
			}
			handle.Return();
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)NetworkattackInstance);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)NetworkattackInstance);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<AbilityInstance>(ref attackInstance, (Action<AbilityInstance, AbilityInstance>)null, reader, ref ___attackInstanceNetId);
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<AbilityInstance>(ref attackInstance, (Action<AbilityInstance, AbilityInstance>)null, reader, ref ___attackInstanceNetId);
		}
	}
}
