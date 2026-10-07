using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Mirror;
using UnityEngine;

public class DewNetworkBehaviour : NetworkBehaviour, ILogicUpdate
{
	private static readonly WaitForEndOfFrame _endOfFrame = new WaitForEndOfFrame();

	private static readonly List<DewNetworkBehaviour> _lateStartServerQueue = new List<DewNetworkBehaviour>();

	private static readonly List<DewNetworkBehaviour> _lateStartClientQueue = new List<DewNetworkBehaviour>();

	private static readonly List<DewNetworkBehaviour> _lateStartProcessBuffer = new List<DewNetworkBehaviour>();

	private static bool _lateStartPumpStarted;

	private bool _didRegisterLogic;

	protected virtual void Awake()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		base.syncInterval = 0.01f;
		base.syncMode = (SyncMode)0;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void ResetLateStartPump()
	{
		_lateStartPumpStarted = false;
		_lateStartServerQueue.Clear();
		_lateStartClientQueue.Clear();
		_lateStartProcessBuffer.Clear();
	}

	private static void EnsureLateStartPump()
	{
		if (!_lateStartPumpStarted)
		{
			_lateStartPumpStarted = true;
			Dew.GetCoroutiner().StartCoroutine(RoutineLateStartPump());
		}
	}

	private static IEnumerator RoutineLateStartPump()
	{
		while (true)
		{
			yield return _endOfFrame;
			ProcessLateStartQueue(_lateStartServerQueue, server: true);
			ProcessLateStartQueue(_lateStartClientQueue, server: false);
		}
	}

	private static void ProcessLateStartQueue(List<DewNetworkBehaviour> queue, bool server)
	{
		if (queue.Count == 0)
		{
			return;
		}
		List<DewNetworkBehaviour> lateStartProcessBuffer = _lateStartProcessBuffer;
		lateStartProcessBuffer.AddRange(queue);
		queue.Clear();
		for (int i = 0; i < lateStartProcessBuffer.Count; i++)
		{
			DewNetworkBehaviour dewNetworkBehaviour = lateStartProcessBuffer[i];
			if ((UnityEngine.Object)(object)dewNetworkBehaviour == null || !((Behaviour)(object)dewNetworkBehaviour).isActiveAndEnabled)
			{
				continue;
			}
			if (server)
			{
				try
				{
					dewNetworkBehaviour.OnLateStartServer();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception, (UnityEngine.Object)(object)dewNetworkBehaviour);
				}
			}
			try
			{
				dewNetworkBehaviour.OnLateStart();
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2, (UnityEngine.Object)(object)dewNetworkBehaviour);
			}
		}
		lateStartProcessBuffer.Clear();
	}

	public void InvalidateInstance()
	{
		SpawnManager.InvalidateInstance((Component)(object)this);
	}

	public override void OnStartServer()
	{
		((NetworkBehaviour)this).OnStartServer();
		try
		{
			OnStart();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception, (UnityEngine.Object)(object)this);
		}
		_lateStartServerQueue.Add(this);
		EnsureLateStartPump();
	}

	public override void OnStartClient()
	{
		((NetworkBehaviour)this).OnStartClient();
		if (((NetworkBehaviour)this).isServer)
		{
			return;
		}
		try
		{
			OnStart();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception, (UnityEngine.Object)(object)this);
		}
		_lateStartClientQueue.Add(this);
		EnsureLateStartPump();
		try
		{
			DewNetworkManager.instance.ClientEvent_OnDewNetworkBehaviourStart?.Invoke(this);
		}
		catch (Exception exception2)
		{
			Debug.LogException(exception2);
		}
	}

	public override void OnStopServer()
	{
		((NetworkBehaviour)this).OnStopServer();
		try
		{
			OnStop();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception, (UnityEngine.Object)(object)this);
		}
	}

	public override void OnStopClient()
	{
		((NetworkBehaviour)this).OnStopClient();
		if (((NetworkBehaviour)this).isServer)
		{
			return;
		}
		try
		{
			OnStop();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception, (UnityEngine.Object)(object)this);
		}
		try
		{
			DewNetworkManager.instance.ClientEvent_OnDewNetworkBehaviourStop?.Invoke(this);
		}
		catch (Exception exception2)
		{
			Debug.LogException(exception2);
		}
	}

	public virtual void OnStart()
	{
		LogicUpdateManager.Register(this);
		_didRegisterLogic = true;
	}

	public virtual void OnLateStart()
	{
	}

	public virtual void OnLateStartServer()
	{
	}

	public virtual void OnStop()
	{
		if (_didRegisterLogic)
		{
			LogicUpdateManager.Unregister(this);
			_didRegisterLogic = false;
		}
	}

	protected virtual void OnDestroy()
	{
		if (_didRegisterLogic)
		{
			LogicUpdateManager.Unregister(this);
			_didRegisterLogic = false;
		}
	}

	public virtual void LogicUpdate(float dt)
	{
	}

	public virtual void FrameUpdate()
	{
	}

	protected void GetComponent<T>(out T comp)
	{
		comp = ((Component)this).GetComponent<T>();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void FxPlayNewNetworked(GameObject effect, Vector3 position, Quaternion? rotation)
	{
		DewEffect.PlayNewNetworked(((NetworkBehaviour)this).netIdentity, effect, position, rotation);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void FxPlayNewNetworked(GameObject effect, Entity entity)
	{
		DewEffect.PlayNewNetworked(((NetworkBehaviour)this).netIdentity, effect, entity);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void FxPlayNewNetworked(GameObject effect, Entity entity, Vector3 position, Quaternion? rotation)
	{
		DewEffect.PlayNewNetworked(((NetworkBehaviour)this).netIdentity, effect, entity, position, rotation);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void FxPlayNewNetworked(GameObject effect)
	{
		DewEffect.PlayNewNetworked(((NetworkBehaviour)this).netIdentity, effect);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void FxPlayNetworked(GameObject effect)
	{
		DewEffect.PlayNetworked(((NetworkBehaviour)this).netIdentity, effect);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void FxPlayNetworked(GameObject effect, Vector3 position, Quaternion? rotation)
	{
		DewEffect.PlayNetworked(((NetworkBehaviour)this).netIdentity, effect, position, rotation);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void FxPlayNetworked(GameObject effect, Entity entity)
	{
		DewEffect.PlayNetworked(((NetworkBehaviour)this).netIdentity, effect, entity);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void FxPlayNetworked(GameObject effect, Entity entity, Vector3 position, Quaternion? rotation)
	{
		DewEffect.PlayNetworked(((NetworkBehaviour)this).netIdentity, effect, entity, position, rotation);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void FxStopNetworked(GameObject effect)
	{
		DewEffect.StopNetworked(((NetworkBehaviour)this).netIdentity, effect);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public GameObject FxPlayNew(GameObject effect)
	{
		return DewEffect.PlayNew(effect, ((NetworkBehaviour)this).netIdentity);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public GameObject FxPlayNew(GameObject effect, Vector3 position, Quaternion? rotation)
	{
		return DewEffect.PlayNew(effect, position, rotation, ((NetworkBehaviour)this).netIdentity);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public GameObject FxPlayNew(GameObject effect, Entity attach)
	{
		return DewEffect.PlayNew(effect, attach, ((NetworkBehaviour)this).netIdentity);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public GameObject FxPlayNew(GameObject effect, Entity attach, Vector3 position, Quaternion? rotation)
	{
		return DewEffect.PlayNew(effect, attach, position, rotation, ((NetworkBehaviour)this).netIdentity);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void FxPlayDetached(GameObject inPrefabChild)
	{
		DewEffect.PlayDetached(inPrefabChild, ((NetworkBehaviour)this).netIdentity);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void FxPlayDetached(GameObject inPrefabChild, Vector3 position, Quaternion? rotation)
	{
		DewEffect.PlayDetached(inPrefabChild, position, rotation, ((NetworkBehaviour)this).netIdentity);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void FxPlayDetached(GameObject inPrefabChild, Entity attach)
	{
		DewEffect.PlayDetached(inPrefabChild, attach, ((NetworkBehaviour)this).netIdentity);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void FxPlayDetached(GameObject inPrefabChild, Entity attach, Vector3 position, Quaternion? rotation)
	{
		DewEffect.PlayDetached(inPrefabChild, attach, position, rotation, ((NetworkBehaviour)this).netIdentity);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void FxPlay(GameObject effect)
	{
		DewEffect.Play(effect, ((NetworkBehaviour)this).netIdentity);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void FxPlay(GameObject effect, Vector3 position, Quaternion? rotation)
	{
		DewEffect.Play(effect, position, rotation, ((NetworkBehaviour)this).netIdentity);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void FxPlay(GameObject effect, Entity attach)
	{
		DewEffect.Play(effect, attach, ((NetworkBehaviour)this).netIdentity);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void FxPlay(GameObject effect, Entity attach, Vector3 position, Quaternion? rotation)
	{
		DewEffect.Play(effect, attach, position, rotation, ((NetworkBehaviour)this).netIdentity);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void FxStop(GameObject effect)
	{
		DewEffect.Stop(effect);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void FxApplySpeedMultiplier(GameObject effect, float multiplier)
	{
		DewEffect.ApplySpeedMultiplier(effect, multiplier);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void FxApplySpeedMultiplierNetworked(GameObject effect, float multiplier)
	{
		DewEffect.ApplySpeedMultiplierNetworked(((NetworkBehaviour)this).netIdentity, effect, multiplier);
	}

	private void MirrorProcessed()
	{
	}
}
