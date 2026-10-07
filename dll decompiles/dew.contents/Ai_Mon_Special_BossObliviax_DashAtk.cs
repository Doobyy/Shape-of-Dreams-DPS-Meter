using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossObliviax_DashAtk : AbilityInstance
{
	public ScalingValue damage = "1.5ap";

	public GameObject fxTelegraph;

	public GameObject fxDash;

	public GameObject fxHit;

	public ChannelData channel;

	public float dashDuration;

	public DewCollider range;

	public float postDelay;

	public Knockback knockback;

	[SyncVar]
	public float telegraphDuration = 1f;

	public float NetworktelegraphDuration
	{
		get
		{
			return telegraphDuration;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref telegraphDuration, 64uL, (Action<float, float>)null);
		}
	}

	protected override void OnCreate()
	{
		position = info.caster.agentPosition;
		rotation = info.rotation;
		FxApplySpeedMultiplier(fxTelegraph, 1f / telegraphDuration);
		BoxTelegraphController componentInChildren = fxTelegraph.GetComponentInChildren<BoxTelegraphController>();
		componentInChildren.width = range.size.x;
		componentInChildren.height = range.size.y;
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			info.caster.Control.Rotate(info.rotation, immediately: true);
			FxPlayNetworked(fxTelegraph, info.caster);
			channel.duration = telegraphDuration;
			channel.Get().AddOnComplete(DoDash).AddOnCancel(DestroyIfActive)
				.Dispatch(info.caster);
			CreateBasicEffect(info.caster, new UnstoppableEffect(), telegraphDuration + dashDuration, "ObliviaxUnstoppable").DestroyOnDestroy(this);
		}
	}

	private void DoDash()
	{
		FxPlayNetworked(fxDash, info.caster);
		Vector3 dest = ((Component)(object)this).transform.position + info.forward * (range.size.y - 3f);
		info.caster.Control.StartDaze(dashDuration + postDelay);
		info.caster.Control.StartDisplacement(new DispByDestination
		{
			canGoOverTerrain = false,
			destination = dest,
			isFriendly = true,
			rotateForward = true,
			affectedByMovementSpeed = false,
			isCanceledByCC = false,
			duration = dashDuration,
			ease = DewEase.EaseOutQuad,
			rotateSmoothly = false
		});
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			int iterations = 4;
			foreach (List<Entity> item in range.SweepEntitiesFromOrigin(iterations, tvDefaultHarmfulEffectTargets))
			{
				foreach (Entity item2 in item)
				{
					DefaultDamage(damage).SetDirection(info.forward).Dispatch(item2);
					knockback.distance = Vector3.Distance(item2.agentPosition, dest) + 3f;
					knockback.ApplyWithDirection(info.forward, item2);
					FxPlayNewNetworked(fxHit, item2);
				}
				yield return new WaitForSeconds(dashDuration / (float)iterations);
			}
			DestroyIfActive();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxTelegraph);
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
			NetworkWriterExtensions.WriteFloat(writer, telegraphDuration);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, telegraphDuration);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref telegraphDuration, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref telegraphDuration, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
