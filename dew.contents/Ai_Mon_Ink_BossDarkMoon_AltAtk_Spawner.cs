using System;
using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Ai_Mon_Ink_BossDarkMoon_AltAtk_Spawner : AbilityInstance
{
	public bool doUnstoppableOnChainAtk;

	public float hallucinationDelay;

	public GameObject fxHallucinationStart;

	public GameObject fxHallucinationEnd;

	public DewAnimationClip hallucinationClip;

	public float noChainAtkDelay;

	public float noChainAtkPostDelay;

	public float firstPhaseAtkInterval;

	public GameObject fxFirstAtk;

	public GameObject fxSecondAtk;

	public DewAnimationClip firstAtkClip;

	public DewAnimationClip secondAtkClip;

	public DewAnimationClip secondAtkNoChainAtkClip;

	public float backDashDelay;

	public float backDashDistance;

	public float backDashDuration;

	public float stingDelay;

	public float stingPostDelay;

	public float rotStartSpeed;

	public float rotMaxSpeed;

	public DewEase backDashEase;

	public GameObject fxBackDash;

	public GameObject fxTelegraphing;

	public GameObject fxStingTelegraph;

	public DewAnimationClip backDashClip;

	public DewAnimationClip stingReadyClip;

	public DewAnimationClip stingClip;

	public float horizontalAtkDelay;

	public float horizontalAtkPostDelay;

	public float horizontallAtkDistance;

	public GameObject fxHorizontalAtkStart;

	public GameObject fxHorizontalTelegraph;

	public DewAnimationClip horizontalAtkReadyClip;

	public DewAnimationClip horizontalAtkStartClip;

	public float jumpAtkDelay;

	public float jumpAtkPostDelay;

	public float jumpDuration;

	public float jumpDistance;

	public float jumpAtkOffset;

	public DewEase jumpEase;

	public GameObject fxJumpStart;

	public GameObject fxJumpTelegraph;

	public DewAnimationClip jumpAtkReadyClip;

	public DewAnimationClip jumpAtkStartClip;

	public float rageAtkDelay;

	public float rageAtkPostDelay;

	public float rageAtkRotDuration;

	public GameObject fxRageAtkStart;

	public GameObject fxRageAtkEnd;

	public GameObject fxRageAtkTelegraph;

	public DewAnimationClip rageAtkReadyClip;

	public DewAnimationClip rageAtkEndClip;

	private bool _isHallucination;

	private bool _isRage;

	private Channel _channel;

	[SyncVar]
	private Quaternion _syncedRotation;

	private IEnumerator _routine;

	private Vector3 _baseJumpTelegraphScale;

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
		if (fxJumpTelegraph != null)
		{
			_baseJumpTelegraphScale = fxJumpTelegraph.transform.localScale;
		}
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		if (_isHallucination)
		{
			FxPlayNewNetworked(fxHallucinationStart, info.caster);
			info.caster.Animation.PlayAbilityAnimation(hallucinationClip, 0.4f);
			yield return new SI.WaitForSeconds(hallucinationDelay);
		}
		if (!_isHallucination)
		{
			_isRage = ((Mon_Ink_BossDarkMoon)info.caster)._isRage;
		}
		Network_syncedRotation = info.caster.rotation;
		_channel = info.caster.Control.StartChannel(new Channel
		{
			blockedActions = Channel.BlockedAction.Everything,
			duration = float.PositiveInfinity,
			isAttack = true,
			onCancel = DestroyIfActive
		});
		int num = UnityEngine.Random.Range(0, 4);
		if (_isRage)
		{
			num = UnityEngine.Random.Range(0, 5);
		}
		if (_isHallucination)
		{
			num = 0;
		}
		_routine = null;
		switch (num)
		{
		case 1:
			_routine = StingAtkRoutine();
			break;
		case 2:
			_routine = HorizontalAtkRoutine();
			break;
		case 3:
			_routine = JumpAtkRoutine();
			break;
		case 4:
			_routine = RageAtkRoutine();
			break;
		}
		FxPlayNetworked(fxFirstAtk, info.caster);
		info.caster.Animation.PlayAbilityAnimation(firstAtkClip);
		CreateAbilityInstance<Ai_Mon_Ink_BossDarkMoon_AltAtk_FirstAtk>(info.caster.agentPosition, null, info);
		yield return new SI.WaitForSeconds(firstPhaseAtkInterval);
		FxPlayNetworked(fxSecondAtk, info.caster);
		info.caster.Animation.PlayAbilityAnimation(secondAtkClip);
		CreateAbilityInstance(info.caster.agentPosition, null, info, (Ai_Mon_Ink_BossDarkMoon_AltAtk_FirstAtk b) =>
		{
			b.isSecondAtk = true;
		});
		yield return new SI.WaitForSeconds(firstPhaseAtkInterval);
		if (_routine != null)
		{
			((MonoBehaviour)(object)this).StartCoroutine(_routine);
		}
		else
		{
			if (_isRage && !_isHallucination)
			{
				Hero closestAliveHero = Dew.GetClosestAliveHero(info.caster.agentPosition, fallbackToDead: true, info.caster);
				if (!closestAliveHero.IsNullOrInactive())
				{
					Vector3 aIAgentPosition = closestAliveHero.GetAIAgentPosition(info.caster);
					Vector3 vector = Quaternion.AngleAxis(UnityEngine.Random.Range(-45, 45), Vector3.up) * info.forward;
					Vector3 vector2 = aIAgentPosition + vector * 5f;
					vector2 = Dew.GetPositionOnGround(vector2);
					Mon_Ink_BossDarkMoonHallucination mon_Ink_BossDarkMoonHallucination = SpawnEntity(vector2, Quaternion.LookRotation(aIAgentPosition - vector2), DewPlayer.creep, info.caster.level, (Mon_Ink_BossDarkMoonHallucination b) =>
					{
						b.Network_isSpecialAtk = true;
					});
					CreateAbilityInstance(mon_Ink_BossDarkMoonHallucination.agentPosition, mon_Ink_BossDarkMoonHallucination.rotation, new CastInfo(mon_Ink_BossDarkMoonHallucination, CastInfo.GetAngle(aIAgentPosition - mon_Ink_BossDarkMoonHallucination.agentPosition)), (Ai_Mon_Ink_BossDarkMoon_AltAtk_Spawner b) =>
					{
						b._isHallucination = true;
					});
				}
			}
			info.caster.Animation.PlayAbilityAnimation(secondAtkNoChainAtkClip);
			yield return new SI.WaitForSeconds(noChainAtkDelay);
			if (_isHallucination)
			{
				FxPlayNetworked(fxHallucinationEnd, info.caster);
				yield return new SI.WaitForSeconds(1f);
			}
			yield return new SI.WaitForSeconds(noChainAtkPostDelay);
			Destroy();
		}
		if (doUnstoppableOnChainAtk)
		{
			CreateBasicEffect(info.caster, new UnstoppableEffect(), float.PositiveInfinity).DestroyOnDestroy(this);
		}
		IEnumerator HorizontalAtkRoutine()
		{
			FxPlayNetworked(fxHorizontalAtkStart, info.caster);
			Hero closestAliveHero2 = Dew.GetClosestAliveHero(info.caster.agentPosition, fallbackToDead: true, info.caster);
			Vector3 vector3 = info.caster.agentPosition;
			if (!closestAliveHero2.IsNullOrInactive())
			{
				vector3 = AbilityTrigger.PredictPoint_Simple(info.caster, NetworkedManagerBase<GameManager>.instance.GetPredictionStrength(), closestAliveHero2, horizontalAtkDelay);
			}
			float value = Vector3.Dot(vector3 - info.caster.agentPosition, info.forward);
			value = Mathf.Clamp(value, 0f, horizontallAtkDistance);
			Vector3 atkPoint = info.caster.agentPosition + info.forward * value;
			FxPlayNetworked(fxHorizontalTelegraph, atkPoint, info.caster.rotation);
			info.caster.Animation.PlayAbilityAnimation(horizontalAtkReadyClip);
			yield return new WaitForSeconds(horizontalAtkDelay);
			FxStopNetworked(fxHorizontalTelegraph);
			info.caster.Animation.PlayAbilityAnimation(horizontalAtkStartClip);
			CreateAbilityInstance(atkPoint, info.caster.rotation, info, (Ai_Mon_Ink_BossDarkMoon_AltAtk_HorizontalAtk b) =>
			{
				b.isRage = _isRage;
				b.isMainAtk = true;
			});
			yield return new WaitForSeconds(horizontalAtkPostDelay);
			Destroy();
		}
		IEnumerator JumpAtkRoutine()
		{
			float scale = (_isRage ? 1.4f : 1f);
			info.caster.Animation.PlayAbilityAnimation(jumpAtkReadyClip);
			yield return new WaitForSeconds(jumpAtkDelay);
			float num2 = 45f;
			Vector3 vector3 = info.forward;
			Hero closestAliveHero2 = Dew.GetClosestAliveHero(info.caster.agentPosition, fallbackToDead: true, info.caster);
			if (!closestAliveHero2.IsNullInactiveDeadOrKnockedOut())
			{
				Vector3 to = closestAliveHero2.GetAIAgentPosition(info.caster) - info.caster.agentPosition;
				float y = Mathf.Clamp(Vector3.SignedAngle(info.forward, to, Vector3.up), 0f - num2, num2);
				vector3 = Quaternion.Euler(0f, y, 0f) * info.forward;
			}
			Vector3 end = info.caster.agentPosition + vector3 * jumpDistance;
			end = Dew.GetValidAgentDestination_LinearSweep(info.caster.agentPosition, end);
			end = Dew.GetPositionOnGround(end);
			Vector3 jumpAtkPoint = end + vector3 * jumpAtkOffset;
			info.caster.Control.StartDisplacement(new DispByDestination
			{
				affectedByMovementSpeed = false,
				canGoOverTerrain = false,
				destination = end,
				ease = jumpEase,
				duration = jumpDuration,
				isCanceledByCC = false,
				isFriendly = true,
				onCancel = DestroyIfActive,
				onFinish = () =>
				{
					info.caster.Animation.PlayAbilityAnimation(jumpAtkStartClip);
					CreateAbilityInstance(jumpAtkPoint, null, new CastInfo(info.caster, jumpAtkPoint), (Ai_Mon_Ink_BossDarkMoon_AltAtk_JumpAtk b) =>
					{
						b.isRage = _isRage;
						b.scaleMultiplier = scale;
					});
				}
			});
			FxPlayNetworked(fxJumpStart, info.caster);
			RpcPlayJumpAtkTelegraph(jumpAtkPoint, scale);
			yield return new WaitForSeconds(jumpDuration + jumpAtkPostDelay);
			DestroyIfActive();
		}
		IEnumerator RageAtkRoutine()
		{
			info.caster.Animation.PlayAbilityAnimation(rageAtkReadyClip);
			yield return new WaitForSeconds(rageAtkDelay);
			float num2 = 80f;
			Vector3 dir = info.forward;
			Hero closestAliveHero2 = Dew.GetClosestAliveHero(info.caster.agentPosition, fallbackToDead: true, info.caster);
			if (!closestAliveHero2.IsNullInactiveDeadOrKnockedOut())
			{
				Vector3 to = AbilityTrigger.PredictPoint_Simple(info.caster, NetworkedManagerBase<GameManager>.instance.GetPredictionStrength(), closestAliveHero2, horizontalAtkDelay) - info.caster.agentPosition;
				float y = Mathf.Clamp(Vector3.SignedAngle(info.forward, to, Vector3.up), 0f - num2, num2);
				dir = Quaternion.Euler(0f, y, 0f) * info.forward;
			}
			float elapsedTime = 0f;
			Quaternion startRot = ((Component)(object)info.caster).transform.rotation;
			Quaternion targetRot = Quaternion.LookRotation(dir);
			FxPlayNetworked(fxRageAtkStart, info.caster);
			FxPlayNetworked(fxRageAtkTelegraph, info.caster.agentPosition, targetRot);
			info.caster.Control.StartChannel(new Channel
			{
				blockedActions = Channel.BlockedAction.Everything,
				duration = rageAtkRotDuration,
				onCancel = DestroyIfActive,
				onTick = (float dt) =>
				{
					elapsedTime += dt;
					float t = Mathf.Clamp01(elapsedTime / rageAtkRotDuration);
					Network_syncedRotation = Quaternion.Slerp(startRot, targetRot, t);
					info.caster.Control.Rotate(_syncedRotation, immediately: false);
				},
				onComplete = () =>
				{
					((MonoBehaviour)(object)this).StartCoroutine(Routine2());
				}
			});
			IEnumerator Routine2()
			{
				FxPlayNetworked(fxRageAtkEnd, info.caster);
				info.caster.Animation.PlayAbilityAnimation(rageAtkEndClip);
				info.caster.Control.Rotate(dir, immediately: true);
				CreateAbilityInstance<Ai_Mon_Ink_BossDarkMoon_ThrowSpear_TeleportAtk_AfterAtk>(info.caster.agentPosition, info.caster.rotation, new CastInfo(info.caster, CastInfo.GetAngle(targetRot)));
				yield return new WaitForSeconds(rageAtkPostDelay);
				DestroyIfActive();
			}
		}
		IEnumerator Routine()
		{
			FxPlayNetworked(fxStingTelegraph, info.caster);
			info.caster.Control.StopOverrideRotation();
			info.caster.Control.StartDaze(stingPostDelay);
			yield return new WaitForSeconds(stingPostDelay);
			CreateAbilityInstance(info.caster.agentPosition, null, new CastInfo(info.caster, CastInfo.GetAngle(((Component)(object)info.caster).transform.forward)), (Ai_Mon_Ink_BossDarkMoon_Spear_Instance b) =>
			{
				b.NetworkdisableSpearModel = true;
			});
			info.caster.Animation.PlayAbilityAnimation(stingClip);
			Destroy();
		}
		IEnumerator StingAtkRoutine()
		{
			yield return new WaitForSeconds(backDashDelay);
			FxPlayNetworked(fxBackDash, info.caster);
			info.caster.Animation.PlayAbilityAnimation(backDashClip);
			info.caster.Control.StartDisplacement(new DispByDestination
			{
				affectedByMovementSpeed = false,
				canGoOverTerrain = false,
				destination = Dew.GetValidAgentDestination_LinearSweep(info.caster.agentPosition, info.caster.agentPosition - info.forward * backDashDistance),
				ease = backDashEase,
				duration = backDashDuration,
				isCanceledByCC = false,
				isFriendly = true,
				rotateForward = false,
				onCancel = DestroyIfActive,
				onFinish = () =>
				{
					info.caster.Animation.PlayAbilityAnimation(stingReadyClip);
					FxPlayNetworked(fxTelegraphing, info.caster);
				}
			});
			yield return new WaitForSeconds(backDashDuration * 0.5f);
			float elapsedTime = 0f;
			info.caster.Control.StartChannel(new Channel
			{
				blockedActions = Channel.BlockedAction.Everything,
				duration = stingDelay,
				onCancel = DestroyIfActive,
				onTick = (float dt) =>
				{
					elapsedTime += dt;
					float t = Mathf.Clamp01(elapsedTime / stingDelay);
					float num2 = Mathf.Lerp(rotStartSpeed, rotMaxSpeed, t);
					if (info.target.IsNullInactiveDeadOrKnockedOut())
					{
						Hero closestAliveHero2 = Dew.GetClosestAliveHero(info.caster.position, fallbackToDead: false, info.caster);
						if ((UnityEngine.Object)(object)closestAliveHero2 != null)
						{
							info = new CastInfo(info.caster, closestAliveHero2);
						}
					}
					if (!info.target.IsNullInactiveDeadOrKnockedOut())
					{
						Vector3 vector3 = AbilityTrigger.PredictPoint_Simple(info.caster, UnityEngine.Random.Range(0.5f, 1f), info.target, stingPostDelay + dt);
						Network_syncedRotation = Quaternion.RotateTowards(_syncedRotation, Quaternion.LookRotation(vector3 - info.caster.agentPosition), num2 * dt);
					}
					info.caster.Control.Rotate(_syncedRotation, immediately: false);
				},
				onComplete = () =>
				{
					((MonoBehaviour)(object)this).StartCoroutine(Routine());
				}
			});
		}
	}

	[ClientRpc]
	private void RpcPlayJumpAtkTelegraph(Vector3 point, float scale)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteVector3((NetworkWriter)(object)val, point);
		NetworkWriterExtensions.WriteFloat((NetworkWriter)(object)val, scale);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Ai_Mon_Ink_BossDarkMoon_AltAtk_Spawner::RpcPlayJumpAtkTelegraph(UnityEngine.Vector3,System.Single)", 1504011102, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if (_channel != null)
			{
				_channel.Cancel();
				_channel = null;
			}
			if (_routine != null)
			{
				((MonoBehaviour)(object)this).StopCoroutine(_routine);
			}
			if (_isHallucination)
			{
				FxStopNetworked(fxHallucinationEnd);
				info.caster.Destroy();
			}
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_isHallucination = false;
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcPlayJumpAtkTelegraph__Vector3__Single(Vector3 point, float scale)
	{
		fxJumpTelegraph.transform.localScale = _baseJumpTelegraphScale * scale;
		FxPlay(fxJumpTelegraph, point, null);
	}

	protected static void InvokeUserCode_RpcPlayJumpAtkTelegraph__Vector3__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayJumpAtkTelegraph called on server.");
		}
		else
		{
			((Ai_Mon_Ink_BossDarkMoon_AltAtk_Spawner)(object)obj).UserCode_RpcPlayJumpAtkTelegraph__Vector3__Single(NetworkReaderExtensions.ReadVector3(reader), NetworkReaderExtensions.ReadFloat(reader));
		}
	}

	static Ai_Mon_Ink_BossDarkMoon_AltAtk_Spawner()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Ai_Mon_Ink_BossDarkMoon_AltAtk_Spawner), "System.Void Ai_Mon_Ink_BossDarkMoon_AltAtk_Spawner::RpcPlayJumpAtkTelegraph(UnityEngine.Vector3,System.Single)", (RemoteCallDelegate)InvokeUserCode_RpcPlayJumpAtkTelegraph__Vector3__Single);
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
