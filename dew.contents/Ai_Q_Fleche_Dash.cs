using System;
using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Q_Fleche_Dash : AbilityInstance
{
	public GameObject dashEffect;

	public GameObject lightningStartEffect;

	public GameObject lightningDashEffect;

	public GameObject lightningHitEffect;

	public float channelMinDist;

	public float channelDuration;

	public DewAnimationClip channelAnim;

	public DewAnimationClip dashAnim;

	public DewAnimationClip hitAnim;

	public GameObject landEffect;

	public GameObject hitEffectOnTarget;

	public GameObject fxMaxRangeExplosion;

	public DewCollider maxRangeExplosionRange;

	public bool doAttackEffects;

	public bool resetAttackCooldown;

	public bool doChase;

	public ScalingValue damage;

	public float speed;

	public float goalDistance;

	public float maxTime;

	public float unstoppableTime;

	public float speedAmount;

	public float speedDuration;

	public bool isSpeedDecay;

	public float trackerAttachTime;

	[NonSerialized]
	[SyncVar]
	public bool empoweredWithLightning;

	[NonSerialized]
	[SyncVar]
	public bool explodeIfMaxRange;

	[NonSerialized]
	[SyncVar]
	public float explodeDamageAmp;

	private ActorRef<Se_Q_Fleche_VictimTracker> _tracker;

	private Vector3 _startPos;

	private bool _forceExplode;

	private float _range;

	public override bool reuseInRoom => true;

	public bool NetworkempoweredWithLightning
	{
		get
		{
			return empoweredWithLightning;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref empoweredWithLightning, 64uL, (Action<bool, bool>)null);
		}
	}

	public bool NetworkexplodeIfMaxRange
	{
		get
		{
			return explodeIfMaxRange;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref explodeIfMaxRange, 128uL, (Action<bool, bool>)null);
		}
	}

	public float NetworkexplodeDamageAmp
	{
		get
		{
			return explodeDamageAmp;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref explodeDamageAmp, 256uL, (Action<float, float>)null);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		NetworkempoweredWithLightning = false;
		NetworkexplodeIfMaxRange = false;
		NetworkexplodeDamageAmp = 0f;
		_tracker = null;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		((Component)(object)this).transform.SetPositionAndRotation(info.caster.position, Quaternion.LookRotation(info.target.position - info.caster.position).Flattened());
		if (((NetworkBehaviour)this).isServer)
		{
			_startPos = info.caster.position;
			if (Vector3.Distance(info.caster.position, info.target.position) > channelMinDist)
			{
				info.caster.Control.StartChannel(new Channel
				{
					duration = (empoweredWithLightning ? (channelDuration * 0.5f) : channelDuration),
					blockedActions = Channel.BlockedAction.Everything,
					onCancel = Destroy,
					onComplete = DoDash
				});
				info.caster.Animation.PlayAbilityAnimation(channelAnim);
			}
			else
			{
				DoDash();
			}
			if (empoweredWithLightning)
			{
				FxPlayNetworked(lightningStartEffect, info.caster);
			}
			info.caster.Control.RotateTowards(info.target, immediately: true, 1f);
			_range = firstTrigger.configs[0].castMethod._range;
			_forceExplode = explodeIfMaxRange && Vector2.Distance(info.caster.position.ToXY(), info.target.position.ToXY()) > _range * 0.75f;
		}
	}

	private void DoDash()
	{
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		if (firstTrigger is St_Q_Fleche { isActive: not false } st_Q_Fleche)
		{
			st_Q_Fleche.refundList[info.target] = Time.time;
		}
		if (resetAttackCooldown)
		{
			ResetCooldown(info.caster.Ability.attackAbility);
		}
		if (info.target.Status.TryGetStatusEffect<Se_Q_Fleche_VictimTracker>(out var effect))
		{
			effect._isBeingReplaced = true;
			effect.Destroy();
		}
		_tracker = CreateStatusEffect<Se_Q_Fleche_VictimTracker>(info.target, new CastInfo(info.caster));
		if (unstoppableTime > 0f)
		{
			CreateBasicEffect(info.caster, new UnstoppableEffect(), unstoppableTime, "fleche_unstoppable");
		}
		float goalDist = goalDistance;
		float num = Vector2.Distance(info.caster.agentPosition.ToXY(), info.target.agentPosition.ToXY()) - info.caster.Control.outerRadius - info.target.Control.outerRadius;
		if (goalDist > num)
		{
			goalDist = num;
		}
		if (goalDist < 0.1f)
		{
			goalDist = 0.1f;
		}
		if (empoweredWithLightning)
		{
			FxPlayNetworked(lightningDashEffect, info.caster);
			info.caster.Control.StartChannel(new Channel
			{
				duration = 0.02f,
				blockedActions = Channel.BlockedAction.Everything,
				onCancel = Destroy,
				onComplete = () =>
				{
					if (!info.target.IsNullInactiveDeadOrKnockedOut())
					{
						Vector3 end2 = info.target.agentPosition + (info.caster.agentPosition - info.target.agentPosition).normalized * (goalDist + info.target.Control.outerRadius + info.caster.Control.outerRadius);
						end2 = Dew.GetValidAgentDestination_Closest(info.caster.agentPosition, end2);
						Teleport(info.caster, end2);
					}
					Finish();
				}
			});
			return;
		}
		FxPlayNetworked(dashEffect, info.caster);
		info.caster.Animation.PlayAbilityAnimation(dashAnim);
		if ((int)Dew.GetNavMeshPathStatus(info.caster.agentPosition, info.target.agentPosition) == 0)
		{
			info.caster.Control.StartDisplacement(new DispByTarget
			{
				affectedByMovementSpeed = true,
				speed = speed,
				cancelTime = maxTime,
				goalDistance = goalDist,
				isCanceledByCC = false,
				isFriendly = true,
				onCancel = Destroy,
				onFinish = Finish,
				rotateForward = true,
				target = info.target
			});
		}
		else
		{
			Vector3 end = info.target.agentPosition + (info.caster.agentPosition - info.target.agentPosition).normalized * goalDist;
			end = Dew.GetValidAgentDestination_Closest(info.caster.agentPosition, end);
			info.caster.Control.StartDisplacement(new DispByDestination
			{
				affectedByMovementSpeed = true,
				duration = Vector3.Distance(end, info.caster.agentPosition) / speed,
				isCanceledByCC = false,
				isFriendly = true,
				onCancel = Destroy,
				onFinish = Finish,
				rotateForward = true,
				destination = end
			});
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		((Component)(object)this).transform.SetPositionAndRotation(info.caster.position, Quaternion.LookRotation(info.target.position - info.caster.position).Flattened());
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			info.caster.Animation.StopAbilityAnimation(dashAnim);
			FxStopNetworked(dashEffect);
		}
	}

	private void Finish()
	{
		bool flag = !info.target.IsNullInactiveDeadOrKnockedOut();
		Vector3 v = (flag ? info.target.position : ((Component)(object)this).transform.position);
		Vector3 vector = (flag ? info.target.agentPosition : ((Component)(object)this).transform.position);
		info.caster.Animation.PlayAbilityAnimation(hitAnim);
		FxPlayNetworked(landEffect);
		FxPlayNetworked(hitEffectOnTarget, info.target);
		if (empoweredWithLightning)
		{
			FxPlayNetworked(lightningHitEffect, info.target);
		}
		if (explodeIfMaxRange && !_forceExplode && Vector2.Distance(_startPos.ToXY(), v.ToXY()) > _range * 0.75f)
		{
			_forceExplode = true;
		}
		DamageData damageData = Damage(damage).SetOriginPosition(info.caster.position).DoAttackEffect(AttackEffectType.Others, doAttackEffects ? 1f : 0f);
		if (empoweredWithLightning)
		{
			damageData.SetElemental(ElementalType.Light);
		}
		if (_forceExplode)
		{
			damageData.SetAttr(DamageAttribute.IsCrit);
			damageData.ApplyAmplification(explodeDamageAmp);
		}
		damageData.Dispatch(info.target);
		if (explodeIfMaxRange && (_forceExplode || Vector2.Distance(_startPos.ToXY(), v.ToXY()) > _range * 0.75f))
		{
			maxRangeExplosionRange.transform.position = vector;
			ListReturnHandle<Entity> handle;
			foreach (Entity entity in maxRangeExplosionRange.GetEntities(out handle, tvDefaultHarmfulEffectTargets))
			{
				if (!((UnityEngine.Object)(object)entity == (UnityEngine.Object)(object)info.target))
				{
					DamageData damageData2 = Damage(damage).SetOriginPosition(info.caster.position).DoAttackEffect(AttackEffectType.Others, doAttackEffects ? 1f : 0f);
					if (empoweredWithLightning)
					{
						damageData2.SetElemental(ElementalType.Light);
					}
					if (_forceExplode)
					{
						damageData2.SetAttr(DamageAttribute.IsCrit);
						damageData2.ApplyAmplification(explodeDamageAmp);
					}
					damageData2.Dispatch(entity);
				}
			}
			handle.Return();
			FxPlayNetworked(fxMaxRangeExplosion, vector + Vector3.up * 1f, null);
		}
		if (doChase && !info.target.IsNullInactiveDeadOrKnockedOut() && info.caster.Ability.attackAbility.configs[0].targetValidator.Evaluate(info.caster, info.target))
		{
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		if (speedAmount > 0f && speedDuration > 0f)
		{
			CreateBasicEffect(info.caster, new SpeedEffect
			{
				decay = isSpeedDecay,
				strength = speedAmount
			}, speedDuration, "fleche_speed");
		}
		if (firstTrigger is St_Q_Fleche { isActive: not false } st_Q_Fleche && st_Q_Fleche.refundList.ContainsKey(info.target))
		{
			st_Q_Fleche.refundList[info.target] = Time.time;
		}
		if (!_tracker.IsNullOrInactive())
		{
			_tracker.Get().SetTimer(trackerAttachTime);
		}
		Destroy();
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(info.caster.Ability.attackAbility.configs[0].channel.duration / info.caster.Status.attackSpeedMultiplier);
			if (info.caster.Ability.attackAbility is AttackTrigger attackTrigger)
			{
				attackTrigger.UpdateConfigIndexForCrit();
			}
			info.caster.Ability.attackAbility.OnCastComplete(info.caster.Ability.attackAbility.currentConfigIndex, new CastInfo(info.caster, info.target));
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
			NetworkWriterExtensions.WriteBool(writer, empoweredWithLightning);
			NetworkWriterExtensions.WriteBool(writer, explodeIfMaxRange);
			NetworkWriterExtensions.WriteFloat(writer, explodeDamageAmp);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, empoweredWithLightning);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, explodeIfMaxRange);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, explodeDamageAmp);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref empoweredWithLightning, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref explodeIfMaxRange, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref explodeDamageAmp, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref empoweredWithLightning, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref explodeIfMaxRange, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x100L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref explodeDamageAmp, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
