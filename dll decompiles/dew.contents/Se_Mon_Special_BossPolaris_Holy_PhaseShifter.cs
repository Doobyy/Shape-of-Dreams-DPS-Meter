using System;
using System.Collections;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Se_Mon_Special_BossPolaris_Holy_PhaseShifter : StatusEffect
{
	public GameObject fxSecondWind;

	public GameObject fxExplodePrepare;

	public GameObject fxExplode;

	public float cutsceneInitialDelay = 1f;

	public float explodeDelayInCutscene = 4f;

	public AssetRef<DewMusicItem> musPolarisHoly;

	public AssetRef<DewMusicItem> musPolarisMonster;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		DoDeathInterrupt((EventInfoKill _) =>
		{
			Mon_Special_BossPolaris polaris = (Mon_Special_BossPolaris)victim;
			if (polaris.mainPhase != Mon_Special_BossPolaris.MainPhase.Monster)
			{
				polaris.Status.SetHealth(1f);
				if (polaris.mainPhase != Mon_Special_BossPolaris.MainPhase.InTransition)
				{
					((MonoBehaviour)(object)this).StartCoroutine(Routine());
				}
			}
			IEnumerator Routine()
			{
				float defaultVolumeMultiplier = ManagerBase<MusicManager>.instance.volumeMultiplier;
				polaris.NetworkmainPhase = Mon_Special_BossPolaris.MainPhase.InTransition;
				polaris.Control.Stop();
				polaris.Control.CancelOngoingChannels();
				polaris.Control.CancelOngoingDisplacement();
				CleanupBattlefield();
				FxPlayNetworked(fxSecondWind, polaris);
				if (Vector3.Distance(SingletonBehaviour<Room_BossArena>.instance.center, info.caster.agentPosition) < 4f)
				{
					info.caster.Control.StartDisplacement(new DispByDestination
					{
						isFriendly = true,
						isCanceledByCC = false,
						duration = 0.7f,
						ease = DewEase.EaseOutQuad,
						destination = SingletonBehaviour<Room_BossArena>.instance.center,
						rotateForward = false
					});
				}
				polaris.Control.Rotate(ManagerBase<CameraManager>.instance.entityCamAngle + 180f, immediately: true);
				Se_GenericEffectContainer invul = CreateBasicEffect(polaris, new InvulnerableEffect(), 60f);
				yield return new WaitForSeconds(cutsceneInitialDelay / 3f * 2f);
				if (Vector3.Distance(SingletonBehaviour<Room_BossArena>.instance.center, info.caster.agentPosition) > 1f)
				{
					CreateStatusEffect<Se_Mon_Special_BossPolaris_Holy_Dash>(info.caster, new CastInfo(info.caster, SingletonBehaviour<Room_BossArena>.instance.center));
				}
				yield return new WaitForSeconds(cutsceneInitialDelay / 3f * 1f);
				ActorRef<Se_GenericEffectContainer> untar = CreateBasicEffect(polaris, new UntargetableEffect(), 60f);
				ActorRef<Se_GenericEffectContainer> uncol = CreateBasicEffect(polaris, new UncollidableEffect(), 60f);
				DewCutsceneDirector cutscene = UnityEngine.Object.FindObjectsByType<DewCutsceneDirector>(FindObjectsSortMode.None).FirstOrDefault((DewCutsceneDirector d) => ((UnityEngine.Object)(object)d).name.Contains("PHASE_CHANGE", StringComparison.InvariantCultureIgnoreCase));
				if (!((UnityEngine.Object)(object)cutscene == null))
				{
					cutscene.PlayNetworked();
					RpcChangeMusic(isMonster: false);
					yield return new WaitForSeconds(ManagerBase<CameraManager>.instance.cutsceneFadeTime - 0.05f);
					RpcAnimateMusicVolume(0.1f, 5f);
					RpcSetStageState(isBroken: false);
					Teleport(polaris, SingletonBehaviour<Room_BossArena>.instance.center);
					polaris.Control.Rotate(ManagerBase<CameraManager>.instance.entityCamAngle + 180f, immediately: true);
					FxPlayNetworked(fxExplodePrepare, polaris);
					yield return new WaitForSeconds(explodeDelayInCutscene);
					CleanupBattlefield();
					FxStopNetworked(fxExplodePrepare);
					FxPlayNetworked(fxExplode, polaris);
					polaris.SetMonsterPhase();
					RpcChangeMusic(isMonster: true);
					RpcSetStageState(isBroken: true);
					RpcAnimateMusicVolume(defaultVolumeMultiplier, 4f);
					if (polaris.Status.TryGetStatusEffect<Se_Mon_Special_BossPolaris_Adaptation>(out var effect))
					{
						effect.ApplyAdaptationStats();
					}
					yield return new WaitWhile(() => cutscene.isPlaying || ManagerBase<CameraManager>.instance.isPlayingCutscene);
					CleanupBattlefield();
					FxStopNetworked(fxExplode);
					float postDelay = 0.75f;
					polaris.Control.Stop();
					polaris.Control.CancelOngoingChannels();
					polaris.Control.StartDaze(postDelay);
					if (!untar.IsNullOrInactive())
					{
						untar.Get().Destroy();
					}
					if (!uncol.IsNullOrInactive())
					{
						uncol.Get().Destroy();
					}
					Se_GenericHealOverTime actor = CreateStatusEffect(victim, (Se_GenericHealOverTime se) =>
					{
						se.ticks = 8;
						se.tickInterval = postDelay / 8f;
						se.totalAmount = victim.maxHealth;
					});
					invul.DestroyOnDestroy(actor);
				}
			}
		}, 0);
	}

	private void CleanupBattlefield()
	{
		Actor[] array = NetworkedManagerBase<ActorManager>.instance.allActors.ToArray();
		foreach (Actor actor in array)
		{
			if (actor is Monster monster && (UnityEngine.Object)(object)monster.owner == (UnityEngine.Object)(object)victim.owner && !monster.IsAnyBoss())
			{
				monster.Kill();
			}
			else if (actor is Se_Mon_Special_BossPolaris_CleansingFlame se_Mon_Special_BossPolaris_CleansingFlame)
			{
				se_Mon_Special_BossPolaris_CleansingFlame.Destroy();
			}
			else if (actor is Ai_Mon_Special_BossPolaris_Holy_Purgatory_BurningFloor ai_Mon_Special_BossPolaris_Holy_Purgatory_BurningFloor)
			{
				ai_Mon_Special_BossPolaris_Holy_Purgatory_BurningFloor.Destroy();
			}
			else if (actor is Ai_Mon_Special_BossPolaris_Holy_Purgatory_Instance ai_Mon_Special_BossPolaris_Holy_Purgatory_Instance)
			{
				ai_Mon_Special_BossPolaris_Holy_Purgatory_Instance.Destroy();
			}
		}
	}

	[ClientRpc]
	private void RpcChangeMusic(bool isMonster)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteBool((NetworkWriter)(object)val, isMonster);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Se_Mon_Special_BossPolaris_Holy_PhaseShifter::RpcChangeMusic(System.Boolean)", 320602433, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcAnimateMusicVolume(float to, float duration)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteFloat((NetworkWriter)(object)val, to);
		NetworkWriterExtensions.WriteFloat((NetworkWriter)(object)val, duration);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Se_Mon_Special_BossPolaris_Holy_PhaseShifter::RpcAnimateMusicVolume(System.Single,System.Single)", -1174799195, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcSetStageState(bool isBroken)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteBool((NetworkWriter)(object)val, isBroken);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Se_Mon_Special_BossPolaris_Holy_PhaseShifter::RpcSetStageState(System.Boolean)", 524685857, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcChangeMusic__Boolean(bool isMonster)
	{
		AssetRef<DewMusicItem> assetRef = (isMonster ? musPolarisMonster : musPolarisHoly);
		if (!(ManagerBase<MusicManager>.instance.current == assetRef))
		{
			ManagerBase<MusicManager>.instance.DoCrossFade(assetRef, 4f);
		}
	}

	protected static void InvokeUserCode_RpcChangeMusic__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcChangeMusic called on server.");
		}
		else
		{
			((Se_Mon_Special_BossPolaris_Holy_PhaseShifter)(object)obj).UserCode_RpcChangeMusic__Boolean(NetworkReaderExtensions.ReadBool(reader));
		}
	}

	protected void UserCode_RpcAnimateMusicVolume__Single__Single(float to, float duration)
	{
		DOTween.Kill((object)ManagerBase<MusicManager>.instance, false);
		TweenSettingsExtensions.SetId<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => ManagerBase<MusicManager>.instance.volumeMultiplier), (DOSetter<float>)((float x) =>
		{
			ManagerBase<MusicManager>.instance.volumeMultiplier = x;
		}), to, duration), (object)ManagerBase<MusicManager>.instance);
	}

	protected static void InvokeUserCode_RpcAnimateMusicVolume__Single__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcAnimateMusicVolume called on server.");
		}
		else
		{
			((Se_Mon_Special_BossPolaris_Holy_PhaseShifter)(object)obj).UserCode_RpcAnimateMusicVolume__Single__Single(NetworkReaderExtensions.ReadFloat(reader), NetworkReaderExtensions.ReadFloat(reader));
		}
	}

	protected void UserCode_RpcSetStageState__Boolean(bool isBroken)
	{
		StarlessPath_BossPolarisManager starlessPath_BossPolarisManager = Dew.FindActorOfType<StarlessPath_BossPolarisManager>();
		if (!starlessPath_BossPolarisManager.IsNullOrInactive())
		{
			starlessPath_BossPolarisManager.unbrokenStage.SetActive(!isBroken);
			starlessPath_BossPolarisManager.brokenStage.SetActive(isBroken);
		}
	}

	protected static void InvokeUserCode_RpcSetStageState__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetStageState called on server.");
		}
		else
		{
			((Se_Mon_Special_BossPolaris_Holy_PhaseShifter)(object)obj).UserCode_RpcSetStageState__Boolean(NetworkReaderExtensions.ReadBool(reader));
		}
	}

	static Se_Mon_Special_BossPolaris_Holy_PhaseShifter()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected Obj, but got Unknown
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Se_Mon_Special_BossPolaris_Holy_PhaseShifter), "System.Void Se_Mon_Special_BossPolaris_Holy_PhaseShifter::RpcChangeMusic(System.Boolean)", (RemoteCallDelegate)InvokeUserCode_RpcChangeMusic__Boolean);
		RemoteProcedureCalls.RegisterRpc(typeof(Se_Mon_Special_BossPolaris_Holy_PhaseShifter), "System.Void Se_Mon_Special_BossPolaris_Holy_PhaseShifter::RpcAnimateMusicVolume(System.Single,System.Single)", (RemoteCallDelegate)InvokeUserCode_RpcAnimateMusicVolume__Single__Single);
		RemoteProcedureCalls.RegisterRpc(typeof(Se_Mon_Special_BossPolaris_Holy_PhaseShifter), "System.Void Se_Mon_Special_BossPolaris_Holy_PhaseShifter::RpcSetStageState(System.Boolean)", (RemoteCallDelegate)InvokeUserCode_RpcSetStageState__Boolean);
	}
}
