using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_EmbraceNewIdentity : AbilityInstance, IOtherPlayersTonedDownDisable
{
	[NonSerialized]
	[SyncVar]
	public SkillTrigger targetSkill;

	public DewAnimationClip animChannel;

	public float channelTime;

	public DewAnimationClip animChannelEnd;

	public float postDaze;

	public Transform[] onSkillTransform;

	public DewEase skillEase;

	private Vector3 _targetSkillInitPos;

	private EaseFunction _easeFunc;

	protected NetworkBehaviourSyncVar ___targetSkillNetId;

	public SkillTrigger NetworktargetSkill
	{
		get
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return ((NetworkBehaviour)this).GetSyncVarNetworkBehaviour<SkillTrigger>(___targetSkillNetId, ref targetSkill);
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter_NetworkBehaviour<SkillTrigger>(value, ref targetSkill, 64uL, (Action<SkillTrigger, SkillTrigger>)null, ref ___targetSkillNetId);
		}
	}

	private bool IsTargetUnavailable()
	{
		if (!NetworktargetSkill.IsNullOrInactive() && !((UnityEngine.Object)(object)NetworktargetSkill.owner != null))
		{
			return (UnityEngine.Object)(object)NetworktargetSkill.handOwner != null;
		}
		return true;
	}

	protected override void OnCreate()
	{
		_targetSkillInitPos = (((UnityEngine.Object)(object)NetworktargetSkill != null) ? NetworktargetSkill.position : default(Vector3));
		_easeFunc = EasingFunction.GetEasingFunction(skillEase);
		UpdatePositions();
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (IsTargetUnavailable())
		{
			Destroy();
			return;
		}
		info.caster.Animation.PlayAbilityAnimation(animChannel);
		info.caster.Control.RotateTowards(NetworktargetSkill.position, immediately: true);
		info.caster.Control.CancelOngoingChannels();
		info.caster.Control.CancelOngoingDisplacement();
		CreateBasicEffect(info.caster, new InvulnerableEffect(), channelTime + postDaze);
		CreateBasicEffect(info.caster, new UntargetableEffect(), channelTime + postDaze);
		CreateBasicEffect(info.caster, new InvisibleEffect
		{
			ignoreReveal = true
		}, channelTime + postDaze);
		info.caster.Control.StartChannel(new Channel
		{
			blockedActions = Channel.BlockedAction.Everything,
			duration = channelTime,
			onCancel = DestroyIfActive,
			onComplete = () =>
			{
				if (IsTargetUnavailable())
				{
					DestroyIfActive();
				}
				else
				{
					Hero hero = (Hero)info.caster;
					if ((UnityEngine.Object)(object)hero.Skill.Identity != null)
					{
						SkillTrigger skillTrigger = hero.Skill.UnequipSkill(HeroSkillLocation.Identity, hero.position, ignoreCanReplace: true);
						skillTrigger._lastDismantler = hero;
						skillTrigger.DismantleSkill();
					}
					hero.Skill.EquipSkill(HeroSkillLocation.Identity, NetworktargetSkill, ignoreCanReplace: true);
					hero.Skill.RpcInvokeOnSkillPickup(NetworktargetSkill);
					info.caster.Animation.PlayAbilityAnimation(animChannelEnd);
					info.caster.Control.StartDaze(postDaze);
					DestroyIfActive();
				}
			}
		});
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !NetworktargetSkill.IsNullOrInactive() && (UnityEngine.Object)(object)NetworktargetSkill.owner == null)
		{
			NetworktargetSkill.isAnimatingEmbraceNewIdentity = false;
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		UpdatePositions();
	}

	private void UpdatePositions()
	{
		if (!((UnityEngine.Object)(object)NetworktargetSkill != null))
		{
			return;
		}
		float v = Mathf.Clamp01((Time.time - creationTime) / channelTime);
		((Component)(object)NetworktargetSkill).transform.position = Vector3.Lerp(_targetSkillInitPos, info.caster.position, _easeFunc(0f, 1f, v));
		Transform[] array = onSkillTransform;
		foreach (Transform transform in array)
		{
			if (!(transform == null))
			{
				transform.position = NetworktargetSkill.position;
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
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)NetworktargetSkill);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)NetworktargetSkill);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<SkillTrigger>(ref targetSkill, (Action<SkillTrigger, SkillTrigger>)null, reader, ref ___targetSkillNetId);
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<SkillTrigger>(ref targetSkill, (Action<SkillTrigger, SkillTrigger>)null, reader, ref ___targetSkillNetId);
		}
	}
}
