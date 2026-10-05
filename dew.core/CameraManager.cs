using System;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraManager : ManagerBase<CameraManager>
{
	public Action<bool> onIsSpectatingChanged;

	public Action<Entity, Entity> onFocusedEntityChanged;

	public float followSmoothTime = 0.05f;

	public float startSpectatingTime = 3f;

	public float nextSpectateTargetTime = 2f;

	public Vector3 farZoomBody;

	public Vector3 midZoombody;

	public Vector3 closeZoomBody;

	public int zoomSteps;

	public int defaultZoomLevel = 1;

	public float zoomSmoothTime = 0.2f;

	public GameObject bigDamageShake;

	public GameObject smallDamageShake;

	public float smallDamageThreshold = 0.01f;

	public float smallDamageAlpha = 0.2f;

	public float bigDamageThreshold = 0.2f;

	public float bigDamageAlpha = 1f;

	public float minAlphaHealthThreshold = 0.5f;

	public float maxAlphaHealthThreshold = 0.25f;

	public float maxAlpha = 0.6f;

	public float damageOverlayDecaySpeed = 0.5f;

	public CanvasGroup damageOverlay;

	public float occlusionTestRadius;

	public CinemachineVirtualCamera selectedEntityCamera;

	public CanvasGroup cutsceneTransitionFade;

	public float cutsceneFadeTime;

	public float genericFadeTime = 0.5f;

	public Volume fadeOutVolume;

	public GameObject cutsceneSkipButtonObject;

	public RectTransform cutsceneTopLetterBox;

	public RectTransform cutsceneBottomLetterBox;

	[NonSerialized]
	public bool disableSeeThrough;

	private float _entityCamAngle;

	[NonSerialized]
	public Quaternion entityCamAngleRotation = Quaternion.identity;

	private float _startSpectateCurrentElapsedTime;

	private float _nextSpectateTargetElapsedTime;

	private float _damageOverlayMinAlpha;

	private Vector3 _followCv;

	private Vector3 _zoomCv;

	private Vector4[] _characterPositionsBuffer;

	private Transform _followTransform;

	private CinemachineTransposer _body;

	private Vector2 _topLetterBoxSizeDelta;

	private Vector2 _bottomLetterBoxSizeDelta;

	private List<CameraModifierBase> _cameraModifiers = new List<CameraModifierBase>();

	private static readonly int CharacterPositions = Shader.PropertyToID("_CharacterPositions");

	private static readonly int CharacterPositionsCount = Shader.PropertyToID("_CharacterPositionsCount");

	private static readonly int MainCharacterPosition = Shader.PropertyToID("_MainCharacterPosition");

	private static readonly int DistanceFromCameraMultiplier = Shader.PropertyToID("_DistanceFromCameraMultiplier");

	private readonly List<object> _currentObjects = new List<object>();

	private readonly List<Vector4> _currentPositions = new List<Vector4>();

	private readonly ComponentCache<Collider, IInteractable> _interactableCache = new ComponentCache<Collider, IInteractable>(128, ComponentCache<Collider, IInteractable>.ComponentSource.Parent);

	private int _occlusionQueryCounter;

	private readonly List<(object, float)> _preferredObjects = new List<(object, float)>();

	public bool isSpectating { get; private set; }

	public float entityCamAngle
	{
		get
		{
			return _entityCamAngle;
		}
		set
		{
			_entityCamAngle = value;
			entityCamAngleRotation = Quaternion.Euler(0f, _entityCamAngle, 0f);
			ShaderManager.UpdateRoomRotation();
			SnapCameraToFocusedEntity();
		}
	}

	public int currentZoomIndex { get; private set; }

	public Entity focusedEntity { get; private set; }

	public bool isPlayingCutscene { get; internal set; }

	public DewCutsceneDirector currentCutsceneDirector { get; internal set; }

	protected override void Awake()
	{
		base.Awake();
		_topLetterBoxSizeDelta = cutsceneTopLetterBox.sizeDelta;
		_bottomLetterBoxSizeDelta = cutsceneBottomLetterBox.sizeDelta;
		damageOverlay.alpha = 0f;
		_followTransform = new GameObject("Follow Transform").transform;
		_followTransform.parent = transform;
		((CinemachineVirtualCameraBase)selectedEntityCamera).Follow = _followTransform;
		((CinemachineVirtualCameraBase)selectedEntityCamera).LookAt = _followTransform;
		_body = selectedEntityCamera.GetCinemachineComponent<CinemachineTransposer>();
		SetZoomLevel(defaultZoomLevel);
	}

	private void Start()
	{
		DewNetworkManager.instance.ClientEvent_OnLoadingStatusChanged += new Action<bool>(OnLoadingStatusChanged);
		NetworkedManagerBase<ClientEventManager>.instance.OnTakeDamage += new Action<EventInfoDamage>(ClientEntityEventOnTakeDamage);
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += (Action<EventInfoLoadRoom>)((EventInfoLoadRoom _) =>
		{
			SnapCameraToFocusedEntity();
		});
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += (Action<EventInfoLoadRoom>)((EventInfoLoadRoom _) =>
		{
			LogicUpdateSpectation(0f);
		});
	}

	private void OnLoadingStatusChanged(bool obj)
	{
		if (!obj)
		{
			SnapCameraToFocusedEntity();
		}
	}

	private void OnDestroy()
	{
		if ((UnityEngine.Object)(object)DewNetworkManager.instance != null)
		{
			DewNetworkManager.instance.ClientEvent_OnLoadingStatusChanged -= new Action<bool>(OnLoadingStatusChanged);
		}
		Shader.SetGlobalInt(CharacterPositionsCount, 0);
		Shader.SetGlobalFloat(DistanceFromCameraMultiplier, 1f);
	}

	private void ClientEntityEventOnTakeDamage(EventInfoDamage obj)
	{
		if ((UnityEngine.Object)(object)obj.victim != (UnityEngine.Object)(object)focusedEntity)
		{
			return;
		}
		float num = (obj.damage.amount + obj.damage.discardedAmount) / obj.victim.maxHealth;
		if (!obj.damage.HasAttr(DamageAttribute.IgnoreShield))
		{
			num -= obj.negatedAmountByShield;
		}
		if ((bool)(UnityEngine.Object)(object)obj.actor && obj.actor.IsDescendantOf(focusedEntity))
		{
			num *= 0.1f;
		}
		if (num > bigDamageThreshold)
		{
			if (damageOverlay.alpha < _damageOverlayMinAlpha + bigDamageAlpha)
			{
				damageOverlay.alpha = _damageOverlayMinAlpha + bigDamageAlpha;
			}
			DewEffect.Play(bigDamageShake);
		}
		else if (num > smallDamageThreshold)
		{
			if (damageOverlay.alpha < _damageOverlayMinAlpha + smallDamageAlpha)
			{
				damageOverlay.alpha = _damageOverlayMinAlpha + smallDamageAlpha;
			}
			DewEffect.Play(smallDamageShake);
		}
		if (focusedEntity is Hero { disableDamageOverlay: not false })
		{
			damageOverlay.alpha = 0f;
		}
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		UpdateInEngineSpectatingFlag();
		if (!((UnityEngine.Object)(object)DewPlayer.local == null))
		{
			LogicUpdateOcclusionTest(dt);
			LogicUpdateSpectation(dt);
		}
	}

	private void UpdateInEngineSpectatingFlag()
	{
		_ = isSpectating;
		TransitionManager transitionManager = ManagerBase<TransitionManager>.softInstance;
		if (transitionManager != null)
		{
			_ = transitionManager.state;
			_ = 1;
		}
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		FrameUpdateDamageOverlay();
		FrameUpdateFollow();
		FrameUpdateZoom();
	}

	private void FrameUpdateZoom()
	{
		if (!InGameUIManager.instance.IsState("Playing"))
		{
			return;
		}
		if (!NetworkedManagerBase<ConsoleManager>.instance.isConsoleWindowOpen)
		{
			if ((bool)ManagerBase<ControlManager>.instance.it_zoomInCamera)
			{
				SetZoomLevel(currentZoomIndex + 1);
			}
			if ((bool)ManagerBase<ControlManager>.instance.it_zoomOutCamera)
			{
				SetZoomLevel(currentZoomIndex - 1);
			}
		}
		_body.m_FollowOffset = Vector3.SmoothDamp(_body.m_FollowOffset, GetFollowOffsetTarget(), ref _zoomCv, zoomSmoothTime);
	}

	private Vector3 GetFollowOffsetTarget()
	{
		float num = currentZoomIndex;
		if (!isSpectating)
		{
			foreach (CameraModifierBase cameraModifier in _cameraModifiers)
			{
				if (cameraModifier is CameraModifierZoom cameraModifierZoom)
				{
					num = cameraModifierZoom.zoomIndex;
				}
			}
		}
		float num2 = num / (float)(zoomSteps - 1);
		Vector3 vector = ((num2 < 0.5f) ? Vector3.LerpUnclamped(farZoomBody, midZoombody, num2 * 2f) : Vector3.LerpUnclamped(midZoombody, closeZoomBody, num2 * 2f - 1f));
		return Quaternion.Euler(0f, entityCamAngle, 0f) * vector;
	}

	public void ZoomIn()
	{
		SetZoomLevel(currentZoomIndex + 1);
	}

	public void ZoomOut()
	{
		SetZoomLevel(currentZoomIndex - 1);
	}

	public void SetZoomLevel(int level)
	{
		currentZoomIndex = Mathf.Clamp(level, 0, zoomSteps - 1);
	}

	public void SnapCameraToFocusedEntity()
	{
		if (!((UnityEngine.Object)(object)focusedEntity == null))
		{
			_followTransform.position = focusedEntity.Visual.GetBasePosition();
			_followCv = default;
			_body.m_FollowOffset = GetFollowOffsetTarget();
			((Behaviour)(object)selectedEntityCamera).enabled = false;
			((Behaviour)(object)selectedEntityCamera).enabled = true;
		}
	}

	public void SetCameraPosition(Vector3 pos)
	{
		_followTransform.position = pos;
		_followCv = default;
	}

	private void LogicUpdateSpectation(float dt)
	{
		if (isSpectating)
		{
			if (!DewPlayer.local.hero.IsNullInactiveDeadOrKnockedOut())
			{
				SetFocusedEntity(DewPlayer.local.hero);
				isSpectating = false;
				onIsSpectatingChanged?.Invoke(isSpectating);
				_nextSpectateTargetElapsedTime = 0f;
				SnapCameraToFocusedEntity();
			}
			else if (focusedEntity.IsNullInactiveDeadOrKnockedOut())
			{
				_nextSpectateTargetElapsedTime += dt;
				if (_nextSpectateTargetElapsedTime > nextSpectateTargetTime && HasAliveHeroToSpectate())
				{
					_nextSpectateTargetElapsedTime = 0f;
					ChooseNextSpectationTarget();
				}
			}
			else
			{
				_nextSpectateTargetElapsedTime = 0f;
			}
		}
		else if ((UnityEngine.Object)(object)DewPlayer.local.hero == null)
		{
			ChooseNextSpectationTarget();
			isSpectating = true;
			onIsSpectatingChanged?.Invoke(isSpectating);
		}
		else if (DewPlayer.local.hero.isKnockedOut && !NetworkedManagerBase<GameManager>.instance.isGameConcluded && DewPlayer.gamePlayers.Count > 1)
		{
			_startSpectateCurrentElapsedTime += dt;
			if (_startSpectateCurrentElapsedTime > startSpectatingTime && HasAliveHeroToSpectate())
			{
				ChooseNextSpectationTarget();
				isSpectating = true;
				onIsSpectatingChanged?.Invoke(isSpectating);
				_startSpectateCurrentElapsedTime = 0f;
			}
		}
		else
		{
			_startSpectateCurrentElapsedTime = 0f;
		}
	}

	public void SpectateImmediatelyIfDead()
	{
		_startSpectateCurrentElapsedTime = float.PositiveInfinity;
	}

	private static bool HasAliveHeroToSpectate()
	{
		List<Hero> allHeroes = NetworkedManagerBase<ActorManager>.instance.allHeroes;
		for (int i = 0; i < allHeroes.Count; i++)
		{
			if (!allHeroes[i].IsNullInactiveDeadOrKnockedOut())
			{
				return true;
			}
		}
		return false;
	}

	private void LogicUpdateOcclusionTest(float dt)
	{
		if (_characterPositionsBuffer == null)
		{
			_characterPositionsBuffer = new Vector4[16];
		}
		if (disableSeeThrough || (UnityEngine.Object)(object)DewPlayer.local.controllingEntity == null)
		{
			Shader.SetGlobalInt(CharacterPositionsCount, 0);
			Shader.SetGlobalFloat(DistanceFromCameraMultiplier, 1f);
			return;
		}
		Vector3 vector = (((UnityEngine.Object)(object)focusedEntity != null) ? focusedEntity.agentPosition : ControlManager.GetWorldPositionOnGroundFromViewportPoint(Vector2.one * 0.5f, forDirectionalAttacks: false));
		if (_occlusionQueryCounter <= 0)
		{
			_occlusionQueryCounter = 3;
			RebuildOcclusionPreferred(vector);
		}
		_occlusionQueryCounter--;
		List<(object, float)> preferredObjects = _preferredObjects;
		for (int i = 0; i < preferredObjects.Count; i++)
		{
			if (_currentObjects.Count >= _characterPositionsBuffer.Length)
			{
				break;
			}
			if (!_currentObjects.Contains(preferredObjects[i].Item1))
			{
				_currentObjects.Add(preferredObjects[i].Item1);
				_currentPositions.Add(default);
			}
		}
		for (int num = _currentObjects.Count - 1; num >= 0; num--)
		{
			bool flag;
			if (_currentObjects[num] is IInteractable interactable)
			{
				flag = ((!(interactable is Actor a)) ? (interactable.interactPivot == null) : a.IsNullOrInactive());
				if (!flag)
				{
					_currentPositions[num] = Vector3FlattenExtension.WithW(interactable.interactPivot.position + Vector3.up, _currentPositions[num].w);
				}
			}
			else if (_currentObjects[num] is Entity entity)
			{
				flag = entity.IsNullOrInactive();
				if (!flag)
				{
					_currentPositions[num] = Vector3FlattenExtension.WithW(entity.Visual.GetCenterPosition(), _currentPositions[num].w);
				}
			}
			else
			{
				flag = true;
			}
			if (!flag)
			{
				object obj = _currentObjects[num];
				flag = true;
				for (int j = 0; j < preferredObjects.Count; j++)
				{
					if (preferredObjects[j].Item1 == obj)
					{
						flag = false;
						break;
					}
				}
			}
			if (flag)
			{
				_currentPositions[num] = _currentPositions[num].WithW(Mathf.Clamp01(_currentPositions[num].w - dt * 1.5f));
				if (_currentPositions[num].w <= 0f)
				{
					_currentObjects.RemoveAt(num);
					_currentPositions.RemoveAt(num);
				}
			}
			else
			{
				float max = 1f;
				if (_currentObjects[num] is Monster { type: Monster.MonsterType.Lesser })
				{
					max = 0.6f;
				}
				_currentPositions[num] = _currentPositions[num].WithW(Mathf.Clamp(_currentPositions[num].w + dt * 1.5f, 0f, max));
			}
		}
		for (int k = 0; k < _currentPositions.Count && k < _characterPositionsBuffer.Length; k++)
		{
			_characterPositionsBuffer[k] = _currentPositions[k];
		}
		int value = Mathf.Min(_characterPositionsBuffer.Length, _currentPositions.Count);
		Shader.SetGlobalInt(CharacterPositionsCount, value);
		Shader.SetGlobalVectorArray(CharacterPositions, _characterPositionsBuffer);
		Shader.SetGlobalVector(MainCharacterPosition, vector);
		float value2;
		if (isPlayingCutscene)
		{
			value2 = 1f;
		}
		else
		{
			float magnitude = farZoomBody.magnitude;
			value2 = (_body.m_FollowOffset.magnitude / magnitude * 3f + 1f) / 4f;
		}
		Shader.SetGlobalFloat(DistanceFromCameraMultiplier, value2);
	}

	private void RebuildOcclusionPreferred(Vector3 center)
	{
		_preferredObjects.Clear();
		Collider[] array = DewPool.GetArray(out ArrayReturnHandle<Collider> handle, 64);
		int num = Physics.OverlapSphereNonAlloc(center, occlusionTestRadius, array, LayerMasks.Interactable | LayerMasks.Entity);
		for (int i = 0; i < num; i++)
		{
			Collider key = array[i];
			IInteractable orAdd = _interactableCache.GetOrAdd(key);
			if (orAdd != null && !(orAdd is Actor { isActive: false }) && orAdd.CanInteract(ManagerBase<ControlManager>.softInstance.controllingEntity))
			{
				_preferredObjects.Add((orAdd, Vector3.Distance(center, orAdd.interactPivot.position)));
			}
		}
		handle.Return();
		ListReturnHandle<Entity> handle2;
		foreach (Entity item in DewPhysics.OverlapCircleAllEntities(out handle2, center, occlusionTestRadius, new CollisionCheckSettings
		{
			includeUncollidable = true
		}))
		{
			if (!item.IsNullInactiveDeadOrKnockedOut() && !(item is IDisableOcclusionTest))
			{
				_preferredObjects.Add((item, Vector3.Distance(center, item.agentPosition)));
			}
		}
		handle2.Return();
		_preferredObjects.Sort(((object, float) x, (object, float) y) => x.Item2.CompareTo(y.Item2));
		while (_preferredObjects.Count > _characterPositionsBuffer.Length)
		{
			_preferredObjects.RemoveAt(_preferredObjects.Count - 1);
		}
	}

	private void FrameUpdateDamageOverlay()
	{
		if ((UnityEngine.Object)(object)focusedEntity == null || focusedEntity is Hero { disableDamageOverlay: not false })
		{
			damageOverlay.alpha = 0f;
			return;
		}
		EntityStatus status = focusedEntity.Status;
		_damageOverlayMinAlpha = 1f - (status.currentHealth / status.maxHealth - maxAlphaHealthThreshold) / (minAlphaHealthThreshold - maxAlphaHealthThreshold);
		_damageOverlayMinAlpha = Mathf.Clamp01(_damageOverlayMinAlpha);
		_damageOverlayMinAlpha *= maxAlpha;
		damageOverlay.alpha = Mathf.MoveTowards(damageOverlay.alpha, _damageOverlayMinAlpha, damageOverlayDecaySpeed * Time.deltaTime);
	}

	private void FrameUpdateFollow()
	{
		if (isSpectating)
		{
			if (DewInput.GetButtonDown(DewSave.profileMain.controls.spectatorNextTarget, checkGameAreaForMouse: true) && InGameUIManager.instance.IsState("Playing") && !InGameUIManager.instance.disablePlayingInput)
			{
				ChooseNextSpectationTarget();
			}
		}
		else if ((UnityEngine.Object)(object)focusedEntity != (UnityEngine.Object)(object)ManagerBase<ControlManager>.instance.controllingEntity)
		{
			SetFocusedEntity(ManagerBase<ControlManager>.instance.controllingEntity);
			SnapCameraToFocusedEntity();
		}
		if ((UnityEngine.Object)(object)focusedEntity == null)
		{
			_followCv = default;
			return;
		}
		Vector3 target = default;
		if (NetworkedManagerBase<ConversationManager>.instance.hasOngoingLocalConversation)
		{
			int num = 0;
			Entity[] speakers = NetworkedManagerBase<ConversationManager>.instance.ongoingLocalConversation.speakers;
			foreach (Entity entity in speakers)
			{
				if (!entity.IsNullInactiveDeadOrKnockedOut())
				{
					target += entity.Visual.GetBasePosition();
					num++;
				}
			}
			if (num == 0)
			{
				target = focusedEntity.Visual.GetBasePosition();
			}
			else
			{
				target /= (float)num;
			}
		}
		else
		{
			target = focusedEntity.Visual.GetBasePosition();
		}
		if (!isSpectating)
		{
			foreach (CameraModifierBase cameraModifier in _cameraModifiers)
			{
				if (cameraModifier is CameraModifierOffset cameraModifierOffset)
				{
					target += cameraModifierOffset.offset;
				}
			}
		}
		_followTransform.position = Vector3.SmoothDamp(_followTransform.position, target, ref _followCv, followSmoothTime);
	}

	private void ChooseNextSpectationTarget()
	{
		List<Hero> allHeroes = NetworkedManagerBase<ActorManager>.instance.allHeroes;
		int num = 0;
		for (int i = 0; i < allHeroes.Count; i++)
		{
			if ((UnityEngine.Object)(object)allHeroes[i] == (UnityEngine.Object)(object)focusedEntity)
			{
				num = i;
				break;
			}
		}
		for (int j = 0; j < allHeroes.Count; j++)
		{
			num = (num + 1) % allHeroes.Count;
			if (!allHeroes[num].IsNullInactiveDeadOrKnockedOut())
			{
				SetFocusedEntity(allHeroes[num]);
				SnapCameraToFocusedEntity();
				break;
			}
		}
	}

	public void SetActiveEntityVCam(bool value)
	{
		((Component)(object)selectedEntityCamera).gameObject.SetActive(value);
	}

	public void DoGenericFadeIn(bool immediately = false)
	{
		ShortcutExtensions.DOKill((Component)(object)fadeOutVolume, false);
		if (immediately)
		{
			fadeOutVolume.weight = 0f;
		}
		else
		{
			fadeOutVolume.DOWeight(0f, genericFadeTime);
		}
	}

	public void DoGenericFadeOut(bool immediately = false)
	{
		ShortcutExtensions.DOKill((Component)(object)fadeOutVolume, false);
		if (immediately)
		{
			fadeOutVolume.weight = 1f;
		}
		else
		{
			fadeOutVolume.DOWeight(1f, genericFadeTime);
		}
	}

	public void ResetLetterBoxPositions()
	{
		cutsceneBottomLetterBox.sizeDelta = _bottomLetterBoxSizeDelta;
		cutsceneTopLetterBox.sizeDelta = _topLetterBoxSizeDelta;
	}

	public void DoLetterBoxFadeIn()
	{
		ShortcutExtensions.DOKill((Component)cutsceneBottomLetterBox, false);
		cutsceneBottomLetterBox.sizeDelta = Vector2.zero;
		TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOSizeDelta(cutsceneBottomLetterBox, _bottomLetterBoxSizeDelta, cutsceneFadeTime, false), (Ease)1);
		ShortcutExtensions.DOKill((Component)cutsceneTopLetterBox, false);
		cutsceneTopLetterBox.sizeDelta = Vector2.zero;
		TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOSizeDelta(cutsceneTopLetterBox, _topLetterBoxSizeDelta, cutsceneFadeTime, false), (Ease)1);
	}

	public void DoLetterBoxFadeOut()
	{
		ResetLetterBoxPositions();
		ShortcutExtensions.DOKill((Component)cutsceneBottomLetterBox, false);
		TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOSizeDelta(cutsceneBottomLetterBox, Vector2.zero, cutsceneFadeTime, false), (Ease)1);
		ShortcutExtensions.DOKill((Component)cutsceneTopLetterBox, false);
		TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOSizeDelta(cutsceneTopLetterBox, Vector2.zero, cutsceneFadeTime, false), (Ease)1);
	}

	public void DoCutsceneFadeOut()
	{
		ShortcutExtensions.DOKill((Component)(object)cutsceneTransitionFade, false);
		DOTweenModuleUI.DOFade(cutsceneTransitionFade, 1f, cutsceneFadeTime);
	}

	public void DoCutsceneFadeIn()
	{
		ShortcutExtensions.DOKill((Component)(object)cutsceneTransitionFade, false);
		DOTweenModuleUI.DOFade(cutsceneTransitionFade, 0f, cutsceneFadeTime);
	}

	internal void AddLocalCameraModifier(CameraModifierBase modifier)
	{
		if (modifier == null)
		{
			throw new ArgumentNullException("modifier");
		}
		_cameraModifiers.Add(modifier);
	}

	internal void RemoveLocalCameraModifier(CameraModifierBase modifier)
	{
		_cameraModifiers.Remove(modifier);
	}

	public void SetFocusedEntity(Entity newValue)
	{
		damageOverlay.alpha = 0f;
		if ((UnityEngine.Object)(object)newValue == (UnityEngine.Object)(object)focusedEntity)
		{
			return;
		}
		Entity arg = focusedEntity;
		focusedEntity = newValue;
		try
		{
			onFocusedEntityChanged?.Invoke(arg, newValue);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public void SkipCurrentCutscene()
	{
		if (isPlayingCutscene && !((UnityEngine.Object)(object)currentCutsceneDirector == null) && currentCutsceneDirector.enableSkip)
		{
			currentCutsceneDirector.CmdSkip();
		}
	}
}
