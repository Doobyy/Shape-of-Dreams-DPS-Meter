using System;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class EntitySound : EntityComponent, ICleanup
{
	public SafeAction ClientEvent_OnFootstep;

	public DewAudioClip voiceStart;

	public DewAudioClip voiceIdle;

	public Vector2 voiceIdleInterval = new Vector2(5f, 10f);

	public DewAudioClip voiceDeath;

	public DewAudioClip conversationStart;

	public DewAudioClip conversationClick;

	private DewAudioSource _voiceSource;

	private float _lastIdleTime;

	private float _nextIdleInterval;

	private float _nextRippleTime;

	private Action<EventInfoKill> _onDeathHandler;

	private const float SurfaceCacheLifetime = 2f;

	private const float SurfaceCacheMaxMoveDistance = 2f;

	private bool _hasSurfaceCache;

	private DewSurfaceData _cachedSurfaceData;

	private Vector2 _surfaceCacheXZ;

	private float _surfaceCacheTime;

	public bool canDestroy => !_voiceSource.isPlaying;

	public override void ClearPooledEventsAndProcessors()
	{
		base.ClearPooledEventsAndProcessors();
		ClientEvent_OnFootstep?.Clear();
	}

	public override void OnStart()
	{
		base.OnStart();
		_voiceSource = ((Component)(object)this).gameObject.AddComponent<DewAudioSource>();
		_voiceSource.space = AudioSpaceType.Normal;
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		_lastIdleTime = Time.time - (voiceIdleInterval.x + voiceIdleInterval.y) * UnityEngine.Random.value * 0.5f;
		_nextIdleInterval = UnityEngine.Random.Range(voiceIdleInterval.x, voiceIdleInterval.y);
		entity.EntityEvent_OnDeath += new Action<EventInfoKill>(EntityEventOnDeath);
	}

	private void EntityEventOnDeath(EventInfoKill obj)
	{
		Say(voiceDeath, interruptPrevious: true);
	}

	public override void OnLateStartServer()
	{
		base.OnLateStartServer();
		if (!entity.Visual.skipSpawning)
		{
			Say(voiceStart, interruptPrevious: false);
		}
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (!entity.isSleeping)
		{
			if (Time.time > _nextRippleTime)
			{
				DoPeriodicSurfaceEffect();
			}
			if (((NetworkBehaviour)this).isServer && entity.isActive && entity.Status.isAlive && voiceIdle != null && Time.time - _lastIdleTime > _nextIdleInterval)
			{
				Say(voiceIdle, interruptPrevious: false);
				_lastIdleTime = Time.time;
				_nextIdleInterval = UnityEngine.Random.Range(voiceIdleInterval.x, voiceIdleInterval.y);
			}
		}
	}

	[Server]
	public void Say(DewAudioClip clip, bool interruptPrevious)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntitySound::Say(DewAudioClip,System.Boolean)' called when server was not active");
		}
		else if (!(clip == null) && (!_voiceSource.isPlaying || !(_voiceSource.clip == clip)))
		{
			RpcSay(clip, interruptPrevious);
		}
	}

	[ClientRpc]
	private void RpcSay(DewAudioClip clip, bool interruptPrevious)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkWriter)(object)val).WriteDewAudioClip(clip);
		NetworkWriterExtensions.WriteBool((NetworkWriter)(object)val, interruptPrevious);
		((NetworkBehaviour)this).SendRPCInternal("System.Void EntitySound::RpcSay(DewAudioClip,System.Boolean)", 448643774, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public void OnCleanup()
	{
	}

	public void DoPeriodicSurfaceEffect()
	{
		if (entity.Visual.currentYOffset > 0.1f)
		{
			_nextRippleTime = Time.time + 0.4f;
			return;
		}
		DewSurfaceData footstepData = GetFootstepData();
		if (footstepData == null || footstepData.fxPeriodicEffect == null || (footstepData.excludeLesserMonster && entity is Monster { type: Monster.MonsterType.Lesser }))
		{
			_nextRippleTime = Time.time + 0.4f;
			return;
		}
		_nextRippleTime = Time.time + UnityEngine.Random.Range(footstepData.periodicEffectInterval.x, footstepData.periodicEffectInterval.y);
		if (!footstepData.onlyWhenWalking || entity.Control.isWalking)
		{
			DewEffect.PlayPooled(footstepData.fxPeriodicEffect, Dew.GetPositionOnGround(entity.position) + Vector3.up * 0.05f, Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f), ((NetworkBehaviour)this).netIdentity);
		}
	}

	public void DoFootstep()
	{
		if (!(entity.Visual.currentYOffset > 0.1f))
		{
			if ((UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.softInstance != null)
			{
				SingletonDewNetworkBehaviour<Room>.softInstance.ClientEvent_OnFootstep?.Invoke(entity);
			}
			ClientEvent_OnFootstep?.Invoke();
			DewSurfaceData footstepData = GetFootstepData();
			if (!(footstepData == null))
			{
				Vector3 bonePosition = entity.Visual.GetBonePosition((HumanBodyBones)5);
				Vector3 bonePosition2 = entity.Visual.GetBonePosition((HumanBodyBones)6);
				DewEffect.PlayPooled(footstepData.fxHeroFootstep, (bonePosition.y < bonePosition2.y) ? bonePosition : bonePosition2, Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f), ((NetworkBehaviour)this).netIdentity);
			}
		}
	}

	private DewSurfaceData GetFootstepData()
	{
		Vector2 vector = entity.position.ToXY();
		if (_hasSurfaceCache && Time.time - _surfaceCacheTime < 2f && Vector2.SqrMagnitude(vector - _surfaceCacheXZ) < 4f)
		{
			return _cachedSurfaceData;
		}
		RaycastHit val = default;
		DewSurfaceData dewSurfaceData;
		if (Physics.Raycast(entity.position + Vector3.up * 1.5f, Vector3.down, ref val, 5f, LayerMasks.Ground) && ((Component)(object)val.collider).TryGetComponent(out Room_SurfaceOverride component))
		{
			dewSurfaceData = ((component.data != null) ? component.data : DewSurfaceData.defaultSurface);
		}
		else
		{
			dewSurfaceData = ((!((UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.softInstance != null)) ? DewSurfaceData.defaultSurface : SingletonDewNetworkBehaviour<Room>.softInstance.defaultSurface);
		}
		_hasSurfaceCache = true;
		_cachedSurfaceData = dewSurfaceData;
		_surfaceCacheXZ = vector;
		_surfaceCacheTime = Time.time;
		return dewSurfaceData;
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcSay__DewAudioClip__Boolean(DewAudioClip clip, bool interruptPrevious)
	{
		if (!(clip == null) && (!_voiceSource.isPlaying || interruptPrevious))
		{
			AudioType type;
			if (((NetworkBehaviour)entity).isOwned)
			{
				type = AudioType.GameSelf;
			}
			else if (entity.IsAnyBoss())
			{
				type = AudioType.GameBoss;
			}
			else
			{
				type = ((!((UnityEngine.Object)(object)entity.owner != null) || !entity.owner.isHumanPlayer) ? AudioType.GameOthers : AudioType.GameOtherPlayers);
			}
			_voiceSource.type = type;
			_voiceSource.clip = clip;
			_voiceSource.Play();
		}
	}

	protected static void InvokeUserCode_RpcSay__DewAudioClip__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSay called on server.");
		}
		else
		{
			((EntitySound)(object)obj).UserCode_RpcSay__DewAudioClip__Boolean(reader.ReadDewAudioClip(), NetworkReaderExtensions.ReadBool(reader));
		}
	}

	static EntitySound()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(EntitySound), "System.Void EntitySound::RpcSay(DewAudioClip,System.Boolean)", (RemoteCallDelegate)InvokeUserCode_RpcSay__DewAudioClip__Boolean);
	}
}
