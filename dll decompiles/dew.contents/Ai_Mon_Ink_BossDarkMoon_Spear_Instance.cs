using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_BossDarkMoon_Spear_Instance : AbilityInstance
{
	public DewCollider range;

	public ScalingValue dmgFactor;

	public float distance;

	public float stunDuration;

	public float forwardOffset;

	public float dashSpeed;

	public float postDelay;

	public DewAnimationClip endClip;

	public GameObject fxHit;

	public GameObject fxHallucination;

	[Space(15f)]
	public float whiteNightRange;

	public float whiteNightChance;

	internal bool _isHallucination;

	[NonSerialized]
	[SyncVar]
	public bool disableSpearModel;

	private Mon_Ink_BossWhiteNight _whiteNight;

	private GameObject _originalStartEffectNoStop;

	public bool NetworkdisableSpearModel
	{
		get
		{
			return disableSpearModel;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref disableSpearModel, 64uL, (Action<bool, bool>)null);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_originalStartEffectNoStop = startEffectNoStop;
	}

	protected override void OnCreate()
	{
		if (disableSpearModel)
		{
			startEffectNoStop = null;
		}
		base.OnCreate();
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		if (!_isHallucination)
		{
			_whiteNight = ((Mon_Ink_BossDarkMoon)info.caster)._bossWhiteNight;
		}
		Vector3 end = info.caster.position + info.forward * (distance + forwardOffset);
		end = Dew.GetValidAgentDestination_LinearSweep(info.caster.position, end);
		float num = (info.caster.agentPosition - end).magnitude / dashSpeed;
		range.transform.position = info.caster.position;
		range.transform.rotation = info.caster.rotation;
		List<Entity> list = new List<Entity>();
		List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
		for (int i = 0; i < entities.Count; i++)
		{
			Entity ent = entities[i];
			Quaternion value = Quaternion.LookRotation(info.forward);
			FxPlayNewNetworked(fxHit, ent, ent.position, value);
			if (!ent.Status.hasCrowdControlImmunity)
			{
				list.Add(ent);
				ent.Control.StartDaze(num);
				ent.Control.StartDisplacement(new DispByDestination
				{
					affectedByMovementSpeed = false,
					canGoOverTerrain = true,
					destination = end,
					duration = num,
					ease = DewEase.EaseOutQuad,
					isCanceledByCC = false,
					isFriendly = false,
					onFinish = () =>
					{
						CreateBasicEffect(ent, new StunEffect(), stunDuration, "darkmoon_stun");
					}
				});
			}
			CreateDamage(DamageData.SourceType.Default, dmgFactor).SetDirection(info.forward).SetOriginPosition(info.caster.position).Dispatch(ent);
		}
		handle.Return();
		end = Dew.GetPositionOnGround(end - info.forward * forwardOffset);
		bool finishedDisplacement = false;
		info.caster.Control.StartDaze(num + postDelay);
		info.caster.Control.StartDisplacement(new DispByDestination
		{
			affectedByMovementSpeed = false,
			canGoOverTerrain = true,
			destination = end,
			duration = num,
			ease = DewEase.EaseOutQuad,
			isCanceledByCC = false,
			isFriendly = true,
			onCancel = DestroyIfActive,
			onFinish = () =>
			{
				finishedDisplacement = true;
			},
			rotateForward = false
		});
		if (!_isHallucination && !_whiteNight.IsNullOrInactive() && UnityEngine.Random.value <= whiteNightChance && _whiteNight.Control.IsActionBlocked(EntityControl.BlockableAction.Ability) == EntityControl.BlockStatus.Allowed && _whiteNight.Ability.GetAbility<At_Mon_Ink_BossWhiteNight_DestructionWave>().currentConfig.selfValidator.Evaluate(_whiteNight) && Vector3.Distance(end, _whiteNight.position) <= whiteNightRange && list.Count > 0)
		{
			Entity entity = list[UnityEngine.Random.Range(0, list.Count)];
			if (!entity.IsNullInactiveDeadOrKnockedOut())
			{
				_whiteNight.AI.Aggro(entity);
				At_Mon_Ink_BossWhiteNight_DestructionWave ability = _whiteNight.Ability.GetAbility<At_Mon_Ink_BossWhiteNight_DestructionWave>();
				ResetCooldown(ability);
				ability.disableRotation = true;
				_whiteNight.Control.Cast(ability, 0, new CastInfo(_whiteNight, entity), allowMoveToCast: true, skipRangeCheck: true);
			}
		}
		yield return new SI.WaitForCondition(() => finishedDisplacement);
		yield return new SI.WaitForSeconds(postDelay - 0.5f);
		info.caster.Animation.PlayAbilityAnimation(endClip);
		if (_isHallucination)
		{
			FxPlayNetworked(fxHallucination, info.caster);
		}
		yield return new SI.WaitForSeconds(1f);
		Destroy();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(startEffect);
			FxStopNetworked(startEffectNoStop);
			if (_isHallucination)
			{
				info.caster.Destroy();
			}
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_isHallucination = false;
		startEffectNoStop = _originalStartEffectNoStop;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteBool(writer, disableSpearModel);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, disableSpearModel);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref disableSpearModel, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref disableSpearModel, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
