using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public abstract class StackedStatusEffect : StatusEffect
{
	public int maxStack = 5;

	public bool killOnZeroStack = true;

	public bool autoDecay = true;

	public float decayTime = 5f;

	public bool decayAllAtOnce = true;

	public int decayCount = 1;

	public bool resetTimerOnStackChange = true;

	[SaveVar(SaveVarFlags.ApplyAfterCreation)]
	[SyncVar(hook = "OnStackChange")]
	[SerializeField]
	private int _stack = 1;

	[SyncVar]
	private float _lastStackDecayTime;

	private float _baseDecayTime;

	private bool _capturedDecay;

	public Action<int, int> _Mirror_SyncVarHookDelegate__stack;

	public int stack => _stack;

	public float remainingDecayTime
	{
		get
		{
			if (!autoDecay)
			{
				return 0f;
			}
			return decayTime - (float)NetworkTime.time + _lastStackDecayTime;
		}
	}

	public int Network_stack
	{
		get
		{
			return _stack;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref _stack, 4096uL, _Mirror_SyncVarHookDelegate__stack);
		}
	}

	public float Network_lastStackDecayTime
	{
		get
		{
			return _lastStackDecayTime;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _lastStackDecayTime, 8192uL, (Action<float, float>)null);
		}
	}

	public void SetStack(int stack)
	{
		if (autoDecay && resetTimerOnStackChange)
		{
			ResetDecayTimer();
		}
		Network_stack = Mathf.Clamp(stack, 0, maxStack);
	}

	public void AddStack(int value = 1)
	{
		if (autoDecay && resetTimerOnStackChange)
		{
			ResetDecayTimer();
		}
		if (value < 0)
		{
			Debug.LogWarning(string.Format("{0} parameter out of range: {1}", "AddStack", value));
		}
		else
		{
			Network_stack = Mathf.Clamp(_stack + value, 0, maxStack);
		}
	}

	public void RemoveStack(int value = 1)
	{
		if (autoDecay && resetTimerOnStackChange)
		{
			ResetDecayTimer();
		}
		if (value < 0)
		{
			Debug.LogWarning(string.Format("{0} parameter out of range: {1}", "RemoveStack", value));
		}
		else
		{
			Network_stack = Mathf.Clamp(_stack - value, 0, maxStack);
		}
	}

	protected virtual void OnStackChange(int oldStack, int newStack)
	{
		if (((NetworkBehaviour)this).isServer && killOnZeroStack && newStack == 0 && isActive)
		{
			Destroy();
		}
	}

	public void ResetDecayTimer()
	{
		if (!autoDecay)
		{
			Debug.LogWarning($"Tried to reset decay timer of non-decaying StatusEffect: {this}");
		}
		else
		{
			Network_lastStackDecayTime = (float)NetworkTime.time;
		}
	}

	protected override void OnCreate()
	{
		if (!_capturedDecay)
		{
			_baseDecayTime = decayTime;
			_capturedDecay = true;
		}
		else
		{
			decayTime = _baseDecayTime;
		}
		base.OnCreate();
		Network_lastStackDecayTime = (float)NetworkTime.time;
		if (((NetworkBehaviour)this).isServer)
		{
			numberDisplay = stack;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && _stack != 0)
		{
			Network_stack = 0;
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		int? num = ((maxStack > 1) ? new int?(stack) : ((int?)null));
		if (num != numberDisplay)
		{
			numberDisplay = num;
		}
		if (!autoDecay)
		{
			return;
		}
		if ((float)NetworkTime.time - _lastStackDecayTime >= decayTime)
		{
			if (decayAllAtOnce)
			{
				SetStack(0);
			}
			else
			{
				RemoveStack(decayCount);
			}
			ResetDecayTimer();
		}
		if (stack == 0)
		{
			ResetDecayTimer();
		}
	}

	protected StackedStatusEffect()
	{
		_Mirror_SyncVarHookDelegate__stack = OnStackChange;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteInt(writer, _stack);
			NetworkWriterExtensions.WriteFloat(writer, _lastStackDecayTime);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x1000L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, _stack);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x2000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _lastStackDecayTime);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref _stack, _Mirror_SyncVarHookDelegate__stack, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _lastStackDecayTime, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x1000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref _stack, _Mirror_SyncVarHookDelegate__stack, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x2000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _lastStackDecayTime, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
