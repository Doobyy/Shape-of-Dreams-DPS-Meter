using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Q_MoonlightPact : AbilityInstance
{
	public DewAnimationClip prepareAnim;

	public DewAnimationClip startAnim;

	public DewAnimationClip endAnim;

	public AnimationCurve yOffsetCurve;

	[NonSerialized]
	[SyncVar]
	public bool noJump;

	private EntityTransformModifier _mod;

	private Sum_Q_MoonlightPact_Fenrir _fenrir;

	[SyncVar]
	private float _duration = 100f;

	private DewAudioSource[] _audioSources;

	private float[] _baseAudioPitch;

	private float[] _baseAudioVolume;

	public override bool reuseInRoom => true;

	public bool NetworknoJump
	{
		get
		{
			return noJump;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref noJump, 64uL, (Action<bool, bool>)null);
		}
	}

	public float Network_duration
	{
		get
		{
			return _duration;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _duration, 128uL, (Action<float, float>)null);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_audioSources = ((Component)(object)this).GetComponentsInChildren<DewAudioSource>(true);
		_baseAudioPitch = new float[_audioSources.Length];
		_baseAudioVolume = new float[_audioSources.Length];
		for (int i = 0; i < _audioSources.Length; i++)
		{
			_baseAudioPitch[i] = _audioSources[i].pitchMultiplier;
			_baseAudioVolume[i] = _audioSources[i].volumeMultiplier;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (_audioSources != null)
		{
			for (int i = 0; i < _audioSources.Length; i++)
			{
				_audioSources[i].pitchMultiplier = _baseAudioPitch[i];
				_audioSources[i].volumeMultiplier = _baseAudioVolume[i];
			}
		}
		NetworknoJump = false;
		Network_duration = 100f;
		_fenrir = null;
	}

	protected override void OnCreate()
	{
		if (noJump)
		{
			ListReturnHandle<DewAudioSource> handle;
			foreach (DewAudioSource item in ((Component)(object)this).GetComponentsInChildrenNonAlloc(out handle))
			{
				item.pitchMultiplier *= 1.5f;
				item.volumeMultiplier = 0.75f;
			}
			handle.Return();
		}
		base.OnCreate();
		if (!(info.caster is Hero hero))
		{
			return;
		}
		foreach (Summon summon in hero.summons)
		{
			if (summon is Sum_Q_MoonlightPact_Fenrir fenrir)
			{
				_fenrir = fenrir;
				break;
			}
		}
		if ((UnityEngine.Object)(object)_fenrir != null)
		{
			_mod = _fenrir.Visual.GetNewTransformModifier();
		}
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if ((UnityEngine.Object)(object)_fenrir == null)
		{
			Destroy();
			return;
		}
		_fenrir.Control.CancelOngoingChannels();
		if (noJump)
		{
			CreateAbilityInstance(_fenrir.agentPosition, _fenrir.rotation, new CastInfo(info.caster), (Ai_Q_MoonlightPact_Land ai) =>
			{
				ai.fenrir = _fenrir;
			});
			Destroy();
			return;
		}
		CreateStatusEffect<Se_Q_MoonlightPact_FlyingFenrir>(_fenrir).DestroyOnDestroy(this);
		Vector3 dest = Dew.GetValidAgentDestination_Closest(_fenrir.agentPosition, info.point);
		_fenrir.Control.RotateTowards(dest, immediately: false);
		_fenrir.Animation.PlayAbilityAnimation(prepareAnim);
		_fenrir.Control.StartChannel(new Channel
		{
			duration = 0.2f,
			onCancel = Destroy,
			blockedActions = Channel.BlockedAction.Everything,
			onComplete = () =>
			{
				Network_duration = Vector2.Distance(_fenrir.agentPosition.ToXY(), dest.ToXY()) / 15f;
				Network_duration = Mathf.Clamp(_duration, 0.3f, 0.6f);
				_fenrir.Animation.PlayAbilityAnimation(startAnim);
				_fenrir.Control.StartDisplacement(new DispByDestination
				{
					destination = dest,
					duration = _duration,
					ease = DewEase.EaseOutQuad,
					isFriendly = true,
					onCancel = Destroy,
					rotateForward = true,
					rotateSmoothly = false,
					affectedByMovementSpeed = false,
					canGoOverTerrain = true,
					isCanceledByCC = false,
					onFinish = () =>
					{
						_fenrir.Animation.PlayAbilityAnimation(endAnim);
						_fenrir.Control.StartDaze(0.3f);
						List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle2, _fenrir.agentPosition, 8f, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
						{
							sortComparer = CollisionCheckSettings.DistanceFromCenter
						});
						if (list.Count > 0)
						{
							_fenrir.Control.RotateTowards(list[0], immediately: true);
							_fenrir.AI.Aggro(list[0]);
							_fenrir.Control.Stop();
							_fenrir.AI.CallAIUpdateImmediately();
						}
						handle2.Return();
						Dew.CallDelayed(() =>
						{
							if (!_fenrir.IsNullInactiveDeadOrKnockedOut())
							{
								CreateAbilityInstance(_fenrir.agentPosition, _fenrir.rotation, new CastInfo(info.caster), (Ai_Q_MoonlightPact_Land ai) =>
								{
									ai.fenrir = _fenrir;
								});
							}
							Destroy();
						}, 5);
					}
				});
			}
		});
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		if (_mod != null && _duration != 0f)
		{
			_mod.localOffset = new Vector3(0f, yOffsetCurve.Evaluate((Time.time - creationTime - 0.2f) / _duration), 0f);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (_mod != null)
		{
			_mod.Stop();
			_mod = null;
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
			NetworkWriterExtensions.WriteBool(writer, noJump);
			NetworkWriterExtensions.WriteFloat(writer, _duration);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, noJump);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _duration);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref noJump, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _duration, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref noJump, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _duration, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
