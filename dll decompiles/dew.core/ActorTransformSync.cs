using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

[LogicUpdatePriority(-201)]
public class ActorTransformSync : DewNetworkBehaviour
{
	[Header("Channels (only enabled channels are sent)")]
	public bool syncPosition = true;

	public bool syncRotation;

	public bool syncScale;

	[Header("Sync Settings")]
	[Tooltip("If the predicted and render positions diverge by more than this distance, snap instantly without smoothing.")]
	public float syncFixWarpDistance = 5f;

	[Tooltip("If the predicted and render rotations diverge by more than this angle (degrees), snap instantly without smoothing.")]
	public float syncFixWarpAngle = 90f;

	[Tooltip("If the predicted and render scales diverge by more than this magnitude, snap instantly without smoothing.")]
	public float syncFixWarpScale = 2f;

	[Tooltip("SmoothDamp time (seconds) for the render pose to chase the predicted pose. Smaller = faster, less smooth.")]
	public float syncFixSmoothTime = 0.1f;

	[Tooltip("Max time (seconds) to predict (extrapolate) the future from velocity. Prevents runaway on packet loss.")]
	public float extrapolateMaxTime = 0.15f;

	[Tooltip("If no new sample arrives for this long (seconds), treat velocity as zero to stop extrapolating.")]
	public float velocityLifetime = 1f;

	[SyncVar(hook = "OnSyncDataReceived")]
	private SyncMovementData _syncData;

	private SyncTransformChannel _channels;

	private bool _hasData;

	private Vector3 _renderPos;

	private Vector3 _renderPosCv;

	private Quaternion _renderRot;

	private Quaternion _renderRotCv;

	private Vector3 _renderScale;

	private Vector3 _renderScaleCv;

	private double _serverLastCaptureTime;

	private bool _serverTickPending;

	public Action<SyncMovementData, SyncMovementData> _Mirror_SyncVarHookDelegate__syncData;

	public SyncMovementData Network_syncData
	{
		get
		{
			return _syncData;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<SyncMovementData>(value, ref _syncData, 1uL, _Mirror_SyncVarHookDelegate__syncData);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_channels = SyncTransformChannel.None;
		if (syncPosition)
		{
			_channels |= SyncTransformChannel.Position;
		}
		if (syncRotation)
		{
			_channels |= SyncTransformChannel.Rotation;
		}
		if (syncScale)
		{
			_channels |= SyncTransformChannel.Scale;
		}
	}

	public override void OnStopServer()
	{
		base.OnStopServer();
		_hasData = false;
		_serverTickPending = false;
	}

	public override void OnStopClient()
	{
		base.OnStopClient();
		_hasData = false;
	}

	[Server]
	public void NotifyTeleport(Vector3 newPos)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ActorTransformSync::NotifyTeleport(UnityEngine.Vector3)' called when server was not active");
			return;
		}
		((Component)(object)this).transform.position = newPos;
		_serverLastCaptureTime = NetworkTime.time;
		Network_syncData = new SyncMovementData
		{
			channels = _channels,
			isTeleport = true,
			timestamp = NetworkTime.time,
			position = ((Component)(object)this).transform.position,
			rotation = ((Component)(object)this).transform.rotation,
			scale = ((Component)(object)this).transform.localScale
		};
		_hasData = true;
		SnapRenderTo(_syncData);
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (_hasData)
		{
			if (_syncData.Has(SyncTransformChannel.Position))
			{
				((Component)(object)this).transform.position = _syncData.position;
			}
			if (_syncData.Has(SyncTransformChannel.Rotation))
			{
				((Component)(object)this).transform.rotation = _syncData.rotation;
			}
			if (_syncData.Has(SyncTransformChannel.Scale))
			{
				((Component)(object)this).transform.localScale = _syncData.scale;
			}
		}
		_serverTickPending = true;
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (((NetworkBehaviour)this).isServer && _serverTickPending)
		{
			_serverTickPending = false;
			CaptureServerAuthority();
		}
		if (!_hasData)
		{
			return;
		}
		float num = (float)(NetworkTime.time - _syncData.timestamp);
		float num2 = Mathf.Clamp(num, 0f, extrapolateMaxTime);
		bool flag = num > velocityLifetime;
		if (_syncData.Has(SyncTransformChannel.Position))
		{
			Vector3 vector = (flag ? Vector3.zero : _syncData.velocity);
			Vector3 vector2 = _syncData.position + vector * num2;
			if ((vector2 - _renderPos).sqrMagnitude > syncFixWarpDistance * syncFixWarpDistance)
			{
				_renderPos = vector2;
				_renderPosCv = Vector3.zero;
			}
			else
			{
				_renderPos = Vector3.SmoothDamp(_renderPos, vector2, ref _renderPosCv, syncFixSmoothTime);
			}
			((Component)(object)this).transform.position = _renderPos;
		}
		if (_syncData.Has(SyncTransformChannel.Rotation))
		{
			Vector3 vector3 = (flag ? Vector3.zero : _syncData.angularVelocity);
			Quaternion quaternion = _syncData.rotation;
			float num3 = vector3.magnitude * num2;
			if (num3 > 0.0001f)
			{
				quaternion = Quaternion.AngleAxis(num3, vector3.normalized) * _syncData.rotation;
			}
			if (Quaternion.Angle(quaternion, _renderRot) > syncFixWarpAngle)
			{
				_renderRot = quaternion;
				_renderRotCv = default;
			}
			else
			{
				_renderRot = SmoothDampRotation(_renderRot, quaternion, ref _renderRotCv, syncFixSmoothTime);
			}
			((Component)(object)this).transform.rotation = _renderRot;
		}
		if (_syncData.Has(SyncTransformChannel.Scale))
		{
			Vector3 vector4 = (flag ? Vector3.zero : _syncData.scaleVelocity);
			Vector3 vector5 = _syncData.scale + vector4 * num2;
			if ((vector5 - _renderScale).sqrMagnitude > syncFixWarpScale * syncFixWarpScale)
			{
				_renderScale = vector5;
				_renderScaleCv = Vector3.zero;
			}
			else
			{
				_renderScale = Vector3.SmoothDamp(_renderScale, vector5, ref _renderScaleCv, syncFixSmoothTime);
			}
			((Component)(object)this).transform.localScale = _renderScale;
		}
	}

	private void CaptureServerAuthority()
	{
		bool flag = !_hasData;
		double time = NetworkTime.time;
		double num = (flag ? 0.0 : (time - _serverLastCaptureTime));
		bool flag2 = !flag && num > 0.0001;
		SyncMovementData network_syncData = new SyncMovementData
		{
			channels = _channels,
			isTeleport = false,
			timestamp = time
		};
		if ((_channels & SyncTransformChannel.Position) != 0)
		{
			network_syncData.position = ((Component)(object)this).transform.position;
			network_syncData.velocity = (flag2 ? ((network_syncData.position - _syncData.position) / (float)num) : Vector3.zero);
		}
		if ((_channels & SyncTransformChannel.Rotation) != 0)
		{
			network_syncData.rotation = ((Component)(object)this).transform.rotation;
			network_syncData.angularVelocity = (flag2 ? ComputeAngularVelocity(_syncData.rotation, network_syncData.rotation, (float)num) : Vector3.zero);
		}
		if ((_channels & SyncTransformChannel.Scale) != 0)
		{
			network_syncData.scale = ((Component)(object)this).transform.localScale;
			network_syncData.scaleVelocity = (flag2 ? ((network_syncData.scale - _syncData.scale) / (float)num) : Vector3.zero);
		}
		_serverLastCaptureTime = time;
		_hasData = true;
		Network_syncData = network_syncData;
		if (flag)
		{
			SnapRenderTo(_syncData);
		}
	}

	private void OnSyncDataReceived(SyncMovementData oldData, SyncMovementData newData)
	{
		if (((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (!_hasData)
		{
			_hasData = true;
			SnapRenderTo(newData);
			return;
		}
		bool flag = newData.isTeleport;
		if (!flag && newData.Has(SyncTransformChannel.Position))
		{
			flag = (newData.position - _renderPos).sqrMagnitude > syncFixWarpDistance * syncFixWarpDistance;
		}
		if (!flag && newData.Has(SyncTransformChannel.Rotation))
		{
			flag = Quaternion.Angle(newData.rotation, _renderRot) > syncFixWarpAngle;
		}
		if (!flag && newData.Has(SyncTransformChannel.Scale))
		{
			flag = (newData.scale - _renderScale).sqrMagnitude > syncFixWarpScale * syncFixWarpScale;
		}
		if (flag)
		{
			SnapRenderTo(newData);
		}
	}

	private void SnapRenderTo(SyncMovementData d)
	{
		if (d.Has(SyncTransformChannel.Position))
		{
			_renderPos = d.position;
			_renderPosCv = Vector3.zero;
			((Component)(object)this).transform.position = d.position;
		}
		if (d.Has(SyncTransformChannel.Rotation))
		{
			_renderRot = d.rotation;
			_renderRotCv = default;
			((Component)(object)this).transform.rotation = d.rotation;
		}
		if (d.Has(SyncTransformChannel.Scale))
		{
			_renderScale = d.scale;
			_renderScaleCv = Vector3.zero;
			((Component)(object)this).transform.localScale = d.scale;
		}
	}

	private static Vector3 ComputeAngularVelocity(Quaternion from, Quaternion to, float dt)
	{
		(to * Quaternion.Inverse(from)).ToAngleAxis(out var angle, out var axis);
		if (float.IsInfinity(axis.x) || float.IsNaN(axis.x) || Mathf.Abs(angle) < 0.0001f)
		{
			return Vector3.zero;
		}
		if (angle > 180f)
		{
			angle -= 360f;
		}
		return axis * (angle / dt);
	}

	private static Quaternion SmoothDampRotation(Quaternion current, Quaternion target, ref Quaternion vel, float smoothTime)
	{
		if (smoothTime < Mathf.Epsilon)
		{
			return target;
		}
		if (Quaternion.Dot(current, target) < 0f)
		{
			target = new Quaternion(0f - target.x, 0f - target.y, 0f - target.z, 0f - target.w);
		}
		return new Quaternion(Mathf.SmoothDamp(current.x, target.x, ref vel.x, smoothTime), Mathf.SmoothDamp(current.y, target.y, ref vel.y, smoothTime), Mathf.SmoothDamp(current.z, target.z, ref vel.z, smoothTime), Mathf.SmoothDamp(current.w, target.w, ref vel.w, smoothTime)).normalized;
	}

	public ActorTransformSync()
	{
		_Mirror_SyncVarHookDelegate__syncData = OnSyncDataReceived;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		((NetworkBehaviour)this).SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteSyncMovementData(_syncData);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteSyncMovementData(_syncData);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		((NetworkBehaviour)this).DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<SyncMovementData>(ref _syncData, _Mirror_SyncVarHookDelegate__syncData, reader.ReadSyncMovementData());
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 1L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<SyncMovementData>(ref _syncData, _Mirror_SyncVarHookDelegate__syncData, reader.ReadSyncMovementData());
		}
	}
}
