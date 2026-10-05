using System;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Se_GenericStealth : StatusEffect
{
	public GameObject fxRevealRangeIndicator;

	public float endOnTakenDamageGracePeriod = 1f;

	public float revealOnProximityGracePeriod = 0.65f;

	[SyncVar]
	public float revealRange = 1.5f;

	public float revealDuration = 1.2f;

	private bool _isRevealRangeIndicatorActive;

	private float _lastPopupTextPrintTime;

	private float _lastRevealCheckTime;

	private AbilityTargetValidator _revealable;

	private Action<EventInfoDamage> _cachedEntityEventOnTakeDamage;

	private Action<EventInfoCast> _cachedEntityEventOnCastComplete;

	private Action _cachedUpdateIconVisibility;

	private float _prefabEndOnTakenDamageGracePeriod;

	private float _prefabRevealOnProximityGracePeriod;

	private float _prefabRevealRange;

	private float _prefabRevealDuration;

	public override bool reuseInRoom => true;

	public float NetworkrevealRange
	{
		get
		{
			return revealRange;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref revealRange, 4096uL, (Action<float, float>)null);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_prefabEndOnTakenDamageGracePeriod = endOnTakenDamageGracePeriod;
		_prefabRevealOnProximityGracePeriod = revealOnProximityGracePeriod;
		_prefabRevealRange = revealRange;
		_prefabRevealDuration = revealDuration;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		endOnTakenDamageGracePeriod = _prefabEndOnTakenDamageGracePeriod;
		revealOnProximityGracePeriod = _prefabRevealOnProximityGracePeriod;
		NetworkrevealRange = _prefabRevealRange;
		revealDuration = _prefabRevealDuration;
		_isRevealRangeIndicatorActive = false;
		_lastPopupTextPrintTime = 0f;
		_lastRevealCheckTime = 0f;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			fxRevealRangeIndicator.transform.localScale = (revealRange + 0.3f) * Vector3.one;
			_revealable = new AbilityTargetValidator
			{
				targets = EntityRelation.Enemy
			};
			DoInvisible();
			DestroyOnDeath(victim, includeKnockOuts: true);
			victim.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(EntityEventOnTakeDamage);
			victim.EntityEvent_OnCastComplete += new Action<EventInfoCast>(EntityEventOnCastComplete);
			victim.Status.ClientEvent_OnStatsCalculated += new Action(UpdateIconVisibility);
			UpdateIconVisibility();
			victim.Status.hasStealth = true;
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || Time.time - creationTime < revealOnProximityGracePeriod)
		{
			return;
		}
		if (!_isRevealRangeIndicatorActive)
		{
			_isRevealRangeIndicatorActive = true;
			FxPlayNetworked(fxRevealRangeIndicator, victim);
		}
		if (!(Time.time - _lastRevealCheckTime < 0.25f))
		{
			if (DewPhysics.OverlapCircleAllEntities(out var handle, victim.agentPosition, revealRange, _revealable, victim, new CollisionCheckSettings
			{
				includeUncollidable = true
			}).Count > 0)
			{
				Reveal(victim, revealDuration);
			}
			handle.Return();
		}
	}

	private void EntityEventOnCastComplete(EventInfoCast obj)
	{
		if (!(Time.time - creationTime < 0.05f))
		{
			Destroy();
		}
	}

	private void EntityEventOnTakeDamage(EventInfoDamage obj)
	{
		if (!isActive)
		{
			return;
		}
		if (Time.time - creationTime < endOnTakenDamageGracePeriod)
		{
			if (Time.time - _lastPopupTextPrintTime > 0.15f)
			{
				RpcPrintUnrevealable();
				_lastPopupTextPrintTime = Time.time;
			}
		}
		else
		{
			Destroy();
		}
	}

	[ClientRpc]
	private void RpcPrintUnrevealable()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Se_GenericStealth::RpcPrintUnrevealable()", 1481957214, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		FxStopNetworked(fxRevealRangeIndicator);
		if ((bool)(UnityEngine.Object)(object)victim)
		{
			victim.Status.hasStealth = false;
			victim.EntityEvent_OnTakeDamage -= _cachedEntityEventOnTakeDamage;
			victim.EntityEvent_OnCastComplete -= _cachedEntityEventOnCastComplete;
			victim.Status.ClientEvent_OnStatsCalculated -= _cachedUpdateIconVisibility;
			if (victim.Status.TryGetStatusEffect<Se_GenericRevealed>(out var effect))
			{
				effect.Destroy();
			}
		}
	}

	private void UpdateIconVisibility()
	{
		bool isUndetectableByNonAllies = victim.Status.isUndetectableByNonAllies;
		if (showIcon != isUndetectableByNonAllies)
		{
			showIcon = isUndetectableByNonAllies;
		}
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcPrintUnrevealable()
	{
		InGameUIManager.instance.ShowWorldPopMessage(new WorldMessageSetting
		{
			color = Color.white,
			worldPosGetter = () => victim.Visual.GetCenterPosition(),
			rawText = DewLocalization.GetUIValue("InGame_Unrevealable")
		});
	}

	protected static void InvokeUserCode_RpcPrintUnrevealable(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPrintUnrevealable called on server.");
		}
		else
		{
			((Se_GenericStealth)(object)obj).UserCode_RpcPrintUnrevealable();
		}
	}

	static Se_GenericStealth()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Se_GenericStealth), "System.Void Se_GenericStealth::RpcPrintUnrevealable()", (RemoteCallDelegate)InvokeUserCode_RpcPrintUnrevealable);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteFloat(writer, revealRange);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x1000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, revealRange);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref revealRange, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x1000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref revealRange, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
