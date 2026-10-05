using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class EditSkillManager : ManagerBase<EditSkillManager>
{
	public enum ModeType
	{
		None = 0,
		Regular = 1,
		EquipGem = 2,
		EquipSkill = 3,
		Sell = 9,
		EditSkillShrine = 20
	}

	public int backButtonPriority = 5;

	public RectTransform dropToGroundBlocker;

	public Action<ModeType> onModeChanged;

	public Action<UnityEngine.Object> onDraggingObjectChanged;

	public Action<HeroSkillLocation, SkillTrigger> OnSkillSlotClientState;

	public Action<GemLocation, Gem> OnGemSlotClientState;

	public Action<SkillTrigger, bool> OnSkillVisibleClientState;

	public Action<Gem, bool> OnGemVisibleClientState;

	private DewInputTrigger it_skillQ;

	private DewInputTrigger it_skillW;

	private DewInputTrigger it_skillE;

	private DewInputTrigger it_skillR;

	private Actor _currentProvider;

	private DewInputTrigger it_exitEditMode;

	private List<RaycastResult> _results = new List<RaycastResult>(64);

	public Action onSelectedSlotChanged;

	private HeroSkillLocation? _selectedSkillSlot;

	private GemLocation? _selectedGemSlot;

	public GameObject skillButtons;

	public DewAudioSource confirmHoldAudio;

	[NonSerialized]
	public bool isConfirmHoldingGem;

	[NonSerialized]
	public bool isConfirmHoldingSkill;

	[NonSerialized]
	public bool didClickWhileConfirmHolding;

	[NonSerialized]
	public bool canRepeatConfirmHold;

	[NonSerialized]
	public HeroSkillLocation confirmHoldSkill;

	[NonSerialized]
	public GemLocation confirmHoldGem;

	[NonSerialized]
	public float confirmHoldNormalizedAmount;

	private float _confirmHoldStartUnscaledTime;

	public ModeType mode { get; private set; }

	public float lastModeSetUnscaledTime { get; private set; }

	public bool isDragging => (object)draggingObject != null;

	public UnityEngine.Object draggingObject { get; private set; }

	public bool shouldEndAfterAction { get; private set; }

	public Actor currentProvider
	{
		get
		{
			return _currentProvider;
		}
		private set
		{
			if (!((UnityEngine.Object)(object)value == (UnityEngine.Object)(object)_currentProvider))
			{
				_currentProvider = value;
				lastCurrentProviderSetUnscaledTime = Time.unscaledTime;
			}
		}
	}

	public float lastCurrentProviderSetUnscaledTime { get; private set; }

	public int currentModeSetFrameCount { get; private set; }

	public HeroSkillLocation? selectedSkillSlot
	{
		get
		{
			return _selectedSkillSlot;
		}
		set
		{
			if (_selectedSkillSlot != value)
			{
				_selectedSkillSlot = value;
				onSelectedSlotChanged?.Invoke();
			}
		}
	}

	public GemLocation? selectedGemSlot
	{
		get
		{
			return _selectedGemSlot;
		}
		set
		{
			if (!(_selectedGemSlot == value))
			{
				_selectedGemSlot = value;
				onSelectedSlotChanged?.Invoke();
			}
		}
	}

	public bool isSelectingGround { get; private set; }

	public bool isConfirmHolding
	{
		get
		{
			if (!isConfirmHoldingGem)
			{
				return isConfirmHoldingSkill;
			}
			return true;
		}
	}

	private void OnDestroy()
	{
		ManagerBase<InputManager>.instance?.RemoveTriggersByOwner(this);
	}

	private void Start()
	{
		it_exitEditMode = new DewInputTrigger
		{
			owner = this,
			binding = () => DewSave.profileMain.controls.interact,
			isValidCheck = () => ManagerBase<ControlManager>.instance.shouldProcessCharacterInput && mode != ModeType.None,
			checkGameAreaForMouse = true,
			priority = -4
		};
		ManagerBase<GlobalUIManager>.instance.AddBackHandler(this, backButtonPriority, () =>
		{
			if (mode == ModeType.None)
			{
				return false;
			}
			if (ManagerBase<ControlManager>.instance.isEditSkillDisabled)
			{
				return false;
			}
			EndEdit();
			return true;
		});
		it_skillQ = new DewInputTrigger
		{
			owner = this,
			binding = () => DewSave.profileMain.controls.skillQ,
			priority = -1,
			isValidCheck = () => DewInput.currentMode == InputMode.Gamepad && (mode != ModeType.None || ManagerBase<GlobalUIManager>.instance.focused != null)
		};
		it_skillW = new DewInputTrigger
		{
			owner = this,
			binding = () => DewSave.profileMain.controls.skillW,
			priority = -1,
			isValidCheck = () => DewInput.currentMode == InputMode.Gamepad && (mode != ModeType.None || ManagerBase<GlobalUIManager>.instance.focused != null)
		};
		it_skillE = new DewInputTrigger
		{
			owner = this,
			binding = () => DewSave.profileMain.controls.skillE,
			priority = -1,
			isValidCheck = () => DewInput.currentMode == InputMode.Gamepad && (mode != ModeType.None || ManagerBase<GlobalUIManager>.instance.focused != null)
		};
		it_skillR = new DewInputTrigger
		{
			owner = this,
			binding = () => DewSave.profileMain.controls.skillR,
			priority = -1,
			isValidCheck = () => DewInput.currentMode == InputMode.Gamepad && (mode != ModeType.None || ManagerBase<GlobalUIManager>.instance.focused != null)
		};
		InitGamepadInputs();
		InGameUIManager inGameUIManager = InGameUIManager.instance;
		inGameUIManager.onWorldDisplayedChanged = (Action<WorldDisplayStatus>)Delegate.Combine(inGameUIManager.onWorldDisplayedChanged, (Action<WorldDisplayStatus>)((WorldDisplayStatus _) =>
		{
			EndEdit();
		}));
		NetworkedManagerBase<ClientEventManager>.instance.OnTakeDamage += (Action<EventInfoDamage>)((EventInfoDamage dmg) =>
		{
			if (mode != ModeType.None && (mode != ModeType.Regular || DewInput.currentMode == InputMode.Gamepad) && (UnityEngine.Object)(object)DewPlayer.local != null && (UnityEngine.Object)(object)dmg.victim == (UnityEngine.Object)(object)DewPlayer.local.hero && !dmg.damage.HasAttr(DamageAttribute.DamageOverTime))
			{
				EndEdit();
			}
		});
	}

	public void StartDrag(UnityEngine.Object obj)
	{
		isSelectingGround = false;
		if (obj == null)
		{
			return;
		}
		if (obj is Gem)
		{
			ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_GemTouch");
			if (selectedGemSlot.HasValue)
			{
				selectedSkillSlot = selectedGemSlot.Value.skill;
				selectedGemSlot = null;
			}
		}
		else if (obj is SkillTrigger skill)
		{
			HeroSkill skill2 = DewPlayer.local.hero.Skill;
			if (skill2.TryGetSkillLocation(skill, out var type) && !skill2.CanReplaceSkill(type))
			{
				InGameUIManager.instance.ShowCenterMessage(CenterMessageType.Error, (type == HeroSkillLocation.Identity) ? "InGame_Message_CantReplaceCharacterSkill" : "InGame_Message_CantReplaceLockedSkill");
				return;
			}
			ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_SkillTouch");
		}
		draggingObject = obj;
		onDraggingObjectChanged?.Invoke(obj);
	}

	public void EndDrag(bool isCancel)
	{
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		if (draggingObject == null)
		{
			return;
		}
		if (isCancel)
		{
			if (draggingObject is Gem)
			{
				ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_GemTouch");
			}
			else if (draggingObject is SkillTrigger)
			{
				ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_SkillTouch");
			}
			draggingObject = null;
			onDraggingObjectChanged?.Invoke(null);
			return;
		}
		Hero hero = DewPlayer.local.hero;
		if ((UnityEngine.Object)(object)hero == null)
		{
			return;
		}
		if (isSelectingGround)
		{
			DropDraggingObjectGamepad();
			return;
		}
		if (DewInput.currentMode == InputMode.KeyboardAndMouse)
		{
			Dew.RaycastAllUIElementsBelowCursor(_results);
			foreach (RaycastResult result in _results)
			{
				RaycastResult current = result;
				IEditSkillDropTargetSkillButton componentInParent = current.gameObject.GetComponentInParent<IEditSkillDropTargetSkillButton>();
				if (componentInParent != null)
				{
					HeroSkillLocation skillType = componentInParent.skillType;
					if (draggingObject is Gem gem)
					{
						HandleGemToSkill(gem, skillType);
					}
					else if (draggingObject is SkillTrigger dragging)
					{
						HandleSkillToSkill(dragging, skillType);
					}
					draggingObject = null;
					onDraggingObjectChanged?.Invoke(null);
					return;
				}
				IEditSkillDropTargetGemSlot componentInParent2 = current.gameObject.GetComponentInParent<IEditSkillDropTargetGemSlot>();
				if (componentInParent2 != null)
				{
					GemLocation location = componentInParent2.location;
					if (draggingObject is Gem gem2)
					{
						HandleGemToGem(gem2, location);
					}
					else if (draggingObject is SkillTrigger)
					{
						HandleSkillToGem();
					}
					draggingObject = null;
					onDraggingObjectChanged?.Invoke(null);
					return;
				}
			}
			if (currentModeSetFrameCount == Time.frameCount)
			{
				return;
			}
			if ((ManagerBase<ControlManager>.instance.dropConstraint != null && !ManagerBase<ControlManager>.instance.dropConstraint(draggingObject)) || (DewInput.currentMode == InputMode.KeyboardAndMouse && dropToGroundBlocker.GetScreenSpaceRect().Contains(Input.mousePosition)))
			{
				draggingObject = null;
				onDraggingObjectChanged?.Invoke(null);
				return;
			}
			Vector3 worldPositionOnGroundOnCursor = ControlManager.GetWorldPositionOnGroundOnCursor();
			if (draggingObject is Gem gem3)
			{
				if (hero.Skill.TryGetGemLocation(gem3, out var location2))
				{
					hero.Skill.CmdUnequipGem(location2, worldPositionOnGroundOnCursor);
				}
				else
				{
					hero.Skill.CmdMoveGem(gem3, worldPositionOnGroundOnCursor);
				}
				ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_GemTouch");
			}
			else if (draggingObject is SkillTrigger skill)
			{
				if (hero.Skill.TryGetSkillLocation(skill, out var type))
				{
					hero.Skill.CmdUnequipSkill(type, worldPositionOnGroundOnCursor);
				}
				else
				{
					hero.Skill.CmdMoveSkill(skill, worldPositionOnGroundOnCursor);
				}
				ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_SkillTouch");
			}
			draggingObject = null;
			onDraggingObjectChanged?.Invoke(null);
			return;
		}
		if (selectedGemSlot.HasValue)
		{
			if (draggingObject is SkillTrigger)
			{
				HandleSkillToGem();
			}
			else if (draggingObject is Gem gem4)
			{
				HandleGemToGem(gem4, selectedGemSlot.Value);
			}
		}
		if (selectedSkillSlot.HasValue)
		{
			if (draggingObject is SkillTrigger dragging2)
			{
				HandleSkillToSkill(dragging2, selectedSkillSlot.Value);
			}
			else if (draggingObject is Gem gem5 && !HandleGemToSkill(gem5, selectedSkillSlot.Value))
			{
				return;
			}
		}
		draggingObject = null;
		onDraggingObjectChanged?.Invoke(null);
		void HandleGemToGem(Gem gem6, GemLocation slotLoc)
		{
			if (hero.Skill.TryGetGemLocation(gem6, out var location3) && slotLoc == location3)
			{
				ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_GemEquip");
			}
			else
			{
				hero.Skill.CmdSwapSlotGem(slotLoc, location3);
				ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_GemEquip");
			}
			ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_GemTouch");
		}
		bool HandleGemToSkill(Gem gem6, HeroSkillLocation heroSkillLocation)
		{
			bool flag = hero.Skill.TryGetGemLocation(gem6, out var location3);
			int emptyGemSlot = hero.Skill.GetEmptyGemSlot(heroSkillLocation);
			if (hero.Skill.GetMaxGemCount(heroSkillLocation) <= 0)
			{
				InGameUIManager.instance.ShowCenterMessage(CenterMessageType.Error, "InGame_Message_NoGemSlotOnSkill");
				return false;
			}
			if (flag && location3.skill == heroSkillLocation)
			{
				ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_GemEquip");
				if (selectedSkillSlot.HasValue)
				{
					selectedSkillSlot = null;
					selectedGemSlot = location3;
				}
			}
			else
			{
				if (emptyGemSlot == -1)
				{
					InGameUIManager.instance.ShowCenterMessage(CenterMessageType.Error, "InGame_Message_GemSlotFull");
					return false;
				}
				if (flag)
				{
					GemLocation gemLocation = new GemLocation(heroSkillLocation, emptyGemSlot);
					hero.Skill.CmdSwapSlotGem(location3, gemLocation);
					if (selectedSkillSlot.HasValue)
					{
						selectedSkillSlot = null;
						selectedGemSlot = gemLocation;
					}
				}
				else
				{
					GemLocation gemLocation2 = new GemLocation(heroSkillLocation, emptyGemSlot);
					hero.Skill.CmdEquipGem(gemLocation2, gem6);
					if (selectedSkillSlot.HasValue)
					{
						selectedSkillSlot = null;
						selectedGemSlot = gemLocation2;
					}
				}
				ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_GemEquip");
			}
			ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_GemTouch");
			return true;
		}
		static void HandleSkillToGem()
		{
			ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_SkillTouch");
		}
		void HandleSkillToSkill(SkillTrigger skill2, HeroSkillLocation targetLocation)
		{
			bool flag = hero.Skill.TryGetSkillLocation(skill2, out var type2);
			if (!hero.Skill.CanReplaceSkill(targetLocation))
			{
				InGameUIManager.instance.ShowCenterMessage(CenterMessageType.Error, (targetLocation == HeroSkillLocation.Identity) ? "InGame_Message_CantReplaceCharacterSkill" : "InGame_Message_CantReplaceLockedSkill");
			}
			else if (flag && targetLocation == type2)
			{
				ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_SkillEquip");
			}
			else
			{
				hero.Skill.CmdSwapSlotSkill(targetLocation, type2);
			}
			ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_SkillTouch");
		}
	}

	private void SetMode(ModeType newMode)
	{
		lastModeSetUnscaledTime = Time.unscaledTime;
		if (mode == ModeType.None && newMode != ModeType.None)
		{
			ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_EditStart");
		}
		else if (mode != ModeType.None && newMode == ModeType.None)
		{
			ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_EditEnd");
		}
		if (ManagerBase<ControlManager>.instance.controllingEntity is Hero hero && !hero.Skill.holdingObject.IsHoldableObjectNullOrInactive() && newMode != ModeType.EquipGem && newMode != ModeType.EquipSkill)
		{
			hero.Skill.CmdStopHoldInHand();
		}
		if (newMode != ModeType.None && ManagerBase<ControlManager>.instance.state.type != ControlManager.ControlStateType.None)
		{
			ManagerBase<ControlManager>.instance.state = default;
		}
		mode = newMode;
		currentModeSetFrameCount = Time.frameCount;
		onModeChanged?.Invoke(newMode);
		if (DewInput.currentMode == InputMode.Gamepad && ManagerBase<FloatingWindowManager>.instance.currentTarget == null)
		{
			SelectAnyRelevantSlot();
			return;
		}
		selectedSkillSlot = null;
		selectedGemSlot = null;
	}

	public void SetClientState_SetSkillSlot(HeroSkillLocation type, SkillTrigger skill)
	{
		OnSkillSlotClientState?.Invoke(type, skill);
		if ((UnityEngine.Object)(object)skill != null)
		{
			OnSkillVisibleClientState?.Invoke(skill, arg2: false);
		}
	}

	public void SetClientState_SetGemSlot(GemLocation loc, Gem gem)
	{
		OnGemSlotClientState?.Invoke(loc, gem);
		if ((UnityEngine.Object)(object)gem != null)
		{
			OnGemVisibleClientState?.Invoke(gem, arg2: false);
		}
	}

	public void SetClientState_MergeGemVictim(Gem gem)
	{
		OnGemVisibleClientState?.Invoke(gem, arg2: false);
	}

	public void StartRegularEdit(bool endAfterAction)
	{
		shouldEndAfterAction = endAfterAction;
		SetMode(ModeType.Regular);
	}

	public void StartEquipGem(Gem gem)
	{
		shouldEndAfterAction = true;
		SetMode(ModeType.EquipGem);
		ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_GemTouch");
	}

	public void StartEquipSkill(SkillTrigger skill)
	{
		shouldEndAfterAction = true;
		SetMode(ModeType.EquipSkill);
		ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_SkillTouch");
	}

	public void StartSell(PropEnt_Merchant_Base merchant)
	{
		shouldEndAfterAction = false;
		currentProvider = merchant;
		SetMode(ModeType.Sell);
		ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_SkillTouch");
	}

	public void StartEditSkillShrine(EditSkillShrine provider)
	{
		currentProvider = provider;
		shouldEndAfterAction = provider.ShouldExitEditModeAfterAction();
		SetMode(ModeType.EditSkillShrine);
	}

	public void EndEdit()
	{
		ModeType modeType = mode;
		if ((modeType == ModeType.EquipGem || modeType == ModeType.EquipSkill) && (UnityEngine.Object)(object)DewPlayer.local != null && (UnityEngine.Object)(object)DewPlayer.local.hero != null)
		{
			DewPlayer.local.hero.Skill.CmdStopHoldInHand();
		}
		currentProvider = null;
		if (draggingObject != null)
		{
			EndDrag(isCancel: true);
		}
		SetMode(ModeType.None);
	}

	public void NotifyEndOfAction()
	{
		switch (mode)
		{
		case ModeType.Regular:
			if (shouldEndAfterAction && !DewInput.GetButton(DewSave.profileMain.controls.editSkillHold, checkGameAreaForMouse: false))
			{
				StartCoroutine(DelayedEndEditRoutine());
			}
			else
			{
				StartRegularEdit(endAfterAction: false);
			}
			break;
		case ModeType.EquipGem:
		case ModeType.EquipSkill:
			EndEdit();
			break;
		case ModeType.Sell:
		case ModeType.EditSkillShrine:
			if (shouldEndAfterAction)
			{
				EndEdit();
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case ModeType.None:
			break;
		}
		IEnumerator DelayedEndEditRoutine()
		{
			yield return new WaitForSecondsRealtime(0.01f);
			EndEdit();
		}
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		LogicUpdate_Hold();
		if ((UnityEngine.Object)(object)DewPlayer.local == null || (UnityEngine.Object)(object)DewPlayer.local.hero == null)
		{
			if (mode != ModeType.None)
			{
				EndEdit();
			}
			return;
		}
		if (isDragging && draggingObject == null)
		{
			EndDrag(isCancel: true);
		}
		ModeType modeType = mode;
		if (modeType == ModeType.Sell || modeType == ModeType.EditSkillShrine)
		{
			if ((UnityEngine.Object)(object)currentProvider == null || !currentProvider.isActive)
			{
				EndEdit();
			}
			else if (currentProvider is IInteractable interactable && !interactable.CanInteract(DewPlayer.local.hero))
			{
				EndEdit();
			}
			else if (Vector3.Distance(currentProvider.position, DewPlayer.local.hero.position) > ManagerBase<FloatingWindowManager>.instance.maxDistance)
			{
				EndEdit();
			}
		}
		if (mode != ModeType.None && ManagerBase<CameraManager>.instance.isSpectating)
		{
			EndEdit();
		}
		modeType = mode;
		if (modeType == ModeType.EquipGem || modeType == ModeType.EquipSkill)
		{
			IItem holdingObject = DewPlayer.local.hero.Skill.holdingObject;
			if (holdingObject.IsHoldableObjectNullOrInactive() || ((UnityEngine.Object)(object)holdingObject.owner != null && (UnityEngine.Object)(object)holdingObject.owner != (UnityEngine.Object)(object)DewPlayer.local.hero) || (UnityEngine.Object)(object)holdingObject.handOwner != (UnityEngine.Object)(object)DewPlayer.local.hero)
			{
				EndEdit();
			}
		}
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		FrameUpdate_Hold();
		if (!InGameUIManager.instance.IsState("Playing") || ManagerBase<CameraManager>.instance.isSpectating || !ManagerBase<ControlManager>.instance.isCharacterControlEnabled)
		{
			if (mode != ModeType.None && !ManagerBase<ControlManager>.instance.isEditSkillDisabled)
			{
				EndEdit();
			}
			return;
		}
		if (DewInput.currentMode == InputMode.Gamepad)
		{
			ModeType modeType = mode;
			if ((modeType == ModeType.EquipGem || modeType == ModeType.EquipSkill) && ManagerBase<GlobalUIManager>.instance.focused == null)
			{
				SetMode(mode);
			}
		}
		if (ManagerBase<ControlManager>.instance.isEditSkillDisabled)
		{
			return;
		}
		if (mode != ModeType.None && it_exitEditMode.down)
		{
			EndEdit();
			return;
		}
		if (ManagerBase<ControlManager>.instance.it_editSkillToggle.down)
		{
			if (mode == ModeType.None)
			{
				StartRegularEdit(endAfterAction: false);
			}
			else
			{
				EndEdit();
			}
		}
		if (DewInput.GetButtonDown(DewSave.profileMain.controls.editSkillHold, checkGameAreaForMouse: false) && mode == ModeType.None && ManagerBase<ControlManager>.instance.shouldProcessCharacterInput)
		{
			StartRegularEdit(endAfterAction: false);
		}
		if (DewInput.GetButtonUp(DewSave.profileMain.controls.editSkillHold, checkGameAreaForMouse: false) && mode != ModeType.None && mode != ModeType.Sell)
		{
			EndEdit();
		}
		if (DewInput.currentMode == InputMode.Gamepad && ManagerBase<GlobalUIManager>.instance.focused == null)
		{
			DoGamepadInputs();
		}
	}

	public bool IsSlotSelectable(HeroSkillLocation skillLoc)
	{
		if (mode == ModeType.None)
		{
			return false;
		}
		if ((UnityEngine.Object)(object)DewPlayer.local == null)
		{
			return false;
		}
		Hero hero = DewPlayer.local.hero;
		if ((UnityEngine.Object)(object)hero == null)
		{
			return false;
		}
		SkillTrigger skill = hero.Skill.GetSkill(skillLoc);
		if (draggingObject is SkillTrigger)
		{
			return DewPlayer.local.hero.Skill.CanReplaceSkill(skillLoc);
		}
		if (mode == ModeType.EquipGem || draggingObject is Gem)
		{
			return hero.Skill.GetMaxGemCount(skillLoc) > 0;
		}
		if (mode == ModeType.EquipSkill)
		{
			if (!((UnityEngine.Object)(object)skill == null))
			{
				return DewPlayer.local.hero.Skill.CanReplaceSkill(skillLoc);
			}
			return true;
		}
		return true;
	}

	public bool IsSlotSelectable(GemLocation gemLoc)
	{
		if (draggingObject is SkillTrigger)
		{
			return false;
		}
		if (mode == ModeType.None)
		{
			return false;
		}
		if ((UnityEngine.Object)(object)DewPlayer.local == null)
		{
			return false;
		}
		Hero hero = DewPlayer.local.hero;
		if ((UnityEngine.Object)(object)hero == null)
		{
			return false;
		}
		Gem gem = hero.Skill.GetGem(gemLoc);
		if (mode == ModeType.EquipGem || draggingObject is Gem)
		{
			int maxGemCount = hero.Skill.GetMaxGemCount(gemLoc.skill);
			if (gemLoc.index >= 0)
			{
				return gemLoc.index < maxGemCount;
			}
			return false;
		}
		if (mode == ModeType.EditSkillShrine && currentProvider is EditSkillShrine editSkillShrine)
		{
			EditSkillTargetType targetTypes = editSkillShrine.GetTargetTypes(DewPlayer.local);
			bool num = (UnityEngine.Object)(object)gem != null && targetTypes.HasFlag(EditSkillTargetType.Gem) && editSkillShrine.GetTargetInfo(DewPlayer.local, gemLoc, gem).cost.CanAfford(DewPlayer.local) == AffordType.Yes;
			bool flag = (UnityEngine.Object)(object)gem == null && targetTypes.HasFlag(EditSkillTargetType.GemEmptySlot) && editSkillShrine.GetTargetInfo(DewPlayer.local, gemLoc, gem).cost.CanAfford(DewPlayer.local) == AffordType.Yes;
			return num | flag;
		}
		if (mode == ModeType.Sell)
		{
			return (UnityEngine.Object)(object)gem != null;
		}
		if (mode == ModeType.Regular)
		{
			return (UnityEngine.Object)(object)gem != null;
		}
		return false;
	}

	public bool IsSlotHighlighted(HeroSkillLocation skillLoc)
	{
		if ((UnityEngine.Object)(object)DewPlayer.local == null || (UnityEngine.Object)(object)DewPlayer.local.hero == null)
		{
			return false;
		}
		SkillTrigger skill = DewPlayer.local.hero.Skill.GetSkill(skillLoc);
		if (draggingObject is SkillTrigger)
		{
			return DewPlayer.local.hero.Skill.CanReplaceSkill(skillLoc);
		}
		if (draggingObject is Gem gem)
		{
			if (DewPlayer.local.hero.Skill.GetEmptyGemSlot(skillLoc) < 0)
			{
				if (DewPlayer.local.hero.Skill.TryGetGemLocation(gem, out var location))
				{
					return location.skill == skillLoc;
				}
				return false;
			}
			return true;
		}
		if (mode == ModeType.EquipGem)
		{
			if (!ManagerBase<ControlManager>.instance.gemLocationConstraint.HasValue || ManagerBase<ControlManager>.instance.gemLocationConstraint == skillLoc)
			{
				return DewPlayer.local.hero.Skill.GetEmptyGemSlot(skillLoc) >= 0;
			}
			return false;
		}
		if (mode == ModeType.Regular)
		{
			return false;
		}
		if (mode == ModeType.EditSkillShrine && currentProvider is EditSkillShrine editSkillShrine)
		{
			EditSkillTargetType targetTypes = editSkillShrine.GetTargetTypes(DewPlayer.local);
			bool num = (UnityEngine.Object)(object)skill != null && targetTypes.HasFlag(EditSkillTargetType.Skill) && editSkillShrine.GetTargetInfo(DewPlayer.local, skillLoc, skill).cost.CanAfford(DewPlayer.local) == AffordType.Yes && editSkillShrine.GetTargetInfo(DewPlayer.local, skillLoc, skill).rejectReasonRawText == null;
			bool flag = (UnityEngine.Object)(object)skill == null && targetTypes.HasFlag(EditSkillTargetType.SkillEmptySlot) && editSkillShrine.GetTargetInfo(DewPlayer.local, skillLoc, skill).cost.CanAfford(DewPlayer.local) == AffordType.Yes && editSkillShrine.GetTargetInfo(DewPlayer.local, skillLoc, skill).rejectReasonRawText == null;
			return num | flag;
		}
		if (mode == ModeType.Sell)
		{
			if ((UnityEngine.Object)(object)skill != null)
			{
				return DewPlayer.local.hero.Skill.CanReplaceSkill(skillLoc);
			}
			return false;
		}
		if (!IsSlotSelectable(skillLoc))
		{
			return false;
		}
		return true;
	}

	public bool IsSlotHighlighted(GemLocation gemLoc)
	{
		if (!IsSlotSelectable(gemLoc))
		{
			return false;
		}
		if (mode == ModeType.EquipGem || draggingObject is Gem)
		{
			if (ManagerBase<ControlManager>.instance.gemLocationConstraint.HasValue)
			{
				return ManagerBase<ControlManager>.instance.gemLocationConstraint == gemLoc.skill;
			}
			return true;
		}
		return true;
	}

	public void DoClickOnSkillButton(HeroSkillLocation skillType)
	{
		SkillTrigger skill = DewPlayer.local.hero.Skill.GetSkill(skillType);
		if (mode == ModeType.None || draggingObject != null)
		{
			return;
		}
		Hero hero = DewPlayer.local.hero;
		if (hero.IsNullInactiveDeadOrKnockedOut())
		{
			return;
		}
		bool flag = hero.Skill.CanReplaceSkill(skillType);
		if (mode == ModeType.Regular)
		{
			if ((UnityEngine.Object)(object)skill != null && (ManagerBase<ControlManager>.instance.dropConstraint == null || ManagerBase<ControlManager>.instance.dropConstraint((UnityEngine.Object)(object)skill)))
			{
				if (flag)
				{
					DewPlayer.local.hero.Skill.CmdUnequipSkill(skillType, DewPlayer.local.hero.position + UnityEngine.Random.insideUnitSphere.Flattened() * 3f);
					ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_SkillUnequip");
				}
				else
				{
					InGameUIManager.instance.ShowCenterMessage(CenterMessageType.Error, (skillType == HeroSkillLocation.Identity) ? "InGame_Message_CantReplaceCharacterSkill" : "InGame_Message_CantReplaceLockedSkill");
				}
			}
		}
		else if (mode == ModeType.EquipGem)
		{
			if (ManagerBase<ControlManager>.instance.gemLocationConstraint.HasValue && skillType != ManagerBase<ControlManager>.instance.gemLocationConstraint.Value)
			{
				return;
			}
			if (hero.Skill.GetMaxGemCount(skillType) <= 0)
			{
				InGameUIManager.instance.ShowCenterMessage(CenterMessageType.Error, "InGame_Message_NoGemSlotOnSkill");
				return;
			}
			int emptyGemSlot = hero.Skill.GetEmptyGemSlot(skillType);
			if (emptyGemSlot == -1)
			{
				InGameUIManager.instance.ShowCenterMessage(CenterMessageType.Error, "InGame_Message_GemSlotFull");
				return;
			}
			hero.Skill.CmdEquipGem(new GemLocation(skillType, emptyGemSlot), (Gem)hero.Skill.holdingObject);
			NotifyEndOfAction();
			ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_GemEquip");
		}
		else if (mode == ModeType.EquipSkill)
		{
			if (!hero.Skill.CanReplaceSkill(skillType))
			{
				InGameUIManager.instance.ShowCenterMessage(CenterMessageType.Error, (skillType == HeroSkillLocation.Identity) ? "InGame_Message_CantReplaceCharacterSkill" : "InGame_Message_CantReplaceLockedSkill");
				return;
			}
			if ((UnityEngine.Object)(object)hero.Skill.GetSkill(skillType) != null)
			{
				hero.Skill.CmdUnequipSkill(skillType, hero.position + UnityEngine.Random.insideUnitSphere.Flattened() * 3f);
			}
			hero.Skill.CmdEquipSkill(skillType, (SkillTrigger)DewPlayer.local.hero.Skill.holdingObject);
			NotifyEndOfAction();
			ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_SkillEquip");
		}
		else if (mode == ModeType.EditSkillShrine && currentProvider is EditSkillShrine editSkillShrine)
		{
			EditSkillTargetType targetTypes = editSkillShrine.GetTargetTypes(DewPlayer.local);
			SkillTrigger skillTrigger = skill;
			if (((UnityEngine.Object)(object)skillTrigger != null && !targetTypes.HasFlag(EditSkillTargetType.Skill)) || ((UnityEngine.Object)(object)skillTrigger == null && !targetTypes.HasFlag(EditSkillTargetType.SkillEmptySlot)))
			{
				return;
			}
			if (DewPlayer.local.hero.Ability.IsAbilityEditLocked((int)skillType))
			{
				InGameUIManager.instance.ShowCenterMessage(CenterMessageType.Error, "InGame_Message_CantTargetLockedSkill");
				return;
			}
			EditSkillTargetInfo targetInfo = editSkillShrine.GetTargetInfo(DewPlayer.local, skillType, skillTrigger);
			if (targetInfo.rejectReasonRawText != null)
			{
				InGameUIManager.instance.ShowCenterMessageRaw(CenterMessageType.Error, targetInfo.rejectReasonRawText);
				return;
			}
			AffordType affordType = targetInfo.cost.CanAfford(DewPlayer.local);
			if (affordType != AffordType.Yes)
			{
				InGameUIManager.instance.ShowCenterMessage(CenterMessageType.Error, "InGame_Message_CannotAfford" + affordType);
				return;
			}
			editSkillShrine.CmdActivateEditSkillAction(skillType, null);
			NotifyEndOfAction();
		}
		else
		{
			if (mode != ModeType.Sell)
			{
				return;
			}
			if (!DewPlayer.local.hero.Skill.CanReplaceSkill(skillType))
			{
				if (skillType == HeroSkillLocation.Identity)
				{
					InGameUIManager.instance.ShowCenterMessage(CenterMessageType.Error, "InGame_Message_CantSellCharacterSkill");
				}
				else
				{
					InGameUIManager.instance.ShowCenterMessage(CenterMessageType.Error, "InGame_Message_CantReplaceLockedSkill");
				}
			}
			else if (!((UnityEngine.Object)(object)skill == null))
			{
				ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_Shop_Sell");
				((PropEnt_Merchant_Base)currentProvider).CmdSell((NetworkBehaviour)(object)skill);
			}
		}
	}

	public void DoClickOnGemSlot(GemLocation location)
	{
		if (draggingObject != null || (UnityEngine.Object)(object)DewPlayer.local == null)
		{
			return;
		}
		Hero hero = DewPlayer.local.hero;
		if (hero.IsNullInactiveDeadOrKnockedOut())
		{
			return;
		}
		Gem gem = hero.Skill.GetGem(location);
		Gem gem2 = gem;
		ModeType modeType = mode;
		switch (modeType)
		{
		case ModeType.Regular:
			if (!ManagerBase<ControlManager>.instance.isEditSkillDisabled && (UnityEngine.Object)(object)gem2 != null && (ManagerBase<ControlManager>.instance.dropConstraint == null || ManagerBase<ControlManager>.instance.dropConstraint((UnityEngine.Object)(object)gem2)))
			{
				DewPlayer.local.hero.Skill.CmdUnequipGem(location, DewPlayer.local.hero.position + UnityEngine.Random.insideUnitSphere.Flattened() * 3f);
				ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_GemUnequip");
			}
			return;
		case ModeType.EquipGem:
			if (!ManagerBase<ControlManager>.instance.gemLocationConstraint.HasValue || (location.skill == ManagerBase<ControlManager>.instance.gemLocationConstraint.Value && !((UnityEngine.Object)(object)gem2 != null)))
			{
				if ((UnityEngine.Object)(object)gem2 != null)
				{
					DewPlayer.local.hero.Skill.CmdUnequipGem(location, DewPlayer.local.hero.position + UnityEngine.Random.insideUnitSphere.Flattened() * 3f);
				}
				hero.Skill.CmdEquipGem(location, (Gem)hero.Skill.holdingObject);
				NotifyEndOfAction();
				ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_GemEquip");
			}
			return;
		case ModeType.EditSkillShrine:
		{
			if (!(currentProvider is EditSkillShrine editSkillShrine))
			{
				break;
			}
			EditSkillTargetType targetTypes = editSkillShrine.GetTargetTypes(DewPlayer.local);
			Gem gem3 = gem;
			if (((UnityEngine.Object)(object)gem3 != null && !targetTypes.HasFlag(EditSkillTargetType.Gem)) || ((UnityEngine.Object)(object)gem3 == null && !targetTypes.HasFlag(EditSkillTargetType.GemEmptySlot)))
			{
				return;
			}
			EditSkillTargetInfo targetInfo = editSkillShrine.GetTargetInfo(DewPlayer.local, location, gem3);
			if (targetInfo.rejectReasonRawText != null)
			{
				InGameUIManager.instance.ShowCenterMessageRaw(CenterMessageType.Error, targetInfo.rejectReasonRawText);
				return;
			}
			AffordType affordType = targetInfo.cost.CanAfford(DewPlayer.local);
			if (affordType != AffordType.Yes)
			{
				InGameUIManager.instance.ShowCenterMessage(CenterMessageType.Error, "InGame_Message_CannotAfford" + affordType);
				return;
			}
			editSkillShrine.CmdActivateEditSkillAction(null, location);
			NotifyEndOfAction();
			return;
		}
		}
		if (modeType == ModeType.Sell && (UnityEngine.Object)(object)gem != null)
		{
			ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_Shop_Sell");
			((PropEnt_Merchant_Base)currentProvider).CmdSell((NetworkBehaviour)(object)gem);
		}
	}

	private void InitGamepadInputs()
	{
		onSelectedSlotChanged = (Action)Delegate.Combine(onSelectedSlotChanged, (Action)(() =>
		{
			isSelectingGround = false;
		}));
	}

	public void SelectGround()
	{
		if (ManagerBase<ControlManager>.instance.dropConstraint == null || ManagerBase<ControlManager>.instance.dropConstraint(draggingObject))
		{
			isSelectingGround = true;
		}
	}

	public void UnselectGround()
	{
		isSelectingGround = false;
	}

	public void ClearGamepadSelection()
	{
		selectedSkillSlot = null;
		selectedGemSlot = null;
	}

	public bool DoDpadUp()
	{
		if (selectedGemSlot.HasValue)
		{
			selectedSkillSlot = selectedGemSlot.Value.skill;
			if (TryGetNextRelevantSkillLocation(next: true, skipStart: false, canWrap: true, out var loc))
			{
				selectedSkillSlot = loc;
				selectedGemSlot = null;
				return true;
			}
			selectedSkillSlot = null;
		}
		if (ManagerBase<FloatingWindowManager>.instance.currentTarget != null)
		{
			return false;
		}
		if (DewInput.GetButtonDown((GamepadButtonEx?)GamepadButtonEx.DpadUp))
		{
			ManagerBase<GlobalUIManager>.instance.SetFocus(null);
		}
		return true;
	}

	public bool DoDpadLeft()
	{
		if (selectedGemSlot.HasValue && TryGetNextRelevantGemSlot(next: false, skipStart: true, draggingObject != null, out var loc))
		{
			selectedGemSlot = loc;
			return true;
		}
		if (selectedSkillSlot.HasValue && TryGetNextRelevantSkillLocation(next: false, skipStart: true, draggingObject != null, out var loc2))
		{
			selectedSkillSlot = loc2;
			return true;
		}
		if (ManagerBase<ControlManager>.instance.dropConstraint != null || ManagerBase<ControlManager>.instance.gemLocationConstraint.HasValue)
		{
			return true;
		}
		return false;
	}

	public bool DoDpadDown()
	{
		if (selectedSkillSlot.HasValue)
		{
			selectedGemSlot = new GemLocation(selectedSkillSlot.Value, 0);
			if (TryGetNextRelevantGemSlot(next: true, skipStart: false, canWrap: true, out var loc))
			{
				selectedGemSlot = loc;
				selectedSkillSlot = null;
			}
			else
			{
				selectedGemSlot = null;
			}
		}
		return true;
	}

	public bool DoDpadRight()
	{
		if (selectedGemSlot.HasValue && TryGetNextRelevantGemSlot(next: true, skipStart: true, draggingObject != null, out var loc))
		{
			selectedGemSlot = loc;
			return true;
		}
		if (selectedSkillSlot.HasValue && TryGetNextRelevantSkillLocation(next: true, skipStart: true, draggingObject != null, out var loc2))
		{
			selectedSkillSlot = loc2;
			return true;
		}
		if (ManagerBase<ControlManager>.instance.dropConstraint != null || ManagerBase<ControlManager>.instance.gemLocationConstraint.HasValue)
		{
			return true;
		}
		return false;
	}

	public bool DoConfirm()
	{
		if ((UnityEngine.Object)(object)DewPlayer.local == null)
		{
			return true;
		}
		Hero hero = DewPlayer.local.hero;
		if ((UnityEngine.Object)(object)hero == null)
		{
			return true;
		}
		if (draggingObject != null)
		{
			EndDrag(isCancel: false);
		}
		else if (mode == ModeType.Regular)
		{
			Gem gem;
			if (selectedSkillSlot.HasValue && hero.Skill.TryGetSkill(selectedSkillSlot.Value, out var skill))
			{
				StartDrag((UnityEngine.Object)(object)skill);
			}
			else if (selectedGemSlot.HasValue && hero.Skill.TryGetGem(selectedGemSlot.Value, out gem))
			{
				StartDrag((UnityEngine.Object)(object)gem);
			}
		}
		else if (selectedSkillSlot.HasValue)
		{
			if (ShouldUseHoldToConfirm(selectedSkillSlot.Value))
			{
				StartConfirmHold(selectedSkillSlot.Value);
			}
			else
			{
				DoClickOnSkillButton(selectedSkillSlot.Value);
			}
		}
		else if (selectedGemSlot.HasValue)
		{
			if (ShouldUseHoldToConfirm(selectedGemSlot.Value))
			{
				StartConfirmHold(selectedGemSlot.Value);
			}
			else
			{
				DoClickOnGemSlot(selectedGemSlot.Value);
			}
		}
		return true;
	}

	public bool DoBack()
	{
		if (ManagerBase<ControlManager>.instance.isEditSkillDisabled)
		{
			return true;
		}
		if (ManagerBase<FloatingWindowManager>.instance.currentTarget != null)
		{
			ManagerBase<FloatingWindowManager>.instance.ClearTarget();
		}
		EndEdit();
		return true;
	}

	private void DropDraggingObjectGamepad()
	{
		if (ManagerBase<ControlManager>.instance.dropConstraint == null || ManagerBase<ControlManager>.instance.dropConstraint(draggingObject))
		{
			Vector3 goodRewardPosition = Dew.GetGoodRewardPosition(DewPlayer.local.hero.agentPosition);
			HeroSkillLocation type;
			if (draggingObject is Gem gem && DewPlayer.local.hero.Skill.TryGetGemLocation(gem, out var location))
			{
				DewPlayer.local.hero.Skill.CmdUnequipGem(location, goodRewardPosition);
				ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_GemTouch");
			}
			else if (draggingObject is SkillTrigger skill && DewPlayer.local.hero.Skill.TryGetSkillLocation(skill, out type))
			{
				DewPlayer.local.hero.Skill.CmdUnequipSkill(type, goodRewardPosition);
				ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_SkillTouch");
			}
			draggingObject = null;
			onDraggingObjectChanged?.Invoke(null);
		}
	}

	private void DoGamepadInputs()
	{
		if (!((UnityEngine.Object)(object)DewPlayer.local == null) && !((UnityEngine.Object)(object)DewPlayer.local.hero == null) && (mode != ModeType.None || (ManagerBase<GlobalUIManager>.instance.focused != null && !InGameUIManager.instance.disablePlayingInput)))
		{
			if (it_skillQ.down)
			{
				ManagerBase<GlobalUIManager>.instance.SetFocus(skillButtons.GetComponent<IGamepadFocusable>());
				selectedSkillSlot = HeroSkillLocation.Q;
				selectedGemSlot = null;
			}
			if (it_skillW.down)
			{
				ManagerBase<GlobalUIManager>.instance.SetFocus(skillButtons.GetComponent<IGamepadFocusable>());
				selectedSkillSlot = HeroSkillLocation.W;
				selectedGemSlot = null;
			}
			if (it_skillE.down)
			{
				ManagerBase<GlobalUIManager>.instance.SetFocus(skillButtons.GetComponent<IGamepadFocusable>());
				selectedSkillSlot = HeroSkillLocation.E;
				selectedGemSlot = null;
			}
			if (it_skillR.down)
			{
				ManagerBase<GlobalUIManager>.instance.SetFocus(skillButtons.GetComponent<IGamepadFocusable>());
				selectedSkillSlot = HeroSkillLocation.R;
				selectedGemSlot = null;
			}
			if (ManagerBase<ControlManager>.instance.aimDirection.HasValue && Time.unscaledTime - lastModeSetUnscaledTime > 0.3f)
			{
				EndEdit();
			}
		}
	}

	public void SelectAnyRelevantSlot()
	{
		selectedSkillSlot = null;
		selectedGemSlot = null;
		HeroSkillLocation loc;
		GemLocation loc2;
		if (ManagerBase<ControlManager>.instance.gemLocationConstraint.HasValue && IsSlotSelectable(ManagerBase<ControlManager>.instance.gemLocationConstraint.Value))
		{
			selectedSkillSlot = ManagerBase<ControlManager>.instance.gemLocationConstraint.Value;
		}
		else if (TryGetNextRelevantSkillLocation(next: true, skipStart: false, canWrap: false, out loc))
		{
			selectedSkillSlot = loc;
		}
		else if (TryGetNextRelevantGemSlot(next: true, skipStart: false, canWrap: false, out loc2))
		{
			selectedGemSlot = loc2;
		}
	}

	private bool TryGetNextRelevantSkillLocation(bool next, bool skipStart, bool canWrap, out HeroSkillLocation loc)
	{
		loc = HeroSkillLocation.Q;
		if ((UnityEngine.Object)(object)DewPlayer.local == null)
		{
			return false;
		}
		if ((UnityEngine.Object)(object)DewPlayer.local.hero == null)
		{
			return false;
		}
		HeroSkillLocation valueOrDefault = selectedSkillSlot.GetValueOrDefault();
		HeroSkillLocation heroSkillLocation = valueOrDefault;
		bool flag = true;
		while (flag || heroSkillLocation != valueOrDefault)
		{
			if ((!flag || !skipStart) && IsSlotSelectable(heroSkillLocation))
			{
				loc = heroSkillLocation;
				return true;
			}
			flag = false;
			if (next)
			{
				if (heroSkillLocation == HeroSkillLocation.Identity)
				{
					if (!canWrap)
					{
						return false;
					}
					heroSkillLocation = HeroSkillLocation.Q;
				}
				else
				{
					heroSkillLocation++;
				}
			}
			else if (heroSkillLocation == HeroSkillLocation.Q)
			{
				if (!canWrap)
				{
					return false;
				}
				heroSkillLocation = HeroSkillLocation.Identity;
			}
			else
			{
				heroSkillLocation--;
			}
		}
		if (IsSlotSelectable(valueOrDefault))
		{
			loc = valueOrDefault;
			return true;
		}
		return false;
	}

	private bool TryGetNextRelevantGemSlot(bool next, bool skipStart, bool canWrap, out GemLocation loc)
	{
		loc = default;
		if ((UnityEngine.Object)(object)DewPlayer.local == null)
		{
			return false;
		}
		Hero hero = DewPlayer.local.hero;
		if ((UnityEngine.Object)(object)hero == null)
		{
			return false;
		}
		GemLocation gemLocation = selectedGemSlot ?? new GemLocation(HeroSkillLocation.Q, 0);
		GemLocation gemLocation2 = gemLocation;
		bool flag = true;
		while (flag || gemLocation2 != gemLocation)
		{
			if ((!flag || !skipStart) && IsSlotSelectable(gemLocation2))
			{
				loc = gemLocation2;
				return true;
			}
			flag = false;
			if (next)
			{
				gemLocation2.index++;
				if (gemLocation2.index < hero.Skill.GetMaxGemCount(gemLocation2.skill))
				{
					continue;
				}
				gemLocation2.index = 0;
				if (gemLocation2.skill == HeroSkillLocation.Identity)
				{
					if (!canWrap)
					{
						return false;
					}
					gemLocation2.skill = HeroSkillLocation.Q;
				}
				else
				{
					gemLocation2.skill++;
				}
				continue;
			}
			gemLocation2.index--;
			if (gemLocation2.index >= 0)
			{
				continue;
			}
			if (gemLocation2.skill == HeroSkillLocation.Q)
			{
				if (!canWrap)
				{
					return false;
				}
				gemLocation2.skill = HeroSkillLocation.Identity;
			}
			else
			{
				gemLocation2.skill--;
			}
			gemLocation2.index = hero.Skill.GetMaxGemCount(gemLocation2.skill) - 1;
		}
		if (IsSlotSelectable(gemLocation))
		{
			loc = gemLocation;
			return true;
		}
		return false;
	}

	public void StartConfirmHold(HeroSkillLocation skill)
	{
		isConfirmHoldingSkill = true;
		isConfirmHoldingGem = false;
		didClickWhileConfirmHolding = false;
		canRepeatConfirmHold = mode == ModeType.EditSkillShrine && currentProvider is EditSkillShrine editSkillShrine && editSkillShrine.canRepeatConfirmHold;
		confirmHoldSkill = skill;
		confirmHoldNormalizedAmount = 0f;
		_confirmHoldStartUnscaledTime = Time.unscaledTime;
	}

	public void StartConfirmHold(GemLocation gem)
	{
		isConfirmHoldingSkill = false;
		isConfirmHoldingGem = true;
		didClickWhileConfirmHolding = false;
		canRepeatConfirmHold = mode == ModeType.EditSkillShrine && currentProvider is EditSkillShrine editSkillShrine && editSkillShrine.canRepeatConfirmHold;
		confirmHoldGem = gem;
		confirmHoldNormalizedAmount = 0f;
		_confirmHoldStartUnscaledTime = Time.unscaledTime;
	}

	public void StopConfirmHold(bool showHelpIfNeeded = false)
	{
		if (showHelpIfNeeded && confirmHoldNormalizedAmount < 0.35f && !didClickWhileConfirmHolding)
		{
			string raw;
			if (mode == ModeType.Sell)
			{
				raw = ((DewInput.currentMode != InputMode.KeyboardAndMouse) ? string.Format(DewLocalization.GetUIValue("InGame_Message_HoldToConfirmSell_Gamepad"), DewInput.GetReadableTextOfGamepad(DewSave.profileMain.controls.confirm)) : DewLocalization.GetUIValue("InGame_Message_HoldToConfirmSell_PC"));
			}
			else
			{
				raw = ((DewInput.currentMode != InputMode.KeyboardAndMouse) ? string.Format(DewLocalization.GetUIValue("InGame_Message_HoldToConfirmGeneric_Gamepad"), DewInput.GetReadableTextOfGamepad(DewSave.profileMain.controls.confirm)) : DewLocalization.GetUIValue("InGame_Message_HoldToConfirmGeneric_PC"));
			}
			InGameUIManager.instance.ShowCenterMessageRaw(CenterMessageType.General, raw);
		}
		isConfirmHoldingSkill = false;
		isConfirmHoldingGem = false;
		confirmHoldNormalizedAmount = 0f;
	}

	public bool ShouldUseHoldToConfirm(HeroSkillLocation skill)
	{
		if (ShouldUseHoldToConfirm())
		{
			return IsSlotHighlighted(skill);
		}
		return false;
	}

	public bool ShouldUseHoldToConfirm(GemLocation gem)
	{
		if (ShouldUseHoldToConfirm())
		{
			return IsSlotHighlighted(gem);
		}
		return false;
	}

	private bool ShouldUseHoldToConfirm()
	{
		if (DewInput.GetButton((Key)51))
		{
			return false;
		}
		ConfirmMemoryEditBehavior confirmMemoryEditByHold = DewSave.profileMain.gameplay.confirmMemoryEditByHold;
		if (confirmMemoryEditByHold == ConfirmMemoryEditBehavior.Off)
		{
			return false;
		}
		if (mode == ModeType.Sell)
		{
			if (confirmMemoryEditByHold != ConfirmMemoryEditBehavior.All)
			{
				return confirmMemoryEditByHold == ConfirmMemoryEditBehavior.SellOnly;
			}
			return true;
		}
		if (mode == ModeType.EditSkillShrine)
		{
			return confirmMemoryEditByHold == ConfirmMemoryEditBehavior.All;
		}
		return false;
	}

	private void FrameUpdate_Hold()
	{
		if (!isConfirmHolding)
		{
			confirmHoldNormalizedAmount = 0f;
			if (confirmHoldAudio.isPlaying)
			{
				confirmHoldAudio.Stop();
			}
			return;
		}
		if ((UnityEngine.Object)(object)DewPlayer.local == null || (UnityEngine.Object)(object)DewPlayer.local.hero == null || (mode != ModeType.EditSkillShrine && mode != ModeType.Sell))
		{
			StopConfirmHold();
			return;
		}
		if (DewInput.currentMode == InputMode.Gamepad)
		{
			if (!DewInput.GetButton(DewSave.profileMain.controls.confirm, checkGameAreaForMouse: false))
			{
				StopConfirmHold(showHelpIfNeeded: true);
				return;
			}
			if (isConfirmHoldingGem && (!selectedGemSlot.HasValue || selectedGemSlot.Value != confirmHoldGem))
			{
				StopConfirmHold();
				return;
			}
			if (isConfirmHoldingSkill && (!selectedSkillSlot.HasValue || selectedSkillSlot.Value != confirmHoldSkill))
			{
				StopConfirmHold();
				return;
			}
		}
		Hero hero = DewPlayer.local.hero;
		if (isConfirmHoldingSkill && hero.Skill.GetSkill(confirmHoldSkill).IsNullOrInactive())
		{
			StopConfirmHold();
			return;
		}
		if (isConfirmHoldingGem && hero.Skill.GetGem(confirmHoldGem).IsNullOrInactive())
		{
			StopConfirmHold();
			return;
		}
		float num = 1f + Mathf.Clamp((Time.unscaledTime - _confirmHoldStartUnscaledTime) * 0.5f, 0f, 10f);
		confirmHoldNormalizedAmount = Mathf.Clamp01(confirmHoldNormalizedAmount + Time.unscaledDeltaTime * num);
		if (confirmHoldNormalizedAmount >= 1f)
		{
			confirmHoldAudio.Stop();
			confirmHoldNormalizedAmount = 0f;
			if (isConfirmHoldingSkill)
			{
				DoClickOnSkillButton(confirmHoldSkill);
			}
			else
			{
				DoClickOnGemSlot(confirmHoldGem);
			}
			didClickWhileConfirmHolding = true;
			if (!canRepeatConfirmHold)
			{
				StopConfirmHold();
				return;
			}
		}
		confirmHoldAudio.pitchMultiplier = num;
		if (!confirmHoldAudio.isPlaying)
		{
			confirmHoldAudio.Play();
		}
	}

	private void LogicUpdate_Hold()
	{
	}
}
