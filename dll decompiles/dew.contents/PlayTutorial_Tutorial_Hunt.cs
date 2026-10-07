using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayTutorial_Tutorial_Hunt : PlayTutorial_Tutorial
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CGoToPlayLobby_003Ed__10 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			//IL_013f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0146: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Unknown result type (might be due to invalid IL or missing references)
			//IL_0107: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_0123: Unknown result type (might be due to invalid IL or missing references)
			//IL_0124: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				Awaiter val;
				UniTask val2;
				if (num != 0)
				{
					if (num == 1)
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default;
						num = (_003C_003E1__state = -1);
						goto IL_0155;
					}
					DewNetworkManager.startSettings.networkMode = DewNetworkMode.Singleplayer;
					DewNetworkManager.startSettings.continueData = null;
					ManagerBase<TransitionManager>.instance.FadeOut(showTips: false);
					if (ManagerBase<AchievementManager>.instance.isTrackingAchievements)
					{
						ManagerBase<AchievementManager>.instance.StopTrackingAchievements();
					}
					val2 = UniTask.WaitForSeconds(1f, true, (PlayerLoopTiming)8, default(CancellationToken));
					val = val2.GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CGoToPlayLobby_003Ed__10>(ref val, ref this);
						return;
					}
				}
				else
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
				}
				val.GetResult();
				ActorManager.CleanupActorsBeforeShutdown();
				NetworkServer.Shutdown();
				NetworkClient.Shutdown();
				if (ManagerBase<GameLogicPackage>.instance != null)
				{
					UnityEngine.Object.Destroy(ManagerBase<GameLogicPackage>.instance.gameObject);
				}
				UnityEngine.Object.Destroy(ManagerBase<NetworkLogicPackage>.instance.gameObject);
				val2 = UniTask.WaitForSeconds(0.25f, true, (PlayerLoopTiming)8, default(CancellationToken));
				val = val2.GetAwaiter();
				if (!val.IsCompleted)
				{
					num = (_003C_003E1__state = 1);
					_003C_003Eu__1 = val;
					_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CGoToPlayLobby_003Ed__10>(ref val, ref this);
					return;
				}
				goto IL_0155;
				IL_0155:
				val.GetResult();
				DewNetworkManager.startSettings = new DewNetworkStartSettings();
				SceneManager.LoadScene("PlayLobby");
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003C_003Et__builder.SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			_003C_003Et__builder.SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	public Shrine[] shrines;

	public Mon_Polaris polaris;

	public GameObject fxImminentHunt;

	public Transform[] hunterSpawnPoses;

	public DewMusicItem huntMusic;

	public Transform polarisRiftPos;

	public Transform playerRiftPos;

	public Rift finalRift;

	public GameObject[] deactivatedObjectsAfterEverything;

	protected override IEnumerator OnStart()
	{
		Shrine[] array = shrines;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].MakeUnavailable();
		}
		ManagerBase<MusicManager>.instance.Stop();
		yield return PauseRoutine(0.5f);
		DewEffect.Play(fxImminentHunt);
		yield return PauseRoutine(2f);
		yield return SayRoutine("PlayTutorial.SomethingIsComing");
		yield return new WaitForSeconds(1.5f);
		ManagerBase<MusicManager>.instance.Play(huntMusic);
		for (int j = 0; j < 8; j++)
		{
			Vector3 end = AbilityTrigger.PredictPoint_Simple(null, UnityEngine.Random.value, h, 1f) + UnityEngine.Random.insideUnitSphere.Flattened() * 6f;
			end = Dew.GetValidAgentDestination_Closest(h.agentPosition, end);
			NetworkedManagerBase<ActorManager>.instance.serverActor.CreateAbilityInstance<Ai_HunterArtillery_Small>(end, null, default);
			yield return new WaitForSeconds(UnityEngine.Random.Range(0.025f, 0.1f));
		}
		NetworkedManagerBase<ActorManager>.instance.serverActor.CreateAbilityInstance<Ai_HunterArtillery_Big>(h.agentPosition, null, default).dealtDamageProcessor.Add(delegate(ref DamageData data, Actor actor, Entity target)
		{
			data.ApplyRawMultiplier(0.2f);
		});
		yield return new WaitForSeconds(2f);
		yield return SayRoutine("PlayTutorial.WhatsTheMeaningOfThis");
		At_Mon_Polaris_Smite smite = polaris.Ability.GetAbility<At_Mon_Polaris_Smite>();
		for (int j = 0; j < 2; j++)
		{
			for (int num = 0; num < hunterSpawnPoses.Length; num++)
			{
				Transform transform = hunterSpawnPoses[num];
				Entity entity;
				if (num % 3 != 0)
				{
					entity = ((num % 3 != 1) ? ((Entity)DewResources.GetByType<Mon_Forest_Hound>(default(ResourceLoadSettings))) : ((Entity)DewResources.GetByType<Mon_Forest_SpiderWarrior>(default(ResourceLoadSettings))));
				}
				else
				{
					entity = DewResources.GetByType<Mon_Forest_Treant>(default(ResourceLoadSettings));
				}
				Quaternion value = Quaternion.LookRotation(h.position - transform.position).Flattened();
				Entity entity2 = Dew.SpawnEntity(entity, transform.position, value, null, DewPlayer.creep, 1);
				entity2.CreateStatusEffect<Se_HunterBuff>(entity2, new CastInfo(entity2, entity2));
				entity2.dealtDamageProcessor.Add(delegate(ref DamageData data, Actor actor, Entity target)
				{
					data.ApplyRawMultiplier(0.33f);
				});
				entity2.AI.Aggro(h);
				AddToSpawnedEntity(entity2);
			}
			bool allDead;
			do
			{
				allDead = true;
				foreach (Entity spawnedEntity in spawnedEntities)
				{
					if (!spawnedEntity.IsNullInactiveDeadOrKnockedOut())
					{
						allDead = false;
						smite.ResetCooldown(smite);
						polaris.Control.Cast(smite, spawnedEntity);
						yield return new WaitForSeconds(UnityEngine.Random.Range(1.5f, 2.5f));
						break;
					}
				}
			}
			while (!allDead);
			spawnedEntities.Clear();
		}
		yield return PauseRoutine(1.5f);
		Vector3 goodRewardPosition = Dew.GetGoodRewardPosition(h.position);
		yield return BlinkPolarisRoutine(goodRewardPosition);
		yield return SayRoutine("PlayTutorial.Hunters");
		yield return PauseRoutine(1f);
		yield return SayRoutine("PlayTutorial.YouAreAboutToDie");
		yield return PauseRoutine(0.5f);
		yield return BlinkPolarisRoutine(polarisRiftPos.position);
		ManagerBase<ControlManager>.instance.DisableCharacterControls();
		h.Control.MoveToDestination(playerRiftPos.position, immediately: true);
		finalRift.Open();
		yield return new WaitForSeconds(1f);
		h.Control.MoveToDestination(playerRiftPos.position, immediately: true);
		yield return WaitForPlayerPrescenceRoutine();
		h.Control.RotateTowards(polaris, immediately: true, float.PositiveInfinity);
		ManagerBase<ControlManager>.instance.EnableCharacterControls();
		yield return PauseRoutine(1.5f);
		yield return SayRoutine("PlayTutorial.FacePrimusAeron");
		ManagerBase<ControlManager>.instance.DisableCharacterControls();
		polaris.CreateStatusEffect<Se_Mon_Polaris_Disappear>(polaris, new CastInfo(polaris));
		GameObject[] array2 = deactivatedObjectsAfterEverything;
		foreach (GameObject gameObject in array2)
		{
			if (gameObject != null)
			{
				gameObject.SetActive(value: false);
			}
		}
		yield return PauseRoutine(2f);
		ManagerBase<CameraManager>.instance.DoGenericFadeOut();
		yield return PauseRoutine(1.5f);
		ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
		{
			buttons = DewMessageSettings.ButtonType.Ok,
			rawContent = DewLocalization.GetUIValue("PlayTutorial_Message_EndTutorialGuidebook")
		});
		yield return new WaitWhile(() => ManagerBase<MessageManager>.instance.isShowingMessage);
		GoToPlayLobby();
	}

	[AsyncStateMachine(typeof(_003CGoToPlayLobby_003Ed__10))]
	public static UniTaskVoid GoToPlayLobby()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		_003CGoToPlayLobby_003Ed__10 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CGoToPlayLobby_003Ed__10>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	private IEnumerator SayRoutine(string key)
	{
		yield return NetworkedManagerBase<ConversationManager>.instance.StartConversationRoutine(new DewConversationSettings
		{
			player = DewPlayer.local,
			rotateTowardsCenter = true,
			speakers = new Entity[2] { h, polaris },
			startConversationKey = key,
			visibility = ConversationVisibility.Everyone
		});
	}

	private IEnumerator PauseRoutine(float duration)
	{
		ManagerBase<ControlManager>.instance.DisableCharacterControls();
		yield return new WaitForSeconds(duration);
		ManagerBase<ControlManager>.instance.EnableCharacterControls();
	}

	private IEnumerator WaitForPlayerPrescenceRoutine()
	{
		ManagerBase<ObjectiveArrowManager>.instance.objectivePosition = polaris.position;
		yield return new WaitWhile(() => Vector2.Distance(polaris.agentPosition.ToXY(), h.agentPosition.ToXY()) > 4.65f);
		ManagerBase<ObjectiveArrowManager>.instance.objectivePosition = null;
	}

	private IEnumerator BlinkPolarisRoutine(Vector3 destination)
	{
		polaris.Control.RotateTowards(h, immediately: false);
		Se_Mon_Polaris_Blink se = polaris.CreateStatusEffect<Se_Mon_Polaris_Blink>(polaris, new CastInfo(polaris, destination));
		yield return new WaitWhile(() => !se.IsNullOrInactive());
		polaris.Control.RotateTowards(h, immediately: false, float.PositiveInfinity);
	}

	protected override void OnCleanup()
	{
		Shrine[] array = shrines;
		foreach (Shrine shrine in array)
		{
			if ((UnityEngine.Object)(object)shrine != null)
			{
				shrine.MakeAvailable();
			}
		}
		if (ManagerBase<CameraManager>.instance != null)
		{
			ManagerBase<CameraManager>.instance.DoGenericFadeIn(immediately: true);
		}
		DewEffect.Stop(fxImminentHunt);
		if ((UnityEngine.Object)(object)polaris != null && polaris.Status.TryGetStatusEffect<Se_Mon_Polaris_Disappear>(out var effect))
		{
			effect.Destroy();
		}
		if (ManagerBase<MusicManager>.instance != null)
		{
			ManagerBase<MusicManager>.instance.Play(SingletonDewNetworkBehaviour<Room>.instance.music);
		}
		if (ManagerBase<ControlManager>.instance != null)
		{
			while (!ManagerBase<ControlManager>.instance.isCharacterControlEnabled)
			{
				ManagerBase<ControlManager>.instance.EnableCharacterControls();
			}
		}
		uint[] array2 = NetworkedManagerBase<ConversationManager>.instance.convSettings.Keys.ToArray();
		foreach (uint id in array2)
		{
			NetworkedManagerBase<ConversationManager>.instance.StopConversation(id);
		}
		GameObject[] array3 = deactivatedObjectsAfterEverything;
		foreach (GameObject gameObject in array3)
		{
			if (gameObject != null)
			{
				gameObject.SetActive(value: true);
			}
		}
		if ((UnityEngine.Object)(object)finalRift != null)
		{
			finalRift.Close();
		}
	}
}
