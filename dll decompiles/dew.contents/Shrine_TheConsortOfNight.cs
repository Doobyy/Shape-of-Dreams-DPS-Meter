using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Shrine_TheConsortOfNight : Shrine, ICustomInteractable
{
	public GameObject fxTeleport;

	public GameObject fxReward;

	public GameObject fxStopOnUse;

	public float rewardDelay;

	public float hopDuration;

	public float teleportDuration;

	private Ge_TheConsortOfNight _ge;

	private Animator _animator;

	private float _elapsedTime;

	public string nameRawText => DewLocalization.GetUIValue(((object)this).GetType().Name + "_Name");

	public string interactActionRawText => DewLocalization.GetUIValue("InGame_Interact_Release");

	protected override void OnCreate()
	{
		base.OnCreate();
		_animator = ((Component)(object)this).GetComponentInChildren<Animator>();
	}

	protected override bool OnUse(Entity entity)
	{
		List<RoomModifierBase> modifierInstances = SingletonDewNetworkBehaviour<Room>.instance.modifiers.modifierInstances;
		for (int i = 0; i < modifierInstances.Count; i++)
		{
			if (modifierInstances[i] is RoomMod_TheConsortOfNight_Raven_Marker roomMod_TheConsortOfNight_Raven_Marker)
			{
				roomMod_TheConsortOfNight_Raven_Marker.RemoveModifier();
			}
		}
		_ge = Dew.FindActorOfType<Ge_TheConsortOfNight>();
		if ((Object)(object)_ge == null)
		{
			_ge = Dew.CreateActor<Ge_TheConsortOfNight>();
		}
		if (_ge.count == 0)
		{
			NetworkedManagerBase<QuestManager>.instance.StartQuest<Quest_TheConsortOfNight>();
		}
		PlayAnimation();
		Ge_TheConsortOfNight ge = _ge;
		ge.Networkcount = ge.count + 1;
		if (_ge.count >= _ge.countForAction)
		{
			foreach (Entity item in new List<Entity>(NetworkedManagerBase<ActorManager>.instance.allEntities))
			{
				if (!item.IsNullInactiveDeadOrKnockedOut() && item is Monster)
				{
					item.Kill();
				}
			}
			SingletonDewNetworkBehaviour<Room>.instance.monsters.FinishAllOngoingSpawns();
			foreach (RoomSection section in SingletonDewNetworkBehaviour<Room>.instance.sections)
			{
				section.monsters.combatAreaSettings = SectionCombatAreaType.No;
				section.monsters.isMarkedAsCombatArea = false;
				section.monsters.didClearCombatArea = true;
			}
			SingletonDewNetworkBehaviour<Room>.instance.ClearRoom();
			CreateActor<Shrine_TheConsortOfNight_Rift>(position, Quaternion.AngleAxis(ManagerBase<CameraManager>.instance.entityCamAngle + 180f, Vector3.up));
			NetworkedManagerBase<ChatManager>.instance.BroadcastMessage(new ChatManager.Message
			{
				type = ChatManager.MessageType.AdvText,
				content = "<i><color=#b08cff>" + DewAdvText.Adv_LocUI("Chat_Notice_TheConsortOfNightRiftOpened") + "</color></i>"
			});
		}
		return true;
	}

	[ClientRpc]
	private void PlayAnimation()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Shrine_TheConsortOfNight::PlayAnimation()", -1110539369, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Command(requiresAuthority = false)]
	private void RewardGiveRoutine()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendCommandInternal("System.Void Shrine_TheConsortOfNight::RewardGiveRoutine()", -1986715989, (NetworkWriter)(object)val, 0, false);
		NetworkWriterPool.Return(val);
	}

	public override bool ShouldBeSavedWithRoom()
	{
		return totalUseCount == 0;
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_PlayAnimation()
	{
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			_animator.Play("Hop");
			if (((NetworkBehaviour)this).isServer)
			{
				RewardGiveRoutine();
			}
			yield return new WaitForSeconds(hopDuration);
			_animator.Play("Teleport");
			ShortcutExtensions.DOMove(model.transform, ((Component)(object)this).transform.position + Vector3.up * 4f, 1.5f, false);
			yield return new WaitForSeconds(1.5f);
			Vector3 localScale = model.transform.localScale;
			TweenSettingsExtensions.Append(TweenSettingsExtensions.Append(TweenSettingsExtensions.AppendCallback(DOTween.Sequence(), (TweenCallback)(() =>
			{
				model.SetActive(value: false);
			})), (Tween)(object)ShortcutExtensions.DOScale(model.transform, localScale, 0.5f)), (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(model.transform, 0.01f, 3f), (Ease)6));
			FxPlay(fxTeleport);
			yield return new WaitForSeconds(teleportDuration);
			FxStop(fxTeleport);
			if (((NetworkBehaviour)this).isServer)
			{
				DestroyIfActive();
			}
		}
	}

	protected static void InvokeUserCode_PlayAnimation(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC PlayAnimation called on server.");
		}
		else
		{
			((Shrine_TheConsortOfNight)(object)obj).UserCode_PlayAnimation();
		}
	}

	protected void UserCode_RewardGiveRoutine()
	{
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(rewardDelay);
			FxPlayNetworked(fxReward);
			NetworkedManagerBase<PickupManager>.instance.DropStarDust(6, Dew.GetGoodRewardPosition(position, 1f));
		}
	}

	protected static void InvokeUserCode_RewardGiveRoutine(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command RewardGiveRoutine called on client.");
		}
		else
		{
			((Shrine_TheConsortOfNight)(object)obj).UserCode_RewardGiveRoutine();
		}
	}

	static Shrine_TheConsortOfNight()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected Obj, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterCommand(typeof(Shrine_TheConsortOfNight), "System.Void Shrine_TheConsortOfNight::RewardGiveRoutine()", (RemoteCallDelegate)InvokeUserCode_RewardGiveRoutine, false);
		RemoteProcedureCalls.RegisterRpc(typeof(Shrine_TheConsortOfNight), "System.Void Shrine_TheConsortOfNight::PlayAnimation()", (RemoteCallDelegate)InvokeUserCode_PlayAnimation);
	}
}
