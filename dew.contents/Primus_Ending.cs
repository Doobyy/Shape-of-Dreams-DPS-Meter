using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Primus_Ending : SingletonDewNetworkBehaviour<Primus_Ending>
{
	public GameObject fxPrimusDeathLoop;

	public GameObject fxAfterCutsceneLoop;

	public GameObject fxThresholdBeforeCutsceneLoop;

	public Transform thresholdTeleportPos;

	public GameObject fxBeforeCutscene;

	public float beforeCutsceneDelay;

	public DewCutsceneDirector endingCutscene;

	[Space]
	public Transform heroFootstepsParent;

	public GameObject fxDefaultFootstep;

	[Space]
	public List<RuntimeAnimatorController> heroControllers;

	private CameraModifierZoom _zoom;

	private void DebugQuickEnd()
	{
		Entity[] array = NetworkedManagerBase<ActorManager>.instance.allEntities.ToArray();
		foreach (Entity entity in array)
		{
			if (!entity.IsNullInactiveDeadOrKnockedOut() && (UnityEngine.Object)(object)entity.owner == (UnityEngine.Object)(object)DewPlayer.creep)
			{
				entity.Destroy();
			}
		}
		ManagerBase<MusicManager>.instance.Stop();
		NetworkedManagerBase<QuestManager>.instance.RemoveArtifact();
		List<Hero> list = (from p in DewPlayer.gamePlayers
			where !p.hero.IsNullOrInactive()
			select p.hero).ToList();
		List<Entity> list2 = new List<Entity>();
		list2.AddRange(list);
		foreach (Hero item in list)
		{
			list2.AddRange(item.summons);
		}
		Vector3 positionOnGround = Dew.GetPositionOnGround(Dew.FindActorOfType<Shrine_PrimusEndingLightPillar>().position);
		foreach (Entity item2 in list2)
		{
			StatusEffect[] array2 = item2.Status.statusEffects.ToArray();
			foreach (StatusEffect statusEffect in array2)
			{
				if (!statusEffect.IsNullOrInactive() && (statusEffect is Se_HeroKnockedOut || statusEffect is ElementalStatusEffect || statusEffect is Se_MirageSkin_Delusion_Delusional || statusEffect is CurseStatusEffect))
				{
					statusEffect.Destroy();
				}
			}
			item2.Status.SetHealth(item2.maxHealth);
			item2.Control.Stop();
			item2.Control.CancelOngoingChannels();
			item2.Control.CancelOngoingDisplacement();
			item2.Ability.GetNewAbilityLockHandle().LockAllAbilitiesCast();
			item2.Ability.GetNewAbilityLockHandle().LockAllMainSkillsEdit();
			item2.Control.Teleport(positionOnGround);
			item2.Control.Rotate(thresholdTeleportPos.forward, immediately: true);
			item2.CreateBasicEffect(item2, new SlowEffect
			{
				strength = 25f
			}, float.PositiveInfinity);
			if (item2 is Summon summon)
			{
				summon.AddDuration(float.PositiveInfinity, clampToMaxDuration: false);
			}
		}
		RpcSetIsDoingEnding();
		FxPlayNetworked(fxThresholdBeforeCutsceneLoop);
	}

	[Server]
	public void StartPrimusDeath()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Primus_Ending::StartPrimusDeath()' called when server was not active");
		}
		else
		{
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			Entity[] array = NetworkedManagerBase<ActorManager>.instance.allEntities.ToArray();
			foreach (Entity entity in array)
			{
				if (!entity.IsNullInactiveDeadOrKnockedOut() && (UnityEngine.Object)(object)entity.owner == (UnityEngine.Object)(object)DewPlayer.creep)
				{
					entity.Destroy();
				}
			}
			RpcDisableActions();
			FxPlayNetworked(fxPrimusDeathLoop);
			yield return new WaitForSeconds(4.5f);
			RpcSetWhiteFade(value: true);
			yield return new WaitForSeconds(4f);
			yield return new WaitForSeconds(0.5f);
			RpcZoomInCamera();
			FxStopNetworked(fxPrimusDeathLoop);
			NetworkedManagerBase<QuestManager>.instance.RemoveArtifact();
			List<Hero> list = (from p in DewPlayer.gamePlayers
				where !p.hero.IsNullOrInactive()
				select p.hero).ToList();
			List<Entity> list2 = new List<Entity>();
			list2.AddRange(list);
			foreach (Hero h in list)
			{
				h.Skill.StopHoldInHand();
				list2.AddRange(h.summons);
				h.CreateBasicEffect(h, new DeathInterruptEffect
				{
					onInterrupt = (EventInfoKill _) =>
					{
						h.Status.SetHealth(1f);
					}
				}, 3600f);
			}
			foreach (Entity item in list2)
			{
				StatusEffect[] array2 = item.Status.statusEffects.ToArray();
				foreach (StatusEffect statusEffect in array2)
				{
					if (!statusEffect.IsNullOrInactive() && (statusEffect is Se_HeroKnockedOut || statusEffect is ElementalStatusEffect || statusEffect is Se_MirageSkin_Delusion_Delusional || statusEffect is CurseStatusEffect))
					{
						statusEffect.Destroy();
					}
				}
				item.Status.SetHealth(item.maxHealth);
				item.Control.Stop();
				item.Control.CancelOngoingChannels();
				item.Control.CancelOngoingDisplacement();
				item.Ability.GetNewAbilityLockHandle().LockAllAbilitiesCast();
				item.Ability.GetNewAbilityLockHandle().LockAllMainSkillsEdit();
				Vector3 positionOnGround = Dew.GetPositionOnGround(thresholdTeleportPos.position + UnityEngine.Random.onUnitSphere.Flattened() * 1.5f);
				item.Control.Teleport(positionOnGround);
				item.Control.Rotate(thresholdTeleportPos.forward, immediately: true);
				item.Control.StartDaze(4f);
				item.CreateBasicEffect(item, new SlowEffect
				{
					strength = 25f
				}, float.PositiveInfinity);
				if (item is Summon summon)
				{
					summon.AddDuration(float.PositiveInfinity, clampToMaxDuration: false);
				}
			}
			RpcSetIsDoingEnding();
			yield return new WaitForSeconds(3f);
			RpcZoomOutCamera();
			RpcSetWhiteFade(value: false);
			FxPlayNetworked(fxThresholdBeforeCutsceneLoop);
		}
	}

	[Server]
	public void StartEndingCutscene()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Primus_Ending::StartEndingCutscene()' called when server was not active");
		}
		else
		{
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			FxStopNetworked(fxThresholdBeforeCutsceneLoop);
			FxPlayNetworked(fxBeforeCutscene);
			foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
			{
				allEntity.Control.StartDaze(beforeCutsceneDelay + 1f);
			}
			yield return new WaitForSeconds(beforeCutsceneDelay);
			endingCutscene.PlayNetworked();
			RpcPrepareCharactersForEnding();
			DewCutsceneDirector dewCutsceneDirector = endingCutscene;
			dewCutsceneDirector.onFinish = (Action)Delegate.Combine(dewCutsceneDirector.onFinish, (Action)(() =>
			{
				NetworkedManagerBase<GameManager>.instance.ConcludePureWhiteDream();
			}));
		}
	}

	[ClientRpc]
	private void RpcPrepareCharactersForEnding()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Primus_Ending::RpcPrepareCharactersForEnding()", 1472016199, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcSetWhiteFade(bool value)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteBool((NetworkWriter)(object)val, value);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Primus_Ending::RpcSetWhiteFade(System.Boolean)", -528910014, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcZoomInCamera()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Primus_Ending::RpcZoomInCamera()", -212665819, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcZoomOutCamera()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Primus_Ending::RpcZoomOutCamera()", 1766714522, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcDisableActions()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Primus_Ending::RpcDisableActions()", -1037990467, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcSetIsDoingEnding()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Primus_Ending::RpcSetIsDoingEnding()", -666203526, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcPrepareCharactersForEnding()
	{
		DewCutsceneDirector dewCutsceneDirector = endingCutscene;
		dewCutsceneDirector.onFinish = (Action)Delegate.Combine(dewCutsceneDirector.onFinish, (Action)(() =>
		{
			FxPlay(fxAfterCutsceneLoop);
		}));
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(ManagerBase<CameraManager>.instance.cutsceneFadeTime);
			foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
			{
				if ((UnityEngine.Object)(object)allEntity != (UnityEngine.Object)(object)DewPlayer.local.hero)
				{
					allEntity.Visual.DisableRenderersLocal();
				}
			}
			DewPlayer.local.hero.isWeaponHolstered = true;
			DewPlayer.local.hero.Visual.disableModelTransformUpdate = true;
			((Behaviour)(object)DewPlayer.local.hero.Animation).enabled = false;
			RuntimeAnimatorController val = heroControllers.Find((RuntimeAnimatorController c) => ((UnityEngine.Object)(object)c).name.EndsWith(((object)DewPlayer.local.hero).GetType().Name));
			if ((UnityEngine.Object)(object)val == null)
			{
				val = heroControllers[0];
			}
			DewPlayer.local.hero.Animation.animator.runtimeAnimatorController = val;
			DewPlayer.local.hero.Sound.ClientEvent_OnFootstep += (Action)(() =>
			{
				GameObject gameObject = fxDefaultFootstep;
				Transform transform = heroFootstepsParent.Find(((object)DewPlayer.local.hero).GetType().Name);
				if (transform != null)
				{
					gameObject = transform.gameObject;
				}
				Vector3 bonePosition = DewPlayer.local.hero.Visual.GetBonePosition((HumanBodyBones)5);
				Vector3 bonePosition2 = DewPlayer.local.hero.Visual.GetBonePosition((HumanBodyBones)6);
				Vector3 position = ((bonePosition.y < bonePosition2.y) ? bonePosition : bonePosition2);
				FxPlayNew(gameObject, Dew.GetPositionOnGround(position), null);
			});
		}
	}

	protected static void InvokeUserCode_RpcPrepareCharactersForEnding(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPrepareCharactersForEnding called on server.");
		}
		else
		{
			((Primus_Ending)(object)obj).UserCode_RpcPrepareCharactersForEnding();
		}
	}

	protected void UserCode_RpcSetWhiteFade__Boolean(bool value)
	{
		InGameUIManager.instance.SetWhiteFade(value);
	}

	protected static void InvokeUserCode_RpcSetWhiteFade__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetWhiteFade called on server.");
		}
		else
		{
			((Primus_Ending)(object)obj).UserCode_RpcSetWhiteFade__Boolean(NetworkReaderExtensions.ReadBool(reader));
		}
	}

	protected void UserCode_RpcZoomInCamera()
	{
		_zoom = new CameraModifierZoom();
		_zoom.zoomIndex = 7f;
		_zoom.Apply();
	}

	protected static void InvokeUserCode_RpcZoomInCamera(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcZoomInCamera called on server.");
		}
		else
		{
			((Primus_Ending)(object)obj).UserCode_RpcZoomInCamera();
		}
	}

	protected void UserCode_RpcZoomOutCamera()
	{
		if (_zoom != null)
		{
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			CameraModifierZoom zoom = _zoom;
			float startZoom = zoom.zoomIndex;
			float endZoom = 0f;
			float duration = 5f;
			for (float t = 0f; t < duration; t += Time.deltaTime)
			{
				float t2 = EasingFunction.EaseInOutQuad(0f, 1f, t / duration);
				zoom.zoomIndex = Mathf.Lerp(startZoom, endZoom, t2);
				yield return null;
			}
			zoom.Remove();
			if (_zoom == zoom)
			{
				_zoom = null;
			}
		}
	}

	protected static void InvokeUserCode_RpcZoomOutCamera(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcZoomOutCamera called on server.");
		}
		else
		{
			((Primus_Ending)(object)obj).UserCode_RpcZoomOutCamera();
		}
	}

	protected void UserCode_RpcDisableActions()
	{
		ManagerBase<ControlManager>.instance.isEditSkillDisabled = true;
		ManagerBase<ControlManager>.instance.isMainSkillDisabled = true;
		ManagerBase<ControlManager>.instance.isDodgeDisabled = true;
		ManagerBase<ControlManager>.instance.isWorldMapDisabled = true;
		ManagerBase<EditSkillManager>.instance.EndEdit();
	}

	protected static void InvokeUserCode_RpcDisableActions(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDisableActions called on server.");
		}
		else
		{
			((Primus_Ending)(object)obj).UserCode_RpcDisableActions();
		}
	}

	protected void UserCode_RpcSetIsDoingEnding()
	{
		InGameUIManager.instance.SetIsDoingEnding(value: true);
		ManagerBase<EditSkillManager>.instance.EndEdit();
	}

	protected static void InvokeUserCode_RpcSetIsDoingEnding(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetIsDoingEnding called on server.");
		}
		else
		{
			((Primus_Ending)(object)obj).UserCode_RpcSetIsDoingEnding();
		}
	}

	static Primus_Ending()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected Obj, but got Unknown
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected Obj, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected Obj, but got Unknown
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected Obj, but got Unknown
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Primus_Ending), "System.Void Primus_Ending::RpcPrepareCharactersForEnding()", (RemoteCallDelegate)InvokeUserCode_RpcPrepareCharactersForEnding);
		RemoteProcedureCalls.RegisterRpc(typeof(Primus_Ending), "System.Void Primus_Ending::RpcSetWhiteFade(System.Boolean)", (RemoteCallDelegate)InvokeUserCode_RpcSetWhiteFade__Boolean);
		RemoteProcedureCalls.RegisterRpc(typeof(Primus_Ending), "System.Void Primus_Ending::RpcZoomInCamera()", (RemoteCallDelegate)InvokeUserCode_RpcZoomInCamera);
		RemoteProcedureCalls.RegisterRpc(typeof(Primus_Ending), "System.Void Primus_Ending::RpcZoomOutCamera()", (RemoteCallDelegate)InvokeUserCode_RpcZoomOutCamera);
		RemoteProcedureCalls.RegisterRpc(typeof(Primus_Ending), "System.Void Primus_Ending::RpcDisableActions()", (RemoteCallDelegate)InvokeUserCode_RpcDisableActions);
		RemoteProcedureCalls.RegisterRpc(typeof(Primus_Ending), "System.Void Primus_Ending::RpcSetIsDoingEnding()", (RemoteCallDelegate)InvokeUserCode_RpcSetIsDoingEnding);
	}
}
