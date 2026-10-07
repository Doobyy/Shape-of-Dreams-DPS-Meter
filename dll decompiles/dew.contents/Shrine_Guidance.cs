using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DG.Tweening;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Shrine_Guidance : Shrine, ICustomInteractable
{
	public DewCollider range;

	public float healRatio;

	public int ticks;

	public float tickInterval;

	public float healDelayByDistance;

	public float explodeDelay;

	public GameObject healExplodeEffect;

	public SafeAction<Entity> actionOverride;

	public Transform customPivot;

	public bool canBreak = true;

	public GameObject fxBreakTap;

	public GameObject fxBreak;

	private float _lastBreakTapTime;

	[SyncVar]
	private float _breakProgress;

	public override Transform interactPivot
	{
		get
		{
			if (!(customPivot != null))
			{
				return base.interactPivot;
			}
			return customPivot;
		}
	}

	string ICustomInteractable.nameRawText => DewLocalization.GetUIValue(((object)this).GetType().Name + "_Name");

	string ICustomInteractable.interactActionRawText => DewLocalization.GetUIValue("InGame_Interact_ShrineActivate");

	Cost ICustomInteractable.cost => GetCost(DewPlayer.local.hero).GetValueOrDefault();

	bool ICustomInteractable.canAltInteract
	{
		get
		{
			if (canBreak)
			{
				return DewPlayer.local.hasPolarisEndingUnlocked;
			}
			return false;
		}
	}

	string ICustomInteractable.interactAltActionRawText => DewLocalization.GetUIValue("InGame_Interact_Break");

	float? ICustomInteractable.altInteractProgress => _breakProgress;

	public float Network_breakProgress
	{
		get
		{
			return _breakProgress;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _breakProgress, 256uL, (Action<float, float>)null);
		}
	}

	protected override bool OnUse(Entity entity)
	{
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		return true;
		IEnumerator HealRoutine(Entity e, float delay)
		{
			yield return new WaitForSeconds(delay);
			if (actionOverride == null || actionOverride.Count == 0)
			{
				OnHealEntity(e);
				yield break;
			}
			try
			{
				actionOverride?.Invoke(e);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(explodeDelay);
			FxPlayNewNetworked(healExplodeEffect);
			List<Entity> entities = range.GetEntities(out var handle, new CollisionCheckSettings
			{
				includeUncollidable = true
			});
			for (int i = 0; i < entities.Count; i++)
			{
				Entity entity2 = entities[i];
				if (entity2.owner.isHumanPlayer)
				{
					((MonoBehaviour)(object)this).StartCoroutine(HealRoutine(entity2, Vector3.Distance(position, entity2.position) * healDelayByDistance));
				}
			}
			handle.Return();
		}
	}

	public virtual void OnHealEntity(Entity e)
	{
		CreateStatusEffect(e, default, (Se_GenericHealOverTime h) =>
		{
			h.ticks = ticks;
			h.tickInterval = tickInterval;
			h.totalAmount = healRatio * e.maxHealth;
		});
	}

	public override Cost? GetCost(Entity activator)
	{
		Cost? cost = base.GetCost(activator);
		if (!activator.Status.TryGetStatusEffect<Se_Star_L_GuidanceShrineDiscount>(out var effect))
		{
			return cost;
		}
		if (cost.HasValue)
		{
			return (Cost)(cost * (1f - effect.GetValue(effect.discountRatio))).Value;
		}
		return null;
	}

	public override void OnInteract(Entity entity, bool alt)
	{
		if (!alt)
		{
			base.OnInteract(entity, alt);
		}
		else
		{
			if (!((NetworkBehaviour)this).isServer || !entity.owner.hasPolarisEndingUnlocked || Time.time - _lastBreakTapTime < 0.15f || !canBreak)
			{
				return;
			}
			FxPlayNewNetworked(fxBreakTap);
			RpcShakeModel();
			_lastBreakTapTime = Time.time;
			Network_breakProgress = _breakProgress + 0.3f;
			if (!(_breakProgress >= 1f))
			{
				return;
			}
			FxPlayNewNetworked(fxBreak);
			foreach (Hero allHero in NetworkedManagerBase<ActorManager>.instance.allHeroes)
			{
				if (allHero.Status.TryGetStatusEffect<Se_StarlessPath_PowerOfGuidance>(out var effect))
				{
					effect.AddStack();
				}
				else
				{
					allHero.CreateStatusEffect<Se_StarlessPath_PowerOfGuidance>(allHero, new CastInfo(allHero));
				}
				if (!allHero.IsNullInactiveDeadOrKnockedOut())
				{
					CreateAbilityInstance(position, null, new CastInfo(allHero, allHero), (Ai_StarlessPath_PowerOfGuidance_MockProjectile ai) =>
					{
						ai.SetCustomStartPosition(position + Vector3.up * 2f);
					});
				}
			}
			Gem_U_GuidingCompass_NotCharged gem_U_GuidingCompass_NotCharged = null;
			foreach (Actor allActor in NetworkedManagerBase<ActorManager>.instance.allActors)
			{
				if (allActor is Gem_U_GuidingCompass_NotCharged gem_U_GuidingCompass_NotCharged2 && !((UnityEngine.Object)(object)gem_U_GuidingCompass_NotCharged2.owner == null))
				{
					gem_U_GuidingCompass_NotCharged = gem_U_GuidingCompass_NotCharged2;
					break;
				}
			}
			if ((UnityEngine.Object)(object)gem_U_GuidingCompass_NotCharged != null)
			{
				gem_U_GuidingCompass_NotCharged.quality += gem_U_GuidingCompass_NotCharged.addedPerGuidanceBreak;
			}
			NetworkedManagerBase<ChatManager>.instance.BroadcastMessage(new ChatManager.Message
			{
				type = ChatManager.MessageType.Notice,
				content = "Shrine_Guidance_ReceivePowerOfGuidance"
			});
			Destroy();
		}
	}

	[ClientRpc]
	private void RpcShakeModel()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Shrine_Guidance::RpcShakeModel()", -20290119, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && Time.time - _lastBreakTapTime > 1.5f && _breakProgress != 0f)
		{
			Network_breakProgress = 0f;
		}
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcShakeModel()
	{
		if ((bool)model)
		{
			ShortcutExtensions.DOShakePosition(model.transform, 0.25f, 0.25f, 50, 90f, false, true, (ShakeRandomnessMode)0);
		}
		if ((bool)availableEffect)
		{
			ShortcutExtensions.DOShakePosition(availableEffect.transform, 0.25f, 0.25f, 50, 90f, false, true, (ShakeRandomnessMode)0);
		}
	}

	protected static void InvokeUserCode_RpcShakeModel(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcShakeModel called on server.");
		}
		else
		{
			((Shrine_Guidance)(object)obj).UserCode_RpcShakeModel();
		}
	}

	static Shrine_Guidance()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Shrine_Guidance), "System.Void Shrine_Guidance::RpcShakeModel()", (RemoteCallDelegate)InvokeUserCode_RpcShakeModel);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteFloat(writer, _breakProgress);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _breakProgress);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _breakProgress, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x100L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _breakProgress, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
