using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Ai_Mon_Despair_BossAzurak_Roll : AbilityInstance
{
	public GameObject fxPrepare;

	public GameObject fxRolling;

	public GameObject fxStopped;

	public float afterStopDelay = 0.5f;

	public int pillarCount = 5;

	public float pillarPlacementRadius = 8f;

	public AnimationCurve yOffsetCurve;

	public DewAnimationClip animStart;

	public DewAnimationClip animRollStationary;

	public float angularSpeedPerSpeed = 100f;

	public float initRollSpeed = 15f;

	public float maxRollSpeed = 30f;

	public float rollSpeedBonusPerSuccessfulRoll = 2f;

	public float hitRadius;

	public float rollCompleteRestTime = 0.5f;

	public Knockback knockback;

	public ScalingValue damage;

	public GameObject fxHit;

	private EntityTransformModifier _mod;

	private Vector3 _lastPosition;

	private float _rotationX;

	private float _nextRollSpeed;

	private Dictionary<Entity, float> _hitTimes = new Dictionary<Entity, float>();

	private List<Mon_Despair_AzurakRollPillar> _pillars = new List<Mon_Despair_AzurakRollPillar>();

	private Channel _channel;

	private Mon_Despair_BossAzurak _azurak;

	public GameObject fxRollStartTelegraph;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		_azurak = (Mon_Despair_BossAzurak)info.caster;
		if ((UnityEngine.Object)(object)_azurak != null)
		{
			GameObject[] headParts = _azurak.headParts;
			for (int i = 0; i < headParts.Length; i++)
			{
				headParts[i].SetActive(value: false);
			}
		}
		_mod = info.caster.Visual.GetNewTransformModifier();
		((MonoBehaviour)(object)this).StartCoroutine(InitialBounceRoutine());
		if (((NetworkBehaviour)this).isServer)
		{
			info.caster.Control.freeMovement = true;
			_channel = info.caster.Control.StartChannel(new Channel
			{
				duration = float.PositiveInfinity,
				blockedActions = Channel.BlockedAction.Everything
			});
			FxPlayNetworked(fxPrepare, info.caster);
			BossMonster.RevealStealthedBeforeSpecialAttack();
			DestroyOnDeath(info.caster);
			CreateStatusEffect<Se_Mon_Despair_BossAzurak_Roll_DeathInterrupt>(info.caster).DestroyOnDestroy(this);
			float num = UnityEngine.Random.Range(0f, 360f);
			for (int j = 0; j < pillarCount; j++)
			{
				Vector3 vector = position;
				num += 360f / (float)pillarCount;
				Mon_Despair_AzurakRollPillar item = Dew.SpawnEntity<Mon_Despair_AzurakRollPillar>(vector + Quaternion.Euler(0f, num, 0f) * Vector3.forward * pillarPlacementRadius, Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f), null, info.caster.owner, info.caster.level);
				_pillars.Add(item);
			}
			info.caster.Animation.PlayAbilityAnimation(animStart);
			yield return new SI.WaitForSeconds(0.75f);
			info.caster.Animation.PlayAbilityAnimation(animRollStationary);
			yield return new SI.WaitForSeconds(0.75f);
			_nextRollSpeed = initRollSpeed;
			FxStopNetworked(fxPrepare);
			FxPlayNetworked(fxRolling, info.caster);
			Vector3 rollDestination = GetRollDestination(info.caster.agentPosition + ((Component)(object)info.caster).transform.forward);
			FxPlayNewNetworked(fxRollStartTelegraph, info.caster.agentPosition, Quaternion.LookRotation(rollDestination - info.caster.agentPosition));
			StartRoll(rollDestination, OnRollComplete, _nextRollSpeed, DewEase.EaseInQuad);
		}
		IEnumerator InitialBounceRoutine()
		{
			Keyframe[] keys = yOffsetCurve.keys;
			for (float t = 0f; t < keys[keys.Length - 1].time; t += Time.deltaTime)
			{
				if (_mod == null)
				{
					yield break;
				}
				_mod.localOffset = new Vector3(0f, yOffsetCurve.Evaluate(t), 0f);
				yield return null;
			}
			if (_mod != null)
			{
				_mod.localOffset = Vector3.zero;
			}
		}
	}

	private Vector3 GetRollDestination(Vector3? targetPos = null)
	{
		Vector3 agentPosition = info.caster.agentPosition;
		if (!targetPos.HasValue)
		{
			targetPos = Dew.SelectRandomAliveHero(fallbackToDead: true, skipStealthed: true).GetAIAgentPosition(info.caster);
		}
		Vector3 center = SingletonBehaviour<Room_BossArena>.instance.center;
		float radius = SingletonBehaviour<Room_BossArena>.instance.radius;
		Vector3 vector = targetPos.Value - agentPosition;
		vector.y = 0f;
		Vector3 normalized = vector.normalized;
		return center + normalized * (radius + 25f);
	}

	private void OnRollComplete()
	{
		float restTime = rollCompleteRestTime * UnityEngine.Random.Range(0.8f, 1.2f);
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			RpcSetScale(0f);
			yield return new WaitForSeconds(restTime);
			info.caster.Visual.DisableRenderers();
			Vector3 vector = Dew.SelectRandomAliveHero(fallbackToDead: true, skipStealthed: true).GetAIAgentPosition(info.caster) + UnityEngine.Random.onUnitSphere.Flattened() * 5f;
			Vector3 normalized = (SingletonBehaviour<Room_BossArena>.instance.center - vector).normalized;
			Vector3 vector2 = position + Quaternion.Euler(0f, UnityEngine.Random.Range(-60f, 60f), 0f) * normalized * (SingletonBehaviour<Room_BossArena>.instance.radius + 25f);
			info.caster.Control.Teleport(vector2);
			info.caster.Visual.EnableRenderers();
			RpcSetScale(1f);
			_nextRollSpeed = Mathf.MoveTowards(_nextRollSpeed, maxRollSpeed, rollSpeedBonusPerSuccessfulRoll);
			Vector3 rollDestination = GetRollDestination(vector);
			FxPlayNewNetworked(fxRollStartTelegraph, vector2, Quaternion.LookRotation(rollDestination - vector2));
			StartRoll(rollDestination, OnRollComplete, _nextRollSpeed);
		}
	}

	[ClientRpc]
	private void RpcSetScale(float scale)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteFloat((NetworkWriter)(object)val, scale);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Ai_Mon_Despair_BossAzurak_Roll::RpcSetScale(System.Single)", 2113634643, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void StartRoll(Vector3 to, Action onRollComplete, float rollSpeed, DewEase ease = DewEase.Linear)
	{
		float num = Vector3.Distance(info.caster.agentPosition, to);
		info.caster.Control.StartDisplacement(new DispByDestination
		{
			duration = num / rollSpeed,
			destination = to,
			ease = ease,
			isFriendly = true,
			rotateForward = true,
			rotateSmoothly = false,
			affectedByMovementSpeed = false,
			canGoOverTerrain = true,
			isCanceledByCC = false,
			onFinish = onRollComplete
		});
	}

	private Vector3? GetDestinationDistance()
	{
		Vector3 forward = ((Component)(object)info.caster).transform.forward;
		Vector3 agentPosition = info.caster.agentPosition;
		Vector3 center = SingletonBehaviour<Room_BossArena>.instance.center;
		float num = SingletonBehaviour<Room_BossArena>.instance.radius + 25f;
		Vector3 vector = agentPosition - center;
		vector.y = 0f;
		float num2 = 1f;
		float num3 = 2f * Vector3.Dot(vector, forward);
		float num4 = Vector3.Dot(vector, vector) - num * num;
		float num5 = 0f;
		float num6 = num3 * num3 - 4f * num2 * num4;
		if (num6 < 0f)
		{
			return null;
		}
		num5 = (0f - num3 + Mathf.Sqrt(num6)) / (2f * num2);
		return agentPosition + forward * num5;
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		//IL_03d0: Unknown result type (might be due to invalid IL or missing references)
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || !info.caster.Control.isDisplacing)
		{
			return;
		}
		int num = 0;
		ListReturnHandle<Entity> handle;
		foreach (Entity item in DewPhysics.OverlapCircleAllEntities(out handle, info.caster.agentPosition, hitRadius, tvDefaultHarmfulEffectTargets))
		{
			Vector3? vector = null;
			if (num == 0)
			{
				vector = GetDestinationDistance();
				num++;
			}
			if (_hitTimes.TryGetValue(item, out var value) && Time.time - value < 1.5f)
			{
				continue;
			}
			_hitTimes[item] = Time.time;
			Damage(damage).SetOriginPosition(info.caster.agentPosition).Dispatch(item);
			knockback.ApplyWithOrigin(info.caster.agentPosition, item);
			FxPlayNewNetworked(fxHit, item);
			if (item.Status.hasCrowdControlImmunity)
			{
				info.caster.Control.CancelOngoingDisplacement();
				if (!vector.HasValue)
				{
					vector = info.caster.agentPosition + ((Component)(object)info.caster).transform.forward;
				}
				StartRoll(vector.Value, OnRollComplete, _nextRollSpeed * 0.8f, DewEase.EaseInQuad);
			}
		}
		handle.Return();
		num = 0;
		ListReturnHandle<Entity> handle2;
		foreach (Entity item2 in DewPhysics.OverlapCircleAllEntities(out handle2, info.caster.agentPosition, hitRadius))
		{
			if (!(item2 is Mon_Despair_AzurakRollPillar))
			{
				continue;
			}
			Vector3? vector2 = null;
			if (num == 0)
			{
				vector2 = GetDestinationDistance();
				num++;
			}
			if (!_hitTimes.TryGetValue(item2, out var value2) || !(Time.time - value2 < 1.5f))
			{
				_hitTimes[item2] = Time.time;
				float num2 = ((NetworkedManagerBase<GameManager>.instance.difficulty.specialSkillChanceMultiplier > 0.8f) ? 0.334f : 0.501f);
				PureDamage(item2.maxHealth * num2).SetOriginPosition(info.caster.agentPosition).Dispatch(item2);
				FxPlayNewNetworked(fxHit, item2);
				info.caster.Control.CancelOngoingDisplacement();
				if (!vector2.HasValue)
				{
					vector2 = info.caster.agentPosition + ((Component)(object)info.caster).transform.forward;
				}
				StartRoll(vector2.Value, OnRollComplete, _nextRollSpeed * 0.8f, DewEase.EaseInQuad);
			}
		}
		handle2.Return();
		bool flag = true;
		foreach (Mon_Despair_AzurakRollPillar pillar in _pillars)
		{
			if (!pillar.IsNullInactiveDeadOrKnockedOut())
			{
				flag = false;
				break;
			}
		}
		if (flag && (int)Dew.GetNavMeshPathStatus(Dew.GetPositionOnGround(SingletonBehaviour<Room_BossArena>.instance.center), info.caster.agentPosition) == 0)
		{
			RpcHeadPartEnable();
			info.caster.Control.CancelOngoingDisplacement();
			FxStopNetworked(fxRolling);
			FxPlayNetworked(fxStopped, info.caster);
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(afterStopDelay);
			Se_Mon_Despair_BossAzurak_Hide burrow = CreateStatusEffect<Se_Mon_Despair_BossAzurak_Hide>(info.caster);
			burrow.onBurrowComplete += (Action)(() =>
			{
				info.caster.Control.Teleport(SingletonBehaviour<Room_BossArena>.instance.center);
				info.caster.Control.Rotate(ManagerBase<CameraManager>.instance.entityCamAngle + 180f, immediately: true);
				burrow.UnburrowAndDestroy();
				Destroy();
			});
		}
	}

	[ClientRpc]
	private void RpcHeadPartEnable()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Ai_Mon_Despair_BossAzurak_Roll::RpcHeadPartEnable()", 1690359782, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (_mod != null)
		{
			_mod.Stop();
			_mod = null;
		}
		if ((UnityEngine.Object)(object)_azurak != null)
		{
			GameObject[] headParts = _azurak.headParts;
			for (int i = 0; i < headParts.Length; i++)
			{
				headParts[i].SetActive(value: true);
			}
		}
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		info.caster.Control.freeMovement = false;
		if (info.caster.Control.isDisplacing)
		{
			info.caster.Control.CancelOngoingDisplacement();
		}
		if (_channel != null && _channel.isAlive)
		{
			_channel.Cancel();
			_channel = null;
		}
		FxStopNetworked(fxPrepare);
		FxStopNetworked(fxRolling);
		foreach (Mon_Despair_AzurakRollPillar pillar in _pillars)
		{
			if (!pillar.IsNullInactiveDeadOrKnockedOut())
			{
				pillar.Kill();
			}
		}
		_pillars.Clear();
		_hitTimes.Clear();
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcSetScale__Single(float scale)
	{
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			if (_mod != null)
			{
				float start = _mod.scaleMultiplier.x;
				float end = scale;
				for (float t = 0f; t < 1f; t += Time.deltaTime * 4f)
				{
					if (_mod == null)
					{
						yield break;
					}
					_mod.scaleMultiplier = Mathf.Lerp(start, end, t) * Vector3.one;
					yield return null;
				}
				if (_mod != null)
				{
					_mod.scaleMultiplier = end * Vector3.one;
				}
			}
		}
	}

	protected static void InvokeUserCode_RpcSetScale__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetScale called on server.");
		}
		else
		{
			((Ai_Mon_Despair_BossAzurak_Roll)(object)obj).UserCode_RpcSetScale__Single(NetworkReaderExtensions.ReadFloat(reader));
		}
	}

	protected void UserCode_RpcHeadPartEnable()
	{
		if ((UnityEngine.Object)(object)_azurak != null)
		{
			GameObject[] headParts = _azurak.headParts;
			for (int i = 0; i < headParts.Length; i++)
			{
				headParts[i].SetActive(value: true);
			}
		}
	}

	protected static void InvokeUserCode_RpcHeadPartEnable(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcHeadPartEnable called on server.");
		}
		else
		{
			((Ai_Mon_Despair_BossAzurak_Roll)(object)obj).UserCode_RpcHeadPartEnable();
		}
	}

	static Ai_Mon_Despair_BossAzurak_Roll()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Ai_Mon_Despair_BossAzurak_Roll), "System.Void Ai_Mon_Despair_BossAzurak_Roll::RpcSetScale(System.Single)", (RemoteCallDelegate)InvokeUserCode_RpcSetScale__Single);
		RemoteProcedureCalls.RegisterRpc(typeof(Ai_Mon_Despair_BossAzurak_Roll), "System.Void Ai_Mon_Despair_BossAzurak_Roll::RpcHeadPartEnable()", (RemoteCallDelegate)InvokeUserCode_RpcHeadPartEnable);
	}
}
