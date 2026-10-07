using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class TimescaleManager : NetworkedManagerBase<TimescaleManager>
{
	private class TimescaleChange
	{
		public float target;

		public float initDuration;

		public float remainingDuration;
	}

	private float _desiredTimescale = 1f;

	[SyncVar(hook = "TimescaleChanged")]
	private float _finalTimescale = 1f;

	private List<AudioSource> _pausedSources = new List<AudioSource>();

	[CompilerGenerated]
	[SyncVar]
	private float effectTimescale__BackingField = 1f;

	private List<TimescaleChange> _ongoingTimescaleChanges = new List<TimescaleChange>();

	private TimescaleChange _menuSlowTime;

	public Action<float, float> _Mirror_SyncVarHookDelegate__finalTimescale;

	public float desiredTimescale
	{
		get
		{
			return _desiredTimescale;
		}
		set
		{
			_desiredTimescale = value;
		}
	}

	public bool shouldTimeBeSlowedBySpecialMenu { get; internal set; }

	public float effectTimescale
	{
		[CompilerGenerated]
		get
		{
			return effectTimescale__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CeffectTimescale_003Ek__BackingField = value;
		}
	}

	public float Network_finalTimescale
	{
		get
		{
			return _finalTimescale;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _finalTimescale, 1uL, _Mirror_SyncVarHookDelegate__finalTimescale);
		}
	}

	public float Network_003CeffectTimescale_003Ek__BackingField
	{
		get
		{
			return effectTimescale__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref effectTimescale__BackingField, 2uL, (Action<float, float>)null);
		}
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		_menuSlowTime = new TimescaleChange
		{
			target = 1f,
			remainingDuration = float.PositiveInfinity,
			initDuration = float.PositiveInfinity
		};
		_ongoingTimescaleChanges.Add(_menuSlowTime);
	}

	public void TimescaleChanged(float oldVal, float newVal)
	{
		Time.timeScale = newVal;
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (!NetworkServer.active)
		{
			return;
		}
		if (shouldTimeBeSlowedBySpecialMenu)
		{
			_menuSlowTime.target = 0.025f;
		}
		else
		{
			_menuSlowTime.target = 1f;
		}
		Network_003CeffectTimescale_003Ek__BackingField = 1f;
		for (int num = _ongoingTimescaleChanges.Count - 1; num >= 0; num--)
		{
			TimescaleChange timescaleChange = _ongoingTimescaleChanges[num];
			timescaleChange.remainingDuration -= Time.unscaledDeltaTime;
			if (timescaleChange.remainingDuration < 0f)
			{
				timescaleChange.remainingDuration = 0f;
				_ongoingTimescaleChanges.RemoveAt(num);
			}
			else
			{
				Network_003CeffectTimescale_003Ek__BackingField = Mathf.Min(effectTimescale, timescaleChange.target);
			}
		}
		float num2 = _desiredTimescale * effectTimescale;
		if (_finalTimescale != num2)
		{
			Network_finalTimescale = num2;
		}
		if (InGameUIManager.softInstance == null)
		{
			Time.timeScale = _finalTimescale;
		}
		else if (NetworkServer.connections.Count <= 1 && DewNetworkManager.startSettings.networkMode == DewNetworkMode.Singleplayer && InGameUIManager.instance.IsState("Menu") && !DewNetworkManager.instance.isEndingSession)
		{
			if (!(Time.timeScale > 0f))
			{
				return;
			}
			Time.timeScale = 0f;
			AudioSource[] array = UnityEngine.Object.FindObjectsOfType<AudioSource>();
			foreach (AudioSource val in array)
			{
				if (val.isPlaying)
				{
					_pausedSources.Add(val);
					val.Pause();
				}
			}
		}
		else
		{
			if (!(Math.Abs(Time.timeScale - _finalTimescale) > 0.0001f))
			{
				return;
			}
			Time.timeScale = _finalTimescale;
			if (DewNetworkManager.instance.isEndingSession)
			{
				return;
			}
			foreach (AudioSource pausedSource in _pausedSources)
			{
				if ((UnityEngine.Object)(object)pausedSource != null)
				{
					pausedSource.UnPause();
				}
			}
			_pausedSources.Clear();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Time.timeScale = 1f;
	}

	[Server]
	public void ChangeTimescale(float timescale, float duration)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void TimescaleManager::ChangeTimescale(System.Single,System.Single)' called when server was not active");
			return;
		}
		_ongoingTimescaleChanges.Add(new TimescaleChange
		{
			target = timescale,
			remainingDuration = duration,
			initDuration = duration
		});
	}

	public TimescaleManager()
	{
		_Mirror_SyncVarHookDelegate__finalTimescale = TimescaleChanged;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		((NetworkBehaviour)this).SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteFloat(writer, _finalTimescale);
			NetworkWriterExtensions.WriteFloat(writer, effectTimescale__BackingField);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 1L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _finalTimescale);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 2L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, effectTimescale__BackingField);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		((NetworkBehaviour)this).DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _finalTimescale, _Mirror_SyncVarHookDelegate__finalTimescale, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref effectTimescale__BackingField, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 1L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _finalTimescale, _Mirror_SyncVarHookDelegate__finalTimescale, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 2L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref effectTimescale__BackingField, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
