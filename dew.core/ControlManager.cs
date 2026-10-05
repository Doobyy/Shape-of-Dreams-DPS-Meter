using System;
using System.Collections.Generic;
using DG.Tweening;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[LogicUpdatePriority(-500)]
public class ControlManager : ManagerBase<ControlManager>
{
	private enum AttackMoveType
	{
		UseUserSettings,
		UseDistanceFromSelf,
		UseDistanceFromDestination
	}

	public enum ControlStateType
	{
		None = 0,
		AttackMove = 1,
		Cast = 4
	}

	public struct ControlState
	{
		public ControlStateType type;

		public AbilityTrigger trigger;

		public int configIndex;

		public DewBinding castKey;

		public CastConfirmType castType;

		public bool isCastInDirectionOfMovement;
	}

	private enum GpState
	{
		Idle,
		Holding
	}

	private class GamepadPingState
	{
		public GpState current;

		public bool isRightStick;

		public float startTime = float.NegativeInfinity;

		public void Reset()
		{
			current = GpState.Idle;
			isRightStick = false;
			startTime = float.NegativeInfinity;
		}
	}

	private class ScreenPointDistanceComparer : IComparer<RaycastHit>
	{
		public int Compare(RaycastHit x, RaycastHit y)
		{
			return x.distance.CompareTo(y.distance);
		}
	}

	private class ScreenPointRayPerpComparer : IComparer<RaycastHit>
	{
		public Ray ray;

		public int Compare(RaycastHit x, RaycastHit y)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			return Perp(x).CompareTo(Perp(y));
		}

		private float Perp(RaycastHit hit)
		{
			return Vector3.Cross(ray.direction, hit.point - ray.origin).magnitude;
		}
	}

	public const float DirectionalControlStandStillActionGracePeriod = 0.3f;

	private const float RaycastMaxDistance = 200f;

	private const float ContinuousMoveDispatchInterval = 0.0625f;

	private const float DirectionalMoveDispatchInterval = 1f / 60f;

	private const float AttackInPlaceDispatchInterval = 1f / 60f;

	private const bool NonChanneledAttackInPlaceStopsMovement = true;

	private const bool AttackInPlaceAutoTarget = true;

	private const float MovementDirectionStaleTime = 0.4f;

	private const float MinWalkStrengthForGamepads = 0.4f;

	private const bool EnableContinuousAttackMove = true;

	public Action<AbilityTrigger> onCastFailed;

	public Action<Entity, Entity> onSelectedEntityChanged;

	public ControlState state;

	private byte _disableCharacterControlCounter;

	[NonSerialized]
	public bool isMainSkillDisabled;

	[NonSerialized]
	public bool isDodgeDisabled;

	[NonSerialized]
	public bool isEditSkillDisabled;

	[NonSerialized]
	public bool isDismantleDisabled;

	[NonSerialized]
	public bool isWorldMapDisabled;

	[NonSerialized]
	public HeroSkillLocation? gemLocationConstraint;

	[NonSerialized]
	public Func<UnityEngine.Object, bool> dismantleConstraint;

	[NonSerialized]
	public Func<UnityEngine.Object, bool> dropConstraint;

	public CommandMarker attackMarker;

	public CommandMarker moveMarker;

	public CommandMarker interactMarker;

	public GameObject noTargetIndicator;

	public Transform noTargetIndicatorParent;

	public GameObject targetEnemyIndicatorWorld;

	public GameObject targetEnemyIndicatorUi;

	public GameObject aimPointUi;

	private readonly Collider[] _interactCheckColliders = new Collider[128];

	private IInteractable _interactable;

	private float _lastInteractableCheck;

	private bool _isContinuousMove;

	private List<RaycastResult> _raycastResults;

	private float _lastLookSendUnscaledTime;

	private float _lastTargetEnemyUpdateTime;

	private Func<Entity, bool> _targetEnemyValidator;

	private float _lastStopIssueTime;

	private bool _isDoingDirectionalMovement;

	private float _lastMovementDispatchUnscaledTime;

	private float _lastMovementDirectionUnscaledTime = float.NegativeInfinity;

	private bool _isDoingContinuousAttack;

	private float _lastAttackInPlaceDispatchTime = float.NegativeInfinity;

	private float _lastAttackMoveDispatchTime = float.NegativeInfinity;

	private static bool _isInputFieldFocused;

	private static int _isInputFieldFocusedCachedFrame;

	private float _lastNoTargetShowTime;

	private float _lastAttackRangeShowTime;

	public static readonly HeroSkillLocation[] HeroSkillTypes = new HeroSkillLocation[6]
	{
		HeroSkillLocation.Q,
		HeroSkillLocation.W,
		HeroSkillLocation.E,
		HeroSkillLocation.R,
		HeroSkillLocation.Identity,
		HeroSkillLocation.Movement
	};

	private GamepadPingState _gpState = new GamepadPingState();

	private const float DirectionalAttackYOffset = 0.75f;

	private static readonly RaycastHit[] s_screenPointHits = new RaycastHit[128];

	private static readonly ScreenPointDistanceComparer s_screenPointDistanceComparer = new ScreenPointDistanceComparer();

	private static readonly ScreenPointRayPerpComparer s_screenPointRayPerpComparer = new ScreenPointRayPerpComparer();

	private static Vector3 _cachedPosition;

	private static int _cachedFrame = -1;

	private const float NearbyInteractableCheckInterval = 0.1f;

	private const float InteractionCheckRange = 6f;

	private readonly ComponentCache<Collider, IInteractable> _interactableCache = new ComponentCache<Collider, IInteractable>(128, ComponentCache<Collider, IInteractable>.ComponentSource.Parent);

	public Action<IInteractable> onFocusedInteractableChanged;

	private float _lastInteractAltUnscaledTime;

	private const float CastByKeyPressStaleTime = 1f;

	internal CastInfo _currentCastInfo;

	private const float SampleUpdateInterval = 0.05f;

	private float _lastSampleUpdateTime = float.NegativeInfinity;

	internal Dictionary<HeroSkillLocation, (DewInputTrigger, float)> _castByKeyInfo = new Dictionary<HeroSkillLocation, (DewInputTrigger, float)>();

	public DewInputTrigger it_confirmCast;

	public DewInputTrigger it_move;

	public DewInputTrigger it_attackMoveNormal;

	public DewInputTrigger it_attackMoveImmediately;

	public DewInputTrigger it_attackMoveOnRelease;

	public DewInputTrigger it_attackInPlace;

	public DewInputTrigger it_scoreboard;

	public DewInputTrigger it_worldMap;

	public DewInputTrigger it_stop;

	public DewInputTrigger it_moveUp;

	public DewInputTrigger it_moveLeft;

	public DewInputTrigger it_moveDown;

	public DewInputTrigger it_moveRight;

	public DewInputTrigger it_skillQ;

	public DewInputTrigger it_skillW;

	public DewInputTrigger it_skillE;

	public DewInputTrigger it_skillR;

	public DewInputTrigger it_skillMovement;

	public DewInputTrigger it_cancelNormalCast;

	public DewInputTrigger it_skillQNormal;

	public DewInputTrigger it_skillWNormal;

	public DewInputTrigger it_skillENormal;

	public DewInputTrigger it_skillRNormal;

	public DewInputTrigger it_skillMovementNormal;

	public DewInputTrigger it_skillQImmediately;

	public DewInputTrigger it_skillWImmediately;

	public DewInputTrigger it_skillEImmediately;

	public DewInputTrigger it_skillRImmediately;

	public DewInputTrigger it_skillMovementImmediately;

	public DewInputTrigger it_skillQOnRelease;

	public DewInputTrigger it_skillWOnRelease;

	public DewInputTrigger it_skillEOnRelease;

	public DewInputTrigger it_skillROnRelease;

	public DewInputTrigger it_skillMovementOnRelease;

	public DewInputTrigger it_skillQSelf;

	public DewInputTrigger it_skillWSelf;

	public DewInputTrigger it_skillESelf;

	public DewInputTrigger it_skillRSelf;

	public DewInputTrigger it_skillQEdit;

	public DewInputTrigger it_skillWEdit;

	public DewInputTrigger it_skillEEdit;

	public DewInputTrigger it_skillREdit;

	public DewInputTrigger it_skillIdentityEdit;

	public DewInputTrigger it_interact;

	public DewInputTrigger it_interactAlt;

	public DewInputTrigger it_editSkillHold;

	public DewInputTrigger it_editSkillToggle;

	public DewInputTrigger it_showDetails;

	public DewInputTrigger it_zoomOutCamera;

	public DewInputTrigger it_zoomInCamera;

	public DewInputTrigger it_ping;

	public DewInputTrigger it_travelVote;

	public DewInputTrigger it_travelVoteCancel;

	public DewInputTrigger it_spectatorNextTarget;

	public Entity controllingEntity
	{
		get
		{
			if ((UnityEngine.Object)(object)DewPlayer.local == null)
			{
				return null;
			}
			return DewPlayer.local.controllingEntity;
		}
	}

	public bool isCharacterControlEnabled => _disableCharacterControlCounter == 0;

	public bool shouldProcessCharacterInput { get; private set; }

	public bool shouldProcessCharacterInputAllowKnockedOut { get; private set; }

	public Vector3? aimDirection { get; private set; }

	public Vector3? aimPoint { get; private set; }

	public Entity targetEnemy { get; private set; }

	public bool isMovementSchemeDirectional { get; private set; }

	public bool isLastMovementDirectionFresh => Time.unscaledTime - _lastMovementDirectionUnscaledTime < 0.4f;

	public Vector3 lastMovementDirection { get; private set; }

	public float lastMovementStrength { get; private set; }

	public IInteractable focusedInteractable { get; private set; }

	public bool isFocusedInteractableAtCursor { get; private set; }

	public SampleCastInfoContext? localSampleContext { get; internal set; }

	private bool canSendSampleUpdate => Time.unscaledTime - _lastSampleUpdateTime > 0.05f;

	protected override void Awake()
	{
		base.Awake();
		InitializeTriggers();
	}

	private void Start()
	{
		onCastFailed = (Action<AbilityTrigger>)Delegate.Combine(onCastFailed, (Action<AbilityTrigger>)((AbilityTrigger skill) =>
		{
			if ((UnityEngine.Object)(object)DewPlayer.local.hero.Skill.Movement == (UnityEngine.Object)(object)skill)
			{
				ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_Control_DodgeUnavailable");
			}
			else
			{
				ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_Control_CastUnavailable");
			}
		}));
		ManagerBase<GlobalUIManager>.instance.AddBackHandler(this, 25, () =>
		{
			if (state.type == ControlStateType.Cast && state.castType == CastConfirmType.Normal)
			{
				state = default;
				return true;
			}
			return false;
		});
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		shouldProcessCharacterInputAllowKnockedOut = GetShouldProcessCharacterInputAllowKnockedOut();
		shouldProcessCharacterInput = shouldProcessCharacterInputAllowKnockedOut && (!(controllingEntity is Hero hero) || !hero.isKnockedOut);
		if (shouldProcessCharacterInput)
		{
			GetCharInputOfVote();
		}
		GetScoreboardAndMapInput();
		if (!shouldProcessCharacterInput || InGameUIManager.instance.isWorldDisplayed != WorldDisplayStatus.None)
		{
			targetEnemy = null;
		}
		if (!shouldProcessCharacterInput)
		{
			if (ShouldProcessPingInput())
			{
				GetCharInputOfPing();
			}
			SetFocusedInteractable(null);
			state = default;
			_isDoingContinuousAttack = false;
			aimDirection = null;
			aimPoint = null;
			if (_isDoingDirectionalMovement)
			{
				_isDoingDirectionalMovement = false;
				controllingEntity.Control.CmdClearMovement();
			}
		}
		else
		{
			Input.imeCompositionMode = (IMECompositionMode)2;
			if (Time.time - _lastInteractableCheck > 0.1f)
			{
				_lastInteractableCheck = Time.time;
				UpdateInteractableFocus(alsoCheckNearby: true);
			}
			else
			{
				UpdateInteractableFocus(alsoCheckNearby: false);
			}
			GetCharacterInputs();
		}
	}

	private void LateUpdate()
	{
		if (!(ManagerBase<DewCamera>.instance == null))
		{
			if (DewInput.currentMode != InputMode.Gamepad || (UnityEngine.Object)(object)targetEnemy == null)
			{
				targetEnemyIndicatorWorld.SetActive(value: false);
				targetEnemyIndicatorUi.SetActive(value: false);
			}
			else
			{
				targetEnemyIndicatorWorld.SetActive(value: true);
				targetEnemyIndicatorUi.SetActive(value: true);
				targetEnemyIndicatorWorld.transform.position = targetEnemy.position;
				targetEnemyIndicatorUi.transform.position = ManagerBase<DewCamera>.softInstance.mainCamera.WorldToScreenPoint(targetEnemy.Visual.GetCenterPosition());
			}
			if (aimPoint.HasValue)
			{
				aimPointUi.transform.position = ManagerBase<DewCamera>.softInstance.mainCamera.WorldToScreenPoint(aimPoint.Value + Vector3.up * 0.75f);
				aimPointUi.SetActive(value: true);
			}
			else
			{
				aimPointUi.SetActive(value: false);
			}
		}
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (ManagerBase<CameraManager>.instance.isPlayingCutscene && (UnityEngine.Object)(object)controllingEntity != null && Time.time - _lastStopIssueTime > 0.25f)
		{
			_lastStopIssueTime = Time.time;
			controllingEntity.Control.CmdStop();
		}
	}

	public void EnableCharacterControls()
	{
		checked
		{
			_disableCharacterControlCounter = (byte)(unchecked((uint)_disableCharacterControlCounter) - 1u);
		}
	}

	public void DisableCharacterControls()
	{
		if ((UnityEngine.Object)(object)DewPlayer.local != null && (UnityEngine.Object)(object)DewPlayer.local.hero != null)
		{
			DewPlayer.local.hero.Control.CmdCancelOngoingChannels();
			DewPlayer.local.hero.Control.CmdStop();
		}
		checked
		{
			_disableCharacterControlCounter = (byte)(unchecked((uint)_disableCharacterControlCounter) + 1u);
		}
	}

	private void GetCharacterInputs()
	{
		if (isEditSkillDisabled && ManagerBase<EditSkillManager>.instance.mode != EditSkillManager.ModeType.None)
		{
			if (_isDoingDirectionalMovement)
			{
				_isDoingDirectionalMovement = false;
				controllingEntity.Control.CmdClearMovement();
			}
			return;
		}
		GetCharInputOfAimDirection();
		UpdateTargetEnemy();
		GetCharInputOfInteractByButtonPress();
		GetCharInputOfMove();
		GetCharInputOfCast();
		GetCharInputOfAttack();
		GetCharInputOfStop();
		GetCharInputOfPing();
		GetCharInputOfConfirmCast();
	}

	private void GetCharInputOfAimDirection()
	{
		Vector2 vector = DewInput.GetRightJoystick();
		if (DewInput.currentMode == InputMode.Gamepad && (ManagerBase<GlobalUIManager>.instance.focused != null || SingletonBehaviour<UI_EmoteWheel>.instance.isShown))
		{
			vector = Vector2.zero;
		}
		if (vector == Vector2.zero)
		{
			aimDirection = null;
			aimPoint = null;
			return;
		}
		aimDirection = ManagerBase<CameraManager>.instance.entityCamAngleRotation * new Vector3(vector.x, 0f, vector.y);
		float num = 8f;
		if (localSampleContext.HasValue && localSampleContext.Value.castMethod.type == CastMethodType.Point && localSampleContext.Value.castMethod.pointData.range > num)
		{
			num = localSampleContext.Value.castMethod.pointData.range;
		}
		Vector3 agentPosition = controllingEntity.agentPosition;
		Vector3? vector2 = aimDirection * num;
		aimPoint = agentPosition + vector2;
		if (Time.unscaledTime - _lastLookSendUnscaledTime > 0.05f && !controllingEntity.Control.isGamepadRotationLocked)
		{
			controllingEntity.Control.CmdLookInDirection(aimDirection.Value);
			_lastLookSendUnscaledTime = Time.unscaledTime;
		}
	}

	private bool ValidateTargetEnemy(Entity e)
	{
		Entity entity = controllingEntity;
		if ((UnityEngine.Object)(object)entity != null && entity.CheckEnemyOrNeutral(e) && !e.Status.hasUntargetable && !e.Status.isUndetectableByNonAllies)
		{
			return !(e is IDisableGamepadTargeting);
		}
		return false;
	}

	private void UpdateTargetEnemy()
	{
		if ((UnityEngine.Object)(object)targetEnemy != null && (!targetEnemy.isActive || targetEnemy.Status.isUndetectableByNonAllies))
		{
			targetEnemy = null;
		}
		if (InGameUIManager.instance.isWorldDisplayed != WorldDisplayStatus.None || Time.time - _lastTargetEnemyUpdateTime < 0.05f)
		{
			return;
		}
		_lastTargetEnemyUpdateTime = Time.time;
		if (_targetEnemyValidator == null)
		{
			_targetEnemyValidator = ValidateTargetEnemy;
		}
		List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, aimPoint ?? controllingEntity.agentPosition, 12f, _targetEnemyValidator, new CollisionCheckSettings
		{
			includeUncollidable = true,
			sortComparer = CollisionCheckSettings.DistanceFromCenter
		});
		if (list.Count == 0)
		{
			targetEnemy = null;
		}
		else
		{
			Entity entity = list[0];
			for (int i = 0; i < list.Count; i++)
			{
				if (controllingEntity.GetRelation(list[i]) == EntityRelation.Enemy)
				{
					entity = list[i];
					break;
				}
			}
			if ((UnityEngine.Object)(object)targetEnemy != (UnityEngine.Object)(object)entity)
			{
				targetEnemy = entity;
				targetEnemyIndicatorUi.transform.localScale = Vector3.one;
				ShortcutExtensions.DOKill((Component)targetEnemyIndicatorUi.transform, true);
				ShortcutExtensions.DOPunchScale(targetEnemyIndicatorUi.transform, new Vector3(0.5f, 0.5f, 0.5f), 0.5f, 10, 1f);
			}
		}
		handle.Return();
	}

	private void GetCharInputOfVote()
	{
		if (NetworkedManagerBase<ZoneManager>.instance.isVoting)
		{
			if (it_travelVote.down)
			{
				DewPlayer.local.CmdSetIsReady(!DewPlayer.local.isReady);
			}
			if (it_travelVoteCancel.down && !DewPlayer.local.hero.IsNullInactiveDeadOrKnockedOut())
			{
				NetworkedManagerBase<ZoneManager>.instance.CmdCancelVote();
			}
		}
	}

	private void GetCharInputOfConfirmCast()
	{
		if (state.type == ControlStateType.AttackMove && it_confirmCast.down)
		{
			DoAttackMoveAtCursor();
			state = default;
		}
		if (state.type != ControlStateType.Cast || !it_confirmCast.down)
		{
			return;
		}
		if (!ShouldInvalidateCurrentCast())
		{
			if (state.isCastInDirectionOfMovement ? CastAbilityInDirectionOfMovement(state.trigger) : CastAbilityAtCursor(state.trigger))
			{
				state.type = ControlStateType.None;
			}
		}
		else
		{
			state.type = ControlStateType.None;
		}
	}

	private void GetCharInputOfPing()
	{
		if (DewInput.currentMode == InputMode.KeyboardAndMouse)
		{
			if (it_ping.down)
			{
				SendPingPC();
			}
		}
		else
		{
			GetCharInputOfGamepadPingAndEmote();
		}
	}

	private void SendPingPC()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Expected Obj, but got Unknown
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Expected Obj, but got Unknown
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Expected Obj, but got Unknown
		PingManager.Ping ping = default;
		_raycastResults = new List<RaycastResult>();
		Dew.RaycastAllUIElementsBelowCursor(_raycastResults);
		foreach (RaycastResult raycastResult in _raycastResults)
		{
			RaycastResult current = raycastResult;
			IPingableWorldNode componentInParent = current.gameObject.GetComponentInParent<IPingableWorldNode>();
			if (componentInParent != null)
			{
				ping.type = PingManager.PingType.WorldNode;
				ping.itemIndex = componentInParent.nodeIndex;
				NetworkedManagerBase<PingManager>.instance.CmdSendPing(ping);
				return;
			}
			IPingableUIElement componentInParent2 = current.gameObject.GetComponentInParent<IPingableUIElement>();
			if (componentInParent2 != null && componentInParent2.pingTarget != null)
			{
				if (componentInParent2.pingTarget is SkillTrigger target)
				{
					ping.target = (NetworkBehaviour)(object)target;
				}
				else if (componentInParent2.pingTarget is Gem target2)
				{
					ping.target = (NetworkBehaviour)(object)target2;
				}
				ping.type = PingManager.PingType.EquippedItem;
				NetworkedManagerBase<PingManager>.instance.CmdSendPing(ping);
				return;
			}
			IPingableShopItem componentInParent3 = current.gameObject.GetComponentInParent<IPingableShopItem>();
			if (componentInParent3 != null && componentInParent3.shop != null)
			{
				ping.type = PingManager.PingType.ShopItem;
				ping.target = (NetworkBehaviour)componentInParent3.shop;
				ping.itemIndex = componentInParent3.merchandiseIndex;
				NetworkedManagerBase<PingManager>.instance.CmdSendPing(ping);
				return;
			}
			IPingableChoiceItem componentInParent4 = current.gameObject.GetComponentInParent<IPingableChoiceItem>();
			if (componentInParent4 != null && componentInParent4.shrine != null)
			{
				ping.type = PingManager.PingType.ChoiceItem;
				ping.target = (NetworkBehaviour)componentInParent4.shrine;
				ping.itemIndex = componentInParent4.choiceIndex;
				NetworkedManagerBase<PingManager>.instance.CmdSendPing(ping);
				return;
			}
			IPingableCustom componentInParent5 = current.gameObject.GetComponentInParent<IPingableCustom>();
			if (componentInParent5 == null)
			{
				continue;
			}
			try
			{
				if (componentInParent5.OnPing())
				{
					return;
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		IInteractable interactableOnCursor = GetInteractableOnCursor();
		if (interactableOnCursor != null)
		{
			ping.type = PingManager.PingType.Interactable;
			ping.target = (NetworkBehaviour)interactableOnCursor;
			NetworkedManagerBase<PingManager>.instance.CmdSendPing(ping);
		}
		else if ((UnityEngine.Object)(object)GetEntityOnCursor() != null)
		{
			Entity entityOnCursor = GetEntityOnCursor();
			ping.target = (NetworkBehaviour)(object)entityOnCursor;
			ping.type = PingManager.PingType.Entity;
			NetworkedManagerBase<PingManager>.instance.CmdSendPing(ping);
		}
		else
		{
			ping.type = PingManager.PingType.Move;
			ping.position = GetWorldPositionOnGroundOnCursor();
			NetworkedManagerBase<PingManager>.instance.CmdSendPing(ping);
		}
	}

	public void SendPingGamepad()
	{
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Expected Obj, but got Unknown
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Expected Obj, but got Unknown
		PingManager.Ping ping = default;
		if (ManagerBase<GlobalUIManager>.instance.focused != null)
		{
			MonoBehaviour monoBehaviour = (MonoBehaviour)ManagerBase<GlobalUIManager>.instance.focused;
			if (!Check(monoBehaviour) && monoBehaviour.TryGetComponent<IGamepadPingProxyParent>(out var component))
			{
				Check(component.pingableTarget);
			}
			return;
		}
		IInteractable interactable = focusedInteractable;
		Entity focusedEntity = targetEnemy;
		if (aimPoint.HasValue)
		{
			Vector3 vector = ManagerBase<DewCamera>.instance.mainCamera.WorldToScreenPoint(aimPoint.Value);
			interactable = GetInteractableFromScreenPoint(vector);
			if (interactable == null)
			{
				interactable = GetInteractableFromScreenPoint(vector, 1f);
			}
			if (interactable == null)
			{
				interactable = GetInteractableFromScreenPoint(vector, 2.5f);
			}
			if (interactable != null)
			{
				ping.type = PingManager.PingType.Interactable;
				ping.target = (NetworkBehaviour)interactable;
				NetworkedManagerBase<PingManager>.instance.CmdSendPing(ping);
				return;
			}
			focusedEntity = GetEntityFromScreenPoint(vector);
			if ((UnityEngine.Object)(object)focusedEntity == null)
			{
				focusedEntity = GetEntityFromScreenPoint(vector, 1f);
			}
			if ((UnityEngine.Object)(object)focusedEntity == null)
			{
				focusedEntity = GetEntityFromScreenPoint(vector, 2.5f);
			}
			if ((UnityEngine.Object)(object)focusedEntity != null)
			{
				ping.target = (NetworkBehaviour)(object)focusedEntity;
				ping.type = PingManager.PingType.Entity;
				NetworkedManagerBase<PingManager>.instance.CmdSendPing(ping);
			}
			else
			{
				ping.type = PingManager.PingType.Move;
				ping.position = aimPoint.Value;
				NetworkedManagerBase<PingManager>.instance.CmdSendPing(ping);
			}
		}
		else if (interactable != null)
		{
			ping.type = PingManager.PingType.Interactable;
			ping.target = (NetworkBehaviour)interactable;
			NetworkedManagerBase<PingManager>.instance.CmdSendPing(ping);
		}
		else
		{
			if ((UnityEngine.Object)(object)focusedEntity == null)
			{
				focusedEntity = ManagerBase<CameraManager>.instance.focusedEntity;
			}
			if ((UnityEngine.Object)(object)focusedEntity != null)
			{
				ping.target = (NetworkBehaviour)(object)focusedEntity;
				ping.type = PingManager.PingType.Entity;
				NetworkedManagerBase<PingManager>.instance.CmdSendPing(ping);
			}
		}
		bool Check(MonoBehaviour obj)
		{
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f4: Expected Obj, but got Unknown
			//IL_0148: Unknown result type (might be due to invalid IL or missing references)
			//IL_0152: Expected Obj, but got Unknown
			if (obj == null)
			{
				return false;
			}
			IPingableWorldNode componentInParent = obj.GetComponentInParent<IPingableWorldNode>();
			if (componentInParent != null)
			{
				ping.type = PingManager.PingType.WorldNode;
				ping.itemIndex = componentInParent.nodeIndex;
				NetworkedManagerBase<PingManager>.instance.CmdSendPing(ping);
				return true;
			}
			IPingableUIElement componentInParent2 = obj.GetComponentInParent<IPingableUIElement>();
			if (componentInParent2 != null && componentInParent2.pingTarget != null)
			{
				if (componentInParent2.pingTarget is SkillTrigger target)
				{
					ping.target = (NetworkBehaviour)(object)target;
				}
				else if (componentInParent2.pingTarget is Gem target2)
				{
					ping.target = (NetworkBehaviour)(object)target2;
				}
				ping.type = PingManager.PingType.EquippedItem;
				NetworkedManagerBase<PingManager>.instance.CmdSendPing(ping);
				return true;
			}
			IPingableShopItem componentInParent3 = obj.GetComponentInParent<IPingableShopItem>();
			if (componentInParent3 != null && componentInParent3.shop != null)
			{
				ping.type = PingManager.PingType.ShopItem;
				ping.target = (NetworkBehaviour)componentInParent3.shop;
				ping.itemIndex = componentInParent3.merchandiseIndex;
				NetworkedManagerBase<PingManager>.instance.CmdSendPing(ping);
				return true;
			}
			IPingableChoiceItem componentInParent4 = obj.GetComponentInParent<IPingableChoiceItem>();
			if (componentInParent4 != null && componentInParent4.shrine != null)
			{
				ping.type = PingManager.PingType.ChoiceItem;
				ping.target = (NetworkBehaviour)componentInParent4.shrine;
				ping.itemIndex = componentInParent4.choiceIndex;
				NetworkedManagerBase<PingManager>.instance.CmdSendPing(ping);
				return true;
			}
			return false;
		}
	}

	private void GetScoreboardAndMapInput()
	{
		if (InGameUIManager.instance.IsState("Playing"))
		{
			if (it_scoreboard.down)
			{
				InGameUIManager.instance.isScoreboardDisplayed = true;
			}
			if (it_scoreboard.up && DewInput.currentMode == InputMode.KeyboardAndMouse)
			{
				InGameUIManager.instance.isScoreboardDisplayed = false;
			}
			if (isWorldMapDisabled || IsInputFieldFocused() || !it_worldMap.down)
			{
				return;
			}
			if (InGameUIManager.instance.isWorldDisplayed == WorldDisplayStatus.None)
			{
				if (NetworkedManagerBase<ZoneManager>.instance.currentZone.useSpecialGeneration)
				{
					InGameUIManager.instance.ShowCenterMessage(CenterMessageType.Error, "InGame_Message_NoWorldMapAvailable");
				}
				else if (NetworkedManagerBase<ZoneManager>.instance.currentNode.type != WorldNodeType.ExitBoss && !Rift_RoomExit.instance.IsNullOrInactive() && Rift_RoomExit.instance.isOpen && !Rift_RoomExit.instance.isLocked && !controllingEntity.IsNullInactiveDeadOrKnockedOut() && Vector2.Distance(((Component)(object)Rift_RoomExit.instance).transform.position.ToXY(), ((Component)(object)controllingEntity).transform.position.ToXY()) < 8f)
				{
					InGameUIManager.instance.isWorldDisplayed = WorldDisplayStatus.Shown;
				}
				else
				{
					InGameUIManager.instance.isWorldDisplayed = WorldDisplayStatus.ShownNoMove;
				}
				if ((UnityEngine.Object)(object)controllingEntity != null)
				{
					controllingEntity.Control.CmdStop();
				}
			}
			else
			{
				InGameUIManager.instance.isWorldDisplayed = WorldDisplayStatus.None;
			}
		}
		else if (InGameUIManager.instance.IsState("Menu"))
		{
			InGameUIManager.instance.isScoreboardDisplayed = false;
			InGameUIManager.instance.isWorldDisplayed = WorldDisplayStatus.None;
		}
	}

	private void GetCharInputOfStop()
	{
		if (it_stop.down && Time.unscaledTime - _lastStopIssueTime > 0.1f)
		{
			_lastStopIssueTime = Time.unscaledTime;
			controllingEntity.Control.CmdStop();
		}
	}

	private bool InteractWithInteractableOnCursor()
	{
		IInteractable interactableOnCursor = GetInteractableOnCursor();
		if (interactableOnCursor != null && interactableOnCursor.canInteractWithMouse)
		{
			controllingEntity.Control.CmdInteract(interactableOnCursor, isAlt: false, isMouse: true);
			Component component = (Component)interactableOnCursor;
			CommandMarker.Spawn(interactMarker, component.transform.position, Quaternion.identity).followTransform = component.transform;
			HighlightProvider componentInChildren = component.GetComponentInChildren<HighlightProvider>();
			if (componentInChildren != null)
			{
				componentInChildren.ShowClick();
			}
			return true;
		}
		return false;
	}

	private void GetCharInputOfMove()
	{
		if (isEditSkillDisabled && ManagerBase<EditSkillManager>.instance.mode != EditSkillManager.ModeType.None)
		{
			return;
		}
		if (it_move.down)
		{
			if (state.type != ControlStateType.None)
			{
				state = default;
			}
			if (NetworkedManagerBase<ConversationManager>.instance.hasOngoingLocalConversation || !InteractWithInteractableOnCursor())
			{
				if (DewSave.profileMain.controls.attackByIssuingMoveOnEnemy && (UnityEngine.Object)(object)controllingEntity.Ability.attackAbility != null)
				{
					TriggerConfig config = controllingEntity.Ability.attackAbility.currentConfig;
					if ((UnityEngine.Object)(object)GetEntityFromScreenPoint(GetMousePositionWithInversionInMind(), (Entity candidate) => config.targetValidator.Evaluate(controllingEntity, candidate), 0.75f) != null)
					{
						DoAttackMoveAtCursor(AttackMoveType.UseDistanceFromDestination);
						goto IL_00dd;
					}
				}
				MoveToCursor();
				_isContinuousMove = true;
			}
		}
		goto IL_00dd;
		IL_00dd:
		if (_isContinuousMove)
		{
			if (!it_move)
			{
				_isContinuousMove = false;
				Vector3 worldPositionOnGroundOnCursor = GetWorldPositionOnGroundOnCursor();
				worldPositionOnGroundOnCursor = Dew.GetValidAgentDestination_Closest(controllingEntity.agentPosition, worldPositionOnGroundOnCursor);
				controllingEntity.Control.CmdMoveToDestination(worldPositionOnGroundOnCursor, immediately: true);
				_lastMovementDispatchUnscaledTime = Time.unscaledTime;
			}
			else if (Time.unscaledTime - _lastMovementDispatchUnscaledTime > 0.0625f)
			{
				Vector3 vector = GetWorldPositionOnGroundOnCursor();
				Vector3 vector2 = vector - controllingEntity.agentPosition;
				float num = 1f;
				if (vector2.magnitude < num)
				{
					Vector3 worldPositionOnGroundFromViewportPoint = GetWorldPositionOnGroundFromViewportPoint(new Vector2(0.5f, 0.53f), forDirectionalAttacks: false);
					float num2 = 0f - ManagerBase<DewCamera>.instance.mainCamera.transform.eulerAngles.x;
					float num3 = 1f / Mathf.Tan(num2 * ((float)Math.PI / 180f));
					Vector3 vector3 = worldPositionOnGroundFromViewportPoint + ManagerBase<DewCamera>.instance.mainCamera.transform.forward.Flattened().normalized * num3;
					Vector3 vector4 = (vector - vector3).normalized * num;
					vector = controllingEntity.agentPosition + vector4;
				}
				vector = Dew.GetValidAgentDestination_Closest(controllingEntity.agentPosition, vector);
				controllingEntity.Control.CmdMoveToDestination(vector, immediately: true);
				_lastMovementDispatchUnscaledTime = Time.unscaledTime;
			}
		}
		Vector2 v = DewInput.GetLeftJoystick();
		if (DewInput.currentMode == InputMode.Gamepad && (ManagerBase<GlobalUIManager>.instance.focused != null || SingletonBehaviour<UI_EmoteWheel>.instance.isShown))
		{
			v = Vector2.zero;
		}
		if (DewSave.profileMain.controls.enableDirMoveKeys && DewInput.currentMode == InputMode.KeyboardAndMouse)
		{
			DewInputTrigger dewInputTrigger = it_moveUp;
			DewInputTrigger dewInputTrigger2 = it_moveLeft;
			DewInputTrigger dewInputTrigger3 = it_moveDown;
			DewInputTrigger dewInputTrigger4 = it_moveRight;
			if ((bool)dewInputTrigger && !dewInputTrigger3)
			{
				v.y = 1f;
			}
			else if (!dewInputTrigger && (bool)dewInputTrigger3)
			{
				v.y = -1f;
			}
			if ((bool)dewInputTrigger2 && !dewInputTrigger4)
			{
				v.x = -1f;
			}
			else if (!dewInputTrigger2 && (bool)dewInputTrigger4)
			{
				v.x = 1f;
			}
			if (AreControlsInverted())
			{
				v *= -1f;
			}
		}
		float num4 = v.magnitude;
		if (num4 > 1f)
		{
			v = v.normalized;
			num4 = 1f;
		}
		if (num4 > 0.0001f)
		{
			if (Time.unscaledTime - ManagerBase<FloatingWindowManager>.instance.lastSetTargetUnscaledTime > 0.3f)
			{
				ManagerBase<FloatingWindowManager>.instance.ClearTarget();
			}
			if (Time.unscaledTime - InGameUIManager.instance.lastWorldDisplayUnscaledTime > 0.3f)
			{
				InGameUIManager.instance.isWorldDisplayed = WorldDisplayStatus.None;
			}
			if (ManagerBase<EditSkillManager>.instance.mode != EditSkillManager.ModeType.None && ManagerBase<EditSkillManager>.instance.mode != EditSkillManager.ModeType.Regular && Time.unscaledTime - ManagerBase<EditSkillManager>.instance.lastModeSetUnscaledTime > 0.3f)
			{
				ManagerBase<EditSkillManager>.instance.EndEdit();
			}
			_isDoingDirectionalMovement = true;
			isMovementSchemeDirectional = true;
			lastMovementDirection = (ManagerBase<CameraManager>.instance.entityCamAngleRotation * v.ToXZ()).normalized;
			lastMovementStrength = num4;
			_lastMovementDirectionUnscaledTime = Time.unscaledTime;
			if (Time.unscaledTime - _lastMovementDispatchUnscaledTime > 1f / 60f)
			{
				controllingEntity.Control.CmdMoveWithDirection(lastMovementDirection * (0.4f + 0.6f * num4));
				_lastMovementDispatchUnscaledTime = Time.unscaledTime;
			}
		}
		else if (_isDoingDirectionalMovement)
		{
			_isDoingDirectionalMovement = false;
			controllingEntity.Control.CmdClearMovement();
		}
	}

	private void GetCharInputOfAttack()
	{
		if ((UnityEngine.Object)(object)controllingEntity.Ability.attackAbility == null)
		{
			return;
		}
		if (DewSave.profileMain.controls.gamepadAttackByAiming && aimDirection.HasValue && aimDirection.Value.magnitude > 0.99f && SingletonBehaviour<UI_EmoteWheel>.instance.GetElapsedTimeAfterLastClose() > 0.4f && controllingEntity.Control.IsActionBlocked(EntityControl.BlockableAction.Attack) == EntityControl.BlockStatus.Allowed)
		{
			DoAttackInPlaceWithAim();
		}
		if (it_attackMoveImmediately.down)
		{
			_lastAttackMoveDispatchTime = Time.time;
			DoAttackMoveAtCursor();
		}
		else if ((bool)it_attackMoveImmediately && Time.time - _lastAttackMoveDispatchTime > 1f / 60f)
		{
			_lastAttackMoveDispatchTime = Time.time;
			DoAttackMoveAtCursor(AttackMoveType.UseUserSettings, hideIndicator: true);
		}
		if (it_attackMoveNormal.down)
		{
			state.type = ControlStateType.AttackMove;
			state.castType = CastConfirmType.Normal;
		}
		if (it_attackMoveOnRelease.down)
		{
			state.type = ControlStateType.AttackMove;
			state.castType = CastConfirmType.OnRelease;
		}
		if (state.type == ControlStateType.AttackMove && state.castType == CastConfirmType.OnRelease && it_attackMoveOnRelease.up)
		{
			DoAttackMoveAtCursor();
			state = default;
		}
		if (it_attackInPlace.down)
		{
			if (GetInteractableOnCursor() is Entity || !InteractWithInteractableOnCursor())
			{
				_lastNoTargetShowTime = float.NegativeInfinity;
				_isDoingContinuousAttack = true;
			}
		}
		else if (_isDoingContinuousAttack && !it_attackInPlace)
		{
			_isDoingContinuousAttack = false;
		}
		if (!_isDoingContinuousAttack || !(Time.time - _lastAttackInPlaceDispatchTime > 1f / 60f))
		{
			return;
		}
		if (DewInput.currentMode == InputMode.Gamepad)
		{
			if (aimDirection.HasValue)
			{
				DoAttackInPlaceWithAim();
			}
			else
			{
				DoAttackInPlaceAuto();
			}
		}
		else
		{
			DoAttackInPlaceAtCursor();
		}
		_lastAttackInPlaceDispatchTime = Time.time;
	}

	private void GetCharInputOfCast()
	{
		if ((bool)it_editSkillHold)
		{
			return;
		}
		bool isSamplingCastInfo = ProcessCastInfoSampling();
		if (controllingEntity is Hero)
		{
			if (it_cancelNormalCast.down)
			{
				state = default;
			}
			CheckSkillCast(HeroSkillLocation.Q, it_skillQ, isSelfCast: false, null);
			CheckSkillCast(HeroSkillLocation.W, it_skillW, isSelfCast: false, null);
			CheckSkillCast(HeroSkillLocation.E, it_skillE, isSelfCast: false, null);
			CheckSkillCast(HeroSkillLocation.R, it_skillR, isSelfCast: false, null);
			CheckSkillCast(HeroSkillLocation.Movement, it_skillMovement, isSelfCast: false, null);
			CheckSkillCast(HeroSkillLocation.Q, it_skillQSelf, isSelfCast: true, null);
			CheckSkillCast(HeroSkillLocation.W, it_skillWSelf, isSelfCast: true, null);
			CheckSkillCast(HeroSkillLocation.E, it_skillESelf, isSelfCast: true, null);
			CheckSkillCast(HeroSkillLocation.R, it_skillRSelf, isSelfCast: true, null);
			CheckSkillCast(HeroSkillLocation.Q, it_skillQImmediately, isSelfCast: false, CastConfirmType.Immediately);
			CheckSkillCast(HeroSkillLocation.W, it_skillWImmediately, isSelfCast: false, CastConfirmType.Immediately);
			CheckSkillCast(HeroSkillLocation.E, it_skillEImmediately, isSelfCast: false, CastConfirmType.Immediately);
			CheckSkillCast(HeroSkillLocation.R, it_skillRImmediately, isSelfCast: false, CastConfirmType.Immediately);
			CheckSkillCast(HeroSkillLocation.Movement, it_skillMovementImmediately, isSelfCast: false, CastConfirmType.Immediately);
			CheckSkillCast(HeroSkillLocation.Q, it_skillQOnRelease, isSelfCast: false, CastConfirmType.OnRelease);
			CheckSkillCast(HeroSkillLocation.W, it_skillWOnRelease, isSelfCast: false, CastConfirmType.OnRelease);
			CheckSkillCast(HeroSkillLocation.E, it_skillEOnRelease, isSelfCast: false, CastConfirmType.OnRelease);
			CheckSkillCast(HeroSkillLocation.R, it_skillROnRelease, isSelfCast: false, CastConfirmType.OnRelease);
			CheckSkillCast(HeroSkillLocation.Movement, it_skillMovementOnRelease, isSelfCast: false, CastConfirmType.OnRelease);
			CheckSkillCast(HeroSkillLocation.Q, it_skillQNormal, isSelfCast: false, CastConfirmType.Normal);
			CheckSkillCast(HeroSkillLocation.W, it_skillWNormal, isSelfCast: false, CastConfirmType.Normal);
			CheckSkillCast(HeroSkillLocation.E, it_skillENormal, isSelfCast: false, CastConfirmType.Normal);
			CheckSkillCast(HeroSkillLocation.R, it_skillRNormal, isSelfCast: false, CastConfirmType.Normal);
			CheckSkillCast(HeroSkillLocation.Movement, it_skillMovementNormal, isSelfCast: false, CastConfirmType.Normal);
		}
		if (state.type != ControlStateType.Cast || state.castType != CastConfirmType.OnRelease || !DewInput.GetButtonUp(state.castKey, checkGameAreaForMouse: false))
		{
			return;
		}
		if (!ShouldInvalidateCurrentCast())
		{
			if (state.isCastInDirectionOfMovement)
			{
				CastAbilityInDirectionOfMovement(state.trigger);
			}
			else
			{
				CastAbilityAtCursor(state.trigger);
			}
		}
		state = default;
		void CheckSkillCast(HeroSkillLocation type, DewInputTrigger it, bool isSelfCast, CastConfirmType? confirmType)
		{
			Hero hero = controllingEntity as Hero;
			if (!((UnityEngine.Object)(object)hero == null) && (!isMainSkillDisabled || !type.IsMainSkill()) && (!isDodgeDisabled || type != HeroSkillLocation.Movement) && it.down)
			{
				if (state.type == ControlStateType.Cast && state.castType == CastConfirmType.Normal)
				{
					state = default;
				}
				if (hero.Skill.TryGetSkill(type, out var skill))
				{
					if (!skill.currentConfig.isActive)
					{
						onCastFailed?.Invoke(skill);
					}
					else if (!skill.CanBeReserved())
					{
						onCastFailed?.Invoke(skill);
					}
					else
					{
						if (isSamplingCastInfo && !skill.currentConfig.ignoreBlock)
						{
							EntityControl.BlockableAction action = EntityControl.BlockableAction.Ability;
							if (skill.abilityIndex == 5)
							{
								action = EntityControl.BlockableAction.Dodge;
							}
							if (hero.Control.IsActionBlocked(action) != EntityControl.BlockStatus.Allowed)
							{
								onCastFailed?.Invoke(skill);
								return;
							}
						}
						ManagerBase<FloatingWindowManager>.instance.ClearTarget();
						InGameUIManager.instance.isWorldDisplayed = WorldDisplayStatus.None;
						if ((UnityEngine.Object)(object)ManagerBase<EditSkillManager>.instance.currentProvider != null)
						{
							ManagerBase<EditSkillManager>.instance.EndEdit();
						}
						if (isSelfCast)
						{
							bool flag;
							switch (skill.currentConfig.castMethod.type)
							{
							case CastMethodType.None:
							case CastMethodType.Cone:
							case CastMethodType.Arrow:
							case CastMethodType.Point:
								flag = true;
								break;
							case CastMethodType.Target:
								flag = skill.currentConfig.targetValidator.Evaluate(controllingEntity, controllingEntity);
								break;
							default:
								throw new ArgumentOutOfRangeException();
							}
							if (flag)
							{
								CastInfo info;
								switch (skill.currentConfig.castMethod.type)
								{
								case CastMethodType.None:
									info = new CastInfo(controllingEntity);
									break;
								case CastMethodType.Cone:
								case CastMethodType.Arrow:
									info = new CastInfo(controllingEntity, CastInfo.GetAngle(controllingEntity.rotation));
									break;
								case CastMethodType.Target:
									controllingEntity.Visual.highlight.ShowClick();
									info = new CastInfo(controllingEntity, controllingEntity);
									break;
								case CastMethodType.Point:
									info = new CastInfo(controllingEntity, controllingEntity.agentPosition);
									break;
								default:
									throw new ArgumentOutOfRangeException();
								}
								bool shouldMoveToCast = !_isDoingDirectionalMovement && !_isContinuousMove;
								CastAbility(skill, info, shouldMoveToCast);
								SetCastByKeyFlag(skill, value: true, it);
								return;
							}
						}
						bool flag2 = ShouldCastInDirectionOfMovement(skill);
						if (DewInput.currentMode == InputMode.Gamepad)
						{
							bool value = ((flag2 && (skill.currentConfig.ignoreAimDirectionGamepad || !aimDirection.HasValue)) ? CastAbilityInDirectionOfMovement(skill) : CastAbilityGamepad(skill));
							SetCastByKeyFlag(skill, value, it);
						}
						else
						{
							CastConfirmType castConfirmType = (confirmType.HasValue ? confirmType.Value : GetAbilityCastType(skill));
							if (castConfirmType == CastConfirmType.Immediately || skill.currentConfig.castMethod.type == CastMethodType.None || skill.currentConfig.alwaysCastImmediately)
							{
								bool value2 = (flag2 ? CastAbilityInDirectionOfMovement(skill) : CastAbilityAtCursor(skill));
								SetCastByKeyFlag(skill, value2, it);
							}
							else if (castConfirmType == CastConfirmType.Normal || castConfirmType == CastConfirmType.OnRelease)
							{
								state = new ControlState
								{
									type = ControlStateType.Cast,
									trigger = skill,
									configIndex = skill.currentConfigIndex,
									castKey = it._binding,
									castType = castConfirmType,
									isCastInDirectionOfMovement = flag2
								};
							}
						}
					}
				}
			}
		}
	}

	private bool GetShouldProcessCharacterInputAllowKnockedOut()
	{
		if (!NetworkClient.active || controllingEntity.IsNullOrInactive() || !InGameUIManager.instance.IsState("Playing") || !isCharacterControlEnabled || NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition || ManagerBase<MessageManager>.instance.isShowingMessage || InGameUIManager.instance.disablePlayingInput)
		{
			return false;
		}
		if (IsInputFieldFocused())
		{
			return false;
		}
		return true;
	}

	public bool ShouldProcessPingInput()
	{
		if (!NetworkClient.active || !InGameUIManager.instance.IsState("Playing") || !isCharacterControlEnabled || NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition)
		{
			return false;
		}
		if (IsInputFieldFocused())
		{
			return false;
		}
		return true;
	}

	public static bool IsInputFieldFocused()
	{
		if (_isInputFieldFocusedCachedFrame != Time.frameCount)
		{
			if ((UnityEngine.Object)(object)EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
			{
				GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
				_isInputFieldFocused = (currentSelectedGameObject.TryGetComponent<InputField>(out var component) && ((Behaviour)(object)component).isActiveAndEnabled && ((Selectable)component).interactable) || (currentSelectedGameObject.TryGetComponent<TMP_InputField>(out var component2) && ((Behaviour)(object)component2).isActiveAndEnabled && ((Selectable)component2).interactable);
			}
			else
			{
				_isInputFieldFocused = false;
			}
			_isInputFieldFocusedCachedFrame = Time.frameCount;
		}
		return _isInputFieldFocused;
	}

	public bool CastAbilityInDirectionOfMovement(AbilityTrigger trigger)
	{
		Vector3 vector = (isLastMovementDirectionFresh ? (lastMovementDirection * 1f) : ((Component)(object)controllingEntity).transform.forward.Flattened().normalized);
		CastInfo info;
		switch (trigger.currentConfig.castMethod.type)
		{
		case CastMethodType.None:
			info = new CastInfo(controllingEntity);
			break;
		case CastMethodType.Cone:
		case CastMethodType.Arrow:
			info = new CastInfo(controllingEntity, CastInfo.GetAngle(vector));
			break;
		case CastMethodType.Target:
			return false;
		case CastMethodType.Point:
		{
			Vector3 vector2 = vector * trigger.currentConfig.effectiveRange;
			Vector3 point = controllingEntity.agentPosition + vector2;
			info = new CastInfo(controllingEntity, point);
			break;
		}
		default:
			return false;
		}
		CastAbility(trigger, info, shouldMoveToCast: true);
		return true;
	}

	public bool CastAbilityAtCursor(AbilityTrigger trigger)
	{
		CastInfo info;
		switch (trigger.currentConfig.castMethod.type)
		{
		case CastMethodType.None:
			info = new CastInfo(controllingEntity);
			break;
		case CastMethodType.Cone:
		case CastMethodType.Arrow:
			info = new CastInfo(controllingEntity, CastInfo.GetAngle(GetWorldPositionOnGroundOnCursor(forDirectionalAttacks: true) - ((Component)(object)controllingEntity).transform.position));
			break;
		case CastMethodType.Target:
		{
			Entity entityOnCursor = GetEntityOnCursor(controllingEntity, trigger.currentConfig.targetValidator, GetAimAssistSphereCastRadius());
			if ((UnityEngine.Object)(object)entityOnCursor == null && DewSave.profileMain.controls.autoTargetSelfIfPossible && trigger.currentConfig.targetValidator.Evaluate(controllingEntity, controllingEntity))
			{
				entityOnCursor = controllingEntity;
			}
			if ((UnityEngine.Object)(object)entityOnCursor == null)
			{
				ShowNoTargetIndicatorAtCursor();
				return false;
			}
			entityOnCursor.Visual.highlight.ShowClick();
			info = new CastInfo(controllingEntity, entityOnCursor);
			break;
		}
		case CastMethodType.Point:
			info = new CastInfo(controllingEntity, GetWorldPositionOnGroundOnCursor());
			break;
		default:
			return false;
		}
		bool shouldMoveToCast = !_isDoingDirectionalMovement && !_isContinuousMove;
		CastAbility(trigger, info, shouldMoveToCast);
		return true;
	}

	public void CastAbility(AbilityTrigger trigger, CastInfo info, bool shouldMoveToCast)
	{
		if ((!isMainSkillDisabled || !(trigger is SkillTrigger skill) || !(controllingEntity is Hero hero) || !hero.Skill.TryGetSkillLocation(skill, out var type) || !type.IsMainSkill()) && (!isDodgeDisabled || !(trigger is SkillTrigger skill2) || !(controllingEntity is Hero hero2) || !hero2.Skill.TryGetSkillLocation(skill2, out var type2) || type2 != HeroSkillLocation.Movement))
		{
			controllingEntity.Control.CmdCast(trigger, trigger.currentConfigIndex, info, shouldMoveToCast, skipRangeCheck: false);
			if (!trigger.currentConfig.postponeBasicCommand)
			{
				controllingEntity.Control.CmdAttack(null, doChase: false);
			}
		}
	}

	private void DoAttackInPlaceAtWorldPosition(Vector3 pos)
	{
		AttackTrigger attackTrigger = controllingEntity.Ability.attackAbility as AttackTrigger;
		if (attackTrigger.IsNullOrInactive())
		{
			return;
		}
		if (attackTrigger.allowNonTargetedCast)
		{
			Entity entity = null;
			if (!DewSave.profileMain.controls.turnOffAimAssistMeleeDirectionalAttack || !(controllingEntity is Hero hero) || !Dew.IsMeleeHero(hero.classType))
			{
				List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, pos, GetAimAssistAttackTargetRange(), attackTrigger.currentConfig.targetValidator, controllingEntity);
				float num = float.NegativeInfinity;
				foreach (Entity item in list)
				{
					float num2 = 0f - Vector3.Distance(pos, item.agentPosition);
					if (attackTrigger.currentConfig.CheckRange(controllingEntity, item))
					{
						num2 += 1000f;
					}
					if (num < num2)
					{
						num = num2;
						entity = item;
					}
				}
				handle.Return();
				if ((UnityEngine.Object)(object)entity != null)
				{
					pos = entity.position;
				}
			}
			float angle = CastInfo.GetAngle(pos.Flattened() - controllingEntity.agentPosition.Flattened());
			Vector3 vector = pos - controllingEntity.agentPosition;
			Vector3 point = controllingEntity.agentPosition + Vector3.ClampMagnitude(vector, attackTrigger.currentConfig.effectiveRange);
			if ((UnityEngine.Object)(object)entity != null && attackTrigger.currentConfig.CheckRange(controllingEntity, entity))
			{
				controllingEntity.Control.CmdCast(attackTrigger, attackTrigger.currentConfigIndex, new CastInfo
				{
					caster = controllingEntity,
					target = entity
				}, allowMoveToCast: false, skipRangeCheck: true);
			}
			else
			{
				controllingEntity.Control.CmdCast(attackTrigger, attackTrigger.currentConfigIndex, new CastInfo
				{
					caster = controllingEntity,
					angle = angle,
					point = point
				}, allowMoveToCast: false, skipRangeCheck: false);
			}
			return;
		}
		List<Entity> list2 = DewPhysics.OverlapCircleAllEntities(out var handle2, pos, 6f, attackTrigger.currentConfig.targetValidator, controllingEntity);
		float num3 = float.NegativeInfinity;
		Entity entity2 = null;
		foreach (Entity item2 in list2)
		{
			float num4 = 0f - Vector3.Distance(pos, item2.agentPosition);
			if (attackTrigger.currentConfig.CheckRange(controllingEntity, item2))
			{
				num4 += 1000f;
			}
			if (num3 < num4)
			{
				num3 = num4;
				entity2 = item2;
			}
		}
		handle2.Return();
		if ((UnityEngine.Object)(object)entity2 == null)
		{
			if (Time.time - _lastNoTargetShowTime > 2.5f)
			{
				_lastNoTargetShowTime = Time.time;
				ShowNoTargetIndicatorAtCursor();
			}
		}
		else
		{
			controllingEntity.Control.CmdAttack(entity2, doChase: false);
		}
	}

	private void DoAttackInPlaceWithAim()
	{
		Vector3 vector = aimDirection.Value;
		AttackTrigger attackTrigger = controllingEntity.Ability.attackAbility as AttackTrigger;
		if (attackTrigger.IsNullOrInactive())
		{
			return;
		}
		float effectiveRange = attackTrigger.currentConfig.effectiveRange;
		if (attackTrigger.allowNonTargetedCast)
		{
			Entity entity = null;
			if (!DewSave.profileMain.controls.turnOffAimAssistMeleeDirectionalAttack || !(controllingEntity is Hero hero) || !Dew.IsMeleeHero(hero.classType))
			{
				List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, controllingEntity.agentPosition, effectiveRange + 5f, attackTrigger.currentConfig.targetValidator, controllingEntity);
				float num = float.NegativeInfinity;
				foreach (Entity item in list)
				{
					float num2 = Vector2.Angle(vector.ToXY(), (item.agentPosition - controllingEntity.agentPosition).ToXY());
					float num3 = 0f - num2;
					if (attackTrigger.currentConfig.CheckRange(controllingEntity, item))
					{
						num3 += 30f;
					}
					if (!(num2 > GetAimAssistAttackTargetAngleGamepad()) && num < num3)
					{
						num = num3;
						entity = item;
					}
				}
				handle.Return();
				if ((UnityEngine.Object)(object)entity != null)
				{
					vector = (entity.agentPosition - controllingEntity.agentPosition).Flattened().normalized;
				}
			}
			if ((UnityEngine.Object)(object)entity != null && attackTrigger.currentConfig.CheckRange(controllingEntity, entity))
			{
				controllingEntity.Control.CmdCast(attackTrigger, attackTrigger.currentConfigIndex, new CastInfo
				{
					caster = controllingEntity,
					target = entity
				}, allowMoveToCast: false, skipRangeCheck: true);
				return;
			}
			float angle = CastInfo.GetAngle(vector);
			Vector3 point = controllingEntity.agentPosition + vector * attackTrigger.currentConfig.effectiveRange;
			controllingEntity.Control.CmdCast(attackTrigger, attackTrigger.currentConfigIndex, new CastInfo
			{
				caster = controllingEntity,
				angle = angle,
				point = point
			}, allowMoveToCast: false, skipRangeCheck: false);
			return;
		}
		List<Entity> list2 = DewPhysics.OverlapCircleAllEntities(out var handle2, controllingEntity.agentPosition, effectiveRange + 5f, attackTrigger.currentConfig.targetValidator, controllingEntity);
		float num4 = float.NegativeInfinity;
		Entity entity2 = null;
		foreach (Entity item2 in list2)
		{
			float num5 = 0f - Vector2.Angle(vector.ToXY(), (item2.agentPosition - controllingEntity.agentPosition).ToXY());
			if (attackTrigger.currentConfig.CheckRange(controllingEntity, item2))
			{
				num5 += 1000f;
			}
			if (num4 < num5)
			{
				num4 = num5;
				entity2 = item2;
			}
		}
		handle2.Return();
		if ((UnityEngine.Object)(object)entity2 == null)
		{
			if (Time.time - _lastNoTargetShowTime > 2.5f)
			{
				_lastNoTargetShowTime = Time.time;
				ShowNoTargetIndicatorWithAim();
				ManagerBase<CastIndicatorManager>.instance.IndicateRangeFailure(effectiveRange);
			}
		}
		else
		{
			controllingEntity.Control.CmdAttack(entity2, doChase: false);
		}
	}

	private void DoAttackInPlaceAtCursor()
	{
		DoAttackInPlaceAtWorldPosition(GetWorldPositionOnGroundOnCursor(forDirectionalAttacks: true));
	}

	private void DoAttackInPlaceAuto()
	{
		AttackTrigger attackTrigger = controllingEntity.Ability.attackAbility as AttackTrigger;
		if (attackTrigger.IsNullOrInactive())
		{
			return;
		}
		if (attackTrigger.allowNonTargetedCast)
		{
			Vector3 vector = (isLastMovementDirectionFresh ? (lastMovementDirection * lastMovementStrength) : ((Component)(object)controllingEntity).transform.forward.Flattened().normalized);
			float angle = (((UnityEngine.Object)(object)targetEnemy != null) ? CastInfo.GetAngle(targetEnemy.agentPosition.Flattened() - controllingEntity.agentPosition.Flattened()) : CastInfo.GetAngle(vector));
			Vector3 point = ((!((UnityEngine.Object)(object)targetEnemy != null)) ? (controllingEntity.agentPosition + vector * attackTrigger.currentConfig.effectiveRange) : (controllingEntity.agentPosition + Vector3.ClampMagnitude(targetEnemy.agentPosition - controllingEntity.agentPosition, attackTrigger.currentConfig.effectiveRange)));
			if ((UnityEngine.Object)(object)targetEnemy != null && attackTrigger.currentConfig.CheckRange(controllingEntity, targetEnemy))
			{
				controllingEntity.Control.CmdCast(attackTrigger, attackTrigger.currentConfigIndex, new CastInfo
				{
					caster = controllingEntity,
					target = targetEnemy
				}, allowMoveToCast: false, skipRangeCheck: true);
			}
			else
			{
				controllingEntity.Control.CmdCast(attackTrigger, attackTrigger.currentConfigIndex, new CastInfo
				{
					caster = controllingEntity,
					angle = angle,
					point = point
				}, allowMoveToCast: false, skipRangeCheck: false);
			}
		}
		else if ((UnityEngine.Object)(object)targetEnemy == null || !attackTrigger.currentConfig.CheckRange(controllingEntity, targetEnemy))
		{
			if (Time.time - _lastNoTargetShowTime > 2.5f)
			{
				_lastNoTargetShowTime = Time.time;
				ShowNoTargetIndicatorWithAim();
				if (Time.time - _lastAttackRangeShowTime > 3f)
				{
					ManagerBase<CastIndicatorManager>.instance.IndicateRangeFailure(attackTrigger.currentConfig.effectiveRange);
					_lastAttackRangeShowTime = Time.time;
				}
			}
		}
		else
		{
			controllingEntity.Control.CmdAttack(targetEnemy, doChase: false);
		}
	}

	private void DoAttackMoveAtCursor(AttackMoveType type = AttackMoveType.UseUserSettings, bool hideIndicator = false)
	{
		if ((UnityEngine.Object)(object)controllingEntity.Ability.attackAbility != null)
		{
			Entity entityFromScreenPoint = GetEntityFromScreenPoint(GetMousePositionWithInversionInMind(), controllingEntity, controllingEntity.Ability.attackAbility.currentConfig.targetValidator, GetAimAssistSphereCastRadius());
			if ((UnityEngine.Object)(object)entityFromScreenPoint != null)
			{
				if (!_isContinuousMove)
				{
					controllingEntity.Control.CmdClearMovement();
				}
				controllingEntity.Control.CmdAttack(entityFromScreenPoint, !_isContinuousMove);
				if (!hideIndicator)
				{
					entityFromScreenPoint.Visual.highlight.ShowClick();
				}
				if (!hideIndicator)
				{
					CommandMarker.Spawn(attackMarker, entityFromScreenPoint.Visual.GetBasePosition(), Quaternion.identity).followTransform = ((Component)(object)entityFromScreenPoint).transform;
				}
				return;
			}
			Vector2 vector = ((type == AttackMoveType.UseDistanceFromDestination || (type == AttackMoveType.UseUserSettings && DewSave.profileMain.controls.attackMoveUseDistanceFromDestination)) ? GetWorldPositionOnGroundOnCursor().ToXY() : controllingEntity.position.ToXY());
			Entity entity = ActionAttackMove.FindAttackMoveTarget(controllingEntity, vector);
			if ((UnityEngine.Object)(object)entity != null)
			{
				if (!_isContinuousMove)
				{
					controllingEntity.Control.CmdClearMovement();
				}
				controllingEntity.Control.CmdAttack(entity, !_isContinuousMove);
				if (!hideIndicator)
				{
					CommandMarker.Spawn(attackMarker, GetWorldPositionOnGroundOnCursor(), Quaternion.identity);
				}
			}
			else
			{
				Vector3 worldPositionOnGroundOnCursor = GetWorldPositionOnGroundOnCursor();
				if (!hideIndicator)
				{
					CommandMarker.Spawn(attackMarker, worldPositionOnGroundOnCursor, Quaternion.identity);
				}
				if (!_isContinuousMove)
				{
					controllingEntity.Control.CmdAttackMove(worldPositionOnGroundOnCursor, DewSave.profileMain.controls.attackMoveUseDistanceFromDestination);
				}
			}
		}
		else
		{
			MoveToCursor();
		}
	}

	private void MoveToCursor()
	{
		isMovementSchemeDirectional = false;
		Vector3 worldPositionOnGroundOnCursor = GetWorldPositionOnGroundOnCursor();
		controllingEntity.Control.CmdMoveToDestination(worldPositionOnGroundOnCursor, immediately: true);
		CommandMarker.Spawn(moveMarker, worldPositionOnGroundOnCursor, Quaternion.identity);
	}

	private void ShowNoTargetIndicatorWithAim()
	{
		ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_Control_CastUnavailable");
		if (SpawnManager.Create(noTargetIndicator, Vector3.left * 1000f, Quaternion.identity, noTargetIndicatorParent).TryGetComponent<IWorldPositionReceiver>(out var component))
		{
			component.SetWorldPosition(aimPoint ?? controllingEntity.agentPosition);
		}
	}

	private void ShowNoTargetIndicatorAtCursor()
	{
		ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_Control_CastUnavailable");
		if (SpawnManager.Create(noTargetIndicator, GetMousePositionWithInversionInMind(), Quaternion.identity, noTargetIndicatorParent).TryGetComponent<IWorldPositionReceiver>(out var component))
		{
			component.SetWorldPosition(GetWorldPositionOnGroundOnCursor());
		}
	}

	public bool CastAbilityGamepad(AbilityTrigger trigger)
	{
		if (!aimDirection.HasValue)
		{
			return CastAbilityAuto(trigger);
		}
		return CastAbilityWithAim(trigger);
	}

	public bool CastAbilityWithAim(AbilityTrigger trigger)
	{
		CastInfo info;
		switch (trigger.currentConfig.castMethod.type)
		{
		case CastMethodType.None:
			info = new CastInfo(controllingEntity);
			break;
		case CastMethodType.Cone:
		case CastMethodType.Arrow:
			info = new CastInfo(controllingEntity, CastInfo.GetAngle(aimDirection.Value));
			break;
		case CastMethodType.Target:
		{
			Entity entity = targetEnemy;
			if (entity.IsNullInactiveDeadOrKnockedOut() || !trigger.currentConfig.targetValidator.Evaluate(controllingEntity, entity) || !trigger.currentConfig.CheckRange(controllingEntity, entity))
			{
				entity = null;
				List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, aimPoint.Value, 12f, (Entity e) => trigger.currentConfig.targetValidator.Evaluate(controllingEntity, e) && trigger.currentConfig.CheckRange(controllingEntity, e) && Vector3.Angle(aimDirection.Value, e.agentPosition - controllingEntity.agentPosition) < 80f, new CollisionCheckSettings
				{
					sortComparer = CollisionCheckSettings.DistanceFromCenter,
					includeUncollidable = true
				});
				if (list.Count > 0)
				{
					entity = list[0];
				}
				handle.Return();
			}
			if (entity.IsNullInactiveDeadOrKnockedOut())
			{
				ManagerBase<CastIndicatorManager>.instance.IndicateRangeFailure(trigger.currentConfig.effectiveRange);
				ShowNoTargetIndicatorWithAim();
				return false;
			}
			info = new CastInfo(controllingEntity, entity);
			break;
		}
		case CastMethodType.Point:
			info = new CastInfo(controllingEntity, aimPoint.Value);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		CastAbility(trigger, info, shouldMoveToCast: true);
		return true;
	}

	public bool CastAbilityAuto(AbilityTrigger trigger)
	{
		Entity entity = targetEnemy;
		if (entity.IsNullInactiveDeadOrKnockedOut() || !trigger.currentConfig.targetValidator.Evaluate(controllingEntity, entity) || (trigger.currentConfig.castMethod.type == CastMethodType.Target && !trigger.currentConfig.CheckRange(controllingEntity, entity)))
		{
			if (trigger.currentConfig.castMethod.type == CastMethodType.Target)
			{
				entity = null;
			}
			float effectiveRange = trigger.currentConfig.effectiveRange;
			List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, controllingEntity.agentPosition, effectiveRange, (Entity e) => trigger.currentConfig.targetValidator.Evaluate(controllingEntity, e), new CollisionCheckSettings
			{
				sortComparer = CollisionCheckSettings.DistanceFromCenter
			});
			if (list.Count > 0)
			{
				entity = list[0];
			}
			handle.Return();
		}
		CastInfo info;
		if (!entity.IsNullInactiveDeadOrKnockedOut())
		{
			info = trigger.GetPredictedCastInfoToTarget(entity, 0.5f);
		}
		else
		{
			Vector3 vector = (isLastMovementDirectionFresh ? (lastMovementDirection * lastMovementStrength) : ((Component)(object)controllingEntity).transform.forward.Flattened().normalized);
			switch (trigger.currentConfig.castMethod.type)
			{
			case CastMethodType.None:
				info = new CastInfo(controllingEntity);
				break;
			case CastMethodType.Cone:
			case CastMethodType.Arrow:
				info = new CastInfo(controllingEntity, CastInfo.GetAngle(vector));
				break;
			case CastMethodType.Target:
				ShowNoTargetIndicatorWithAim();
				ManagerBase<CastIndicatorManager>.instance.IndicateRangeFailure(trigger.currentConfig.effectiveRange);
				return false;
			case CastMethodType.Point:
				info = new CastInfo(controllingEntity, controllingEntity.agentPosition + vector * trigger.currentConfig.castMethod.pointData.range);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
		CastAbility(trigger, info, shouldMoveToCast: true);
		return true;
	}

	private void GetCharInputOfGamepadPingAndEmote()
	{
		switch (_gpState.current)
		{
		case GpState.Idle:
		{
			bool flag = DewSave.profileMain.controls.leftJoystickClickAction == JoystickClickAction.PingAndEmotes;
			bool flag2 = DewSave.profileMain.controls.rightJoystickClickAction == JoystickClickAction.PingAndEmotes;
			bool num = flag && DewInput.GetButtonDown((GamepadButtonEx?)GamepadButtonEx.LeftStick);
			bool flag3 = flag2 && DewInput.GetButtonDown((GamepadButtonEx?)GamepadButtonEx.RightStick);
			if (num || flag3)
			{
				if (ManagerBase<GlobalUIManager>.instance.focused != null)
				{
					CheckAndSendPing();
					break;
				}
				_gpState.isRightStick = flag3;
				_gpState.startTime = Time.unscaledTime;
				_gpState.current = GpState.Holding;
			}
			break;
		}
		case GpState.Holding:
			if (!DewInput.GetButton((GamepadButtonEx?)(_gpState.isRightStick ? GamepadButtonEx.RightStick : GamepadButtonEx.LeftStick)))
			{
				CheckAndSendPing();
				_gpState.Reset();
			}
			else if (Time.unscaledTime - _gpState.startTime > 0.15f && SingletonBehaviour<UI_EmoteWheel>.softInstance != null)
			{
				SingletonBehaviour<UI_EmoteWheel>.instance.Show(InputMode.Gamepad, _gpState.isRightStick);
			}
			break;
		}
	}

	public void CancelGamepadEmote()
	{
		_gpState.Reset();
	}

	public void CheckAndSendPing()
	{
		if (ShouldProcessPingInput())
		{
			SendPingGamepad();
		}
	}

	public bool ShouldCastInDirectionOfMovement(AbilityTrigger trigger)
	{
		if (DewInput.currentMode == InputMode.KeyboardAndMouse && isMovementSchemeDirectional && trigger.currentConfig.castByMoveDirectionByDefault)
		{
			switch (DewSave.profileMain.controls.dashDirectionWhenDirectionalMovement)
			{
			case DashDirection.CurrentMovement:
				return true;
			case DashDirection.CurrentMovementOnlyDodge:
				return ((object)trigger).GetType().Name.Contains("_M_");
			case DashDirection.AllTowardsCursor:
				return false;
			}
		}
		if (DewInput.currentMode == InputMode.Gamepad)
		{
			return trigger.currentConfig.castByMoveDirectionGamepad;
		}
		return false;
	}

	public DewBinding GetSkillBinding(HeroSkillLocation type)
	{
		return DewSave.profileMain.controls.GetSkillBinding(type);
	}

	public DewInputTrigger GetSkillInputTrigger(HeroSkillLocation type)
	{
		return type switch
		{
			HeroSkillLocation.Q => it_skillQ, 
			HeroSkillLocation.W => it_skillW, 
			HeroSkillLocation.E => it_skillE, 
			HeroSkillLocation.R => it_skillR, 
			HeroSkillLocation.Movement => it_skillMovement, 
			_ => DewInputTrigger.MockTrigger, 
		};
	}

	public static Entity GetEntityFromScreenPoint(Vector2 screenPoint, float sphereCastRadius = -1f)
	{
		return GetEntityFromScreenPoint(screenPoint, (Entity _) => true, sphereCastRadius);
	}

	public static Entity GetEntityFromScreenPoint(Vector2 screenPoint, Entity self, IBinaryEntityValidator validator, float sphereCastRadius = -1f)
	{
		return GetFromScreenPoint(screenPoint, LayerMasks.Entity, (self, validator), (Entity entity, (Entity self, IBinaryEntityValidator validator) s) => s.validator.Evaluate(s.self, entity) && !entity.Status.hasUntargetable && (!entity.Status.isUndetectableByNonAllies || !s.self.CheckEnemyOrNeutral(entity)), sphereCastRadius);
	}

	public static Entity GetEntityFromScreenPoint(Vector2 screenPoint, IEntityValidator validator, float sphereCastRadius = -1f)
	{
		return GetFromScreenPoint(screenPoint, LayerMasks.Entity, validator, (Entity entity, IEntityValidator v) => v.Evaluate(entity), sphereCastRadius);
	}

	public static T GetFromScreenPoint<T>(Vector2 screenPoint, int layerMask, Func<T, bool> validator = null, float sphereCastRadius = -1f)
	{
		return GetFromScreenPoint(screenPoint, layerMask, validator, (T t, Func<T, bool> v) => v?.Invoke(t) ?? true, sphereCastRadius);
	}

	public static T GetFromScreenPoint<T, TState>(Vector2 screenPoint, int layerMask, TState state, Func<T, TState, bool> validator, float sphereCastRadius = -1f)
	{
		Ray ray = Dew.mainCamera.ScreenPointToRay(screenPoint);
		int num = Physics.RaycastNonAlloc(ray, s_screenPointHits, 200f, layerMask, (QueryTriggerInteraction)2);
		Array.Sort(s_screenPointHits, 0, num, s_screenPointDistanceComparer);
		for (int i = 0; i < num; i++)
		{
			T componentInParent = ((Component)(object)s_screenPointHits[i].collider).GetComponentInParent<T>();
			if (componentInParent == null || componentInParent is Actor { isActive: false } || componentInParent is Hero { isKnockedOut: not false })
			{
				continue;
			}
			if (componentInParent is Component component)
			{
				Actor componentInParent2 = component.GetComponentInParent<Actor>();
				if (((UnityEngine.Object)(object)componentInParent2 != null && !componentInParent2.isActive) || componentInParent2 is Hero { isKnockedOut: not false })
				{
					continue;
				}
			}
			if (validator == null || validator(componentInParent, state))
			{
				return componentInParent;
			}
		}
		if (sphereCastRadius <= 0f)
		{
			return default;
		}
		num = Physics.SphereCastNonAlloc(ray, sphereCastRadius, s_screenPointHits, 200f, layerMask, (QueryTriggerInteraction)2);
		s_screenPointRayPerpComparer.ray = ray;
		Array.Sort(s_screenPointHits, 0, num, s_screenPointRayPerpComparer);
		for (int j = 0; j < num; j++)
		{
			T componentInParent3 = ((Component)(object)s_screenPointHits[j].collider).GetComponentInParent<T>();
			if (componentInParent3 == null || (componentInParent3 is Actor actor2 && ((UnityEngine.Object)(object)actor2 == null || !actor2.isActive)) || componentInParent3 is Hero { isKnockedOut: not false })
			{
				continue;
			}
			if (componentInParent3 is Component component2)
			{
				Actor componentInParent4 = component2.GetComponentInParent<Actor>();
				if (((UnityEngine.Object)(object)componentInParent4 != null && !componentInParent4.isActive) || componentInParent4 is Hero { isKnockedOut: not false })
				{
					continue;
				}
			}
			if (validator == null || validator(componentInParent3, state))
			{
				return componentInParent3;
			}
		}
		return default;
	}

	public static Entity GetEntityFromScreenPoint(Vector2 screenPoint, Func<Entity, bool> validator, float sphereCastRadius = -1f)
	{
		return GetFromScreenPoint(screenPoint, LayerMasks.Entity, validator, sphereCastRadius);
	}

	public static IInteractable GetInteractableFromScreenPoint(Vector2 screenPoint, float sphereCastRadius = -1f)
	{
		return GetFromScreenPoint(screenPoint, LayerMasks.Entity | LayerMasks.Interactable, (IInteractable i) => i.CanInteract(ManagerBase<ControlManager>.instance.controllingEntity), sphereCastRadius);
	}

	public static HighlightProvider GetHighlightableFromScreenPoint(Vector2 screenPoint)
	{
		return GetFromScreenPoint<HighlightProvider>(screenPoint, LayerMasks.Entity | LayerMasks.Interactable);
	}

	public static Entity GetEntityOnCursor(float sphereCastRadius = -1f)
	{
		return GetEntityFromScreenPoint(GetMousePositionWithInversionInMind(), sphereCastRadius);
	}

	public static IInteractable GetInteractableOnCursor()
	{
		return GetInteractableFromScreenPoint(GetMousePositionWithInversionInMind());
	}

	public static HighlightProvider GetHighlightableOnCursor()
	{
		return GetHighlightableFromScreenPoint(GetMousePositionWithInversionInMind());
	}

	public static Entity GetEntityOnCursor(Entity self, IBinaryEntityValidator validator, float sphereCastRadius = -1f)
	{
		return GetEntityFromScreenPoint(GetMousePositionWithInversionInMind(), self, validator, sphereCastRadius);
	}

	public static Entity GetEntityOnCursor(IEntityValidator validator, float sphereCastRadius = -1f)
	{
		return GetEntityFromScreenPoint(GetMousePositionWithInversionInMind(), validator, sphereCastRadius);
	}

	public static Entity GetEntityOnCursor(Func<Entity, bool> validator, float sphereCastRadius = -1f)
	{
		return GetEntityFromScreenPoint(GetMousePositionWithInversionInMind(), validator, sphereCastRadius);
	}

	private bool ShouldInvalidateCurrentCast()
	{
		if (!((UnityEngine.Object)(object)state.trigger == null) && !((UnityEngine.Object)(object)state.trigger.owner != (UnityEngine.Object)(object)controllingEntity))
		{
			return state.configIndex != state.trigger.currentConfigIndex;
		}
		return true;
	}

	private CastConfirmType GetAbilityCastType(SkillTrigger skill)
	{
		if (((object)skill).GetType().Name.Contains("_M_"))
		{
			return DewSave.profileMain.controls.movementHeroAbilityCastType;
		}
		return DewSave.profileMain.controls.defaultHeroAbilityCastType;
	}

	public static Vector3 GetWorldPositionOnGroundFromViewportPoint(Vector2 viewportPoint, bool forDirectionalAttacks)
	{
		Ray ray = Dew.mainCamera.ViewportPointToRay(viewportPoint);
		Vector3 vector = (forDirectionalAttacks ? new Vector3(0f, 0.75f, 0f) : Vector3.zero);
		if (ManagerBase<CameraManager>.softInstance != null && (UnityEngine.Object)(object)ManagerBase<CameraManager>.softInstance.focusedEntity != null && new Plane(Vector3.up, ManagerBase<CameraManager>.softInstance.focusedEntity.agentPosition + vector).Raycast(ray, out var enter))
		{
			return ray.origin + ray.direction * enter;
		}
		if (new Plane(Vector3.up, vector).Raycast(ray, out var enter2))
		{
			return ray.origin + ray.direction * enter2;
		}
		return ray.origin + ray.direction * 100f;
	}

	public static Vector3 GetWorldPositionOnGroundFromScreenPoint(Vector2 screenPoint, bool forDirectionalAttacks)
	{
		return GetWorldPositionOnGroundFromViewportPoint(new Vector2(screenPoint.x / (float)Screen.width, screenPoint.y / (float)Screen.height), forDirectionalAttacks);
	}

	public static bool AreControlsInverted()
	{
		if (ManagerBase<ControlManager>.softInstance != null && (UnityEngine.Object)(object)ManagerBase<ControlManager>.softInstance.controllingEntity != null)
		{
			return ManagerBase<ControlManager>.softInstance.controllingEntity.Control.isControlReversed;
		}
		return false;
	}

	public static Vector2 GetMousePositionWithInversionInMind()
	{
		Vector3 vector = Input.mousePosition;
		if (AreControlsInverted())
		{
			Vector3 vector2 = new Vector3((float)Screen.width * 0.5f, (float)Screen.height * 0.5f, 0f);
			Vector3 vector3 = vector - vector2;
			vector = vector2 - vector3;
		}
		return vector;
	}

	public static Vector3 GetWorldPositionOnGroundOnCursor(bool forDirectionalAttacks = false)
	{
		if (Time.frameCount != _cachedFrame)
		{
			_cachedPosition = GetWorldPositionOnGroundFromScreenPoint(GetMousePositionWithInversionInMind(), forDirectionalAttacks);
		}
		return _cachedPosition;
	}

	public static float GetAimAssistSphereCastRadius()
	{
		return DewSave.profileMain.controls.targetAssist switch
		{
			AimAssistType.None => 0f, 
			AimAssistType.Medium => 0.75f, 
			AimAssistType.High => 1.5f, 
			AimAssistType.VeryHigh => 2.5f, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public static float GetAimAssistAttackTargetRange()
	{
		return DewSave.profileMain.controls.targetAssist switch
		{
			AimAssistType.None => 0f, 
			AimAssistType.Medium => 2.5f, 
			AimAssistType.High => 3.5f, 
			AimAssistType.VeryHigh => 5f, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public static float GetAimAssistAttackTargetAngleGamepad()
	{
		return DewSave.profileMain.controls.attackTargetAssistGamepad switch
		{
			AimAssistType.None => 0f, 
			AimAssistType.Medium => 30f, 
			AimAssistType.High => 45f, 
			AimAssistType.VeryHigh => 90f, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private void SetFocusedInteractable(IInteractable i, bool isAtCursor = false)
	{
		if (i == focusedInteractable && (i == null || isAtCursor == isFocusedInteractableAtCursor))
		{
			return;
		}
		focusedInteractable = i;
		isFocusedInteractableAtCursor = isAtCursor;
		try
		{
			onFocusedInteractableChanged?.Invoke(i);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private IInteractable GetNearbyInteractable()
	{
		int num = Physics.OverlapSphereNonAlloc(controllingEntity.position, 6f, _interactCheckColliders, LayerMasks.Interactable | LayerMasks.Entity);
		IInteractable result = null;
		float num2 = float.PositiveInfinity;
		int num3 = int.MaxValue;
		for (int i = 0; i < num; i++)
		{
			IInteractable orAdd = _interactableCache.GetOrAdd(_interactCheckColliders[i]);
			if (orAdd != null && !orAdd.IsUnityNull() && !(orAdd is Actor { isActive: false }) && orAdd.CanInteract(controllingEntity) && num3 >= orAdd.priority)
			{
				float num4 = Vector2.Distance(DewPlayer.local.hero.position.ToXY(), ((Component)orAdd).transform.position.ToXY());
				if (!(num4 > orAdd.focusDistance) && (num3 != orAdd.priority || !(num4 > num2)))
				{
					num2 = num4;
					num3 = orAdd.priority;
					result = orAdd;
				}
			}
		}
		return result;
	}

	private void GetCharInputOfInteractByButtonPress()
	{
		if (focusedInteractable == null || focusedInteractable.IsUnityNull() || !focusedInteractable.CanInteract(controllingEntity))
		{
			return;
		}
		bool down = it_interactAlt.down;
		bool flag = Time.unscaledTime - _lastInteractAltUnscaledTime > 0.35f && (bool)it_interactAlt;
		if (it_interact.down | down | flag)
		{
			if (down | flag)
			{
				_lastInteractAltUnscaledTime = Time.unscaledTime;
			}
			controllingEntity.Control.CmdInteract(focusedInteractable, down | flag, isMouse: false);
			Component component = (Component)focusedInteractable;
			CommandMarker.Spawn(interactMarker, component.transform.position, Quaternion.identity).followTransform = component.transform;
			if (state.type != ControlStateType.None)
			{
				state = default;
			}
			HighlightProvider componentInChildren = component.GetComponentInChildren<HighlightProvider>();
			if (componentInChildren != null)
			{
				componentInChildren.ShowClick();
			}
		}
	}

	internal void UpdateInteractableFocus(bool alsoCheckNearby)
	{
		if (ManagerBase<FloatingWindowManager>.instance.currentTarget != null || NetworkedManagerBase<ConversationManager>.instance.hasOngoingLocalConversation || InGameUIManager.instance.isWorldDisplayed != WorldDisplayStatus.None)
		{
			SetFocusedInteractable(null);
			return;
		}
		switch (ManagerBase<EditSkillManager>.instance.mode)
		{
		case EditSkillManager.ModeType.Sell:
		case EditSkillManager.ModeType.EditSkillShrine:
			SetFocusedInteractable(null);
			break;
		default:
			SetFocusedInteractable(null);
			break;
		case EditSkillManager.ModeType.None:
			if (DewInput.currentMode == InputMode.KeyboardAndMouse)
			{
				IInteractable interactableOnCursor = GetInteractableOnCursor();
				if (interactableOnCursor != null)
				{
					SetFocusedInteractable(interactableOnCursor, isAtCursor: true);
					break;
				}
			}
			if (alsoCheckNearby)
			{
				SetFocusedInteractable(GetNearbyInteractable());
			}
			break;
		}
	}

	public void SetCastByKeyFlag(AbilityTrigger trigger, bool value, DewInputTrigger it)
	{
		if (trigger is SkillTrigger skill && DewPlayer.local.hero.Skill.TryGetSkillLocation(skill, out var type))
		{
			if (!value)
			{
				_castByKeyInfo.Remove(type);
			}
			else
			{
				_castByKeyInfo[type] = (it, Time.unscaledTime);
			}
		}
	}

	private bool ProcessCastInfoSampling()
	{
		if ((UnityEngine.Object)(object)DewPlayer.local == null || !localSampleContext.HasValue)
		{
			localSampleContext = null;
			return false;
		}
		SampleCastInfoContext value = localSampleContext.Value;
		CastMethodData castMethod = value.castMethod;
		CastInfo castInfo = GetCastInfo(castMethod, value.targetValidator, value.currentInfo);
		if (value.angleSpeedLimit > 0.0001f)
		{
			castInfo.angle = Mathf.MoveTowardsAngle(value.currentInfo.angle, castInfo.angle, value.angleSpeedLimit * Time.deltaTime);
		}
		value.currentInfo = castInfo;
		localSampleContext = value;
		if (canSendSampleUpdate)
		{
			DewPlayer.local.DispatchSample_Update(castInfo);
			_lastSampleUpdateTime = Time.unscaledTime;
		}
		if (DewInput.GetButtonDown(DewSave.profileMain.controls.attackInPlace, checkGameAreaForMouse: false) || DewInput.GetButtonDown(MouseButton.Left, checkGameArea: false))
		{
			DewPlayer.local.DispatchSample_Cast(castInfo);
		}
		else if (value.castOnButton != SampleCastInfoContext.CastOnButtonType.None && value.castKey != null)
		{
			if (value.castOnButton == SampleCastInfoContext.CastOnButtonType.ByButton)
			{
				if (value.trigger is SkillTrigger skill && DewPlayer.local.hero.Skill.TryGetSkillLocation(skill, out var type))
				{
					if (_castByKeyInfo.ContainsKey(type) && Time.unscaledTime - _castByKeyInfo[type].Item2 < 1f)
					{
						value.castOnButton = SampleCastInfoContext.CastOnButtonType.ByButtonRelease;
						_castByKeyInfo.Remove(type);
					}
					else
					{
						value.castOnButton = SampleCastInfoContext.CastOnButtonType.ByButtonPress;
					}
				}
				else
				{
					Debug.LogWarning($"Cast on release sampling started from unknown trigger: {value.trigger}");
					value.castOnButton = SampleCastInfoContext.CastOnButtonType.ByButtonPress;
				}
				localSampleContext = value;
			}
			if (value.castOnButton == SampleCastInfoContext.CastOnButtonType.ByButtonPress && value.castKey.down)
			{
				DewPlayer.local.DispatchSample_Cast(castInfo);
			}
			if (value.castOnButton == SampleCastInfoContext.CastOnButtonType.ByButtonRelease && !value.castKey)
			{
				DewPlayer.local.DispatchSample_Cast(castInfo);
			}
		}
		return true;
	}

	public CastInfo GetCastInfo(CastMethodData method, AbilityTargetValidator targetValidator, CastInfo prev = default(CastInfo))
	{
		CastInfo result;
		switch (method.type)
		{
		case CastMethodType.None:
			result = new CastInfo(controllingEntity);
			break;
		case CastMethodType.Cone:
		case CastMethodType.Arrow:
			if (DewInput.currentMode != InputMode.KeyboardAndMouse)
			{
				if (aimDirection.HasValue)
				{
					result = new CastInfo(controllingEntity, CastInfo.GetAngle(aimDirection.Value));
				}
				else
				{
					result = ((!(Time.unscaledTime - _lastMovementDirectionUnscaledTime < 0.05f)) ? new CastInfo(controllingEntity, prev.angle) : new CastInfo(controllingEntity, CastInfo.GetAngle(lastMovementDirection)));
				}
			}
			else
			{
				result = new CastInfo(controllingEntity, CastInfo.GetAngle(GetWorldPositionOnGroundOnCursor(forDirectionalAttacks: true) - ((Component)(object)controllingEntity).transform.position));
			}
			break;
		case CastMethodType.Target:
		{
			Entity entityOnCursor = GetEntityOnCursor(controllingEntity, targetValidator);
			if ((UnityEngine.Object)(object)entityOnCursor == null)
			{
				return default;
			}
			result = new CastInfo(controllingEntity, entityOnCursor);
			break;
		}
		case CastMethodType.Point:
			if (DewInput.currentMode == InputMode.KeyboardAndMouse)
			{
				result = new CastInfo(controllingEntity, GetWorldPositionOnGroundOnCursor());
				break;
			}
			if (aimPoint.HasValue)
			{
				result = new CastInfo(controllingEntity, aimPoint.Value);
				break;
			}
			return prev;
		default:
			return default;
		}
		return result;
	}

	private void OnDestroy()
	{
		ManagerBase<InputManager>.instance?.RemoveTriggersByOwner(this);
	}

	private void InitializeTriggers()
	{
		it_confirmCast = new DewInputTrigger
		{
			owner = this,
			binding = () => DewSave.profileMain.controls.confirmCast,
			isValidCheck = () => shouldProcessCharacterInput && state.type != ControlStateType.None,
			checkGameAreaForMouse = false,
			priority = -5
		};
		it_move = CharacterControl(() => DewSave.profileMain.controls.move, checkGameAreaForMouse: true);
		it_attackMoveNormal = CharacterControl(() => DewSave.profileMain.controls.attackMoveNormal, checkGameAreaForMouse: true);
		it_attackMoveImmediately = CharacterControl(() => DewSave.profileMain.controls.attackMoveImmediately, checkGameAreaForMouse: true);
		it_attackMoveOnRelease = CharacterControl(() => DewSave.profileMain.controls.attackMoveOnRelease, checkGameAreaForMouse: true);
		it_attackInPlace = new DewInputTrigger
		{
			owner = this,
			binding = () => DewSave.profileMain.controls.attackInPlace,
			isValidCheck = () => shouldProcessCharacterInput && ManagerBase<EditSkillManager>.instance.mode == EditSkillManager.ModeType.None,
			checkGameAreaForMouse = true,
			priority = -1
		};
		it_scoreboard = CharacterControl(() => DewSave.profileMain.controls.scoreboard, checkGameAreaForMouse: false, 1, allowKnockOut: true);
		it_worldMap = CharacterControl(() => DewSave.profileMain.controls.worldMap, checkGameAreaForMouse: false, 1, allowKnockOut: true);
		it_stop = CharacterControl(() => DewSave.profileMain.controls.stop, checkGameAreaForMouse: true);
		it_moveUp = DirectionalMovementControl(() => DewSave.profileMain.controls.moveUp);
		it_moveLeft = DirectionalMovementControl(() => DewSave.profileMain.controls.moveLeft);
		it_moveDown = DirectionalMovementControl(() => DewSave.profileMain.controls.moveDown);
		it_moveRight = DirectionalMovementControl(() => DewSave.profileMain.controls.moveRight);
		it_cancelNormalCast = new DewInputTrigger
		{
			owner = this,
			binding = () => DewBinding.KeyboardAndMouseOnly(MouseButton.Right),
			isValidCheck = () => shouldProcessCharacterInput && DewInput.currentMode == InputMode.KeyboardAndMouse && state.type == ControlStateType.Cast && state.castType == CastConfirmType.Normal,
			checkGameAreaForMouse = false,
			priority = -1
		};
		it_skillQ = CharacterControl(() => DewSave.profileMain.controls.skillQ, checkGameAreaForMouse: true);
		it_skillW = CharacterControl(() => DewSave.profileMain.controls.skillW, checkGameAreaForMouse: true);
		it_skillE = CharacterControl(() => DewSave.profileMain.controls.skillE, checkGameAreaForMouse: true);
		it_skillR = CharacterControl(() => DewSave.profileMain.controls.skillR, checkGameAreaForMouse: true);
		it_skillMovement = CharacterControl(() =>
		{
			DewBinding dewBinding = DewSave.profileMain.controls.skillMovement;
			if (DewSave.profileMain.controls.leftJoystickClickAction == JoystickClickAction.Dodge)
			{
				dewBinding = dewBinding.CloneWith(GamepadButtonEx.LeftStick);
			}
			if (DewSave.profileMain.controls.rightJoystickClickAction == JoystickClickAction.Dodge)
			{
				dewBinding = dewBinding.CloneWith(GamepadButtonEx.RightStick);
			}
			return dewBinding;
		}, checkGameAreaForMouse: true);
		it_skillQNormal = CharacterControl(() => DewSave.profileMain.controls.skillQNormal, checkGameAreaForMouse: true);
		it_skillWNormal = CharacterControl(() => DewSave.profileMain.controls.skillWNormal, checkGameAreaForMouse: true);
		it_skillENormal = CharacterControl(() => DewSave.profileMain.controls.skillENormal, checkGameAreaForMouse: true);
		it_skillRNormal = CharacterControl(() => DewSave.profileMain.controls.skillRNormal, checkGameAreaForMouse: true);
		it_skillMovementNormal = CharacterControl(() => DewSave.profileMain.controls.skillMovementNormal, checkGameAreaForMouse: true);
		it_skillQImmediately = CharacterControl(() => DewSave.profileMain.controls.skillQImmediately, checkGameAreaForMouse: true);
		it_skillWImmediately = CharacterControl(() => DewSave.profileMain.controls.skillWImmediately, checkGameAreaForMouse: true);
		it_skillEImmediately = CharacterControl(() => DewSave.profileMain.controls.skillEImmediately, checkGameAreaForMouse: true);
		it_skillRImmediately = CharacterControl(() => DewSave.profileMain.controls.skillRImmediately, checkGameAreaForMouse: true);
		it_skillMovementImmediately = CharacterControl(() => DewSave.profileMain.controls.skillMovementImmediately, checkGameAreaForMouse: true);
		it_skillQOnRelease = CharacterControl(() => DewSave.profileMain.controls.skillQOnRelease, checkGameAreaForMouse: true);
		it_skillWOnRelease = CharacterControl(() => DewSave.profileMain.controls.skillWOnRelease, checkGameAreaForMouse: true);
		it_skillEOnRelease = CharacterControl(() => DewSave.profileMain.controls.skillEOnRelease, checkGameAreaForMouse: true);
		it_skillROnRelease = CharacterControl(() => DewSave.profileMain.controls.skillROnRelease, checkGameAreaForMouse: true);
		it_skillMovementOnRelease = CharacterControl(() => DewSave.profileMain.controls.skillMovementOnRelease, checkGameAreaForMouse: true);
		it_skillQSelf = CharacterControl(() => DewSave.profileMain.controls.skillQSelf, checkGameAreaForMouse: true);
		it_skillWSelf = CharacterControl(() => DewSave.profileMain.controls.skillWSelf, checkGameAreaForMouse: true);
		it_skillESelf = CharacterControl(() => DewSave.profileMain.controls.skillESelf, checkGameAreaForMouse: true);
		it_skillRSelf = CharacterControl(() => DewSave.profileMain.controls.skillRSelf, checkGameAreaForMouse: true);
		it_skillQEdit = EditSkillControl(() => DewSave.profileMain.controls.skillQEdit);
		it_skillWEdit = EditSkillControl(() => DewSave.profileMain.controls.skillWEdit);
		it_skillEEdit = EditSkillControl(() => DewSave.profileMain.controls.skillEEdit);
		it_skillREdit = EditSkillControl(() => DewSave.profileMain.controls.skillREdit);
		it_skillIdentityEdit = EditSkillControl(() => DewSave.profileMain.controls.skillIdentityEdit);
		it_interact = CharacterControl(() => DewSave.profileMain.controls.interact, checkGameAreaForMouse: true, -3, allowKnockOut: false, invalidIfHasFocus: false, () => focusedInteractable != null);
		it_interactAlt = CharacterControl(() => DewSave.profileMain.controls.interactAlt, checkGameAreaForMouse: true, -3, allowKnockOut: false, invalidIfHasFocus: true, () => focusedInteractable != null);
		it_editSkillHold = CharacterControl(() => DewSave.profileMain.controls.editSkillHold);
		it_editSkillToggle = new DewInputTrigger
		{
			owner = this,
			binding = () => DewSave.profileMain.controls.editSkillToggle,
			isValidCheck = () => ManagerBase<GlobalUIManager>.instance.focused == null && shouldProcessCharacterInput,
			checkGameAreaForMouse = false,
			priority = -1
		};
		it_showDetails = new DewInputTrigger
		{
			owner = this,
			binding = () => DewSave.profileMain.controls.showDetails,
			isValidCheck = () => shouldProcessCharacterInput,
			canConsume = false,
			checkGameAreaForMouse = false,
			priority = 0
		};
		it_zoomOutCamera = CharacterControl(() => DewSave.profileMain.controls.zoomOut, checkGameAreaForMouse: true, 1, allowKnockOut: true);
		it_zoomInCamera = CharacterControl(() => DewSave.profileMain.controls.zoomIn, checkGameAreaForMouse: true, 1, allowKnockOut: true);
		it_ping = CharacterControl(() => DewSave.profileMain.controls.ping, checkGameAreaForMouse: false, 0, allowKnockOut: true);
		it_travelVote = new DewInputTrigger
		{
			owner = this,
			binding = () => DewSave.profileMain.controls.travelVote,
			isValidCheck = () => shouldProcessCharacterInput && NetworkedManagerBase<ZoneManager>.instance.isVoting,
			checkGameAreaForMouse = false,
			priority = -10
		};
		it_travelVoteCancel = new DewInputTrigger
		{
			owner = this,
			binding = () => DewSave.profileMain.controls.travelVoteCancel,
			isValidCheck = () => shouldProcessCharacterInput && NetworkedManagerBase<ZoneManager>.instance.isVoting,
			checkGameAreaForMouse = false,
			priority = -10
		};
		it_spectatorNextTarget = new DewInputTrigger
		{
			owner = this,
			binding = () => DewSave.profileMain.controls.spectatorNextTarget,
			isValidCheck = () => ManagerBase<CameraManager>.instance.isSpectating,
			checkGameAreaForMouse = true,
			priority = -5
		};
		DewInputTrigger CharacterControl(Func<DewBinding> binding, bool checkGameAreaForMouse = false, int priority = 0, bool allowKnockOut = false, bool invalidIfHasFocus = false, Func<bool> customValidCheck = null)
		{
			return new DewInputTrigger
			{
				owner = this,
				binding = binding,
				isValidCheck = () => (allowKnockOut ? shouldProcessCharacterInputAllowKnockedOut : shouldProcessCharacterInput) && (!invalidIfHasFocus || ManagerBase<GlobalUIManager>.instance.focused == null) && (customValidCheck == null || customValidCheck()),
				checkGameAreaForMouse = checkGameAreaForMouse,
				priority = priority
			};
		}
		DewInputTrigger DirectionalMovementControl(Func<DewBinding> binding)
		{
			return new DewInputTrigger
			{
				owner = this,
				binding = binding,
				isValidCheck = () => DewSave.profileMain.controls.enableDirMoveKeys && shouldProcessCharacterInput
			};
		}
		DewInputTrigger EditSkillControl(Func<DewBinding> binding)
		{
			return new DewInputTrigger
			{
				owner = this,
				binding = binding,
				isValidCheck = () =>
				{
					if (shouldProcessCharacterInput)
					{
						EditSkillManager.ModeType mode = ManagerBase<EditSkillManager>.instance.mode;
						return mode == EditSkillManager.ModeType.EquipGem || mode == EditSkillManager.ModeType.EquipSkill;
					}
					return false;
				}
			};
		}
	}
}
