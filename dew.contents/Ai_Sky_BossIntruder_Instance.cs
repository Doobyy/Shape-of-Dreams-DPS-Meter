using System;
using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Sky_BossIntruder_Instance : AbilityInstance
{
	public Knockback knockback;

	public GameObject fxBlackhole;

	public GameObject fxEnd;

	public GameObject fxStrongAttract;

	public float strongestAtrractionDelay;

	public float radius;

	public float blackholeDuration;

	public float atrractionDelay;

	public float tickInterval;

	public AnimationCurve attractStrengthMulOverLifetime;

	[SyncVar]
	private float _blackholeStartNetworkTime;

	[SyncVar]
	private bool _isStartAtrraction;

	private float _lastTickTime;

	private bool _isEveryoneInBlackhole;

	private bool _isRemainStrongAttract = true;

	public float Network_blackholeStartNetworkTime
	{
		get
		{
			return _blackholeStartNetworkTime;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _blackholeStartNetworkTime, 64uL, (Action<float, float>)null);
		}
	}

	public bool Network_isStartAtrraction
	{
		get
		{
			return _isStartAtrraction;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref _isStartAtrraction, 128uL, (Action<bool, bool>)null);
		}
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		FxPlayNetworked(fxBlackhole);
		ListReturnHandle<Entity> handle;
		foreach (Entity item in DewPhysics.OverlapCircleAllEntities(out handle, position, 6f))
		{
			if (!item.IsNullInactiveDeadOrKnockedOut() && !(item is Monster))
			{
				knockback.ApplyWithOrigin(position, item);
			}
		}
		handle.Return();
		yield return new SI.WaitForSeconds(atrractionDelay);
		Network_isStartAtrraction = true;
		Network_blackholeStartNetworkTime = (float)NetworkTime.time;
		FxPlayNetworked(fxStrongAttract, position, Quaternion.identity);
		yield return new SI.WaitForCondition(() => _isEveryoneInBlackhole);
		FxPlayNetworked(fxEnd);
		yield return new SI.WaitForSeconds(1f);
		FxStopNetworked(fxBlackhole);
		if (!NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition)
		{
			NetworkedManagerBase<ZoneManager>.instance.LoadSidetrackRoom("Room_Special_TheConsortOfNight_Boss");
		}
		Destroy();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxBlackhole);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!_isStartAtrraction)
		{
			return;
		}
		float time = Mathf.Clamp01((float)(NetworkTime.time - (double)_blackholeStartNetworkTime) / blackholeDuration);
		foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
		{
			if (!((UnityEngine.Object)(object)allEntity == (UnityEngine.Object)(object)info.caster) && !(allEntity is Monster) && !allEntity.IsNullInactiveDeadOrKnockedOut() && allEntity.Control.isLocalMovementProcessor)
			{
				float num = attractStrengthMulOverLifetime.Evaluate(time);
				allEntity.Control.SetAgentPosition(allEntity.agentPosition + (position - allEntity.agentPosition).normalized * (num * dt));
			}
		}
		if (!((NetworkBehaviour)this).isServer || !(Time.time - _lastTickTime > tickInterval))
		{
			return;
		}
		_lastTickTime = Time.time;
		int aliveHeroCount = Dew.GetAliveHeroCount();
		int num2 = 0;
		ListReturnHandle<Entity> handle;
		foreach (Entity item in DewPhysics.OverlapCircleAllEntities(out handle, position, radius))
		{
			if (!((UnityEngine.Object)(object)item == (UnityEngine.Object)(object)info.caster) && !(item is Monster) && !item.IsNullInactiveDeadOrKnockedOut() && item is Hero)
			{
				num2++;
			}
		}
		handle.Return();
		if (num2 >= aliveHeroCount)
		{
			_isEveryoneInBlackhole = true;
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
			NetworkWriterExtensions.WriteFloat(writer, _blackholeStartNetworkTime);
			NetworkWriterExtensions.WriteBool(writer, _isStartAtrraction);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _blackholeStartNetworkTime);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, _isStartAtrraction);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _blackholeStartNetworkTime, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isStartAtrraction, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _blackholeStartNetworkTime, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isStartAtrraction, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
