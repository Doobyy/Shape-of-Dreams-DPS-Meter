using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Mirror;
using TMPro;
using UnityEngine;

public class InGameUIManager : UIManager
{
	private class Ad_SharedMessage
	{
		public float lastShowTime;
	}

	public Action<WorldMessageSetting> onShowWorldMessage;

	public GameObject cmsgGeneralPrefab;

	public GameObject cmsgErrorPrefab;

	public Transform cmsgContainer;

	public Transform cmsgStartPivot;

	private int _disablePlayingInputCounter;

	private bool _disablePlayingInputByView;

	public GameObject[] disableOnEndingObjects;

	public CanvasGroup whiteFadeCg;

	private byte _worldNodePingCounter;

	public Action<bool> onHasWorldNodePingChanged;

	public Action<bool> onScoreboardDisplayedChanged;

	private bool _isScoreboardDisplayed;

	public Action<WorldDisplayStatus> onWorldDisplayedChanged;

	public Rift_MockExit currentMockExit;

	private WorldDisplayStatus _isWorldDisplayed;

	[NonSerialized]
	public List<RectTransform> fullWorldMapNodeItems = new List<RectTransform>();

	[NonSerialized]
	public List<RectTransform> miniWorldMapNodeItems = new List<RectTransform>();

	public new static InGameUIManager instance => ManagerBase<UIManager>.instance as InGameUIManager;

	public new static InGameUIManager softInstance => ManagerBase<UIManager>.softInstance as InGameUIManager;

	public bool disablePlayingInput
	{
		get
		{
			if (_disablePlayingInputCounter <= 0)
			{
				return _disablePlayingInputByView;
			}
			return true;
		}
	}

	public bool isDoingEnding { get; private set; }

	public bool hasWorldNodePing { get; private set; }

	public bool isScoreboardDisplayed
	{
		get
		{
			return _isScoreboardDisplayed;
		}
		set
		{
			if (_isScoreboardDisplayed == value)
			{
				return;
			}
			_isScoreboardDisplayed = value;
			try
			{
				onScoreboardDisplayedChanged?.Invoke(value);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	public WorldDisplayStatus isWorldDisplayed
	{
		get
		{
			return _isWorldDisplayed;
		}
		set
		{
			if (_isWorldDisplayed == value)
			{
				return;
			}
			_isWorldDisplayed = value;
			if (_isWorldDisplayed != WorldDisplayStatus.None)
			{
				lastWorldDisplayUnscaledTime = Time.unscaledTime;
			}
			try
			{
				onWorldDisplayedChanged?.Invoke(value);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	public float lastWorldDisplayUnscaledTime { get; private set; }

	private void Start()
	{
		AddSharedItemHandler();
		InitWorld();
		SetupGamepadBack();
		DewSteam.onGameOverlayShownChanged += new Action<bool>(OnGameOverlayShownChanged);
	}

	private void OnDestroy()
	{
		DewSteam.onGameOverlayShownChanged -= new Action<bool>(OnGameOverlayShownChanged);
	}

	private void OnGameOverlayShownChanged(bool obj)
	{
		if (obj && ManagerBase<GlobalUIManager>.instance != null && ManagerBase<GlobalUIManager>.instance.currentMenuView is UnityEngine.Object obj2 && obj2 != null && !ManagerBase<GlobalUIManager>.instance.isTutorialHighlighting && ManagerBase<GlobalUIManager>.instance.currentMenuView.CanShowMenu() && !ManagerBase<GlobalUIManager>.instance.currentMenuView.IsShowing())
		{
			ManagerBase<GlobalUIManager>.instance.currentMenuView.ShowMenu();
		}
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (isWorldDisplayed != WorldDisplayStatus.None && ManagerBase<ControlManager>.instance.it_interact.down)
		{
			isWorldDisplayed = WorldDisplayStatus.None;
		}
	}

	public void ShowCenterMessage(CenterMessageType type, string key, object[] formatArgs = null)
	{
		string text = DewLocalization.GetUIValue(key);
		if (formatArgs != null)
		{
			text = string.Format(text, formatArgs);
		}
		ShowCenterMessageRaw(type, text);
	}

	public void ShowCenterMessageRaw(CenterMessageType type, string raw)
	{
		((TMP_Text)UnityEngine.Object.Instantiate(type switch
		{
			CenterMessageType.General => cmsgGeneralPrefab, 
			CenterMessageType.Error => cmsgErrorPrefab, 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		}, cmsgStartPivot.position, Quaternion.identity, cmsgContainer).GetComponentInChildren<TextMeshProUGUI>()).text = raw;
	}

	protected override void OnStateChanged(string oldState, string newState)
	{
		base.OnStateChanged(oldState, newState);
		ManagerBase<FPSManager>.instance.UpdateFPSLimit();
	}

	public override bool ShouldDoAutoFocus()
	{
		if (!ManagerBase<MessageManager>.instance.isShowingMessage && !disablePlayingInput)
		{
			if (((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.softInstance == null || !NetworkedManagerBase<ZoneManager>.softInstance.isInAnyTransition) && (ManagerBase<ControlManager>.softInstance == null || !ManagerBase<ControlManager>.softInstance.shouldProcessCharacterInput))
			{
				return state != "Playing";
			}
			return false;
		}
		return true;
	}

	public void EnablePlayingInput()
	{
		_disablePlayingInputCounter--;
	}

	public void DisablePlayingInput()
	{
		_disablePlayingInputCounter++;
	}

	internal void UpdateDisablePlayingInputByView()
	{
		_disablePlayingInputByView = false;
		for (int num = View.instances.Count - 1; num >= 0; num--)
		{
			if (View.instances[num] == null)
			{
				View.instances.RemoveAt(num);
			}
			else if (View.instances[num].isShowing && View.instances[num].disablesInGamePlayingInput)
			{
				_disablePlayingInputByView = true;
				break;
			}
		}
	}

	public string GetContinueRelatedDescription()
	{
		if (!NetworkedManagerBase<GameManager>.instance.IsContinueSaveSupported())
		{
			return "";
		}
		if (NetworkServer.active)
		{
			if (NetworkedManagerBase<GameManager>.instance.IsEligibleForMidRunSave())
			{
				return " " + DewLocalization.GetUIValue("Menu_Message_ContinueSave_WillBeSaved");
			}
			string arg = "<color=#f77d74>" + DewLocalization.GetUIValue("Menu_Message_ContinueSave_LastSave_NoSave") + "</color>";
			try
			{
				if (DewSave.profileContinue.continueData != null)
				{
					DewPersistence.GameData gameData = DewPersistence.FromJson<DewPersistence.GameData>(DewSave.profileContinue.continueData);
					if (gameData != null)
					{
						float num = NetworkedManagerBase<GameManager>.instance.elapsedGameTime - (float)gameData.GetElapsedGameTimeSeconds();
						if (num < 0f)
						{
							num = 0f;
						}
						TimeSpan t = TimeSpan.FromSeconds(num);
						arg = "<color=#fffd7d>" + string.Format(DewLocalization.GetUIValue("Menu_Message_ContinueSave_LastSave_PastTimeTemplate"), Dew.GetReadableTimespanConcise(t)) + "</color>";
					}
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			string text = string.Format(DewLocalization.GetUIValue("Menu_Message_ContinueSave_LastSave_Template"), arg);
			return " " + DewLocalization.GetUIValue("Menu_Message_ContinueSave_SomeProgressWillBeLost") + "\n\n" + text;
		}
		if (NetworkedManagerBase<GameSettingsManager>.instance.midJoinBanType == MidJoinBanType.None && NetworkedManagerBase<GameSettingsManager>.instance.allowMidJoins == AllowMidJoinType.Disallow)
		{
			return "\n\n<color=#f77d74>" + DewLocalization.GetUIValue("Menu_Message_ContinueSave_CannotJoinLater_MidJoinDisabled") + "</color>";
		}
		if (NetworkedManagerBase<GameSettingsManager>.instance.midJoinBanType != MidJoinBanType.None)
		{
			return "\n\n<color=#f77d74>" + DewLocalization.GetUIValue("Menu_Message_ContinueSave_CannotJoinLater_" + NetworkedManagerBase<GameSettingsManager>.instance.midJoinBanType) + "</color>";
		}
		return " " + DewLocalization.GetUIValue("Menu_Message_ContinueSave_CanJoinLater");
	}

	public void SetWhiteFade(bool value)
	{
		ShortcutExtensions.DOKill((Component)(object)whiteFadeCg, false);
		if (value)
		{
			DOTweenModuleUI.DOFade(whiteFadeCg, 1f, 4f);
		}
		else
		{
			DOTweenModuleUI.DOFade(whiteFadeCg, 0f, 2f);
		}
	}

	public void SetIsDoingEnding(bool value)
	{
		disableOnEndingObjects.SetActiveAll(value: false);
		isDoingEnding = value;
	}

	private void SetupGamepadBack()
	{
		ManagerBase<GlobalUIManager>.instance.AddBackHandler(this, 2000, () =>
		{
			if (isScoreboardDisplayed)
			{
				isScoreboardDisplayed = false;
				return true;
			}
			if (DewInput.currentMode == InputMode.Gamepad && ManagerBase<GlobalUIManager>.instance.focused != null && ManagerBase<ControlManager>.instance.shouldProcessCharacterInput)
			{
				ManagerBase<GlobalUIManager>.instance.SetFocus(null);
				return false;
			}
			return false;
		});
		NetworkedManagerBase<ClientEventManager>.instance.OnTakeDamage += (Action<EventInfoDamage>)((EventInfoDamage dmg) =>
		{
			if ((UnityEngine.Object)(object)DewPlayer.local != null && (UnityEngine.Object)(object)dmg.victim == (UnityEngine.Object)(object)DewPlayer.local.hero && !dmg.damage.HasAttr(DamageAttribute.DamageOverTime) && DewInput.currentMode == InputMode.Gamepad && ManagerBase<GlobalUIManager>.instance.focused != null && ManagerBase<ControlManager>.instance.shouldProcessCharacterInput)
			{
				ManagerBase<GlobalUIManager>.instance.SetFocus(null);
			}
		});
	}

	public static bool ValidateInGameActionMessage(bool requireAliveHero = true)
	{
		if ((UnityEngine.Object)(object)NetworkedManagerBase<GameManager>.softInstance != null && !NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition)
		{
			if (requireAliveHero)
			{
				return !DewPlayer.local.hero.IsNullInactiveDeadOrKnockedOut();
			}
			return true;
		}
		return false;
	}

	public void ShowWorldPopMessage(WorldMessageSetting message)
	{
		try
		{
			onShowWorldMessage?.Invoke(message);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private void AddSharedItemHandler()
	{
		NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd += new Action<Actor>(OnActorAddSharedItem);
	}

	private void OnActorAddSharedItem(Actor a)
	{
		if (a is SkillTrigger st)
		{
			AddSharedOwnerHandlers(st);
		}
		else if (a is Gem g)
		{
			AddSharedOwnerHandlers(g);
		}
	}

	private void AddSharedOwnerHandlers(SkillTrigger st)
	{
		st.ClientEvent_OnTempOwnerChanged += (Action<DewPlayer, DewPlayer>)((DewPlayer _, DewPlayer __) =>
		{
			CheckSharedMessage(st);
		});
		st.ClientEvent_OnHandOwnerChanged += (Action<Hero, Hero>)((Hero _, Hero __) =>
		{
			CheckSharedMessage(st);
		});
		st.ClientEvent_OnOwnerChanged += (Action<Entity, Entity>)((Entity _, Entity __) =>
		{
			CheckSharedMessage(st);
		});
	}

	private void AddSharedOwnerHandlers(Gem g)
	{
		g.ClientEvent_OnTempOwnerChanged += (Action<DewPlayer, DewPlayer>)((DewPlayer _, DewPlayer __) =>
		{
			CheckSharedMessage(g);
		});
		g.ClientEvent_OnHandOwnerChanged += (Action<Hero, Hero>)((Hero _, Hero __) =>
		{
			CheckSharedMessage(g);
		});
		g.ClientEvent_OnOwnerChanged += (Action<Hero, Hero>)((Hero _, Hero __) =>
		{
			CheckSharedMessage(g);
		});
	}

	private void CheckSharedMessage(IItem item)
	{
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			yield return null;
			yield return null;
			if (DewPlayer.gamePlayers.Count > 1 && item != null)
			{
				Actor actor = item as Actor;
				if (!actor.IsNullOrInactive() && !((UnityEngine.Object)(object)item.tempOwner != null) && !((UnityEngine.Object)(object)item.owner != null) && !((UnityEngine.Object)(object)item.handOwner != null))
				{
					if (!actor.TryGetData<Ad_SharedMessage>(out var data))
					{
						data = new Ad_SharedMessage
						{
							lastShowTime = float.NegativeInfinity
						};
						actor.AddData(data);
					}
					if (!(Time.time - data.lastShowTime < 3f))
					{
						data.lastShowTime = Time.time;
						ShowSharedMessage(item);
					}
				}
			}
		}
	}

	private void ShowSharedMessage(IItem item)
	{
		ShowWorldPopMessage(new WorldMessageSetting
		{
			rawText = "<size=130%><color=#91faff>" + DewLocalization.GetUIValue("InGame_Shared") + "</color></size>",
			worldPosGetter = () => (item.worldModel == null) ? (Vector3.down * 10000f) : item.worldModel.iconQuad.transform.position
		});
	}

	private void InitWorld()
	{
		for (int i = 0; i < 100; i++)
		{
			fullWorldMapNodeItems.Add(null);
		}
		for (int j = 0; j < 100; j++)
		{
			miniWorldMapNodeItems.Add(null);
		}
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnNodesChanged += new Action(ClientEventOnNodesChanged);
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoadStarted += (Action<EventInfoLoadRoom>)((EventInfoLoadRoom _) =>
		{
			isWorldDisplayed = WorldDisplayStatus.None;
		});
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnIsInTransitionChanged += (Action<bool>)((bool _) =>
		{
			isWorldDisplayed = WorldDisplayStatus.None;
		});
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnVoteStarted += (Action<DewPlayer>)((DewPlayer _) =>
		{
			isWorldDisplayed = WorldDisplayStatus.None;
		});
		ClientEventOnNodesChanged();
		ManagerBase<GlobalUIManager>.instance.AddBackHandler(this, 20, () =>
		{
			if (isWorldDisplayed == WorldDisplayStatus.None)
			{
				return false;
			}
			isWorldDisplayed = WorldDisplayStatus.None;
			return true;
		});
		NetworkedManagerBase<ClientEventManager>.instance.OnTakeDamage += (Action<EventInfoDamage>)((EventInfoDamage dmg) =>
		{
			if (isWorldDisplayed != WorldDisplayStatus.None && (UnityEngine.Object)(object)DewPlayer.local != null && (UnityEngine.Object)(object)dmg.victim == (UnityEngine.Object)(object)DewPlayer.local.hero && !dmg.damage.HasAttr(DamageAttribute.DamageOverTime))
			{
				isWorldDisplayed = WorldDisplayStatus.None;
			}
		});
	}

	public void IncrementWorldNodePingCounter()
	{
		checked
		{
			_worldNodePingCounter = (byte)(unchecked((uint)_worldNodePingCounter) + 1u);
			UpdateHasWorldNodePing();
		}
	}

	public void DecrementWorldNodePingCounter()
	{
		checked
		{
			_worldNodePingCounter = (byte)(unchecked((uint)_worldNodePingCounter) - 1u);
			UpdateHasWorldNodePing();
		}
	}

	private void UpdateHasWorldNodePing()
	{
		bool flag = _worldNodePingCounter > 0;
		if (flag != hasWorldNodePing)
		{
			hasWorldNodePing = flag;
			try
			{
				onHasWorldNodePingChanged?.Invoke(hasWorldNodePing);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	private void ClientEventOnNodesChanged()
	{
		int count = NetworkedManagerBase<ZoneManager>.instance.nodes.Count;
		while (fullWorldMapNodeItems.Count < count)
		{
			fullWorldMapNodeItems.Add(null);
		}
		while (miniWorldMapNodeItems.Count < count)
		{
			miniWorldMapNodeItems.Add(null);
		}
	}

	public Vector2 GetWorldNodeUIPos(int index)
	{
		if (index < 0 || index >= fullWorldMapNodeItems.Count || fullWorldMapNodeItems[index] == null)
		{
			return new Vector2(-1000f, -1000f);
		}
		return fullWorldMapNodeItems[index].transform.position;
	}
}
