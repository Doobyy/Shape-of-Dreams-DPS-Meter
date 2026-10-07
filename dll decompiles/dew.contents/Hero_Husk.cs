using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Hero_Husk : Hero
{
	private static readonly int CloseValue = Animator.StringToHash("CloseValue");

	[NonSerialized]
	public GameObject fxOpenBlade;

	[NonSerialized]
	public GameObject fxCloseBlade;

	[NonSerialized]
	public Animator bladeAnimator;

	[SyncVar(hook = "OnIsBladeOpenChanged")]
	private bool _isBladeOpen;

	private float _closeValueLinear;

	private float _lastBladeOpenTime = float.NegativeInfinity;

	public Action<bool, bool> _Mirror_SyncVarHookDelegate__isBladeOpen;

	public bool Network_isBladeOpen
	{
		get
		{
			return _isBladeOpen;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref _isBladeOpen, 1024uL, _Mirror_SyncVarHookDelegate__isBladeOpen);
		}
	}

	private void OnIsBladeOpenChanged(bool oldValue, bool newValue)
	{
		if (_isBladeOpen)
		{
			FxPlay(fxOpenBlade);
		}
		else
		{
			FxPlay(fxCloseBlade);
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			ActorEvent_OnAbilityInstanceCreated += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceCreated);
		}
	}

	public override void OnModelLoaded()
	{
		base.OnModelLoaded();
		fxOpenBlade = Visual.model.GetCustomMapping<GameObject>("fxOpenBlade");
		fxCloseBlade = Visual.model.GetCustomMapping<GameObject>("fxCloseBlade");
		bladeAnimator = Visual.model.GetCustomMapping<Animator>("bladeAnimator");
	}

	private void ActorEventOnAbilityInstanceCreated(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_Q_Laceration_Dash)
		{
			_lastBladeOpenTime = Time.time;
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		if (!this.IsNullOrInactive() && !((UnityEngine.Object)(object)bladeAnimator == null))
		{
			_closeValueLinear = Mathf.MoveTowards(_closeValueLinear, _isBladeOpen ? 0f : 1f, Time.deltaTime * (_isBladeOpen ? 3f : 1f));
			bladeAnimator.SetFloat(CloseValue, EasingFunction.EaseOutQuart(0f, 1f, _closeValueLinear));
			if (((NetworkBehaviour)this).isServer)
			{
				Network_isBladeOpen = Time.time - _lastBladeOpenTime < 1f;
			}
		}
	}

	public Hero_Husk()
	{
		_Mirror_SyncVarHookDelegate__isBladeOpen = OnIsBladeOpenChanged;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteBool(writer, _isBladeOpen);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x400L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, _isBladeOpen);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isBladeOpen, _Mirror_SyncVarHookDelegate__isBladeOpen, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x400L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isBladeOpen, _Mirror_SyncVarHookDelegate__isBladeOpen, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
