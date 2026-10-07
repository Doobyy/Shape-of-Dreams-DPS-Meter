using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Mon_Primus_BossPrimusAeron_Adapt_Arbalest : AbilityInstance
{
	public float afterShootDelay = 0.15f;

	public float aimTime = 1.5f;

	public float aimSpeed = 360f;

	public GameObject fxTelegraph;

	public int shootCount = 4;

	public float postDaze = 0.6f;

	[SyncVar]
	private float _desiredAngle;

	private float _cv;

	private float _predictionValue;

	private Entity _target;

	private Ai_Mon_Primus_BossPrimusAeron_Adapt_Arbalest_Projectile _prefab;

	public override bool reuseInRoom => true;

	public float Network_desiredAngle
	{
		get
		{
			return _desiredAngle;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _desiredAngle, 64uL, (Action<float, float>)null);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_target = null;
		_cv = 0f;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		CreateBasicEffect(info.caster, new UnstoppableEffect(), float.PositiveInfinity).DestroyOnDestroy(this);
		DestroyOnCondition(() => ((Mon_Primus_BossPrimusAeron)info.caster).phase != Mon_Primus_BossPrimusAeron.PhaseType.Adapt);
		info.caster.Control.IncrementBlockCounters(Channel.BlockedAction.Everything);
		_prefab = DewResources.GetByType<Ai_Mon_Primus_BossPrimusAeron_Adapt_Arbalest_Projectile>(default(ResourceLoadSettings));
		if (_target.IsNullInactiveDeadOrKnockedOut())
		{
			_target = Dew.SelectBestWithScore((IList<DewPlayer>)DewPlayer.gamePlayers, (Func<DewPlayer, int, float>)((DewPlayer p, int _) =>
			{
				if (p.hero.IsNullInactiveDeadOrKnockedOut())
				{
					return -1000f;
				}
				return p.hero.Status.isUndetectableByNonAllies ? (-100f) : Vector3.Distance(info.caster.agentPosition, p.hero.GetAIAgentPosition(info.caster));
			}), 0f, (DewRandom)null).hero;
		}
		if ((UnityEngine.Object)(object)_target != null)
		{
			Network_desiredAngle = CastInfo.GetAngle(_target.GetAIAgentPosition(info.caster) - info.caster.agentPosition);
		}
		else
		{
			Network_desiredAngle = info.caster.rotation.eulerAngles.y;
		}
		for (int i = 0; i < shootCount; i++)
		{
			_predictionValue = UnityEngine.Random.Range(0.5f, 1f);
			FxPlayNetworked(fxTelegraph, info.caster);
			yield return new SI.WaitForSeconds(aimTime);
			CreateAbilityInstance<Ai_Mon_Primus_BossPrimusAeron_Adapt_Arbalest_Projectile>(info.caster.agentPosition, null, new CastInfo(info.caster, rotation.eulerAngles.y));
			FxStopNetworked(fxTelegraph);
			yield return new SI.WaitForSeconds(afterShootDelay);
		}
		yield return new SI.WaitForSeconds(postDaze);
		DestroyIfActive();
	}

	protected override void OnDestroyActor()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxTelegraph);
			if ((UnityEngine.Object)(object)info.caster != null)
			{
				info.caster.Control.DecrementBlockCounters(Channel.BlockedAction.Everything);
			}
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		float y = Mathf.SmoothDampAngle(rotation.eulerAngles.y, _desiredAngle, ref _cv, 0.1f);
		rotation = Quaternion.Euler(0f, y, 0f);
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (_target.IsNullInactiveDeadOrKnockedOut())
		{
			_target = Dew.SelectBestWithScore((IList<DewPlayer>)DewPlayer.gamePlayers, (Func<DewPlayer, int, float>)((DewPlayer p, int _) =>
			{
				if (p.hero.IsNullInactiveDeadOrKnockedOut())
				{
					return -1000f;
				}
				return p.hero.Status.isUndetectableByNonAllies ? (-100f) : Vector3.Distance(info.caster.agentPosition, p.hero.GetAIAgentPosition(info.caster));
			}), 0.2f, (DewRandom)null).hero;
		}
		if (!_target.IsNullInactiveDeadOrKnockedOut())
		{
			float target = AbilityTrigger.PredictAngle_SpeedAcceleration(info.caster, _predictionValue, _target, info.caster.agentPosition, 0f, _prefab.startInFrontDistance, _prefab.initialSpeed, _prefab.targetSpeed, _prefab.acceleration);
			float num = aimSpeed * Mathf.Lerp(3f, 1f, Vector3.Distance(_target.GetAIAgentPosition(info.caster), info.caster.agentPosition) / 5f);
			Network_desiredAngle = Mathf.MoveTowardsAngle(_desiredAngle, target, num * dt);
			info.caster.Control.Rotate(rotation, immediately: false);
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
			NetworkWriterExtensions.WriteFloat(writer, _desiredAngle);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _desiredAngle);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _desiredAngle, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _desiredAngle, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
