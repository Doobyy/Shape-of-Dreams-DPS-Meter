using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DG.Tweening;
using DG.Tweening.Core;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class StarlessPath_BossPolarisManager : Actor
{
	public enum State
	{
		BeforeFight,
		Fight,
		AfterFight
	}

	[CompilerGenerated]
	[SyncVar(hook = "OnStateChanged")]
	private State state__BackingField;

	public Mon_Polaris polaris;

	public GameObject entranceMapObject;

	public GameObject entranceMapBeforeFightObject;

	public GameObject entranceMapAfterFightObject;

	public GameObject fightMapObject;

	[Header("First Meet Flow")]
	public Transform meetPolarisPos;

	public DewCollider meetPolarisRange;

	public GameObject riftObstacleObject;

	public Rift_Sidetrack exitToRapidsRift;

	[Header("Lift Curse Flow")]
	public GameObject fxLiftCurseStartPolaris;

	public GameObject fxCurseLiftedPolaris;

	public GameObject fxCurseLiftedHero;

	[Header("To Fight Transition Flow")]
	public GameObject fxTransitionPolaris;

	public GameObject fxPlayerFloat;

	public Transform teleportPosition;

	[Header("Fight Flow")]
	public GameObject fxPolarisDeath;

	public GameObject brokenStage;

	public GameObject unbrokenStage;

	private CameraModifierZoom _zoom;

	[Header("After Fight Flow")]
	public Actor[] destroyedOnStart;

	public Transform polarisPos;

	public Transform riftSpawnPos;

	public Transform afterFightStartPos;

	public GameObject fxAfterFightRumbleSoundLoop;

	public GameObject fxPolarisLyingDown;

	[Header("Ending Flow")]
	public DewCutsceneDirector endingCutscene;

	public List<RuntimeAnimatorController> heroControllers;

	public Transform heroFootstepsParent;

	public GameObject fxDefaultFootstep;

	public Transform everyoneTeleportPos;

	public AnimationClip polarisInjuredIdle;

	[Header("Eat Player Flow")]
	public GameObject fxPolarisHorrorVoiceBeforeEat;

	public GameObject fxEatPrepareOnPolaris;

	public GameObject fxEatPrepareOnHero;

	public GameObject fxEatOnPolaris;

	public GameObject fxEatOnHero;

	public GameObject eatPlayerEndingTextObject;

	private List<Channel> _lockChannels = new List<Channel>();

	private bool _isLocalControlLockActive;

	[CompilerGenerated]
	[SyncVar]
	private bool didInterrupt__BackingField;

	private int _nextMonologueIndex;

	private int _nextDialogueIndex;

	private bool _didFinishTalk;

	[CompilerGenerated]
	[SyncVar]
	private bool didStartEndJourney__BackingField;

	private bool _didStartEatPlayer;

	public const float EndingRiftPresenceRange = 8f;

	[CompilerGenerated]
	[SyncVar(hook = "OnIsRiftAvailableChanged")]
	private bool isRiftAvailable__BackingField;

	private bool _didFirstMeetSequence;

	private bool _isEnteringFight;

	public Action<State, State> _Mirror_SyncVarHookDelegate__003Cstate_003Ek__BackingField;

	public Action<bool, bool> _Mirror_SyncVarHookDelegate__003CisRiftAvailable_003Ek__BackingField;

	public State state
	{
		[CompilerGenerated]
		get
		{
			return state__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003Cstate_003Ek__BackingField = value;
		}
	}

	public bool didInterrupt
	{
		[CompilerGenerated]
		get
		{
			return didInterrupt__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CdidInterrupt_003Ek__BackingField = value;
		}
	}

	public bool didStartEndJourney
	{
		[CompilerGenerated]
		get
		{
			return didStartEndJourney__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CdidStartEndJourney_003Ek__BackingField = value;
		}
	}

	public bool isRiftAvailable
	{
		[CompilerGenerated]
		get
		{
			return isRiftAvailable__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CisRiftAvailable_003Ek__BackingField = value;
		}
	}

	public State Network_003Cstate_003Ek__BackingField
	{
		get
		{
			return state__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<State>(value, ref state__BackingField, 8uL, _Mirror_SyncVarHookDelegate__003Cstate_003Ek__BackingField);
		}
	}

	public bool Network_003CdidInterrupt_003Ek__BackingField
	{
		get
		{
			return didInterrupt__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref didInterrupt__BackingField, 16uL, (Action<bool, bool>)null);
		}
	}

	public bool Network_003CdidStartEndJourney_003Ek__BackingField
	{
		get
		{
			return didStartEndJourney__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref didStartEndJourney__BackingField, 32uL, (Action<bool, bool>)null);
		}
	}

	public bool Network_003CisRiftAvailable_003Ek__BackingField
	{
		get
		{
			return isRiftAvailable__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref isRiftAvailable__BackingField, 64uL, _Mirror_SyncVarHookDelegate__003CisRiftAvailable_003Ek__BackingField);
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		OnStateChanged(State.BeforeFight, State.BeforeFight);
		OnIsRiftAvailableChanged(from: false, to: false);
		OnCreate_FirstMeet();
		OnCreate_Fight();
		OnCreate_AfterFight();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		polaris.canInteract = true;
		polaris.onInteract += (Action<Entity>)((Entity e) =>
		{
			if (e is Hero h)
			{
				TalkToPolaris(h);
			}
		});
		for (int num = 0; num < NetworkedManagerBase<ActorManager>.instance.allHeroes.Count; num++)
		{
			Hero hero = NetworkedManagerBase<ActorManager>.instance.allHeroes[num];
			if (hero.isKnockedOut)
			{
				CreateAbilityInstance<Ai_ReviveHero>(Vector3.zero, null, new CastInfo(hero, hero));
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (_isLocalControlLockActive)
		{
			_isLocalControlLockActive = false;
			if (ManagerBase<ControlManager>.softInstance != null)
			{
				ManagerBase<ControlManager>.softInstance.EnableCharacterControls();
			}
		}
		OnDestroyActor_FirstMeet();
		OnDestroyActor_Fight();
		OnDestroyActor_AfterFight();
	}

	private void OnEntityEnter(Entity e)
	{
		if (state == State.BeforeFight && !_didFirstMeetSequence && e is Hero h && !DewPlayer.gamePlayers.Any((DewPlayer p) => !p.hero.IsNullInactiveDeadOrKnockedOut() && p.hero.Status.isInConversation))
		{
			TalkToPolaris(h);
		}
	}

	public void TalkToPolaris(Hero h)
	{
		if (state == State.BeforeFight)
		{
			TalkToPolaris_BeforeFight(h);
		}
		else if (state == State.AfterFight)
		{
			TalkToPolaris_AfterFight(h);
		}
	}

	private IEnumerator StartConvo_Imp(Hero hero, RefValue<bool> didPlayerLeft, string key, Dictionary<string, Action> functions = null, bool rotateTowardsCenter = true)
	{
		if (Vector3.Distance(hero.agentPosition, polaris.agentPosition) > 6f)
		{
			Vector3 end = polaris.agentPosition + (hero.agentPosition - polaris.agentPosition).normalized * 6f;
			end = Dew.GetValidAgentDestination_LinearSweep(polaris.agentPosition, end);
			hero.Control.StartDisplacement(new DispByDestination
			{
				destination = end,
				duration = 2f,
				isFriendly = true,
				isCanceledByCC = false,
				canGoOverTerrain = true,
				ease = DewEase.EaseInOutQuad
			});
		}
		yield return NetworkedManagerBase<ConversationManager>.instance.StartConversationRoutine(new DewConversationSettings
		{
			startConversationKey = key,
			player = hero.owner,
			rotateTowardsCenter = rotateTowardsCenter,
			speakers = new Entity[2] { polaris, hero },
			visibility = ConversationVisibility.Everyone,
			callFunctions = functions
		});
		if (didPlayerLeft != null && hero.IsNullInactiveDeadOrKnockedOut())
		{
			_didFirstMeetSequence = false;
			didPlayerLeft.value = true;
		}
	}

	private void SetControlLock(bool isLocked)
	{
		RpcSetCharacterControlsLocked(isLocked);
		if (isLocked)
		{
			foreach (Hero allHero in NetworkedManagerBase<ActorManager>.instance.allHeroes)
			{
				_lockChannels.Add(allHero.Control.StartChannel(new Channel
				{
					blockedActions = Channel.BlockedAction.Everything,
					duration = 3600f
				}));
				foreach (Summon summon in allHero.summons)
				{
					_lockChannels.Add(summon.Control.StartChannel(new Channel
					{
						blockedActions = Channel.BlockedAction.Everything,
						duration = 3600f
					}));
				}
			}
			return;
		}
		foreach (Channel lockChannel in _lockChannels)
		{
			lockChannel.Cancel();
		}
		_lockChannels.Clear();
	}

	[ClientRpc]
	private void RpcSetCharacterControlsLocked(bool isLocked)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteBool((NetworkWriter)(object)val, isLocked);
		((NetworkBehaviour)this).SendRPCInternal("System.Void StarlessPath_BossPolarisManager::RpcSetCharacterControlsLocked(System.Boolean)", -1418041113, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void OnStateChanged(State from, State to)
	{
		switch (state)
		{
		case State.BeforeFight:
			OnStateStarted_FirstMeet();
			break;
		case State.Fight:
			OnStateStarted_Fight();
			break;
		case State.AfterFight:
			OnStateStarted_AfterFight();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void OnCreate_AfterFight()
	{
		_ = ((NetworkBehaviour)this).isServer;
	}

	private void OnDestroyActor_AfterFight()
	{
		_ = ((NetworkBehaviour)this).isServer;
	}

	private void OnStateStarted_AfterFight()
	{
		entranceMapObject.SetActive(value: true);
		fightMapObject.SetActive(value: false);
		entranceMapBeforeFightObject.SetActive(value: false);
		entranceMapAfterFightObject.SetActive(value: true);
		FxPlay(fxAfterFightRumbleSoundLoop);
		polaris.Animation.ReplaceAnimationLocal(EntityAnimation.ReplaceableAnimationType.Idle, new AnimationClipWithSpeed
		{
			clip = polarisInjuredIdle,
			speed = 0.45f
		});
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		Teleport(polaris, polarisPos.position);
		FxPlayNetworked(fxPolarisLyingDown, polaris);
		polaris.Control.Rotate(polarisPos.rotation, immediately: true);
		polaris.canInteract = true;
		Dew.CreateActor<Shrine_StarlessPath_EndingRift>(riftSpawnPos.position, riftSpawnPos.rotation);
		Actor[] array = destroyedOnStart;
		foreach (Actor actor in array)
		{
			if ((bool)(UnityEngine.Object)(object)actor && actor.isActive)
			{
				actor.Destroy();
			}
		}
	}

	public static bool AreAllHeroesNear(Vector3 pos, out int current, out int required)
	{
		current = 0;
		required = 0;
		foreach (Hero allHero in NetworkedManagerBase<ActorManager>.instance.allHeroes)
		{
			if (!allHero.IsNullOrInactive() && !allHero.isKnockedOut)
			{
				required++;
				if (Vector2.Distance(allHero.agentPosition.ToXY(), pos.ToXY()) < 8f)
				{
					current++;
				}
			}
		}
		return current >= required;
	}

	private bool CheckAllHeroesNearRift(NetworkConnectionToClient sender)
	{
		if (AreAllHeroesNear(riftSpawnPos.position, out var current, out var required))
		{
			return true;
		}
		sender.GetHero()?.owner.TpcShowCenterMessage(CenterMessageType.General, "InGame_Message_NeedAllPlayersPresence", new string[2]
		{
			current.ToString(),
			required.ToString()
		});
		return false;
	}

	[Command(requiresAuthority = false)]
	public void CmdInterruptMe(NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendCommandInternal("System.Void StarlessPath_BossPolarisManager::CmdInterruptMe(Mirror.NetworkConnectionToClient)", 2098186044, (NetworkWriter)(object)val, 0, false);
		NetworkWriterPool.Return(val);
	}

	private void TalkToPolaris_AfterFight(Hero h)
	{
		if (!_didStartEatPlayer && !didStartEndJourney)
		{
			if (!didInterrupt)
			{
				((MonoBehaviour)(object)this).StartCoroutine(AfterFightMonologueRoutine(h));
			}
			else
			{
				((MonoBehaviour)(object)this).StartCoroutine(AfterFightDialogueRoutine(h));
			}
		}
	}

	private IEnumerator AfterFightMonologueRoutine(Hero hero)
	{
		yield return StartConvo_Imp(hero, null, "BossPolaris.AfterFightMonologue" + _nextMonologueIndex.ToString(CultureInfo.InvariantCulture), null, rotateTowardsCenter: false);
		_nextMonologueIndex = Mathf.Clamp(_nextMonologueIndex + 1, 0, 5);
	}

	private IEnumerator AfterFightDialogueRoutine(Hero hero)
	{
		RefValue<bool> didPlayerLeft = new RefValue<bool>(v: false);
		yield return StartConvo("BossPolaris.AfterFightDialogue" + _nextDialogueIndex.ToString(CultureInfo.InvariantCulture), new Dictionary<string, Action>
		{
			{
				"StopMusic",
				() =>
				{
					FxStopNetworked(fxAfterFightRumbleSoundLoop);
					RpcStopMusic();
				}
			},
			{ "PlayPolarisHorrorVoice", RpcPlayPolarisHorrorVoice },
			{
				"EatPlayer",
				() =>
				{
					((MonoBehaviour)(object)this).StartCoroutine(EatPlayerRoutine());
				}
			},
			{
				"Reject",
				() =>
				{
					polaris.canInteract = false;
					_didFinishTalk = true;
				}
			}
		});
		_nextDialogueIndex = Mathf.Clamp(_nextDialogueIndex + 1, 0, 2);
		IEnumerator StartConvo(string key, Dictionary<string, Action> functions = null)
		{
			return StartConvo_Imp(hero, didPlayerLeft, key, functions, rotateTowardsCenter: false);
		}
	}

	[ClientRpc]
	private void RpcPlayPolarisHorrorVoice()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void StarlessPath_BossPolarisManager::RpcPlayPolarisHorrorVoice()", 435642365, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private IEnumerator EatPlayerRoutine()
	{
		if (_didStartEatPlayer || didStartEndJourney)
		{
			yield break;
		}
		_didStartEatPlayer = true;
		polaris.canInteract = false;
		RpcRegisterMoraToPlayersSubtitleText();
		SetControlLock(isLocked: true);
		FxPlayNetworked(fxEatPrepareOnPolaris, polaris);
		foreach (Hero allHero in NetworkedManagerBase<ActorManager>.instance.allHeroes)
		{
			allHero.Control.RotateTowards(polaris, immediately: false);
			FxPlayNewNetworked(fxEatPrepareOnHero, allHero);
		}
		yield return new WaitForSeconds(8.2f);
		FxStopNetworked(fxEatPrepareOnPolaris);
		FxPlayNetworked(fxEatOnPolaris, polaris);
		foreach (Hero allHero2 in NetworkedManagerBase<ActorManager>.instance.allHeroes)
		{
			if (!allHero2.IsNullInactiveDeadOrKnockedOut())
			{
				FxPlayNewNetworked(fxEatOnHero, allHero2);
			}
		}
		NetworkedManagerBase<GameManager>.instance.isGameOverEnabled = false;
		while (true)
		{
			bool flag = false;
			foreach (Hero allHero3 in NetworkedManagerBase<ActorManager>.instance.allHeroes)
			{
				if (!allHero3.IsNullInactiveDeadOrKnockedOut())
				{
					allHero3.Kill();
					flag = true;
				}
			}
			if (!flag)
			{
				break;
			}
			yield return null;
		}
		yield return new WaitForSeconds(0.1f);
		RpcShowEatPlayerEndingText();
		yield return new WaitForSeconds(0.1f);
		polaris.DestroyIfActive();
		yield return new WaitForSeconds(18.5f);
		NetworkedManagerBase<GameManager>.instance.isGameOverEnabled = true;
		SetControlLock(isLocked: false);
	}

	[ClientRpc]
	private void RpcShowEatPlayerEndingText()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void StarlessPath_BossPolarisManager::RpcShowEatPlayerEndingText()", 142747113, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcRegisterMoraToPlayersSubtitleText()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void StarlessPath_BossPolarisManager::RpcRegisterMoraToPlayersSubtitleText()", 161043841, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Command(requiresAuthority = false)]
	public void CmdEndJourney(NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendCommandInternal("System.Void StarlessPath_BossPolarisManager::CmdEndJourney(Mirror.NetworkConnectionToClient)", 957471712, (NetworkWriter)(object)val, 0, false);
		NetworkWriterPool.Return(val);
	}

	[Server]
	public void HidePolaris()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void StarlessPath_BossPolarisManager::HidePolaris()' called when server was not active");
		}
		else if (!polaris.IsNullOrInactive())
		{
			Teleport(polaris, everyoneTeleportPos.position);
			polaris.Visual.DisableRenderers();
		}
	}

	[ClientRpc]
	private void RpcPrepareCharactersForEnding()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void StarlessPath_BossPolarisManager::RpcPrepareCharactersForEnding()", -604667192, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcMakeRumbleSoundQuieter()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void StarlessPath_BossPolarisManager::RpcMakeRumbleSoundQuieter()", -1915988902, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Server]
	public void TurnOffRumbleSound()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void StarlessPath_BossPolarisManager::TurnOffRumbleSound()' called when server was not active");
		}
		else
		{
			FxStopNetworked(fxAfterFightRumbleSoundLoop);
		}
	}

	private void OnCreate_Fight()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnEntityAdd += new Action<Entity>(ClientEventOnEntityAdd);
		}
	}

	private void OnDestroyActor_Fight()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnEntityAdd -= new Action<Entity>(ClientEventOnEntityAdd);
		}
	}

	private void OnStateStarted_Fight()
	{
		entranceMapObject.SetActive(value: false);
		fightMapObject.SetActive(value: true);
		ManagerBase<ObjectiveArrowManager>.instance.objectivePosition = null;
		if (((NetworkBehaviour)this).isServer && !exitToRapidsRift.IsNullOrInactive())
		{
			exitToRapidsRift.DestroyIfActive();
		}
	}

	private void ClientEventOnEntityAdd(Entity obj)
	{
		if (obj is Mon_Special_BossPolaris && state == State.Fight)
		{
			obj.EntityEvent_OnDeath += (Action<EventInfoKill>)((EventInfoKill _) =>
			{
				StartPolarisDeath();
			});
		}
	}

	[Server]
	public void StartPolarisDeath()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void StarlessPath_BossPolarisManager::StartPolarisDeath()' called when server was not active");
		}
		else
		{
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			RpcStopMusic();
			Entity[] array = NetworkedManagerBase<ActorManager>.instance.allEntities.ToArray();
			foreach (Entity entity in array)
			{
				if (!entity.IsNullInactiveDeadOrKnockedOut() && (UnityEngine.Object)(object)entity.owner == (UnityEngine.Object)(object)DewPlayer.creep)
				{
					entity.Destroy();
				}
			}
			RpcDisableActions();
			FxPlayNetworked(fxPolarisDeath);
			yield return new WaitForSeconds(4.5f);
			RpcSetWhiteFade(value: true);
			yield return new WaitForSeconds(4f);
			yield return new WaitForSeconds(0.5f);
			RpcZoomInCamera();
			FxStopNetworked(fxPolarisDeath);
			NetworkedManagerBase<QuestManager>.instance.RemoveArtifact();
			List<Hero> list = (from p in DewPlayer.gamePlayers
				where !p.hero.IsNullOrInactive()
				select p.hero).ToList();
			List<Entity> list2 = new List<Entity>();
			list2.AddRange(list);
			foreach (Hero item in list)
			{
				item.Skill.StopHoldInHand();
				list2.AddRange(item.summons);
			}
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
				Vector3 positionOnGround = Dew.GetPositionOnGround(afterFightStartPos.position + UnityEngine.Random.onUnitSphere.Flattened() * 1.5f);
				item2.Control.Teleport(positionOnGround);
				item2.Control.Rotate(afterFightStartPos.forward, immediately: true);
				item2.Control.StartDaze(4f);
				item2.CreateBasicEffect(item2, new SlowEffect
				{
					strength = 12.5f
				}, float.PositiveInfinity);
				if (item2 is Summon summon)
				{
					summon.AddDuration(float.PositiveInfinity, clampToMaxDuration: false);
				}
			}
			RpcSetIsDoingEnding();
			yield return new WaitForSeconds(3f);
			RpcZoomOutCamera();
			RpcSetWhiteFade(value: false);
			Network_003Cstate_003Ek__BackingField = State.AfterFight;
		}
	}

	[ClientRpc]
	private void RpcDisableActions()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void StarlessPath_BossPolarisManager::RpcDisableActions()", -1415425346, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcSetIsDoingEnding()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void StarlessPath_BossPolarisManager::RpcSetIsDoingEnding()", 1691097915, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcZoomInCamera()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void StarlessPath_BossPolarisManager::RpcZoomInCamera()", -1124789402, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcZoomOutCamera()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void StarlessPath_BossPolarisManager::RpcZoomOutCamera()", -739312775, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcSetWhiteFade(bool value)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteBool((NetworkWriter)(object)val, value);
		((NetworkBehaviour)this).SendRPCInternal("System.Void StarlessPath_BossPolarisManager::RpcSetWhiteFade(System.Boolean)", 1689373891, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void OnCreate_FirstMeet()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			meetPolarisRange.receiveEntityCallbacks = true;
			meetPolarisRange.invokeEventsOnClients = false;
			meetPolarisRange.onEntityEnter.AddListener(OnEntityEnter);
			meetPolarisRange.UpdateProxyCollider();
			NetworkedManagerBase<GameSettingsManager>.instance.midJoinWaitType = MidJoinWaitType.BeforeBossFight;
			NetworkedManagerBase<GameManager>.instance.LockMidRunSave();
		}
	}

	private void OnDestroyActor_FirstMeet()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			if ((bool)(UnityEngine.Object)(object)NetworkedManagerBase<GameSettingsManager>.instance)
			{
				NetworkedManagerBase<GameSettingsManager>.instance.midJoinWaitType = MidJoinWaitType.None;
			}
			if ((bool)(UnityEngine.Object)(object)NetworkedManagerBase<GameManager>.instance)
			{
				NetworkedManagerBase<GameManager>.instance.UnlockMidRunSave();
			}
		}
	}

	private void OnStateStarted_FirstMeet()
	{
		entranceMapObject.SetActive(value: true);
		fightMapObject.SetActive(value: false);
		entranceMapBeforeFightObject.SetActive(value: true);
		entranceMapAfterFightObject.SetActive(value: false);
		if (((NetworkBehaviour)this).isServer)
		{
			Teleport(polaris, meetPolarisPos.position);
			polaris.Control.Rotate(meetPolarisPos.rotation, immediately: true);
		}
	}

	private void TalkToPolaris_BeforeFight(Hero h)
	{
		if (!_isEnteringFight)
		{
			if (!_didFirstMeetSequence)
			{
				((MonoBehaviour)(object)this).StartCoroutine(FirstMeetConvoRoutine(h));
			}
			else
			{
				((MonoBehaviour)(object)this).StartCoroutine(AfterAcceptTalkAgainRoutine(h));
			}
		}
	}

	private IEnumerator FirstMeetConvoRoutine(Hero hero)
	{
		RefValue<bool> didPlayerLeft = new RefValue<bool>(v: false);
		_didFirstMeetSequence = true;
		yield return StartConvo("BossPolaris.FirstMeet0");
		if ((bool)didPlayerLeft)
		{
			yield break;
		}
		if (DewPlayer.gamePlayers.Any((DewPlayer p) => !p.hero.IsNullInactiveDeadOrKnockedOut() && p.hero.Status.HasStatusEffect<Se_Curse_GuidingCompass_CripplingAnxiety>()))
		{
			yield return StartConvo("BossPolaris.FirstMeetCursed");
			polaris.canInteract = false;
			SetControlLock(isLocked: true);
			FxPlayNetworked(fxLiftCurseStartPolaris, polaris);
			yield return new WaitForSeconds(1f);
			FxStopNetworked(fxLiftCurseStartPolaris);
			FxPlayNetworked(fxCurseLiftedPolaris, polaris);
			foreach (Hero allHero in NetworkedManagerBase<ActorManager>.instance.allHeroes)
			{
				if (allHero.Status.TryGetStatusEffect<Se_Curse_GuidingCompass_CripplingAnxiety>(out var effect))
				{
					effect.Destroy();
					FxPlayNetworked(fxCurseLiftedHero, allHero);
				}
			}
			yield return new WaitForSeconds(3.5f);
			polaris.canInteract = true;
			SetControlLock(isLocked: false);
		}
		else
		{
			yield return StartConvo("BossPolaris.FirstMeetNoCurse");
		}
		if ((bool)didPlayerLeft)
		{
			yield break;
		}
		RefValue<bool> didAccept = new RefValue<bool>(v: false);
		yield return StartConvo("BossPolaris.FirstMeet1", new Dictionary<string, Action> { 
		{
			"Accept",
			() =>
			{
				didAccept.value = true;
			}
		} });
		if (!didPlayerLeft)
		{
			if ((bool)didAccept)
			{
				yield return StartConvo("BossPolaris.FirstMeetAccept");
				Network_003CisRiftAvailable_003Ek__BackingField = true;
			}
			else
			{
				yield return RejectAndFightRoutine(hero);
			}
		}
		IEnumerator StartConvo(string key, Dictionary<string, Action> functions = null)
		{
			return StartConvo_Imp(hero, didPlayerLeft, key, functions);
		}
	}

	[ClientRpc]
	private void RpcStopMusic()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void StarlessPath_BossPolarisManager::RpcStopMusic()", -362171268, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private IEnumerator RejectAndFightRoutine(Hero hero)
	{
		if (!_isEnteringFight)
		{
			_isEnteringFight = true;
			polaris.canInteract = false;
			NetworkedManagerBase<GameSettingsManager>.instance.midJoinBanType = MidJoinBanType.TooLateToJoin;
			RpcStopMusic();
			if (!hero.IsNullInactiveDeadOrKnockedOut())
			{
				yield return NetworkedManagerBase<ConversationManager>.instance.StartConversationRoutine(new DewConversationSettings
				{
					startConversationKey = "BossPolaris.FirstMeetReject",
					player = hero.owner,
					rotateTowardsCenter = true,
					speakers = new Entity[2] { polaris, hero },
					visibility = ConversationVisibility.Everyone
				});
			}
			yield return FightTransitionRoutine();
		}
	}

	private IEnumerator FightTransitionRoutine()
	{
		_isEnteringFight = true;
		NetworkedManagerBase<ConversationManager>.instance.StopAllConversations();
		SetControlLock(isLocked: true);
		FxPlayNetworked(fxTransitionPolaris, polaris);
		yield return new WaitForSeconds(2f);
		for (int i = 0; i < NetworkedManagerBase<ActorManager>.instance.allHeroes.Count; i++)
		{
			Hero hero = NetworkedManagerBase<ActorManager>.instance.allHeroes[i];
			if (hero.Status.TryGetStatusEffect<Se_HeroKnockedOut>(out var effect))
			{
				effect.Revive();
			}
			FxPlayNewNetworked(fxPlayerFloat, hero);
		}
		yield return new WaitForSeconds(7.5f);
		DewNetworkManager.instance.SetWhiteLoadingStatus(isLoading: true, DewPlayer.gamePlayers);
		yield return new WaitForSeconds(1f);
		float whiteScreenDuration = 2.5f;
		yield return new WaitForSeconds(whiteScreenDuration / 4f);
		Network_003Cstate_003Ek__BackingField = State.Fight;
		yield return new WaitForSeconds(whiteScreenDuration / 4f);
		TeleportHeroesToArena();
		yield return new WaitForSeconds(whiteScreenDuration / 2f);
		FxStopNetworked(fxTransitionPolaris);
		DewNetworkManager.instance.SetWhiteLoadingStatus(isLoading: false, DewPlayer.gamePlayers);
		yield return new WaitForSeconds(1f);
		SetControlLock(isLocked: false);
	}

	private void TeleportHeroesToArena()
	{
		Vector3 vector = teleportPosition.position;
		Vector3 right = teleportPosition.right;
		for (int i = 0; i < NetworkedManagerBase<ActorManager>.instance.allHeroes.Count; i++)
		{
			Hero entity = NetworkedManagerBase<ActorManager>.instance.allHeroes[i];
			Vector3 vector2 = vector + right * ((float)(NetworkedManagerBase<ActorManager>.instance.allHeroes.Count - 1) / 2f + (float)i);
			vector2 += UnityEngine.Random.insideUnitSphere * 0.7f;
			vector2 = Dew.GetPositionOnGround(vector2);
			Teleport(entity, vector2);
		}
	}

	private IEnumerator AfterAcceptTalkAgainRoutine(Hero hero)
	{
		RefValue<bool> didPlayerLeft = new RefValue<bool>(v: false);
		RefValue<bool> didChangeMind = new RefValue<bool>(v: false);
		yield return StartConvo("BossPolaris.AfterAcceptRandom*", new Dictionary<string, Action> { 
		{
			"ChangeMind",
			() =>
			{
				didChangeMind.value = true;
			}
		} });
		if (!didPlayerLeft && (bool)didChangeMind)
		{
			yield return RejectAndFightRoutine(hero);
		}
		IEnumerator StartConvo(string key, Dictionary<string, Action> functions = null)
		{
			return StartConvo_Imp(hero, didPlayerLeft, key, functions);
		}
	}

	private void OnIsRiftAvailableChanged(bool from, bool to)
	{
		if ((bool)riftObstacleObject)
		{
			riftObstacleObject.SetActive(!isRiftAvailable);
		}
		if (!exitToRapidsRift.IsNullOrInactive())
		{
			exitToRapidsRift.isLocked = !isRiftAvailable;
			if (isRiftAvailable && !exitToRapidsRift.isOpen)
			{
				exitToRapidsRift.Open();
			}
		}
	}

	public StarlessPath_BossPolarisManager()
	{
		_Mirror_SyncVarHookDelegate__003Cstate_003Ek__BackingField = OnStateChanged;
		_Mirror_SyncVarHookDelegate__003CisRiftAvailable_003Ek__BackingField = OnIsRiftAvailableChanged;
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcSetCharacterControlsLocked__Boolean(bool isLocked)
	{
		if (_isLocalControlLockActive != isLocked)
		{
			_isLocalControlLockActive = isLocked;
			if (isLocked)
			{
				ManagerBase<ControlManager>.instance.DisableCharacterControls();
			}
			else
			{
				ManagerBase<ControlManager>.instance.EnableCharacterControls();
			}
		}
	}

	protected static void InvokeUserCode_RpcSetCharacterControlsLocked__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetCharacterControlsLocked called on server.");
		}
		else
		{
			((StarlessPath_BossPolarisManager)(object)obj).UserCode_RpcSetCharacterControlsLocked__Boolean(NetworkReaderExtensions.ReadBool(reader));
		}
	}

	protected void UserCode_CmdInterruptMe__NetworkConnectionToClient(NetworkConnectionToClient sender)
	{
		if (state == State.AfterFight && !didInterrupt && CheckAllHeroesNearRift(sender))
		{
			Network_003CdidInterrupt_003Ek__BackingField = true;
			((MonoBehaviour)(object)this).StartCoroutine(StartConvo_Imp(sender.GetHero(), null, "BossPolaris.AfterFightInterrupt", null, rotateTowardsCenter: false));
		}
	}

	protected static void InvokeUserCode_CmdInterruptMe__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdInterruptMe called on client.");
		}
		else
		{
			((StarlessPath_BossPolarisManager)(object)obj).UserCode_CmdInterruptMe__NetworkConnectionToClient(senderConnection);
		}
	}

	protected void UserCode_RpcPlayPolarisHorrorVoice()
	{
		fxPolarisHorrorVoiceBeforeEat.GetComponent<DewAudioSource>().pitchMultiplier *= 0.8f;
		FxPlay(fxPolarisHorrorVoiceBeforeEat);
	}

	protected static void InvokeUserCode_RpcPlayPolarisHorrorVoice(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayPolarisHorrorVoice called on server.");
		}
		else
		{
			((StarlessPath_BossPolarisManager)(object)obj).UserCode_RpcPlayPolarisHorrorVoice();
		}
	}

	protected void UserCode_RpcShowEatPlayerEndingText()
	{
		eatPlayerEndingTextObject.SetActive(value: true);
	}

	protected static void InvokeUserCode_RpcShowEatPlayerEndingText(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcShowEatPlayerEndingText called on server.");
		}
		else
		{
			((StarlessPath_BossPolarisManager)(object)obj).UserCode_RpcShowEatPlayerEndingText();
		}
	}

	protected void UserCode_RpcRegisterMoraToPlayersSubtitleText()
	{
		if (!((UnityEngine.Object)(object)DewPlayer.local == null) && !DewPlayer.local.hero.IsNullOrInactive())
		{
			NetworkedManagerBase<GameManager>.instance.gameOverSubtitleOverride = DewLocalization.GetUIValue("InGame_Result_Subtitle_GameOver_MoraToEveryone");
		}
	}

	protected static void InvokeUserCode_RpcRegisterMoraToPlayersSubtitleText(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRegisterMoraToPlayersSubtitleText called on server.");
		}
		else
		{
			((StarlessPath_BossPolarisManager)(object)obj).UserCode_RpcRegisterMoraToPlayersSubtitleText();
		}
	}

	protected void UserCode_CmdEndJourney__NetworkConnectionToClient(NetworkConnectionToClient sender)
	{
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			if (state == State.AfterFight && didInterrupt && !didStartEndJourney && !_didStartEatPlayer && CheckAllHeroesNearRift(sender))
			{
				Network_003CdidStartEndJourney_003Ek__BackingField = true;
				polaris.canInteract = false;
				string key = (_didFinishTalk ? "BossPolaris.RiftInteractAfterTalk" : "BossPolaris.RiftInteractNoTalk");
				yield return StartConvo_Imp(sender.GetHero(), null, key, null, rotateTowardsCenter: false);
				foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
				{
					allEntity.Control.StartDaze(10f);
				}
				yield return new WaitForSeconds(0.5f);
				RpcMakeRumbleSoundQuieter();
				endingCutscene.PlayNetworked();
				RpcPrepareCharactersForEnding();
				DewCutsceneDirector dewCutsceneDirector = endingCutscene;
				dewCutsceneDirector.onFinish = (Action)Delegate.Combine(dewCutsceneDirector.onFinish, (Action)(() =>
				{
					FxStopNetworked(fxAfterFightRumbleSoundLoop);
					NetworkedManagerBase<GameManager>.instance.ConcludeStarlessPath();
				}));
				yield return new WaitForSeconds(1f);
				Actor[] array = NetworkedManagerBase<ActorManager>.instance.allActors.ToArray();
				foreach (Actor actor in array)
				{
					if (actor is Shrine_StarlessPath_EndingRift)
					{
						actor.DestroyIfActive();
					}
					if (actor is Hero || actor is Summon)
					{
						Teleport((Entity)actor, everyoneTeleportPos.position);
					}
				}
			}
		}
	}

	protected static void InvokeUserCode_CmdEndJourney__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdEndJourney called on client.");
		}
		else
		{
			((StarlessPath_BossPolarisManager)(object)obj).UserCode_CmdEndJourney__NetworkConnectionToClient(senderConnection);
		}
	}

	protected void UserCode_RpcPrepareCharactersForEnding()
	{
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(ManagerBase<CameraManager>.instance.cutsceneFadeTime);
			foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
			{
				if (!((UnityEngine.Object)(object)allEntity == (UnityEngine.Object)(object)DewPlayer.local.hero) && !(allEntity is Mon_Polaris))
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
				Vector3 vector = ((bonePosition.y < bonePosition2.y) ? bonePosition : bonePosition2);
				FxPlayNew(gameObject, Dew.GetPositionOnGround(vector), null);
			});
			if (((NetworkBehaviour)this).isServer)
			{
				polaris.Control.Rotate(Quaternion.Euler(0f, 171.437f, 0f) * polaris.rotation, immediately: true);
			}
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
			((StarlessPath_BossPolarisManager)(object)obj).UserCode_RpcPrepareCharactersForEnding();
		}
	}

	protected void UserCode_RpcMakeRumbleSoundQuieter()
	{
		DewAudioSource[] componentsInChildren = fxAfterFightRumbleSoundLoop.GetComponentsInChildren<DewAudioSource>();
		foreach (DewAudioSource a in componentsInChildren)
		{
			DOTween.To((DOGetter<float>)(() => a.volumeMultiplier), (DOSetter<float>)((float x) =>
			{
				a.volumeMultiplier = x;
			}), 0.2f, 6f);
		}
	}

	protected static void InvokeUserCode_RpcMakeRumbleSoundQuieter(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcMakeRumbleSoundQuieter called on server.");
		}
		else
		{
			((StarlessPath_BossPolarisManager)(object)obj).UserCode_RpcMakeRumbleSoundQuieter();
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
			((StarlessPath_BossPolarisManager)(object)obj).UserCode_RpcDisableActions();
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
			((StarlessPath_BossPolarisManager)(object)obj).UserCode_RpcSetIsDoingEnding();
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
			((StarlessPath_BossPolarisManager)(object)obj).UserCode_RpcZoomInCamera();
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
			((StarlessPath_BossPolarisManager)(object)obj).UserCode_RpcZoomOutCamera();
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
			((StarlessPath_BossPolarisManager)(object)obj).UserCode_RpcSetWhiteFade__Boolean(NetworkReaderExtensions.ReadBool(reader));
		}
	}

	protected void UserCode_RpcStopMusic()
	{
		ManagerBase<MusicManager>.instance.Stop();
	}

	protected static void InvokeUserCode_RpcStopMusic(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcStopMusic called on server.");
		}
		else
		{
			((StarlessPath_BossPolarisManager)(object)obj).UserCode_RpcStopMusic();
		}
	}

	static StarlessPath_BossPolarisManager()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected Obj, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected Obj, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected Obj, but got Unknown
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Expected Obj, but got Unknown
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Expected Obj, but got Unknown
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected Obj, but got Unknown
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Expected Obj, but got Unknown
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Expected Obj, but got Unknown
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Expected Obj, but got Unknown
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Expected Obj, but got Unknown
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Expected Obj, but got Unknown
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Expected Obj, but got Unknown
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Expected Obj, but got Unknown
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterCommand(typeof(StarlessPath_BossPolarisManager), "System.Void StarlessPath_BossPolarisManager::CmdInterruptMe(Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_CmdInterruptMe__NetworkConnectionToClient, false);
		RemoteProcedureCalls.RegisterCommand(typeof(StarlessPath_BossPolarisManager), "System.Void StarlessPath_BossPolarisManager::CmdEndJourney(Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_CmdEndJourney__NetworkConnectionToClient, false);
		RemoteProcedureCalls.RegisterRpc(typeof(StarlessPath_BossPolarisManager), "System.Void StarlessPath_BossPolarisManager::RpcSetCharacterControlsLocked(System.Boolean)", (RemoteCallDelegate)InvokeUserCode_RpcSetCharacterControlsLocked__Boolean);
		RemoteProcedureCalls.RegisterRpc(typeof(StarlessPath_BossPolarisManager), "System.Void StarlessPath_BossPolarisManager::RpcPlayPolarisHorrorVoice()", (RemoteCallDelegate)InvokeUserCode_RpcPlayPolarisHorrorVoice);
		RemoteProcedureCalls.RegisterRpc(typeof(StarlessPath_BossPolarisManager), "System.Void StarlessPath_BossPolarisManager::RpcShowEatPlayerEndingText()", (RemoteCallDelegate)InvokeUserCode_RpcShowEatPlayerEndingText);
		RemoteProcedureCalls.RegisterRpc(typeof(StarlessPath_BossPolarisManager), "System.Void StarlessPath_BossPolarisManager::RpcRegisterMoraToPlayersSubtitleText()", (RemoteCallDelegate)InvokeUserCode_RpcRegisterMoraToPlayersSubtitleText);
		RemoteProcedureCalls.RegisterRpc(typeof(StarlessPath_BossPolarisManager), "System.Void StarlessPath_BossPolarisManager::RpcPrepareCharactersForEnding()", (RemoteCallDelegate)InvokeUserCode_RpcPrepareCharactersForEnding);
		RemoteProcedureCalls.RegisterRpc(typeof(StarlessPath_BossPolarisManager), "System.Void StarlessPath_BossPolarisManager::RpcMakeRumbleSoundQuieter()", (RemoteCallDelegate)InvokeUserCode_RpcMakeRumbleSoundQuieter);
		RemoteProcedureCalls.RegisterRpc(typeof(StarlessPath_BossPolarisManager), "System.Void StarlessPath_BossPolarisManager::RpcDisableActions()", (RemoteCallDelegate)InvokeUserCode_RpcDisableActions);
		RemoteProcedureCalls.RegisterRpc(typeof(StarlessPath_BossPolarisManager), "System.Void StarlessPath_BossPolarisManager::RpcSetIsDoingEnding()", (RemoteCallDelegate)InvokeUserCode_RpcSetIsDoingEnding);
		RemoteProcedureCalls.RegisterRpc(typeof(StarlessPath_BossPolarisManager), "System.Void StarlessPath_BossPolarisManager::RpcZoomInCamera()", (RemoteCallDelegate)InvokeUserCode_RpcZoomInCamera);
		RemoteProcedureCalls.RegisterRpc(typeof(StarlessPath_BossPolarisManager), "System.Void StarlessPath_BossPolarisManager::RpcZoomOutCamera()", (RemoteCallDelegate)InvokeUserCode_RpcZoomOutCamera);
		RemoteProcedureCalls.RegisterRpc(typeof(StarlessPath_BossPolarisManager), "System.Void StarlessPath_BossPolarisManager::RpcSetWhiteFade(System.Boolean)", (RemoteCallDelegate)InvokeUserCode_RpcSetWhiteFade__Boolean);
		RemoteProcedureCalls.RegisterRpc(typeof(StarlessPath_BossPolarisManager), "System.Void StarlessPath_BossPolarisManager::RpcStopMusic()", (RemoteCallDelegate)InvokeUserCode_RpcStopMusic);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			GeneratedNetworkCode._Write_StarlessPath_BossPolarisManager_002FState(writer, state__BackingField);
			NetworkWriterExtensions.WriteBool(writer, didInterrupt__BackingField);
			NetworkWriterExtensions.WriteBool(writer, didStartEndJourney__BackingField);
			NetworkWriterExtensions.WriteBool(writer, isRiftAvailable__BackingField);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 8L) != 0L)
		{
			GeneratedNetworkCode._Write_StarlessPath_BossPolarisManager_002FState(writer, state__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x10L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, didInterrupt__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x20L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, didStartEndJourney__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, isRiftAvailable__BackingField);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<State>(ref state__BackingField, _Mirror_SyncVarHookDelegate__003Cstate_003Ek__BackingField, GeneratedNetworkCode._Read_StarlessPath_BossPolarisManager_002FState(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref didInterrupt__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref didStartEndJourney__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isRiftAvailable__BackingField, _Mirror_SyncVarHookDelegate__003CisRiftAvailable_003Ek__BackingField, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 8L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<State>(ref state__BackingField, _Mirror_SyncVarHookDelegate__003Cstate_003Ek__BackingField, GeneratedNetworkCode._Read_StarlessPath_BossPolarisManager_002FState(reader));
		}
		if ((num & 0x10L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref didInterrupt__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x20L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref didStartEndJourney__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isRiftAvailable__BackingField, _Mirror_SyncVarHookDelegate__003CisRiftAvailable_003Ek__BackingField, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
