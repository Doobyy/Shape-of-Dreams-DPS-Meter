using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FIMSpace.FSpine;
using FIMSpace.FTail;
using MagicaCloth2;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(EntityHighlightProvider))]
[LogicUpdatePriority(-299)]
public class EntityVisual : EntityComponent, ICleanup
{
	public enum EntityDeathBehavior
	{
		None,
		HideModel,
		Dissolve
	}

	private const float ElementalPropertySpeed = 8f;

	private const float YVelGravity = 18.8f;

	private const float KnockUpStrengthMultiplier = 6f;

	public SafeAction<bool> ClientEvent_OnRendererEnabledChanged;

	public SafeAction<bool> ClientEvent_OnGroundMarkerHiddenChanged;

	public SafeAction ClientEvent_OnModelLoaded;

	[NonSerialized]
	public EntityModel model;

	[SyncVar]
	public float spawnDuration;

	public bool invulnerableWhileSpawning;

	public float dazeAfterSpawnDuration;

	public bool doPositionOffset;

	public AnimationCurve spawnXOffset = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public AnimationCurve spawnYOffset = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public AnimationCurve spawnZOffset = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public bool doScaling;

	public bool useSeparateAxis;

	public AnimationCurve spawnXScale = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	public AnimationCurve spawnYScale = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	public AnimationCurve spawnZScale = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	public DewAnimationClip spawnAnim;

	[FormerlySerializedAs("delayStartEffectTilSpawned")]
	public bool delayLoopEffectTilSpawned;

	public GameObject spawnEffect;

	public bool delaySpawnEffectUntilSpawned;

	public GameObject spawnEffectOnGround;

	public bool delaySpawnEffectOnGroundTilSpawned;

	[NonSerialized]
	[SyncVar]
	public bool skipSpawning;

	[NonSerialized]
	[SyncVar]
	public bool invisibleByDefault;

	[SyncVar(hook = "UpdateRendererOff")]
	private int _rendererOffCounterNetworked;

	private int _rendererOffCounterLocal;

	[SyncVar(hook = "UpdateGroundMarkerHidden")]
	private int _groundMarkerHiddenCounterNetworked;

	private int _groundMarkerHiddenCounterLocal;

	[NonSerialized]
	public List<Renderer> renderers = new List<Renderer>();

	private Renderer[] _characterRenderers;

	[NonSerialized]
	public List<Renderer> solidRenderers = new List<Renderer>();

	[NonSerialized]
	public List<Accessory> accessoryInstances = new List<Accessory>();

	private MagicaCloth[] _clothes;

	private TailAnimator2[] _tails;

	private Transform _apHead;

	private Transform _apLeftHand;

	private Transform _apRightHand;

	private Transform _apLeftFoot;

	private Transform _apRightFoot;

	private Transform _apWeapon;

	private Transform _apMuzzle;

	private Transform _apCenter;

	private bool _isDissolving;

	private Vector3 _currentDamage;

	private Action<EventInfoDamage> _cachedHandleTakeDamage;

	private Action<EventInfoKill> _cachedHandleDeath;

	private GameObject _stunEffect;

	[NonSerialized]
	private bool _didStartClientEffects;

	[NonSerialized]
	private bool _didBakeModelFx;

	[NonSerialized]
	private bool _modelWasFreshlyLoaded;

	private static GameObject s_entityEffectPrefab;

	private static GameObject s_stunEffectPrefab;

	[SyncVar(hook = "OnShouldShowStunnedChanged")]
	private bool _shouldShowStunned;

	[NonSerialized]
	public bool disableModelTransformUpdate;

	internal List<FxAttachToEntity> _attachedEffects;

	private ListReturnHandle<FxAttachToEntity> _attachedEffectsHandle;

	private List<ParticleSystem> _pausedSystemsByRendererOff;

	private ListReturnHandle<ParticleSystem> _pausedSystemsByRendererOffHandle;

	private List<Renderer> _disabledRenderersByRendererOff;

	private ListReturnHandle<Renderer> _disabledRenderersByRendererOffHandle;

	[CompilerGenerated]
	[SyncVar]
	private int genericStackIndicatorMax__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private int genericStackIndicatorValue__BackingField;

	public Dictionary<string, List<GameObject>> mirageSkinObjects = new Dictionary<string, List<GameObject>>();

	private Transform _transform;

	private const float HitEffectRpcMinInterval = 0.05f;

	private float _lastHitEffectRpcTime = float.NegativeInfinity;

	private int _fxTakeDamageFrame = -1;

	private int _fxTakeDamageCountThisFrame;

	private readonly List<ParticleSystem> _pausedSystemsByTeleport = new List<ParticleSystem>(16);

	private static readonly int DissolveStrength;

	private static readonly int CMBaseColor;

	private static readonly int CMEmission;

	private static readonly int CMOpacity;

	private static readonly int CMDissolveColor;

	private List<EntityColorModifier> _colorModifiers = new List<EntityColorModifier>();

	private bool _isColorModifiersDirty;

	private static readonly Stack<EntityColorModifier> _colorModifierPool;

	private static readonly int FireStrength;

	private static readonly int ColdStrength;

	private static readonly int LightStrength;

	private static readonly int VoidStrength;

	internal float _eFireTarget;

	internal float _eColdTarget;

	internal float _eVoidTarget;

	internal float _eLightTarget;

	private float _eFire;

	private float _eCold;

	private float _eVoid;

	private float _eLight;

	private static readonly Dictionary<string, int> _shaderPropertyIdCache;

	private Dictionary<int, float> _shaderFloatProperties;

	private Dictionary<int, Color> _shaderColorProperties;

	private MaterialPropertyBlock _shaderPropertyBlock;

	private bool _isShaderPropertyBlockDirty;

	private int _shaderPropertyBatchDepth;

	private const int BoundsCacheMaxAgeFramesAmortized = 5;

	private const float BoundsCachePositionThresholdSqr = 0.0025000002f;

	private const float BoundsSmoothingTau = 0.08f;

	private const float BoundsSmoothingSnapDistSqr = 4f;

	private int _bodyBoundsCacheFrame = -1;

	private Vector3 _bodyBoundsCachePosition;

	private Bounds _cachedBodyBounds;

	private int _bodyBoundsSmoothedFrame = -1;

	private Vector3 _bodyBoundsSmoothedOffset;

	private Vector3 _bodyBoundsSmoothedSize;

	private int _renderBoundsCacheFrame = -1;

	private Vector3 _renderBoundsCachePosition;

	private Bounds _cachedRenderBounds;

	private List<EntityShellModifier> _shellModifiers = new List<EntityShellModifier>();

	private bool _isShellModifiersDirty;

	public SafeAction ClientEvent_OnSpawnComplete;

	private StatusEffect[] _spawnProtections;

	private EntityTransformModifier _spawnModifier;

	private bool _needToDoSpawning = true;

	private List<EntityTransformModifier> _transformModifiers = new List<EntityTransformModifier>();

	private bool _isTransformModifiersDirty;

	private static readonly Stack<EntityTransformModifier> _transformModifierPool;

	[NonSerialized]
	public Transform modelTransform;

	private Vector3 _modelOriginalLocalPosition;

	private Quaternion _modelOriginalLocalRotation;

	private Vector3 _modelOriginalLocalScale;

	private bool _didHideModelDueToSmallSize;

	private bool _hasLastWrittenModelTransform;

	private Vector3 _lastRootPosition;

	private Quaternion _lastRootRotation;

	private float _lastYOffset;

	private Vector3 _lastWrittenModelPosition;

	private Quaternion _lastWrittenModelLocalRotation;

	private Vector3 _lastWrittenModelLocalScale;

	public Action<int, int> _Mirror_SyncVarHookDelegate__rendererOffCounterNetworked;

	public Action<int, int> _Mirror_SyncVarHookDelegate__groundMarkerHiddenCounterNetworked;

	public Action<bool, bool> _Mirror_SyncVarHookDelegate__shouldShowStunned;

	public float currentYOffset { get; private set; }

	public float currentYVelocity { get; private set; }

	public EntityHighlightProvider highlight { get; private set; }

	public bool isRendererOff { get; private set; }

	private int _rendererOffCounter => _rendererOffCounterNetworked + _rendererOffCounterLocal;

	public bool isGroundMarkerHidden { get; private set; }

	private int _groundMarkerHiddenCounter => _groundMarkerHiddenCounterNetworked + _groundMarkerHiddenCounterLocal;

	public int genericStackIndicatorMax
	{
		[CompilerGenerated]
		get
		{
			return genericStackIndicatorMax__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CgenericStackIndicatorMax_003Ek__BackingField = value;
		}
	}

	public int genericStackIndicatorValue
	{
		[CompilerGenerated]
		get
		{
			return genericStackIndicatorValue__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CgenericStackIndicatorValue_003Ek__BackingField = value;
		}
	}

	bool ICleanup.canDestroy => !_isDissolving;

	private int BoundsCacheMaxAgeFrames
	{
		get
		{
			if (!(entity is Hero) && (!(entity is Monster monster) || (monster.type != Monster.MonsterType.MiniBoss && monster.type != Monster.MonsterType.Boss)))
			{
				return 5;
			}
			return 1;
		}
	}

	public bool isSpawning { get; private set; }

	public Vector3 etWorldOffset { get; private set; }

	public Vector3 etLocalOffset { get; private set; }

	public Vector3 etScaleMultiplier { get; private set; } = Vector3.one;

	public Quaternion etRotation { get; private set; } = Quaternion.identity;

	public float NetworkspawnDuration
	{
		get
		{
			return spawnDuration;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref spawnDuration, 1uL, (Action<float, float>)null);
		}
	}

	public bool NetworkskipSpawning
	{
		get
		{
			return skipSpawning;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref skipSpawning, 2uL, (Action<bool, bool>)null);
		}
	}

	public bool NetworkinvisibleByDefault
	{
		get
		{
			return invisibleByDefault;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref invisibleByDefault, 4uL, (Action<bool, bool>)null);
		}
	}

	public int Network_rendererOffCounterNetworked
	{
		get
		{
			return _rendererOffCounterNetworked;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref _rendererOffCounterNetworked, 8uL, _Mirror_SyncVarHookDelegate__rendererOffCounterNetworked);
		}
	}

	public int Network_groundMarkerHiddenCounterNetworked
	{
		get
		{
			return _groundMarkerHiddenCounterNetworked;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref _groundMarkerHiddenCounterNetworked, 16uL, _Mirror_SyncVarHookDelegate__groundMarkerHiddenCounterNetworked);
		}
	}

	public bool Network_shouldShowStunned
	{
		get
		{
			return _shouldShowStunned;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref _shouldShowStunned, 32uL, _Mirror_SyncVarHookDelegate__shouldShowStunned);
		}
	}

	public int Network_003CgenericStackIndicatorMax_003Ek__BackingField
	{
		get
		{
			return genericStackIndicatorMax__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref genericStackIndicatorMax__BackingField, 64uL, (Action<int, int>)null);
		}
	}

	public int Network_003CgenericStackIndicatorValue_003Ek__BackingField
	{
		get
		{
			return genericStackIndicatorValue__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref genericStackIndicatorValue__BackingField, 128uL, (Action<int, int>)null);
		}
	}

	public override void ClearPooledEventsAndProcessors()
	{
		base.ClearPooledEventsAndProcessors();
		ClientEvent_OnRendererEnabledChanged?.Clear();
		ClientEvent_OnGroundMarkerHiddenChanged?.Clear();
		ClientEvent_OnModelLoaded?.Clear();
		ClientEvent_OnSpawnComplete?.Clear();
		ClientEvent_OnRendererEnabledChanged += new Action<bool>(HandleAttachedEffects);
		ClientEvent_OnRendererEnabledChanged += new Action<bool>(HandleAccessoryRenderers);
	}

	private void UpdateRendererOff(int _, int __)
	{
		bool flag = _rendererOffCounterNetworked + _rendererOffCounterLocal > 0;
		if (flag && !isRendererOff)
		{
			isRendererOff = true;
			foreach (Renderer renderer in renderers)
			{
				if (!(renderer == null))
				{
					renderer.enabled = false;
				}
			}
			ClientEvent_OnRendererEnabledChanged?.Invoke(arg: false);
		}
		else
		{
			if (flag || !isRendererOff)
			{
				return;
			}
			isRendererOff = false;
			foreach (Renderer renderer2 in renderers)
			{
				if (!(renderer2 == null))
				{
					renderer2.enabled = true;
				}
			}
			ClientEvent_OnRendererEnabledChanged?.Invoke(arg: true);
			DoTransformFrameUpdate();
		}
	}

	private void UpdateGroundMarkerHidden(int _, int __)
	{
		bool flag = _groundMarkerHiddenCounter > 0;
		if (flag && !isGroundMarkerHidden)
		{
			isGroundMarkerHidden = true;
			ClientEvent_OnGroundMarkerHiddenChanged?.Invoke(arg: true);
		}
		else if (!flag && isGroundMarkerHidden)
		{
			isGroundMarkerHidden = false;
			ClientEvent_OnGroundMarkerHiddenChanged?.Invoke(arg: false);
		}
	}

	public static void PreloadCachedEffectPrefabs()
	{
		if (s_entityEffectPrefab == null)
		{
			s_entityEffectPrefab = Resources.Load<GameObject>("Effects/EntityEffect");
		}
		if (s_stunEffectPrefab == null)
		{
			s_stunEffectPrefab = Resources.Load<GameObject>("Effects/Status/Stun");
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (_shaderFloatProperties == null)
		{
			_shaderFloatProperties = new Dictionary<int, float>();
		}
		if (_shaderColorProperties == null)
		{
			_shaderColorProperties = new Dictionary<int, Color>();
		}
		if (_shaderPropertyBlock == null)
		{
			_shaderPropertyBlock = new MaterialPropertyBlock();
		}
		_transform = ((Component)(object)this).transform;
		_attachedEffects = DewPool.GetList(out _attachedEffectsHandle);
		_pausedSystemsByRendererOff = DewPool.GetList(out _pausedSystemsByRendererOffHandle);
		_disabledRenderersByRendererOff = DewPool.GetList(out _disabledRenderersByRendererOffHandle);
		highlight = ((Component)(object)this).GetComponent<EntityHighlightProvider>();
		ClientEvent_OnRendererEnabledChanged += new Action<bool>(HandleAttachedEffects);
		ClientEvent_OnRendererEnabledChanged += new Action<bool>(HandleAccessoryRenderers);
		if (((Component)(object)this).GetComponent<Entity>() is Hero hero)
		{
			hero.accessories.Callback += (Operation<string> op, int index, string oldItem, string newItem) =>
			{
				RefreshAccessories();
			};
		}
	}

	private void RefreshAccessories()
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		Hero hero = (((UnityEngine.Object)(object)entity == null) ? ((Component)(object)this).GetComponent<Hero>() : (entity as Hero));
		if ((UnityEngine.Object)(object)hero == null)
		{
			return;
		}
		for (int num = accessoryInstances.Count - 1; num >= 0; num--)
		{
			if (accessoryInstances[num] == null)
			{
				accessoryInstances.RemoveAt(num);
			}
		}
		if (!model)
		{
			return;
		}
		Enumerator<string> enumerator = hero.accessories.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				string accName = enumerator.Current;
				if (accessoryInstances.FindIndex((Accessory i) => i.name == accName) == -1 && hero.owner.IsAllowedToUseItem(accName))
				{
					Accessory accessory = UnityEngine.Object.Instantiate(DewResources.GetByName<Accessory>(accName));
					accessory.Setup(((Component)(object)this).transform, Dew.GetOriginalName(model.name));
					accessory.name = accName;
					accessoryInstances.Add(accessory);
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		for (int num2 = accessoryInstances.Count - 1; num2 >= 0; num2--)
		{
			if (!hero.accessories.Contains(accessoryInstances[num2].name))
			{
				UnityEngine.Object.Destroy(accessoryInstances[num2].gameObject);
				accessoryInstances.RemoveAt(num2);
			}
		}
		HandleAccessoryRenderers(!isRendererOff);
	}

	private void HandleAccessoryRenderers(bool enabled)
	{
		foreach (Accessory accessoryInstance in accessoryInstances)
		{
			if (accessoryInstance == null)
			{
				continue;
			}
			SkinnedMeshRenderer[] componentsInChildren = accessoryInstance.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in componentsInChildren)
			{
				if (skinnedMeshRenderer != null)
				{
					skinnedMeshRenderer.enabled = enabled;
				}
			}
			MeshRenderer[] componentsInChildren2 = accessoryInstance.GetComponentsInChildren<MeshRenderer>(includeInactive: true);
			foreach (MeshRenderer meshRenderer in componentsInChildren2)
			{
				if (meshRenderer != null)
				{
					meshRenderer.enabled = enabled;
				}
			}
		}
	}

	public override void OnStart()
	{
		base.OnStart();
		entity.ClientEntityEvent_OnIsSleepingChanged += (Action<bool>)((bool v) =>
		{
			if (v)
			{
				DisableRenderersLocal();
			}
			else
			{
				EnableRenderersLocal();
			}
		});
	}

	[ClientRpc]
	public void LoadModelDefault()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void EntityVisual::LoadModelDefault()", 1625717638, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public void LoadModelDefaultLocal()
	{
		EntityModel componentInChildren = ((Component)(object)this).GetComponentInChildren<EntityModel>();
		if ((bool)componentInChildren)
		{
			if (!componentInChildren.isInitialized)
			{
				LoadModelLocal(componentInChildren);
			}
			else
			{
				LoadModelLocal(((Component)(object)DewResources.GetByType<Entity>(((object)entity).GetType(), default(ResourceLoadSettings))).GetComponentInChildren<EntityModel>());
			}
		}
	}

	public void LoadModelLocal(EntityModel m)
	{
		if (m == null)
		{
			return;
		}
		if (m.isInitialized)
		{
			throw new InvalidOperationException("Provided model cannot be an already initialized model instance");
		}
		if ((UnityEngine.Object)(object)entity != null && (UnityEngine.Object)(object)((NetworkBehaviour)entity).netIdentity != null)
		{
			((NetworkBehaviour)entity).netIdentity.fxPathTable?.Clear();
			((NetworkBehaviour)entity).netIdentity.fxDidBuildPathTable = false;
		}
		if (_didHideModelDueToSmallSize)
		{
			_didHideModelDueToSmallSize = false;
			EnableRenderersLocal();
		}
		if (model == null && !m.transform.IsSelfOrDescendantOf(((Component)(object)this).transform))
		{
			EntityModel componentInChildren = ((Component)(object)this).GetComponentInChildren<EntityModel>();
			if (componentInChildren != null)
			{
				UnityEngine.Object.DestroyImmediate(componentInChildren.gameObject);
			}
		}
		if (model != null)
		{
			UnityEngine.Object.DestroyImmediate(model.gameObject);
			model = null;
		}
		bool flag = true;
		Transform parent = m.transform.parent;
		while (parent != null)
		{
			if (parent == ((Component)(object)this).transform)
			{
				flag = false;
				break;
			}
			parent = parent.parent;
		}
		if (flag)
		{
			m = UnityEngine.Object.Instantiate(m, ((Component)(object)this).transform);
		}
		model = m;
		model.isInitialized = true;
		try
		{
			entity.Animation.SetupModel();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		CollectComponentReferences();
		InitEntityTransform();
		if (model.fxLoop != null)
		{
			if (_needToDoSpawning && !skipSpawning)
			{
				((MonoBehaviour)(object)this).StartCoroutine(Routine());
			}
			else
			{
				Dew.CallDelayed(() =>
				{
					if (!entity.IsNullInactiveDeadOrKnockedOut())
					{
						FxPlay(model.fxLoop, entity);
					}
				});
			}
		}
		isRendererOff = !isRendererOff;
		UpdateRendererOff(0, 0);
		_isTransformModifiersDirty = true;
		DoTransformFrameUpdate();
		RefreshAccessories();
		FixMagicaClothes();
		if (entity is Hero && !((Component)(object)entity.Animation.animator).TryGetComponent(out EntityFootsteps _))
		{
			((Component)(object)entity.Animation.animator).gameObject.AddComponent<EntityFootsteps>();
		}
		entity._skinSkillProcessor = null;
		entity._skinVisualVariantId = 0;
		if (m.TryGetComponent<Skin>(out var component2) && component2.skillVisuals != null && component2.skillVisuals.Length != 0)
		{
			string skinName = Dew.GetOriginalName(component2.name);
			SkillVisualOverrideItem[] skillVisuals = component2.skillVisuals;
			int varId = DewResources.GetVariantIdForSkin(skinName);
			entity._skinVisualVariantId = varId;
			entity._skinSkillProcessor = delegate(ref VariantDef varDef, Actor _, Type childType)
			{
				string name = childType.Name;
				if (!varDef.Contains(varId))
				{
					for (int i = 0; i < skillVisuals.Length; i++)
					{
						string[] targets = skillVisuals[i].targets;
						for (int j = 0; j < targets.Length; j++)
						{
							if (!(targets[j] != name))
							{
								varDef = varDef.Add(varId);
								return;
							}
						}
					}
				}
			};
			DewResources.UnregisterVariantProcessor(varId);
			DewResources.RegisterVariantProcessor(varId, (UnityEngine.Object o) =>
			{
				if (!(o is GameObject gameObject) || !gameObject.TryGetComponent<Actor>(out var component3))
				{
					return (Action)null;
				}
				gameObject.name = gameObject.name + " (" + skinName + ")";
				string name = ((object)component3).GetType().Name;
				foreach (SkillVisualOverrideItem skillVisualOverrideItem in skillVisuals)
				{
					string[] targets = skillVisualOverrideItem.targets;
					for (int j = 0; j < targets.Length; j++)
					{
						if (!(targets[j] != name))
						{
							if (skillVisualOverrideItem.materialReplacements != null)
							{
								SkillMaterialReplacement[] materialReplacements = skillVisualOverrideItem.materialReplacements;
								foreach (SkillMaterialReplacement skillMaterialReplacement in materialReplacements)
								{
									if (skillMaterialReplacement != null)
									{
										DewEffect.ReplaceMaterialsRecursively(gameObject, skillMaterialReplacement.from, skillMaterialReplacement.to);
									}
								}
							}
							DewEffect.ChangeColorRecursively(gameObject, skillVisualOverrideItem.changeHue ? new float?(skillVisualOverrideItem.hue / 360f) : ((float?)null), skillVisualOverrideItem.changeSaturation ? skillVisualOverrideItem.saturationMultiplier : 1f, skillVisualOverrideItem.changeValue ? skillVisualOverrideItem.valueMultiplier : 1f);
							break;
						}
					}
				}
				return (Action)null;
			});
		}
		try
		{
			entity.OnModelLoaded();
		}
		catch (Exception exception2)
		{
			Debug.LogException(exception2);
		}
		ClientEvent_OnModelLoaded?.Invoke();
		IEnumerator Routine()
		{
			if (!skipSpawning && delayLoopEffectTilSpawned && spawnDuration > 0f)
			{
				yield return WaitForSecondsSpawn(spawnDuration);
			}
			else
			{
				yield return null;
			}
			if (!entity.IsNullInactiveDeadOrKnockedOut())
			{
				FxPlay(model.fxLoop, entity);
			}
		}
	}

	private void FixMagicaClothes()
	{
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			yield return null;
			List<MagicaCloth> clothes = ((Component)(object)this).GetComponentsInChildrenNonAlloc(out ListReturnHandle<MagicaCloth> handle);
			foreach (MagicaCloth item in clothes)
			{
				((Behaviour)(object)item).enabled = false;
			}
			yield return null;
			foreach (MagicaCloth item2 in clothes)
			{
				if (!((UnityEngine.Object)(object)item2 == null))
				{
					((Behaviour)(object)item2).enabled = true;
				}
			}
			handle.Return();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_attachedEffects != null)
		{
			_attachedEffectsHandle.Return();
			_attachedEffects = null;
		}
		if (_pausedSystemsByRendererOff != null)
		{
			_pausedSystemsByRendererOffHandle.Return();
			_pausedSystemsByRendererOff = null;
		}
		if (_disabledRenderersByRendererOff != null)
		{
			_disabledRenderersByRendererOffHandle.Return();
			_disabledRenderersByRendererOff = null;
		}
	}

	public override void OnLateStartServer()
	{
		base.OnLateStartServer();
		if (!skipSpawning && spawnAnim != null)
		{
			entity.Animation.PlayAbilityAnimation(spawnAnim);
		}
	}

	private void CollectComponentReferences()
	{
		renderers.Clear();
		Renderer[] componentsInChildren = ((Component)(object)this).GetComponentsInChildren<Renderer>(true);
		foreach (Renderer renderer in componentsInChildren)
		{
			if ((model.fxDeath != null && renderer.transform.IsSelfOrDescendantOf(model.fxDeath.transform)) || (UnityEngine.Object)(object)renderer.GetComponentInParent<Actor>(includeInactive: true) != (UnityEngine.Object)(object)entity)
			{
				continue;
			}
			if (renderer is ParticleSystemRenderer)
			{
				FxParticleSystem componentInParent = renderer.GetComponentInParent<FxParticleSystem>();
				if (componentInParent != null && componentInParent.dontDisableAsPartOfEntityRenderer)
				{
					continue;
				}
			}
			renderers.Add(renderer);
		}
		if (invisibleByDefault)
		{
			DisableRenderersLocal();
		}
		componentsInChildren = ((Component)(object)this).GetComponentsInChildren<SkinnedMeshRenderer>();
		_characterRenderers = componentsInChildren;
		if (_characterRenderers.Length == 0)
		{
			componentsInChildren = ((Component)(object)this).GetComponentsInChildren<MeshRenderer>();
			_characterRenderers = componentsInChildren;
		}
		List<FxGibs> componentsInChildrenNonAlloc = ((Component)(object)this).GetComponentsInChildrenNonAlloc(out ListReturnHandle<FxGibs> handle);
		solidRenderers.Clear();
		solidRenderers.AddRange(model.GetComponentsInChildren<MeshRenderer>(includeInactive: true));
		solidRenderers.AddRange(model.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true));
		for (int num = solidRenderers.Count - 1; num >= 0; num--)
		{
			bool flag = false;
			for (int j = 0; j < componentsInChildrenNonAlloc.Count; j++)
			{
				if (solidRenderers[num].transform.IsSelfOrDescendantOf(componentsInChildrenNonAlloc[j].transform))
				{
					flag = true;
					break;
				}
			}
			MeshFilter component;
			if (flag)
			{
				solidRenderers.RemoveAt(num);
			}
			else if (solidRenderers[num].TryGetComponent<MeshFilter>(out component) && component.mesh != null && component.mesh.name.Contains("Quad"))
			{
				solidRenderers.RemoveAt(num);
			}
		}
		handle.Return();
		ApplyShaderPropertyBlock(force: true);
		_tails = ((Component)(object)this).GetComponentsInChildren<TailAnimator2>();
		_clothes = ((Component)(object)this).GetComponentsInChildren<MagicaCloth>();
		EntityVisualPoint[] componentsInChildren2 = ((Component)(object)this).GetComponentsInChildren<EntityVisualPoint>(true);
		foreach (EntityVisualPoint entityVisualPoint in componentsInChildren2)
		{
			switch (entityVisualPoint.type)
			{
			case EntityVisualPointType.Head:
				_apHead = entityVisualPoint.transform;
				break;
			case EntityVisualPointType.LeftHand:
				_apLeftHand = entityVisualPoint.transform;
				break;
			case EntityVisualPointType.RightHand:
				_apRightHand = entityVisualPoint.transform;
				break;
			case EntityVisualPointType.LeftFoot:
				_apLeftFoot = entityVisualPoint.transform;
				break;
			case EntityVisualPointType.RightFoot:
				_apRightFoot = entityVisualPoint.transform;
				break;
			case EntityVisualPointType.Weapon:
				_apWeapon = entityVisualPoint.transform;
				break;
			case EntityVisualPointType.Muzzle:
				_apMuzzle = entityVisualPoint.transform;
				break;
			case EntityVisualPointType.Center:
				_apCenter = entityVisualPoint.transform;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		PreloadVisuals();
		DoSpawnOnStartClient();
	}

	public void PreloadVisuals()
	{
		if (!_didStartClientEffects)
		{
			_didStartClientEffects = true;
			if (s_entityEffectPrefab == null)
			{
				s_entityEffectPrefab = Resources.Load<GameObject>("Effects/EntityEffect");
			}
			if (s_stunEffectPrefab == null)
			{
				s_stunEffectPrefab = Resources.Load<GameObject>("Effects/Status/Stun");
			}
			UnityEngine.Object.Instantiate(s_entityEffectPrefab, ((Component)(object)this).transform);
			_stunEffect = UnityEngine.Object.Instantiate(s_stunEffectPrefab, ((Component)(object)this).transform);
		}
		_modelWasFreshlyLoaded = model == null;
		if (model == null)
		{
			try
			{
				entity.LoadEntityModelLocal();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		if (!_didBakeModelFx && model != null)
		{
			_didBakeModelFx = true;
			Quality3Levels quality = ((ManagerBase<GraphicsManager>.instance != null) ? ManagerBase<GraphicsManager>.instance.currentEffectQuality : Quality3Levels.High);
			DewEffect.BakeQualityScaling(model.fxLoop, quality, buildPlan: true);
			DewEffect.BakeQualityScaling(model.fxDeath, quality, buildPlan: true);
			DewEffect.BakeQualityScaling(model.fxTakeDamage, quality, buildPlan: true);
			DewEffect.BakeQualityScaling(spawnEffect, quality, buildPlan: true);
			DewEffect.BakeQualityScaling(spawnEffectOnGround, quality, buildPlan: true);
			DewEffect.BakeQualityScaling(_stunEffect, quality, buildPlan: true);
		}
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		entity.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(HandleTakeDamage);
		entity.EntityEvent_OnDeath += new Action<EventInfoKill>(HandleDeath);
	}

	private void HandleDeath(EventInfoKill obj)
	{
		RpcHandleDeath(new GibInfo
		{
			normalizedCurrentDamage = _currentDamage / entity.maxHealth,
			velocity = entity.AI.estimatedVelocityUnclamped,
			yVelocity = entity.Visual.currentYVelocity
		});
	}

	[ClientRpc]
	private void RpcHandleDeath(GibInfo gibInfo)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_GibInfo((NetworkWriter)(object)val, gibInfo);
		((NetworkBehaviour)this).SendRPCInternal("System.Void EntityVisual::RpcHandleDeath(GibInfo)", -1082789707, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private IEnumerator RoutineAnimateDissolve(float delay, float duration)
	{
		_isDissolving = true;
		EntityColorModifier colorMod = GetNewColorModifier();
		float factor = 1f / duration;
		yield return new WaitForSeconds(delay);
		for (float v = 0f; v <= 1f; v += Time.deltaTime * factor)
		{
			colorMod.dissolveAmount = v;
			yield return null;
		}
		colorMod.dissolveAmount = 1f;
		_isDissolving = false;
	}

	public override void OnStop()
	{
		base.OnStop();
		if (solidRenderers == null)
		{
			return;
		}
		foreach (Renderer solidRenderer in solidRenderers)
		{
			if (!(solidRenderer == null))
			{
				solidRenderer.enabled = false;
			}
		}
	}

	public override void OnStopServer()
	{
		base.OnStopServer();
		entity.EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(HandleTakeDamage);
		entity.EntityEvent_OnDeath -= _cachedHandleDeath;
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (!((Behaviour)(object)this).enabled || entity.isSleeping)
		{
			return;
		}
		currentYOffset += currentYVelocity * Time.deltaTime;
		if (currentYOffset > 0f)
		{
			currentYVelocity -= 18.8f * Time.deltaTime;
			if (currentYOffset > 5f && currentYVelocity > 0f)
			{
				currentYVelocity -= 18.8f * Time.deltaTime;
			}
		}
		if (currentYOffset < 0f)
		{
			currentYOffset = 0f;
			currentYVelocity = 0f;
		}
		BeginShaderPropertyBatch();
		try
		{
			DoElementalFrameUpdate();
			DoColorFrameUpdate();
		}
		finally
		{
			EndShaderPropertyBatch();
		}
		DoTransformFrameUpdate();
		DoShellFrameUpdate();
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (!entity.isSleeping)
		{
			DoElementalLogicUpdate();
			DoStunLogicUpdate();
			_currentDamage *= 0.95f;
		}
	}

	[ClientRpc]
	public void SetYOffset(float offset)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteFloat((NetworkWriter)(object)val, offset);
		((NetworkBehaviour)this).SendRPCInternal("System.Void EntityVisual::SetYOffset(System.Single)", -1151011751, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public void SetYOffsetLocal(float offset)
	{
		currentYOffset = offset;
		DoTransformFrameUpdate();
	}

	[ClientRpc]
	public void SetYVelocity(float velocity)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteFloat((NetworkWriter)(object)val, velocity);
		((NetworkBehaviour)this).SendRPCInternal("System.Void EntityVisual::SetYVelocity(System.Single)", -1982953905, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public void SetYVelocityLocal(float velocity)
	{
		currentYVelocity = velocity;
	}

	private void DoStunLogicUpdate()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			bool hasStun = entity.Status.hasStun;
			if (_shouldShowStunned != hasStun)
			{
				Network_shouldShowStunned = hasStun;
			}
		}
	}

	private void OnShouldShowStunnedChanged(bool oldVal, bool newVal)
	{
		if (newVal)
		{
			FxPlay(_stunEffect, entity);
		}
		else
		{
			FxStop(_stunEffect);
		}
	}

	private void HandleTakeDamage(EventInfoDamage info)
	{
		if (info.damage.direction.HasValue)
		{
			_currentDamage += (info.damage.amount + info.damage.discardedAmount) * info.damage.direction.Value;
		}
		if (Time.time - _lastHitEffectRpcTime >= 0.05f)
		{
			_lastHitEffectRpcTime = Time.time;
			RpcShowHitEffect();
		}
		if ((bool)model && (bool)model.fxTakeDamage)
		{
			if (Time.frameCount != _fxTakeDamageFrame)
			{
				_fxTakeDamageFrame = Time.frameCount;
				_fxTakeDamageCountThisFrame = 0;
			}
			if (_fxTakeDamageCountThisFrame < 4)
			{
				_fxTakeDamageCountThisFrame++;
				FxPlayNewNetworked(model.fxTakeDamage, entity);
			}
		}
	}

	[ClientRpc]
	private void RpcShowHitEffect()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void EntityVisual::RpcShowHitEffect()", -1699861942, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void KnockUpLocal(float strength, bool isFriendly)
	{
		if (isFriendly || !entity.Status.hasCrowdControlImmunity)
		{
			if (currentYVelocity > 0.25f)
			{
				strength *= 0.66f;
			}
			if (currentYVelocity > 1f)
			{
				strength *= 0.66f;
			}
			if (currentYVelocity < 0f)
			{
				currentYVelocity *= 0.25f;
			}
			currentYVelocity += strength * 6f;
		}
	}

	void ICleanup.OnCleanup()
	{
		RpcCleanup();
	}

	[ClientRpc]
	private void RpcCleanup()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void EntityVisual::RpcCleanup()", 1675077287, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	internal void DoActionBeforeTeleport()
	{
		entity.Visual.PauseAttachedParticleSystems();
	}

	internal void DoActionAfterTeleport()
	{
		entity.Visual.ResumeAttachedParticleSystems();
		FixTailsAndClothes();
	}

	public void FixTailsAndClothes()
	{
		FixTails();
		FixClothes();
	}

	public void FixTails()
	{
		if (_tails == null)
		{
			return;
		}
		TailAnimator2[] tails = _tails;
		foreach (TailAnimator2 val in tails)
		{
			if (!((UnityEngine.Object)(object)val == null))
			{
				try
				{
					val.User_ReposeTail();
				}
				catch (Exception)
				{
				}
			}
		}
	}

	public void FixClothes()
	{
		if (_clothes == null)
		{
			return;
		}
		MagicaCloth[] clothes = _clothes;
		foreach (MagicaCloth val in clothes)
		{
			if (!((UnityEngine.Object)(object)val == null))
			{
				try
				{
					val.ResetCloth(false);
				}
				catch (Exception)
				{
				}
			}
		}
	}

	internal void PauseAttachedParticleSystems()
	{
		try
		{
			List<ParticleSystem> list = DewPool.GetList(out ListReturnHandle<ParticleSystem> handle);
			foreach (FxAttachToEntity attachedEffect in _attachedEffects)
			{
				if (attachedEffect == null)
				{
					continue;
				}
				attachedEffect.GetComponentsInChildren(list);
				foreach (ParticleSystem item in list)
				{
					if (item.isPlaying && !item.isPaused && item.isEmitting)
					{
						FxParticleSystem componentInParent = ((Component)(object)item).GetComponentInParent<FxParticleSystem>();
						if (!(componentInParent != null) || !componentInParent.dontPauseAttachedWhenTeleport)
						{
							item.Pause(true);
							_pausedSystemsByTeleport.Add(item);
						}
					}
				}
			}
			handle.Return();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	internal void ResumeAttachedParticleSystems()
	{
		try
		{
			foreach (ParticleSystem item in _pausedSystemsByTeleport)
			{
				if ((UnityEngine.Object)(object)item != null)
				{
					item.Play();
				}
			}
			_pausedSystemsByTeleport.Clear();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private void HandleAttachedEffects(bool isRendererOn)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		if (_pausedSystemsByRendererOff == null)
		{
			return;
		}
		if (isRendererOn)
		{
			foreach (ParticleSystem item in _pausedSystemsByRendererOff)
			{
				if (!((UnityEngine.Object)(object)item == null))
				{
					EmissionModule emission = item.emission;
					emission.enabled = true;
				}
			}
			_pausedSystemsByRendererOff.Clear();
			foreach (Renderer item2 in _disabledRenderersByRendererOff)
			{
				if (!(item2 == null))
				{
					item2.enabled = true;
				}
			}
			_disabledRenderersByRendererOff.Clear();
			return;
		}
		for (int num = _attachedEffects.Count - 1; num >= 0; num--)
		{
			FxAttachToEntity fxAttachToEntity = _attachedEffects[num];
			if (fxAttachToEntity == null)
			{
				_attachedEffects.RemoveAt(num);
			}
			else
			{
				ListReturnHandle<ParticleSystem> handle;
				foreach (ParticleSystem item3 in ((Component)fxAttachToEntity).GetComponentsInChildrenNonAlloc(out handle))
				{
					FxParticleSystem componentInParent = ((Component)(object)item3).GetComponentInParent<FxParticleSystem>();
					if (item3.isPlaying && !item3.isPaused && item3.isEmitting)
					{
						if (componentInParent == null || componentInParent.dontPauseAttachedWhenRendererDisabled)
						{
							continue;
						}
						EmissionModule emission2 = item3.emission;
						if (!emission2.enabled)
						{
							continue;
						}
						emission2.enabled = false;
						_pausedSystemsByRendererOff.Add(item3);
					}
					if (componentInParent != null && componentInParent.hideAttachedWhenRendererDisabled)
					{
						ParticleSystemRenderer component = ((Component)(object)item3).GetComponent<ParticleSystemRenderer>();
						if (((Renderer)(object)component).enabled)
						{
							((Renderer)(object)component).enabled = false;
							_disabledRenderersByRendererOff.Add((Renderer)(object)component);
						}
					}
				}
				handle.Return();
			}
		}
	}

	public EntityColorModifier GetNewColorModifier()
	{
		EntityColorModifier entityColorModifier = ((_colorModifierPool.Count > 0) ? _colorModifierPool.Pop() : new EntityColorModifier());
		entityColorModifier.ResetForReuse();
		_colorModifiers.Add(entityColorModifier);
		entityColorModifier._parent = entity;
		return entityColorModifier;
	}

	internal void RemoveColorModifier(EntityColorModifier modifier)
	{
		if (_colorModifiers.Remove(modifier))
		{
			_isColorModifiersDirty = true;
			modifier.ResetForReuse();
			_colorModifierPool.Push(modifier);
		}
	}

	internal void DirtyColorModifiers()
	{
		_isColorModifiersDirty = true;
	}

	private void DoColorFrameUpdate()
	{
		if (!_isColorModifiersDirty)
		{
			return;
		}
		_isColorModifiersDirty = false;
		Color white = Color.white;
		Color black = Color.black;
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 1f;
		Color value = (model.hasGoldDissolve ? new Color(1f, 0.8f, 0.3f) : new Color(0.4f, 0.85f, 1f));
		foreach (EntityColorModifier colorModifier in _colorModifiers)
		{
			white *= colorModifier.baseColor;
			float num5 = GetMagnitude(colorModifier.emission);
			if (num5 > num)
			{
				num = num5;
			}
			black += colorModifier.emission;
			num2 += num5;
			num3 = Mathf.Max(colorModifier.dissolveAmount, num3);
			num4 = Mathf.Min(colorModifier.opacity, num4);
			if (colorModifier.dissolveColor.HasValue)
			{
				value = colorModifier.dissolveColor.Value;
			}
		}
		black = ((!(num2 < 0.0001f)) ? (black / num2 * num) : Color.black);
		num3 = Mathf.Clamp(num3, 0f, 1f);
		SetShaderPropertyLocal(CMBaseColor, white);
		SetShaderPropertyLocal(CMEmission, black);
		SetShaderPropertyLocal(CMOpacity, num4);
		SetShaderPropertyLocal(DissolveStrength, num3);
		SetShaderPropertyLocal(CMDissolveColor, value);
		static float GetMagnitude(Color c)
		{
			return Vector3.Magnitude(new Vector3(c.r, c.g, c.b));
		}
	}

	private void DoElementalLogicUpdate()
	{
		if (!entity.Status.isDead)
		{
			bool flag = entity is BossMonster;
			ref float eFireTarget = ref _eFireTarget;
			float num;
			if (entity.Status.fireStack <= 0)
			{
				num = 0f;
			}
			else
			{
				num = (flag ? 0.4f : 0.8f);
			}
			eFireTarget = num;
			ref float eColdTarget = ref _eColdTarget;
			float num2;
			if (entity.Status.hasCold)
			{
				num2 = (flag ? 0.4f : 1f);
			}
			else
			{
				num2 = 0f;
			}
			eColdTarget = num2;
			_eVoidTarget = (float)entity.Status.darkStack * (flag ? 0.4f : 1f);
			_eLightTarget = (float)entity.Status.lightStack * (flag ? 0.4f : 1f);
		}
	}

	private void DoElementalFrameUpdate()
	{
		if (Mathf.Abs(_eFireTarget - _eFire) > 0.0001f)
		{
			_eFire = Mathf.MoveTowards(_eFire, _eFireTarget, 8f * Time.deltaTime);
			SetShaderPropertyLocal(FireStrength, _eFire);
		}
		if (Mathf.Abs(_eColdTarget - _eCold) > 0.0001f)
		{
			_eCold = Mathf.MoveTowards(_eCold, _eColdTarget, 8f * Time.deltaTime);
			SetShaderPropertyLocal(ColdStrength, _eCold);
		}
		if (Mathf.Abs(_eVoidTarget - _eVoid) > 0.0001f)
		{
			_eVoid = Mathf.MoveTowards(_eVoid, _eVoidTarget, 8f * Time.deltaTime);
			SetShaderPropertyLocal(VoidStrength, _eVoid);
		}
		if (Mathf.Abs(_eLightTarget - _eLight) > 0.0001f)
		{
			_eLight = Mathf.MoveTowards(_eLight, _eLightTarget, 8f * Time.deltaTime);
			SetShaderPropertyLocal(LightStrength, _eLight);
		}
	}

	private static int GetPropertyId(string name)
	{
		if (_shaderPropertyIdCache.TryGetValue(name, out var value))
		{
			return value;
		}
		int num = Shader.PropertyToID(name);
		_shaderPropertyIdCache[name] = num;
		return num;
	}

	private void BeginShaderPropertyBatch()
	{
		_shaderPropertyBatchDepth++;
	}

	private void EndShaderPropertyBatch()
	{
		if (_shaderPropertyBatchDepth <= 0)
		{
			Debug.LogWarning($"Tried to end shader property batch without matching begin on {this}");
			_shaderPropertyBatchDepth = 0;
			return;
		}
		_shaderPropertyBatchDepth--;
		if (_shaderPropertyBatchDepth == 0)
		{
			ApplyShaderPropertyBlock();
		}
	}

	private void MarkShaderPropertyBlockDirty()
	{
		_isShaderPropertyBlockDirty = true;
		if (_shaderPropertyBatchDepth == 0)
		{
			ApplyShaderPropertyBlock();
		}
	}

	private void ApplyShaderPropertyBlock(bool force = false)
	{
		if (!force && !_isShaderPropertyBlockDirty)
		{
			return;
		}
		_isShaderPropertyBlockDirty = false;
		if (solidRenderers.Count == 0)
		{
			return;
		}
		_shaderPropertyBlock.Clear();
		foreach (KeyValuePair<int, float> shaderFloatProperty in _shaderFloatProperties)
		{
			_shaderPropertyBlock.SetFloat(shaderFloatProperty.Key, shaderFloatProperty.Value);
		}
		foreach (KeyValuePair<int, Color> shaderColorProperty in _shaderColorProperties)
		{
			_shaderPropertyBlock.SetColor(shaderColorProperty.Key, shaderColorProperty.Value);
		}
		for (int i = 0; i < solidRenderers.Count; i++)
		{
			Renderer renderer = solidRenderers[i];
			if (!(renderer == null))
			{
				renderer.SetPropertyBlock(_shaderPropertyBlock);
			}
		}
	}

	[ClientRpc]
	public void SetShaderProperty(string key, float value)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteString((NetworkWriter)(object)val, key);
		NetworkWriterExtensions.WriteFloat((NetworkWriter)(object)val, value);
		((NetworkBehaviour)this).SendRPCInternal("System.Void EntityVisual::SetShaderProperty(System.String,System.Single)", -597797103, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public void SetShaderPropertyLocal(string key, float value)
	{
		int propertyId = GetPropertyId(key);
		SetShaderPropertyLocal(propertyId, value);
	}

	public void SetShaderPropertyLocal(int propertyId, float value)
	{
		_shaderColorProperties.Remove(propertyId);
		_shaderFloatProperties[propertyId] = value;
		MarkShaderPropertyBlockDirty();
	}

	[ClientRpc]
	public void SetShaderProperty(string key, Color value)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteString((NetworkWriter)(object)val, key);
		NetworkWriterExtensions.WriteColor((NetworkWriter)(object)val, value);
		((NetworkBehaviour)this).SendRPCInternal("System.Void EntityVisual::SetShaderProperty(System.String,UnityEngine.Color)", -1542144884, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public void SetShaderPropertyLocal(string key, Color value)
	{
		int propertyId = GetPropertyId(key);
		SetShaderPropertyLocal(propertyId, value);
	}

	public void SetShaderPropertyLocal(int propertyId, Color value)
	{
		_shaderFloatProperties.Remove(propertyId);
		_shaderColorProperties[propertyId] = value;
		MarkShaderPropertyBlockDirty();
	}

	[ClientRpc]
	public void KnockUp(float strength, bool isFriendly)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteFloat((NetworkWriter)(object)val, strength);
		NetworkWriterExtensions.WriteBool((NetworkWriter)(object)val, isFriendly);
		((NetworkBehaviour)this).SendRPCInternal("System.Void EntityVisual::KnockUp(System.Single,System.Boolean)", 242751419, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	public void KnockUp(KnockUpStrength strength, bool isFriendly)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_KnockUpStrength((NetworkWriter)(object)val, strength);
		NetworkWriterExtensions.WriteBool((NetworkWriter)(object)val, isFriendly);
		((NetworkBehaviour)this).SendRPCInternal("System.Void EntityVisual::KnockUp(KnockUpStrength,System.Boolean)", 628801700, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Server]
	public void DisableRenderers()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityVisual::DisableRenderers()' called when server was not active");
		}
		else
		{
			Network_rendererOffCounterNetworked = _rendererOffCounterNetworked + 1;
		}
	}

	public void DisableRenderersLocal()
	{
		_rendererOffCounterLocal++;
		UpdateRendererOff(0, 0);
	}

	[Server]
	public void EnableRenderers()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityVisual::EnableRenderers()' called when server was not active");
		}
		else
		{
			Network_rendererOffCounterNetworked = _rendererOffCounterNetworked - 1;
		}
	}

	public void EnableRenderersLocal()
	{
		_rendererOffCounterLocal--;
		UpdateRendererOff(0, 0);
	}

	public void AddSharedMaterialLocal(Material material)
	{
		foreach (Renderer solidRenderer in solidRenderers)
		{
			Material[] array = new Material[solidRenderer.sharedMaterials.Length + 1];
			solidRenderer.sharedMaterials.CopyTo(array, 0);
			array[array.Length - 1] = material;
			solidRenderer.sharedMaterials = array;
		}
		ApplyShaderPropertyBlock(force: true);
	}

	public void RemoveMaterialLocal(Material material)
	{
		bool flag = false;
		foreach (Renderer solidRenderer in solidRenderers)
		{
			if (!solidRenderer.sharedMaterials.Contains(material))
			{
				return;
			}
			List<Material> list = solidRenderer.sharedMaterials.ToList();
			list.RemoveAll((Material m) => m == material);
			solidRenderer.sharedMaterials = list.ToArray();
			flag = true;
		}
		if (flag)
		{
			ApplyShaderPropertyBlock(force: true);
		}
		if (!flag)
		{
			Debug.LogWarning($"Couldn't find {material} from {this}");
		}
	}

	public Bounds GetBodyBounds()
	{
		Vector3 vector = ((modelTransform != null) ? modelTransform.position : _transform.position);
		int boundsCacheMaxAgeFrames = BoundsCacheMaxAgeFrames;
		if (Time.frameCount - _bodyBoundsCacheFrame >= boundsCacheMaxAgeFrames)
		{
			_bodyBoundsCacheFrame = Time.frameCount;
			_bodyBoundsCachePosition = vector;
			_cachedBodyBounds = GetBodyBounds_Imp();
		}
		else if ((vector - _bodyBoundsCachePosition).sqrMagnitude > 0.0025000002f)
		{
			_cachedBodyBounds.center += vector - _bodyBoundsCachePosition;
			_bodyBoundsCachePosition = vector;
		}
		if (boundsCacheMaxAgeFrames <= 1)
		{
			return _cachedBodyBounds;
		}
		return GetSmoothedBodyBounds(vector);
	}

	private Bounds GetSmoothedBodyBounds(Vector3 entityPos)
	{
		Vector3 vector = _cachedBodyBounds.center - entityPos;
		Vector3 size = _cachedBodyBounds.size;
		int frameCount = Time.frameCount;
		if (_bodyBoundsSmoothedFrame < 0 || (vector - _bodyBoundsSmoothedOffset).sqrMagnitude > 4f)
		{
			_bodyBoundsSmoothedOffset = vector;
			_bodyBoundsSmoothedSize = size;
		}
		else if (frameCount != _bodyBoundsSmoothedFrame)
		{
			float t = 1f - Mathf.Exp((0f - Time.deltaTime) / 0.08f);
			_bodyBoundsSmoothedOffset = Vector3.Lerp(_bodyBoundsSmoothedOffset, vector, t);
			_bodyBoundsSmoothedSize = Vector3.Lerp(_bodyBoundsSmoothedSize, size, t);
		}
		_bodyBoundsSmoothedFrame = frameCount;
		return new Bounds(entityPos + _bodyBoundsSmoothedOffset, _bodyBoundsSmoothedSize);
	}

	private Bounds GetBodyBounds_Imp()
	{
		if (!model || model.bodyRenderers == null || model.bodyRenderers.Length == 0)
		{
			return GetRenderBounds();
		}
		Bounds bounds = model.bodyRenderers[0].bounds;
		for (int i = 1; i < model.bodyRenderers.Length; i++)
		{
			bounds.Encapsulate(model.bodyRenderers[i].bounds);
		}
		return bounds;
	}

	public Bounds GetRenderBounds()
	{
		Vector3 vector = ((modelTransform != null) ? modelTransform.position : ((Component)(object)this).transform.position);
		if (Time.frameCount - _renderBoundsCacheFrame >= BoundsCacheMaxAgeFrames)
		{
			_renderBoundsCacheFrame = Time.frameCount;
			_renderBoundsCachePosition = vector;
			_cachedRenderBounds = GetRenderBounds_Imp();
		}
		else if ((vector - _renderBoundsCachePosition).sqrMagnitude > 0.0025000002f)
		{
			_cachedRenderBounds.center += vector - _renderBoundsCachePosition;
			_renderBoundsCachePosition = vector;
		}
		return _cachedRenderBounds;
	}

	private Bounds GetRenderBounds_Imp()
	{
		if (_characterRenderers == null || _characterRenderers.Length == 0 || _characterRenderers[0] == null)
		{
			return new Bounds(((Component)(object)this).transform.position, Vector3.zero);
		}
		Bounds bounds = _characterRenderers[0].bounds;
		for (int i = 1; i < _characterRenderers.Length; i++)
		{
			if (!(_characterRenderers[i] == null))
			{
				bounds.Encapsulate(_characterRenderers[i].bounds);
			}
		}
		return bounds;
	}

	public Vector3 GetBasePosition()
	{
		return ((Component)(object)this).transform.position + currentYOffset * Vector3.up;
	}

	public Vector3 GetCenterPosition()
	{
		if (_apCenter != null)
		{
			return _apCenter.position;
		}
		return GetBodyBounds().center;
	}

	public Vector3 GetAbovePosition()
	{
		Bounds bodyBounds = GetBodyBounds();
		return bodyBounds.center + Vector3.up * bodyBounds.extents.y;
	}

	public Vector3 GetConversationPivotPosition()
	{
		if (!model.conversationPivotPosition)
		{
			return GetAbovePosition();
		}
		return model.conversationPivotPosition.position;
	}

	public Vector3 GetMuzzlePosition()
	{
		if (_apMuzzle == null)
		{
			return GetCenterPosition();
		}
		return _apMuzzle.transform.position;
	}

	public Quaternion GetMuzzleRotation()
	{
		if (_apMuzzle == null)
		{
			return ((Component)(object)this).transform.rotation;
		}
		return _apMuzzle.transform.rotation;
	}

	public Vector3 GetWeaponPosition()
	{
		if (_apWeapon == null)
		{
			return GetCenterPosition();
		}
		return _apWeapon.transform.position;
	}

	public Quaternion GetWeaponRotation()
	{
		if (_apWeapon == null)
		{
			return ((Component)(object)this).transform.rotation;
		}
		return _apWeapon.transform.rotation;
	}

	public Vector3 GetBonePosition(HumanBodyBones bone)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Invalid comparison between Unknown and I4
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Invalid comparison between Unknown and I4
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Invalid comparison between Unknown and I4
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Invalid comparison between Unknown and I4
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		if ((int)bone == 10 && _apHead != null)
		{
			return _apHead.position;
		}
		if ((int)bone == 17 && _apLeftHand != null)
		{
			return _apLeftHand.position;
		}
		if ((int)bone == 18 && _apRightHand != null)
		{
			return _apRightHand.position;
		}
		if ((int)bone == 5 && _apLeftFoot != null)
		{
			return _apLeftFoot.position;
		}
		if ((int)bone == 6 && _apRightFoot != null)
		{
			return _apRightFoot.position;
		}
		if ((UnityEngine.Object)(object)entity.Animation.animator == null || !entity.Animation.animator.isHuman)
		{
			return GetCenterPosition();
		}
		Transform boneTransform = entity.Animation.animator.GetBoneTransform(bone);
		if (boneTransform == null)
		{
			return GetCenterPosition();
		}
		return boneTransform.position;
	}

	public Quaternion GetBoneRotation(HumanBodyBones bone)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Invalid comparison between Unknown and I4
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Invalid comparison between Unknown and I4
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Invalid comparison between Unknown and I4
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Invalid comparison between Unknown and I4
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		if ((int)bone == 10 && _apHead != null)
		{
			return _apHead.rotation;
		}
		if ((int)bone == 17 && _apLeftHand != null)
		{
			return _apLeftHand.rotation;
		}
		if ((int)bone == 18 && _apRightHand != null)
		{
			return _apRightHand.rotation;
		}
		if ((int)bone == 5 && _apLeftFoot != null)
		{
			return _apLeftFoot.rotation;
		}
		if ((int)bone == 6 && _apRightFoot != null)
		{
			return _apRightFoot.rotation;
		}
		if ((UnityEngine.Object)(object)entity.Animation.animator == null || !entity.Animation.animator.isHuman)
		{
			return ((Component)(object)this).transform.rotation;
		}
		Transform boneTransform = entity.Animation.animator.GetBoneTransform(bone);
		if (boneTransform == null)
		{
			return ((Component)(object)this).transform.rotation;
		}
		return boneTransform.rotation;
	}

	[Server]
	public void HideGroundMarker()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityVisual::HideGroundMarker()' called when server was not active");
		}
		else
		{
			Network_groundMarkerHiddenCounterNetworked = _groundMarkerHiddenCounterNetworked + 1;
		}
	}

	public void HideGroundMarkerLocal()
	{
		_groundMarkerHiddenCounterLocal++;
		UpdateGroundMarkerHidden(0, 0);
	}

	[Server]
	public void ShowGroundMarker()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityVisual::ShowGroundMarker()' called when server was not active");
		}
		else
		{
			Network_groundMarkerHiddenCounterNetworked = _groundMarkerHiddenCounterNetworked - 1;
		}
	}

	public void ShowGroundMarkerLocal()
	{
		_groundMarkerHiddenCounterLocal--;
		UpdateGroundMarkerHidden(0, 0);
	}

	public EntityShellModifier GetNewShellModifier()
	{
		EntityShellModifier entityShellModifier = new EntityShellModifier();
		_shellModifiers.Add(entityShellModifier);
		entityShellModifier._parent = entity;
		return entityShellModifier;
	}

	internal void RemoveShellModifier(EntityShellModifier modifier)
	{
		if (_shellModifiers.Remove(modifier))
		{
			_isShellModifiersDirty = true;
		}
	}

	internal void DirtyShellModifiers()
	{
		_isShellModifiersDirty = true;
	}

	private void DoShellFrameUpdate()
	{
		if (!_isShellModifiersDirty)
		{
			return;
		}
		_isShellModifiersDirty = false;
		Color black = Color.black;
		float num = 0f;
		foreach (EntityShellModifier shellModifier in _shellModifiers)
		{
			black += shellModifier.color * shellModifier.opacity;
			num += shellModifier.opacity;
		}
		if (num > 0.001f)
		{
			black /= num;
			black.a = 0f;
			if ((bool)highlight && (bool)(UnityEngine.Object)(object)highlight.meshHighlight)
			{
				highlight.meshHighlight.glowPasses[0].color = black * 1f;
			}
		}
		if ((bool)highlight && (bool)(UnityEngine.Object)(object)highlight.meshHighlight)
		{
			highlight.meshHighlight.glow = Mathf.Clamp01(num);
		}
	}

	[Server]
	public void SkipSpawning()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityVisual::SkipSpawning()' called when server was not active");
		}
		else
		{
			if (!isSpawning)
			{
				return;
			}
			NetworkskipSpawning = true;
			entity.Control.CancelOngoingChannels();
			if (_spawnProtections != null)
			{
				StatusEffect[] spawnProtections = _spawnProtections;
				foreach (StatusEffect statusEffect in spawnProtections)
				{
					if (!statusEffect.IsNullOrInactive())
					{
						statusEffect.Destroy();
					}
				}
			}
			RpcSkipSpawning();
		}
	}

	[ClientRpc]
	private void RpcSkipSpawning()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void EntityVisual::RpcSkipSpawning()", -486594075, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private IEnumerator WaitForSecondsSpawn(float duration)
	{
		float startTime = Time.time;
		while (!skipSpawning && Time.time - startTime < duration)
		{
			yield return null;
		}
	}

	private void DoSpawnOnStartClient()
	{
		_needToDoSpawning = false;
		if (!_modelWasFreshlyLoaded && model != null && model.fxLoop != null)
		{
			((MonoBehaviour)(object)this).StartCoroutine(ReplayLoopEffect());
		}
		if (skipSpawning)
		{
			return;
		}
		if (model != null)
		{
			FSpineAnimator spineAnimator = model.GetComponentInChildren<FSpineAnimator>();
			if ((UnityEngine.Object)(object)spineAnimator != null && ((Behaviour)(object)spineAnimator).enabled)
			{
				((Behaviour)(object)spineAnimator).enabled = false;
				ClientEvent_OnSpawnComplete += (Action)(() =>
				{
					if ((UnityEngine.Object)(object)spineAnimator != null)
					{
						((Behaviour)(object)spineAnimator).enabled = true;
					}
				});
			}
		}
		if (spawnEffect != null)
		{
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		if (spawnEffectOnGround != null)
		{
			((MonoBehaviour)(object)this).StartCoroutine(Routine2());
		}
		if (spawnDuration + dazeAfterSpawnDuration > 0f && ((NetworkBehaviour)this).isServer)
		{
			entity.Control.StartDaze(spawnDuration + dazeAfterSpawnDuration);
		}
		if (spawnDuration > 0f)
		{
			if (((NetworkBehaviour)this).isServer && invulnerableWhileSpawning)
			{
				_spawnProtections = new StatusEffect[4]
				{
					entity.CreateBasicEffect(entity, new InvulnerableEffect(), spawnDuration),
					entity.CreateBasicEffect(entity, new UncollidableEffect(), spawnDuration),
					entity.CreateBasicEffect(entity, new UntargetableEffect(), spawnDuration),
					entity.CreateBasicEffect(entity, new InvisibleEffect
					{
						ignoreReveal = true
					}, spawnDuration)
				};
			}
			((MonoBehaviour)(object)this).StartCoroutine(SetIsSpawningFlag());
		}
		if (doPositionOffset || doScaling)
		{
			_spawnModifier = GetNewTransformModifier();
			((MonoBehaviour)(object)this).StartCoroutine(Animate());
		}
		if (skipSpawning)
		{
			ClientEvent_OnSpawnComplete?.Invoke();
		}
		else
		{
			((MonoBehaviour)(object)this).StartCoroutine(Routine3());
		}
		IEnumerator Animate()
		{
			for (float t = 0f; t < spawnDuration; t += Time.deltaTime)
			{
				float time = t / spawnDuration;
				if (!isSpawning || skipSpawning)
				{
					break;
				}
				if (doPositionOffset)
				{
					Vector3 localOffset = new Vector3(spawnXOffset.Evaluate(time), spawnYOffset.Evaluate(time), spawnZOffset.Evaluate(time));
					_spawnModifier.localOffset = localOffset;
				}
				if (doScaling)
				{
					float num = spawnXScale.Evaluate(time);
					Vector3 scaleMultiplier = (useSeparateAxis ? new Vector3(num, spawnYScale.Evaluate(time), spawnZScale.Evaluate(time)) : new Vector3(num, num, num));
					_spawnModifier.scaleMultiplier = scaleMultiplier;
				}
				if (t == 0f)
				{
					DoTransformFrameUpdate();
				}
				yield return null;
			}
			_spawnModifier.Stop();
			_spawnModifier = null;
		}
		IEnumerator ReplayLoopEffect()
		{
			yield return null;
			if (!entity.IsNullInactiveDeadOrKnockedOut())
			{
				FxPlay(model.fxLoop, entity);
			}
		}
		IEnumerator Routine()
		{
			if (delaySpawnEffectUntilSpawned && spawnDuration > 0f)
			{
				yield return WaitForSecondsSpawn(spawnDuration);
			}
			else
			{
				yield return null;
			}
			if (!entity.IsNullInactiveDeadOrKnockedOut())
			{
				FxPlay(spawnEffect, entity);
			}
		}
		IEnumerator Routine2()
		{
			if (delaySpawnEffectOnGroundTilSpawned && spawnDuration > 0f)
			{
				yield return WaitForSecondsSpawn(spawnDuration);
			}
			else
			{
				yield return null;
			}
			if (!entity.IsNullInactiveDeadOrKnockedOut())
			{
				FxPlay(spawnEffectOnGround, Dew.GetPositionOnGround(entity.position), entity.rotation);
			}
		}
		IEnumerator Routine3()
		{
			yield return WaitForSecondsSpawn(spawnDuration);
			yield return null;
			ClientEvent_OnSpawnComplete?.Invoke();
		}
		IEnumerator SetIsSpawningFlag()
		{
			isSpawning = true;
			for (float t = 0f; t < spawnDuration; t += Time.deltaTime)
			{
				if (entity.IsNullInactiveDeadOrKnockedOut() || entity.Control.isDisplacing || skipSpawning)
				{
					if (((NetworkBehaviour)this).isServer && spawnAnim != null)
					{
						entity.Animation.StopAbilityAnimation(spawnAnim);
					}
					isSpawning = false;
					yield break;
				}
				yield return null;
			}
			isSpawning = false;
		}
	}

	private void InitEntityTransform()
	{
		if ((UnityEngine.Object)(object)entity.Animation.animator != null)
		{
			modelTransform = ((Component)(object)entity.Animation.animator).transform;
		}
		else
		{
			modelTransform = model.transform;
		}
		if (modelTransform != null)
		{
			_modelOriginalLocalPosition = modelTransform.localPosition;
			_modelOriginalLocalRotation = modelTransform.localRotation;
			_modelOriginalLocalScale = modelTransform.localScale;
		}
		_hasLastWrittenModelTransform = false;
	}

	public EntityTransformModifier GetNewTransformModifier()
	{
		EntityTransformModifier entityTransformModifier = ((_transformModifierPool.Count > 0) ? _transformModifierPool.Pop() : new EntityTransformModifier());
		entityTransformModifier.ResetForReuse();
		_transformModifiers.Add(entityTransformModifier);
		entityTransformModifier._parent = entity;
		return entityTransformModifier;
	}

	internal void RemoveTransformModifier(EntityTransformModifier modifier)
	{
		if (_transformModifiers.Remove(modifier))
		{
			_isTransformModifiersDirty = true;
			modifier.ResetForReuse();
			_transformModifierPool.Push(modifier);
		}
	}

	internal void DirtyTransformModifiers()
	{
		_isTransformModifiersDirty = true;
	}

	private void DoTransformFrameUpdate()
	{
		if (modelTransform == null || disableModelTransformUpdate)
		{
			return;
		}
		Transform transform = ((Component)(object)this).transform;
		Vector3 position = transform.position;
		Quaternion rotation = transform.rotation;
		float num = currentYOffset;
		if (!_isTransformModifiersDirty && _hasLastWrittenModelTransform && position == _lastRootPosition && rotation == _lastRootRotation && num == _lastYOffset && modelTransform.position == _lastWrittenModelPosition && modelTransform.localRotation == _lastWrittenModelLocalRotation && modelTransform.localScale == _lastWrittenModelLocalScale)
		{
			return;
		}
		bool flag = false;
		if (_isTransformModifiersDirty)
		{
			bool flag2 = etScaleMultiplier.sqrMagnitude < 0.25f;
			etWorldOffset = Vector3.zero;
			etLocalOffset = Vector3.zero;
			etScaleMultiplier = Vector3.one;
			etRotation = Quaternion.identity;
			foreach (EntityTransformModifier transformModifier in _transformModifiers)
			{
				etWorldOffset += transformModifier.worldOffset;
				etLocalOffset += transformModifier.localOffset;
				etScaleMultiplier = Vector3.Scale(etScaleMultiplier, transformModifier.scaleMultiplier);
				etRotation *= transformModifier.rotation;
			}
			if (flag2 && etScaleMultiplier.sqrMagnitude > 0.25f)
			{
				flag = true;
			}
			_isTransformModifiersDirty = false;
		}
		Vector3 position2 = modelTransform.position;
		Vector3 position3 = position + rotation * (_modelOriginalLocalPosition + etLocalOffset) + etWorldOffset + Vector3.up * num;
		modelTransform.position = position3;
		modelTransform.localRotation = etRotation * _modelOriginalLocalRotation;
		Vector3 vector = etScaleMultiplier;
		if (vector.x < 0.1f || vector.y < 0.1f || vector.z < 0.1f)
		{
			if (!_didHideModelDueToSmallSize)
			{
				_didHideModelDueToSmallSize = true;
				DisableRenderersLocal();
			}
			if (vector.x < 0.1f)
			{
				vector = vector.WithX(0.1f);
			}
			if (vector.y < 0.1f)
			{
				vector = vector.WithY(0.1f);
			}
			if (vector.z < 0.1f)
			{
				vector = vector.WithZ(0.1f);
			}
		}
		else if (_didHideModelDueToSmallSize)
		{
			_didHideModelDueToSmallSize = false;
			EnableRenderersLocal();
		}
		modelTransform.localScale = Vector3.Scale(_modelOriginalLocalScale, vector);
		_lastRootPosition = position;
		_lastRootRotation = rotation;
		_lastYOffset = num;
		_lastWrittenModelPosition = modelTransform.position;
		_lastWrittenModelLocalRotation = modelTransform.localRotation;
		_lastWrittenModelLocalScale = modelTransform.localScale;
		_hasLastWrittenModelTransform = true;
		if (!flag && Vector3.Distance(position2, modelTransform.position) > 1f)
		{
			flag = true;
		}
		if (flag)
		{
			FixTailsAndClothes();
		}
	}

	public EntityVisual()
	{
		_Mirror_SyncVarHookDelegate__rendererOffCounterNetworked = UpdateRendererOff;
		_Mirror_SyncVarHookDelegate__groundMarkerHiddenCounterNetworked = UpdateGroundMarkerHidden;
		_Mirror_SyncVarHookDelegate__shouldShowStunned = OnShouldShowStunnedChanged;
	}

	static EntityVisual()
	{
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Expected Obj, but got Unknown
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Expected Obj, but got Unknown
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Expected Obj, but got Unknown
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Expected Obj, but got Unknown
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Expected Obj, but got Unknown
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Expected Obj, but got Unknown
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Expected Obj, but got Unknown
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Expected Obj, but got Unknown
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Expected Obj, but got Unknown
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Expected Obj, but got Unknown
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Expected Obj, but got Unknown
		DissolveStrength = Shader.PropertyToID("_DissolveStrength");
		CMBaseColor = Shader.PropertyToID("_CMBaseColor");
		CMEmission = Shader.PropertyToID("_CMEmission");
		CMOpacity = Shader.PropertyToID("_CMOpacity");
		CMDissolveColor = Shader.PropertyToID("_CMDissolveColor");
		_colorModifierPool = new Stack<EntityColorModifier>();
		FireStrength = Shader.PropertyToID("_FireStrength");
		ColdStrength = Shader.PropertyToID("_ColdStrength");
		LightStrength = Shader.PropertyToID("_LightStrength");
		VoidStrength = Shader.PropertyToID("_VoidStrength");
		_shaderPropertyIdCache = new Dictionary<string, int>();
		_transformModifierPool = new Stack<EntityTransformModifier>();
		RemoteProcedureCalls.RegisterRpc(typeof(EntityVisual), "System.Void EntityVisual::LoadModelDefault()", (RemoteCallDelegate)InvokeUserCode_LoadModelDefault);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityVisual), "System.Void EntityVisual::RpcHandleDeath(GibInfo)", (RemoteCallDelegate)InvokeUserCode_RpcHandleDeath__GibInfo);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityVisual), "System.Void EntityVisual::SetYOffset(System.Single)", (RemoteCallDelegate)InvokeUserCode_SetYOffset__Single);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityVisual), "System.Void EntityVisual::SetYVelocity(System.Single)", (RemoteCallDelegate)InvokeUserCode_SetYVelocity__Single);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityVisual), "System.Void EntityVisual::RpcShowHitEffect()", (RemoteCallDelegate)InvokeUserCode_RpcShowHitEffect);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityVisual), "System.Void EntityVisual::RpcCleanup()", (RemoteCallDelegate)InvokeUserCode_RpcCleanup);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityVisual), "System.Void EntityVisual::SetShaderProperty(System.String,System.Single)", (RemoteCallDelegate)InvokeUserCode_SetShaderProperty__String__Single);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityVisual), "System.Void EntityVisual::SetShaderProperty(System.String,UnityEngine.Color)", (RemoteCallDelegate)InvokeUserCode_SetShaderProperty__String__Color);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityVisual), "System.Void EntityVisual::KnockUp(System.Single,System.Boolean)", (RemoteCallDelegate)InvokeUserCode_KnockUp__Single__Boolean);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityVisual), "System.Void EntityVisual::KnockUp(KnockUpStrength,System.Boolean)", (RemoteCallDelegate)InvokeUserCode_KnockUp__KnockUpStrength__Boolean);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityVisual), "System.Void EntityVisual::RpcSkipSpawning()", (RemoteCallDelegate)InvokeUserCode_RpcSkipSpawning);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_LoadModelDefault()
	{
		LoadModelDefaultLocal();
	}

	protected static void InvokeUserCode_LoadModelDefault(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC LoadModelDefault called on server.");
		}
		else
		{
			((EntityVisual)(object)obj).UserCode_LoadModelDefault();
		}
	}

	protected void UserCode_RpcHandleDeath__GibInfo(GibInfo gibInfo)
	{
		if (model == null)
		{
			return;
		}
		switch (model.deathBehavior)
		{
		case EntityDeathBehavior.HideModel:
			DisableRenderersLocal();
			break;
		case EntityDeathBehavior.Dissolve:
			((MonoBehaviour)(object)this).StartCoroutine(RoutineAnimateDissolve(model.dissolveDelay, model.dissolveDuration));
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case EntityDeathBehavior.None:
			break;
		}
		if (model.fxDeath != null)
		{
			FxGibs[] componentsInChildren = model.fxDeath.GetComponentsInChildren<FxGibs>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].info = gibInfo;
			}
			FxPlayDetached(model.fxDeath, entity);
		}
		if (model.fxLoop != null)
		{
			FxStop(model.fxLoop);
		}
	}

	protected static void InvokeUserCode_RpcHandleDeath__GibInfo(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcHandleDeath called on server.");
		}
		else
		{
			((EntityVisual)(object)obj).UserCode_RpcHandleDeath__GibInfo(GeneratedNetworkCode._Read_GibInfo(reader));
		}
	}

	protected void UserCode_SetYOffset__Single(float offset)
	{
		SetYOffsetLocal(offset);
	}

	protected static void InvokeUserCode_SetYOffset__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC SetYOffset called on server.");
		}
		else
		{
			((EntityVisual)(object)obj).UserCode_SetYOffset__Single(NetworkReaderExtensions.ReadFloat(reader));
		}
	}

	protected void UserCode_SetYVelocity__Single(float velocity)
	{
		SetYVelocityLocal(velocity);
	}

	protected static void InvokeUserCode_SetYVelocity__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC SetYVelocity called on server.");
		}
		else
		{
			((EntityVisual)(object)obj).UserCode_SetYVelocity__Single(NetworkReaderExtensions.ReadFloat(reader));
		}
	}

	protected void UserCode_RpcShowHitEffect()
	{
		highlight.ShowHit();
	}

	protected static void InvokeUserCode_RpcShowHitEffect(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcShowHitEffect called on server.");
		}
		else
		{
			((EntityVisual)(object)obj).UserCode_RpcShowHitEffect();
		}
	}

	protected void UserCode_RpcCleanup()
	{
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			while (_isDissolving && (UnityEngine.Object)(object)this != null)
			{
				yield return null;
			}
			if (!((UnityEngine.Object)(object)this == null))
			{
				if (model.fxLoop != null)
				{
					FxStop(model.fxLoop);
				}
				DisableRenderersLocal();
			}
		}
	}

	protected static void InvokeUserCode_RpcCleanup(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcCleanup called on server.");
		}
		else
		{
			((EntityVisual)(object)obj).UserCode_RpcCleanup();
		}
	}

	protected void UserCode_SetShaderProperty__String__Single(string key, float value)
	{
		SetShaderPropertyLocal(key, value);
	}

	protected static void InvokeUserCode_SetShaderProperty__String__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC SetShaderProperty called on server.");
		}
		else
		{
			((EntityVisual)(object)obj).UserCode_SetShaderProperty__String__Single(NetworkReaderExtensions.ReadString(reader), NetworkReaderExtensions.ReadFloat(reader));
		}
	}

	protected void UserCode_SetShaderProperty__String__Color(string key, Color value)
	{
		SetShaderPropertyLocal(key, value);
	}

	protected static void InvokeUserCode_SetShaderProperty__String__Color(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC SetShaderProperty called on server.");
		}
		else
		{
			((EntityVisual)(object)obj).UserCode_SetShaderProperty__String__Color(NetworkReaderExtensions.ReadString(reader), NetworkReaderExtensions.ReadColor(reader));
		}
	}

	protected void UserCode_KnockUp__Single__Boolean(float strength, bool isFriendly)
	{
		KnockUpLocal(strength, isFriendly);
	}

	protected static void InvokeUserCode_KnockUp__Single__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC KnockUp called on server.");
		}
		else
		{
			((EntityVisual)(object)obj).UserCode_KnockUp__Single__Boolean(NetworkReaderExtensions.ReadFloat(reader), NetworkReaderExtensions.ReadBool(reader));
		}
	}

	protected void UserCode_KnockUp__KnockUpStrength__Boolean(KnockUpStrength strength, bool isFriendly)
	{
		KnockUpLocal(strength switch
		{
			KnockUpStrength.Small => 0.8f, 
			KnockUpStrength.Normal => 1.1f, 
			KnockUpStrength.Big => 1.5f, 
			_ => throw new ArgumentOutOfRangeException("strength", strength, null), 
		}, isFriendly);
	}

	protected static void InvokeUserCode_KnockUp__KnockUpStrength__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC KnockUp called on server.");
		}
		else
		{
			((EntityVisual)(object)obj).UserCode_KnockUp__KnockUpStrength__Boolean(GeneratedNetworkCode._Read_KnockUpStrength(reader), NetworkReaderExtensions.ReadBool(reader));
		}
	}

	protected void UserCode_RpcSkipSpawning()
	{
		if (spawnEffect != null && spawnEffect.activeSelf)
		{
			FxStop(spawnEffect);
			spawnEffect.SetActive(value: false);
			spawnEffect.SetActive(value: true);
		}
		if (spawnEffectOnGround != null && spawnEffectOnGround.activeSelf)
		{
			FxStop(spawnEffectOnGround);
			spawnEffectOnGround.SetActive(value: false);
			spawnEffectOnGround.SetActive(value: true);
		}
	}

	protected static void InvokeUserCode_RpcSkipSpawning(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSkipSpawning called on server.");
		}
		else
		{
			((EntityVisual)(object)obj).UserCode_RpcSkipSpawning();
		}
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		((NetworkBehaviour)this).SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteFloat(writer, spawnDuration);
			NetworkWriterExtensions.WriteBool(writer, skipSpawning);
			NetworkWriterExtensions.WriteBool(writer, invisibleByDefault);
			NetworkWriterExtensions.WriteInt(writer, _rendererOffCounterNetworked);
			NetworkWriterExtensions.WriteInt(writer, _groundMarkerHiddenCounterNetworked);
			NetworkWriterExtensions.WriteBool(writer, _shouldShowStunned);
			NetworkWriterExtensions.WriteInt(writer, genericStackIndicatorMax__BackingField);
			NetworkWriterExtensions.WriteInt(writer, genericStackIndicatorValue__BackingField);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 1L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, spawnDuration);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 2L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, skipSpawning);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 4L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, invisibleByDefault);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 8L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, _rendererOffCounterNetworked);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x10L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, _groundMarkerHiddenCounterNetworked);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x20L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, _shouldShowStunned);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, genericStackIndicatorMax__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, genericStackIndicatorValue__BackingField);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		((NetworkBehaviour)this).DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref spawnDuration, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref skipSpawning, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref invisibleByDefault, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref _rendererOffCounterNetworked, _Mirror_SyncVarHookDelegate__rendererOffCounterNetworked, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref _groundMarkerHiddenCounterNetworked, _Mirror_SyncVarHookDelegate__groundMarkerHiddenCounterNetworked, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _shouldShowStunned, _Mirror_SyncVarHookDelegate__shouldShowStunned, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref genericStackIndicatorMax__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref genericStackIndicatorValue__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 1L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref spawnDuration, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 2L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref skipSpawning, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 4L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref invisibleByDefault, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 8L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref _rendererOffCounterNetworked, _Mirror_SyncVarHookDelegate__rendererOffCounterNetworked, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x10L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref _groundMarkerHiddenCounterNetworked, _Mirror_SyncVarHookDelegate__groundMarkerHiddenCounterNetworked, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x20L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _shouldShowStunned, _Mirror_SyncVarHookDelegate__shouldShowStunned, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref genericStackIndicatorMax__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref genericStackIndicatorValue__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
	}
}
