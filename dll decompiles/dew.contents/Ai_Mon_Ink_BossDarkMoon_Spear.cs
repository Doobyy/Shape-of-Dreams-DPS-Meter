using System;
using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_BossDarkMoon_Spear : AbilityInstance
{
	public float totalDuration;

	public float postDelay;

	public float rotStartSpeed;

	public float rotMaxSpeed;

	public GameObject fxTelegraphing;

	public GameObject fxTelegraph;

	public DewAnimationClip clip;

	[Space(15f)]
	public float rageRotSpeed;

	public float rageSpawnDuration;

	public float rageSpawnAtkDelay;

	public float rageSpawnInterval;

	public float rageAtkDeviation;

	protected Entity originEntity;

	internal bool _isHallucination;

	private bool _isRage;

	private float _spawnEndTime;

	[SyncVar]
	private Quaternion _syncedRotation;

	private float _baseRotMaxSpeed;

	public Quaternion Network_syncedRotation
	{
		get
		{
			return _syncedRotation;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<Quaternion>(value, ref _syncedRotation, 64uL, (Action<Quaternion, Quaternion>)null);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_baseRotMaxSpeed = rotMaxSpeed;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		rotMaxSpeed = _baseRotMaxSpeed;
		originEntity = null;
		_isHallucination = false;
	}

	protected override void OnPrepare()
	{
		base.OnPrepare();
		Network_syncedRotation = info.caster.rotation;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		info.caster.rotation = _syncedRotation;
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		if (info.caster is Mon_Ink_BossDarkMoon)
		{
			originEntity = info.caster;
		}
		CreateBasicEffect(info.caster, new UnstoppableEffect(), float.PositiveInfinity).DestroyOnDestroy(this);
		if (_isHallucination)
		{
			if (!originEntity.IsNullOrInactive())
			{
				DestroyOnDeath(originEntity);
			}
			DewAudioSource[] componentsInChildren = ((Component)(object)this).GetComponentsInChildren<DewAudioSource>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].volumeMultiplier = 0.7f;
			}
			yield return HallucinationRoutine();
			Destroy();
			yield break;
		}
		_isRage = ((Mon_Ink_BossDarkMoon)info.caster)._isRage;
		FxPlayNetworked(fxTelegraphing, info.caster);
		if (_isRage)
		{
			rotMaxSpeed = rageRotSpeed;
			_spawnEndTime = Time.time + rageSpawnDuration;
			((MonoBehaviour)(object)this).StartCoroutine(SpawnHallucinations());
		}
		float elapsedTime = 0f;
		info.caster.Control.StartChannel(new Channel
		{
			blockedActions = Channel.BlockedAction.Everything,
			duration = totalDuration,
			onCancel = DestroyIfActive,
			onTick = (float dt) =>
			{
				elapsedTime += dt;
				float t = Mathf.Clamp01(elapsedTime / totalDuration);
				float num = Mathf.Lerp(rotStartSpeed, rotMaxSpeed, t);
				if (info.target.IsNullInactiveDeadOrKnockedOut())
				{
					Hero closestAliveHero = Dew.GetClosestAliveHero(info.caster.position, fallbackToDead: false, info.caster);
					if ((UnityEngine.Object)(object)closestAliveHero != null)
					{
						info = new CastInfo(info.caster, closestAliveHero);
					}
				}
				if (!info.target.IsNullInactiveDeadOrKnockedOut())
				{
					Vector3 vector = AbilityTrigger.PredictPoint_Simple(info.caster, UnityEngine.Random.Range(0.5f, 1f), info.target, postDelay + dt);
					Network_syncedRotation = Quaternion.RotateTowards(_syncedRotation, Quaternion.LookRotation(vector - info.caster.agentPosition), num * dt);
				}
				info.caster.Control.Rotate(_syncedRotation, immediately: false);
			},
			onComplete = () =>
			{
				((MonoBehaviour)(object)this).StartCoroutine(Routine());
			}
		});
		IEnumerator Routine()
		{
			FxPlayNetworked(fxTelegraph, info.caster);
			info.caster.Control.StopOverrideRotation();
			info.caster.Control.StartDaze(postDelay);
			yield return new WaitForSeconds(postDelay);
			CreateAbilityInstance<Ai_Mon_Ink_BossDarkMoon_Spear_Instance>(info.caster.agentPosition, null, new CastInfo(info.caster, CastInfo.GetAngle(((Component)(object)info.caster).transform.forward)));
			info.caster.Animation.PlayAbilityAnimation(clip);
			Destroy();
		}
	}

	private IEnumerator HallucinationRoutine()
	{
		FxApplySpeedMultiplierNetworked(fxTelegraphing, 2f);
		FxPlayNetworked(fxTelegraphing, info.caster);
		yield return new SI.WaitForSeconds(rageSpawnAtkDelay);
		FxPlayNetworked(fxTelegraph, info.caster);
		yield return new SI.WaitForSeconds(postDelay);
		CreateAbilityInstance(info.caster.agentPosition, info.caster.rotation, new CastInfo(info.caster, CastInfo.GetAngle(((Component)(object)info.caster).transform.forward)), (Ai_Mon_Ink_BossDarkMoon_Spear_Instance b) =>
		{
			b._isHallucination = true;
		});
		info.caster.Animation.PlayAbilityAnimation(clip);
		yield return null;
	}

	private IEnumerator SpawnHallucinations()
	{
		yield return new WaitForSeconds(0.5f);
		while (_spawnEndTime - Time.time > 0.001f)
		{
			Entity entity = info.target;
			if (entity.IsNullInactiveDeadOrKnockedOut())
			{
				entity = Dew.GetClosestAliveHero(info.caster.position, fallbackToDead: true, info.caster);
			}
			Vector3 vector = AbilityTrigger.PredictPoint_Simple(info.caster, UnityEngine.Random.Range(0.8f, 1f), entity, rageSpawnAtkDelay) + UnityEngine.Random.insideUnitCircle.ToXZ() * UnityEngine.Random.Range(0f, rageAtkDeviation);
			Vector3 vector2 = UnityEngine.Random.insideUnitCircle.ToXZ();
			Vector3 vector3 = vector + vector2 * UnityEngine.Random.Range(7f, 15f);
			vector3 = Dew.GetPositionOnGround(vector3);
			Quaternion value = Quaternion.LookRotation(vector - vector3);
			Mon_Ink_BossDarkMoonHallucination caster = SpawnEntity(vector3, value, DewPlayer.creep, info.caster.level, (Mon_Ink_BossDarkMoonHallucination b) =>
			{
				b.Network_isSpecialAtk = true;
			});
			CreateAbilityInstance(vector3, value, new CastInfo(caster), (Ai_Mon_Ink_BossDarkMoon_Spear b) =>
			{
				b._isHallucination = true;
				b.originEntity = originEntity;
			});
			yield return new WaitForSeconds(rageSpawnInterval);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			((MonoBehaviour)(object)this).StopCoroutine("SpawnHallucinations");
			FxStopNetworked(fxTelegraphing);
			FxStopNetworked(startEffectNoStop);
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
			NetworkWriterExtensions.WriteQuaternion(writer, _syncedRotation);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteQuaternion(writer, _syncedRotation);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Quaternion>(ref _syncedRotation, (Action<Quaternion, Quaternion>)null, NetworkReaderExtensions.ReadQuaternion(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Quaternion>(ref _syncedRotation, (Action<Quaternion, Quaternion>)null, NetworkReaderExtensions.ReadQuaternion(reader));
		}
	}
}
