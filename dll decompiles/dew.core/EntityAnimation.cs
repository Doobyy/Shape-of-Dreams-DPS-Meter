using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

public class EntityAnimation : EntityComponent
{
	public struct AbilityAnimationStatus
	{
		public bool isPlaying;

		public EaseFunction easeFunction;

		public float rawClipDuration;

		public float duration;

		public Vector2 trimRange;

		public float elapsedTime;

		public DewAnimationClip currentClip;

		public float overrideWalkNormalizedDuration;

		public float normalizedTime => elapsedTime / duration;

		public float normalizedTimeParameter => (trimRange.x + (trimRange.y - trimRange.x) * easeFunction(0f, 1f, normalizedTime)) / rawClipDuration;
	}

	public enum ReplaceableAnimationType
	{
		Idle,
		RunForward,
		RunForwardRight,
		RunRight,
		RunBackwardRight,
		RunBackward,
		RunBackwardLeft,
		RunLeft,
		RunForwardLeft,
		Ability,
		Stagger,
		Death
	}

	public enum LocomotionType
	{
		Simple,
		FourDirections,
		EightDirections
	}

	private const float AbilityAnimationTransitionTime = 0.1f;

	private const float AbilityAnimationCancelByWalkGraceTime = 0.075f;

	private static readonly int LocomotionType_ID;

	private static readonly int WalkSpeedMultiplier_ID;

	private static readonly int Stagger_ID;

	private static readonly int AbilityAnimationSpeed_ID;

	private static readonly int AbilityAnimationNormalizedTime_ID;

	private static readonly int StartAbilityAnimation_ID;

	private static readonly int StopAbilityAnimation_ID;

	private static readonly int IsWalking_ID;

	private static readonly int IsDead_ID;

	private static readonly int WalkDirX_ID;

	private static readonly int WalkDirY_ID;

	private static readonly int[] SpeedByType_IDs;

	[NonSerialized]
	public Animator animator;

	private AnimatorOverrideController _animatorOverrides;

	internal Animator _abilitySampler;

	private AnimatorOverrideController _abilitySamplerOverrides;

	private Transform _rotationFixTransformTarget;

	private Transform _rotationFixTransformSource;

	private AnimationClip[] _baseClips = new AnimationClip[12];

	private AnimationClip[] _currentOverrides = new AnimationClip[12];

	private readonly List<KeyValuePair<AnimationClip, AnimationClip>> _overridesBuffer = new List<KeyValuePair<AnimationClip, AnimationClip>>(1);

	public AbilityAnimationStatus abilityAnimStatus;

	private Animator _paramCacheAnimator;

	private bool _boolParamsCacheValid;

	private bool _walkSpeedCacheValid;

	private bool _cachedIsWalking;

	private bool _cachedIsDead;

	private float _cachedWalkSpeedMultiplier;

	private const float StateTimeReanchorThreshold = 1000f;

	private const float StateTimeReanchorInterval = 1f;

	private float _stateTimeReanchorTimer;

	[CompilerGenerated]
	[SyncVar]
	private CounterBool isDeathAnimationForced__BackingField;

	public CounterBool isDeathAnimationForced
	{
		[CompilerGenerated]
		get
		{
			return isDeathAnimationForced__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CisDeathAnimationForced_003Ek__BackingField = value;
		}
	}

	public EntityModel model => entity.Visual.model;

	public CounterBool Network_003CisDeathAnimationForced_003Ek__BackingField
	{
		get
		{
			return isDeathAnimationForced__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<CounterBool>(value, ref isDeathAnimationForced__BackingField, 1uL, (Action<CounterBool, CounterBool>)null);
		}
	}

	private static int[] BuildSpeedByTypeIds()
	{
		int[] array = (int[])Enum.GetValues(typeof(ReplaceableAnimationType));
		int num = 0;
		int[] array2 = array;
		foreach (int num2 in array2)
		{
			if (num2 > num)
			{
				num = num2;
			}
		}
		int[] array3 = new int[num + 1];
		string[] names = Enum.GetNames(typeof(ReplaceableAnimationType));
		for (int j = 0; j < array.Length; j++)
		{
			array3[array[j]] = Animator.StringToHash("speed" + names[j]);
		}
		return array3;
	}

	private void RefreshParamCacheAnimator()
	{
		if (!((UnityEngine.Object)(object)_paramCacheAnimator == (UnityEngine.Object)(object)animator))
		{
			_paramCacheAnimator = animator;
			InvalidateAnimatorParamCache();
		}
	}

	private void InvalidateAnimatorParamCache()
	{
		_boolParamsCacheValid = false;
		_walkSpeedCacheValid = false;
	}

	public void SetupModel()
	{
		if (model == null)
		{
			return;
		}
		InitAnimator();
		if ((UnityEngine.Object)(object)animator == null)
		{
			return;
		}
		if ((UnityEngine.Object)(object)model.idle.clip != null)
		{
			ReplaceAnimationLocal(ReplaceableAnimationType.Idle, model.idle);
		}
		switch (model.locomotion)
		{
		case LocomotionType.Simple:
			if ((UnityEngine.Object)(object)model.runForwardClip != null)
			{
				ReplaceAnimationLocal(ReplaceableAnimationType.RunForward, model.runForwardClip);
			}
			break;
		case LocomotionType.FourDirections:
			if ((UnityEngine.Object)(object)model.runForwardClip != null)
			{
				ReplaceAnimationLocal(ReplaceableAnimationType.RunForward, model.runForwardClip);
			}
			if ((UnityEngine.Object)(object)model.runRightClip != null)
			{
				ReplaceAnimationLocal(ReplaceableAnimationType.RunRight, model.runRightClip);
			}
			if ((UnityEngine.Object)(object)model.runBackwardClip != null)
			{
				ReplaceAnimationLocal(ReplaceableAnimationType.RunBackward, model.runBackwardClip);
			}
			if ((UnityEngine.Object)(object)model.runLeftClip != null)
			{
				ReplaceAnimationLocal(ReplaceableAnimationType.RunLeft, model.runLeftClip);
			}
			break;
		case LocomotionType.EightDirections:
			if ((UnityEngine.Object)(object)model.runForwardClip != null)
			{
				ReplaceAnimationLocal(ReplaceableAnimationType.RunForward, model.runForwardClip);
			}
			if ((UnityEngine.Object)(object)model.runForwardRightClip != null)
			{
				ReplaceAnimationLocal(ReplaceableAnimationType.RunForwardRight, model.runForwardRightClip);
			}
			if ((UnityEngine.Object)(object)model.runRightClip != null)
			{
				ReplaceAnimationLocal(ReplaceableAnimationType.RunRight, model.runRightClip);
			}
			if ((UnityEngine.Object)(object)model.runBackwardRightClip != null)
			{
				ReplaceAnimationLocal(ReplaceableAnimationType.RunBackwardRight, model.runBackwardRightClip);
			}
			if ((UnityEngine.Object)(object)model.runBackwardClip != null)
			{
				ReplaceAnimationLocal(ReplaceableAnimationType.RunBackward, model.runBackwardClip);
			}
			if ((UnityEngine.Object)(object)model.runBackwardLeftClip != null)
			{
				ReplaceAnimationLocal(ReplaceableAnimationType.RunBackwardLeft, model.runBackwardLeftClip);
			}
			if ((UnityEngine.Object)(object)model.runLeftClip != null)
			{
				ReplaceAnimationLocal(ReplaceableAnimationType.RunLeft, model.runLeftClip);
			}
			if ((UnityEngine.Object)(object)model.runForwardLeftClip != null)
			{
				ReplaceAnimationLocal(ReplaceableAnimationType.RunForwardLeft, model.runForwardLeftClip);
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		animator.SetInteger(LocomotionType_ID, (int)model.locomotion);
		if ((UnityEngine.Object)(object)model.stagger.clip != null)
		{
			ReplaceAnimationLocal(ReplaceableAnimationType.Stagger, model.stagger);
		}
		if ((UnityEngine.Object)(object)model.death.clip != null)
		{
			ReplaceAnimationLocal(ReplaceableAnimationType.Death, model.death);
		}
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (((Behaviour)(object)this).enabled && !entity.isSleeping && entity.isActive && !((UnityEngine.Object)(object)animator == null))
		{
			RefreshParamCacheAnimator();
			float num = entity.Control.walkStrength * model.walkAnimationSpeed * entity.Status.movementSpeedMultiplier / Mathf.Clamp(entity.Visual.etScaleMultiplier.x, 0.01f, 100f);
			if (!_walkSpeedCacheValid || num != _cachedWalkSpeedMultiplier)
			{
				animator.SetFloat(WalkSpeedMultiplier_ID, num);
				_cachedWalkSpeedMultiplier = num;
				_walkSpeedCacheValid = true;
			}
		}
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (!((Behaviour)(object)this).enabled || entity.isSleeping || (UnityEngine.Object)(object)animator == null)
		{
			return;
		}
		FrameUpdateAnimations();
		_stateTimeReanchorTimer += Time.deltaTime;
		if (_stateTimeReanchorTimer >= 1f)
		{
			_stateTimeReanchorTimer = 0f;
			ReanchorLoopingStates(animator);
			if ((UnityEngine.Object)(object)_abilitySampler != null)
			{
				ReanchorLoopingStates(_abilitySampler);
			}
		}
	}

	private static void ReanchorLoopingStates(Animator a)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (!((Behaviour)(object)a).isActiveAndEnabled)
		{
			return;
		}
		int layerCount = a.layerCount;
		for (int i = 0; i < layerCount; i++)
		{
			if (a.IsInTransition(i))
			{
				continue;
			}
			AnimatorStateInfo currentAnimatorStateInfo = a.GetCurrentAnimatorStateInfo(i);
			if (currentAnimatorStateInfo.loop)
			{
				float normalizedTime = currentAnimatorStateInfo.normalizedTime;
				if (!(normalizedTime < 1000f))
				{
					a.Play(currentAnimatorStateInfo.fullPathHash, i, normalizedTime % 1f);
				}
			}
		}
	}

	public void PlayStaggerAnimation()
	{
		RpcPlayStaggerAnimation();
	}

	[ClientRpc]
	private void RpcPlayStaggerAnimation()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void EntityAnimation::RpcPlayStaggerAnimation()", -1820776812, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void AssignBaseClipReferences(AnimationClip[] clips)
	{
		string[] names = Enum.GetNames(typeof(ReplaceableAnimationType));
		int[] array = (int[])Enum.GetValues(typeof(ReplaceableAnimationType));
		for (int i = 0; i < clips.Length; i++)
		{
			for (int j = 0; j < names.Length; j++)
			{
				if (names[j] == ((UnityEngine.Object)(object)clips[i]).name)
				{
					_baseClips[array[j]] = clips[i];
					break;
				}
			}
		}
	}

	[Server]
	public void ReplaceAnimation(ReplaceableAnimationType type, AnimationClip clip)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityAnimation::ReplaceAnimation(EntityAnimation/ReplaceableAnimationType,UnityEngine.AnimationClip)' called when server was not active");
		}
		else
		{
			RpcReplaceAnimation(type, clip);
		}
	}

	public void ReplaceAnimationLocal(ReplaceableAnimationType type, AnimationClip newClip)
	{
		if ((UnityEngine.Object)(object)_currentOverrides[(int)type] == (UnityEngine.Object)(object)newClip)
		{
			return;
		}
		AnimationClip val = _baseClips[(int)type];
		if (!((UnityEngine.Object)(object)val == null) && !((UnityEngine.Object)(object)_animatorOverrides == null))
		{
			_overridesBuffer.Clear();
			_overridesBuffer.Add(new KeyValuePair<AnimationClip, AnimationClip>(val, newClip));
			_animatorOverrides.ApplyOverrides((IList<KeyValuePair<AnimationClip, AnimationClip>>)_overridesBuffer);
			if ((UnityEngine.Object)(object)_abilitySamplerOverrides != null)
			{
				_abilitySamplerOverrides.ApplyOverrides((IList<KeyValuePair<AnimationClip, AnimationClip>>)_overridesBuffer);
			}
			_currentOverrides[(int)type] = newClip;
			InvalidateAnimatorParamCache();
		}
	}

	public void ReplaceAnimationLocal(ReplaceableAnimationType type, AnimationClipWithSpeed newData)
	{
		ReplaceAnimationLocal(type, newData.clip);
		int num = SpeedByType_IDs[(int)type];
		animator.SetFloat(num, newData.speed);
		if ((UnityEngine.Object)(object)_abilitySampler != null)
		{
			_abilitySampler.SetFloat(num, newData.speed);
		}
	}

	[ClientRpc]
	private void RpcReplaceAnimation(ReplaceableAnimationType type, AnimationClip clip)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_EntityAnimation_002FReplaceableAnimationType((NetworkWriter)(object)val, type);
		((NetworkWriter)(object)val).WriteAnimationClip(clip);
		((NetworkBehaviour)this).SendRPCInternal("System.Void EntityAnimation::RpcReplaceAnimation(EntityAnimation/ReplaceableAnimationType,UnityEngine.AnimationClip)", 1139056298, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[Server]
	public void PlayAbilityAnimation(DewAnimationClip clip, float speed = 1f)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityAnimation::PlayAbilityAnimation(DewAnimationClip,System.Single)' called when server was not active");
		}
		else if (!(clip == null))
		{
			RpcPlayAbilityAnimation(clip, clip.GetEntryIndex(), speed);
		}
	}

	[Server]
	public void PlayAbilityAnimation(DewAnimationClip clip, float speed, float clipSelectValue)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityAnimation::PlayAbilityAnimation(DewAnimationClip,System.Single,System.Single)' called when server was not active");
		}
		else if (!(clip == null))
		{
			RpcPlayAbilityAnimation(clip, clip.GetEntryIndex(clipSelectValue), speed);
		}
	}

	[ClientRpc]
	private void RpcPlayAbilityAnimation(DewAnimationClip clip, int index, float speed)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkWriter)(object)val).WriteDewAnimationClip(clip);
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, index);
		NetworkWriterExtensions.WriteFloat((NetworkWriter)(object)val, speed);
		((NetworkBehaviour)this).SendRPCInternal("System.Void EntityAnimation::RpcPlayAbilityAnimation(DewAnimationClip,System.Int32,System.Single)", -998352957, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private DewAnimationClip ResolveAbilityClip(DewAnimationClip clip)
	{
		List<AbilityAnimationReplacementPair> list = ((model != null) ? model.abilityAnimationReplacements : null);
		if (list == null)
		{
			return clip;
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].from.asset == clip && list[i].to.asset != null)
			{
				return list[i].to.asset;
			}
		}
		return clip;
	}

	[Server]
	public void StopAbilityAnimation()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityAnimation::StopAbilityAnimation()' called when server was not active");
		}
		else
		{
			RpcStopAbilityAnimation();
		}
	}

	[Server]
	public void StopAbilityAnimation(DewAnimationClip clip)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityAnimation::StopAbilityAnimation(DewAnimationClip)' called when server was not active");
		}
		else
		{
			RpcStopAbilityAnimation(clip);
		}
	}

	[ClientRpc]
	private void RpcStopAbilityAnimation(DewAnimationClip clip)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkWriter)(object)val).WriteDewAnimationClip(clip);
		((NetworkBehaviour)this).SendRPCInternal("System.Void EntityAnimation::RpcStopAbilityAnimation(DewAnimationClip)", 318803303, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcStopAbilityAnimation()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void EntityAnimation::RpcStopAbilityAnimation()", 1050101317, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void InitAnimator()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected Obj, but got Unknown
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Expected Obj, but got Unknown
		animator = model.GetComponentInChildren<Animator>();
		if ((UnityEngine.Object)(object)animator == null)
		{
			return;
		}
		animator.keepAnimatorStateOnDisable = true;
		_animatorOverrides = new AnimatorOverrideController(animator.runtimeAnimatorController);
		animator.runtimeAnimatorController = (RuntimeAnimatorController)(object)_animatorOverrides;
		InvalidateAnimatorParamCache();
		Array.Clear(_currentOverrides, 0, _currentOverrides.Length);
		AssignBaseClipReferences(animator.runtimeAnimatorController.animationClips);
		if (!animator.isHuman)
		{
			return;
		}
		_abilitySampler = UnityEngine.Object.Instantiate<Animator>(animator, ((Component)(object)animator).transform);
		((UnityEngine.Object)(object)_abilitySampler).name = "Model (AbilitySampler)";
		_abilitySampler.cullingMode = (AnimatorCullingMode)0;
		Component[] componentsInChildren = ((Component)(object)_abilitySampler).GetComponentsInChildren<Component>(true);
		foreach (Component component in componentsInChildren)
		{
			try
			{
				if (component == null || component is Transform || component == (UnityEngine.Object)(object)_abilitySampler)
				{
					continue;
				}
				if (component is Light && component.TryGetComponent<UniversalAdditionalLightData>(out var component2))
				{
					UnityEngine.Object.DestroyImmediate((UnityEngine.Object)(object)component2);
				}
				if (component is Light && component.TryGetComponent<FxPointLight>(out var component3))
				{
					UnityEngine.Object.DestroyImmediate(component3);
				}
				if (component is SkinnedMeshRenderer && component.TryGetComponent<Cloth>(out var component4))
				{
					UnityEngine.Object.DestroyImmediate((UnityEngine.Object)(object)component4);
				}
				if (component is Volume && component.TryGetComponent<FxVolume>(out var component5))
				{
					UnityEngine.Object.DestroyImmediate(component5);
				}
				if (component is VisualEffect && component.TryGetComponent<VFXPropertyBinder>(out var component6))
				{
					if (component.TryGetComponent<VFXBinderBase>(out var component7))
					{
						UnityEngine.Object.DestroyImmediate((UnityEngine.Object)(object)component7);
					}
					if (component.TryGetComponent<VFXPropertyBinder>(out var component8))
					{
						UnityEngine.Object.DestroyImmediate((UnityEngine.Object)(object)component8);
					}
					UnityEngine.Object.DestroyImmediate((UnityEngine.Object)(object)component6);
				}
				UnityEngine.Object.DestroyImmediate(component);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		_abilitySamplerOverrides = new AnimatorOverrideController(_abilitySampler.runtimeAnimatorController);
		_abilitySampler.runtimeAnimatorController = (RuntimeAnimatorController)(object)_abilitySamplerOverrides;
		_rotationFixTransformTarget = animator.GetBoneTransform((HumanBodyBones)7);
		_rotationFixTransformSource = _abilitySampler.GetBoneTransform((HumanBodyBones)7);
	}

	[Server]
	public void StartDeathAnimation()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityAnimation::StartDeathAnimation()' called when server was not active");
		}
		else
		{
			isDeathAnimationForced += 1;
		}
	}

	[Server]
	public void StopDeathAnimation()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityAnimation::StopDeathAnimation()' called when server was not active");
		}
		else
		{
			isDeathAnimationForced -= 1;
		}
	}

	private void FrameUpdateAnimations()
	{
		RefreshParamCacheAnimator();
		bool flag = entity.Control.isWalking && (!abilityAnimStatus.isPlaying || abilityAnimStatus.currentClip.overrideWalk != DewAnimationClip.OverrideWalkBehavior.Full);
		bool flag2 = ((!(entity is Hero hero)) ? (!entity.isActive) : (!entity.isActive || hero.isKnockedOut || (bool)isDeathAnimationForced));
		if (!_boolParamsCacheValid || flag != _cachedIsWalking)
		{
			animator.SetBool(IsWalking_ID, flag);
		}
		if (!_boolParamsCacheValid || flag2 != _cachedIsDead)
		{
			animator.SetBool(IsDead_ID, flag2);
		}
		_cachedIsWalking = flag;
		_cachedIsDead = flag2;
		_boolParamsCacheValid = true;
		if (abilityAnimStatus.isPlaying)
		{
			if (!Dew.IsOkay(abilityAnimStatus.elapsedTime) || !Dew.IsOkay(abilityAnimStatus.normalizedTime))
			{
				abilityAnimStatus.elapsedTime = 0f;
				abilityAnimStatus.isPlaying = false;
			}
			abilityAnimStatus.elapsedTime = Mathf.MoveTowards(abilityAnimStatus.elapsedTime, abilityAnimStatus.duration, Time.deltaTime);
			if (abilityAnimStatus.elapsedTime >= abilityAnimStatus.duration || !entity.isActive)
			{
				abilityAnimStatus.isPlaying = false;
			}
			else if (entity.Control.isWalking && abilityAnimStatus.elapsedTime > 0.075f && (abilityAnimStatus.currentClip.overrideWalk == DewAnimationClip.OverrideWalkBehavior.None || abilityAnimStatus.normalizedTime > abilityAnimStatus.overrideWalkNormalizedDuration))
			{
				abilityAnimStatus.isPlaying = false;
			}
			animator.SetFloat(AbilityAnimationNormalizedTime_ID, abilityAnimStatus.normalizedTimeParameter);
			if ((UnityEngine.Object)(object)_abilitySampler != null)
			{
				_abilitySampler.SetFloat(AbilityAnimationNormalizedTime_ID, abilityAnimStatus.normalizedTimeParameter);
			}
		}
		float layerWeight = animator.GetLayerWeight(1);
		int num = (abilityAnimStatus.isPlaying ? 1 : 0);
		float num2 = Mathf.MoveTowards(layerWeight, num, Time.deltaTime / 0.1f);
		if (num2 != layerWeight)
		{
			animator.SetLayerWeight(1, num2);
		}
	}

	private void LateUpdate()
	{
		if ((UnityEngine.Object)(object)animator == null)
		{
			return;
		}
		if (_rotationFixTransformSource != null)
		{
			float layerWeight = animator.GetLayerWeight(1);
			if (layerWeight > 0f)
			{
				_rotationFixTransformTarget.rotation = Quaternion.Slerp(_rotationFixTransformTarget.rotation, _rotationFixTransformSource.rotation, layerWeight);
			}
		}
		if (model.locomotion != LocomotionType.Simple)
		{
			Vector3 agentVelocity = entity.Control.agentVelocity;
			if (agentVelocity.sqrMagnitude > 0.001f)
			{
				Vector3 normalized = (Quaternion.Inverse(((Component)(object)this).transform.rotation) * Quaternion.LookRotation(agentVelocity) * Vector3.forward).normalized;
				animator.SetFloat(WalkDirX_ID, normalized.x, 0.1f, Time.deltaTime);
				animator.SetFloat(WalkDirY_ID, normalized.z, 0.1f, Time.deltaTime);
			}
		}
	}

	static EntityAnimation()
	{
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Expected Obj, but got Unknown
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Expected Obj, but got Unknown
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Expected Obj, but got Unknown
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Expected Obj, but got Unknown
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Expected Obj, but got Unknown
		LocomotionType_ID = Animator.StringToHash("locomotionType");
		WalkSpeedMultiplier_ID = Animator.StringToHash("walkSpeedMultiplier");
		Stagger_ID = Animator.StringToHash("Stagger");
		AbilityAnimationSpeed_ID = Animator.StringToHash("abilityAnimationSpeed");
		AbilityAnimationNormalizedTime_ID = Animator.StringToHash("abilityAnimationNormalizedTime");
		StartAbilityAnimation_ID = Animator.StringToHash("StartAbilityAnimation");
		StopAbilityAnimation_ID = Animator.StringToHash("StopAbilityAnimation");
		IsWalking_ID = Animator.StringToHash("isWalking");
		IsDead_ID = Animator.StringToHash("isDead");
		WalkDirX_ID = Animator.StringToHash("walkDirX");
		WalkDirY_ID = Animator.StringToHash("walkDirY");
		SpeedByType_IDs = BuildSpeedByTypeIds();
		RemoteProcedureCalls.RegisterRpc(typeof(EntityAnimation), "System.Void EntityAnimation::RpcPlayStaggerAnimation()", (RemoteCallDelegate)InvokeUserCode_RpcPlayStaggerAnimation);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityAnimation), "System.Void EntityAnimation::RpcReplaceAnimation(EntityAnimation/ReplaceableAnimationType,UnityEngine.AnimationClip)", (RemoteCallDelegate)InvokeUserCode_RpcReplaceAnimation__ReplaceableAnimationType__AnimationClip);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityAnimation), "System.Void EntityAnimation::RpcPlayAbilityAnimation(DewAnimationClip,System.Int32,System.Single)", (RemoteCallDelegate)InvokeUserCode_RpcPlayAbilityAnimation__DewAnimationClip__Int32__Single);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityAnimation), "System.Void EntityAnimation::RpcStopAbilityAnimation(DewAnimationClip)", (RemoteCallDelegate)InvokeUserCode_RpcStopAbilityAnimation__DewAnimationClip);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityAnimation), "System.Void EntityAnimation::RpcStopAbilityAnimation()", (RemoteCallDelegate)InvokeUserCode_RpcStopAbilityAnimation);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcPlayStaggerAnimation()
	{
		if ((UnityEngine.Object)(object)animator != null)
		{
			animator.SetTrigger(Stagger_ID);
		}
	}

	protected static void InvokeUserCode_RpcPlayStaggerAnimation(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayStaggerAnimation called on server.");
		}
		else
		{
			((EntityAnimation)(object)obj).UserCode_RpcPlayStaggerAnimation();
		}
	}

	protected void UserCode_RpcReplaceAnimation__ReplaceableAnimationType__AnimationClip(ReplaceableAnimationType type, AnimationClip clip)
	{
		ReplaceAnimationLocal(type, clip);
	}

	protected static void InvokeUserCode_RpcReplaceAnimation__ReplaceableAnimationType__AnimationClip(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcReplaceAnimation called on server.");
		}
		else
		{
			((EntityAnimation)(object)obj).UserCode_RpcReplaceAnimation__ReplaceableAnimationType__AnimationClip(GeneratedNetworkCode._Read_EntityAnimation_002FReplaceableAnimationType(reader), reader.ReadAnimationClip());
		}
	}

	protected void UserCode_RpcPlayAbilityAnimation__DewAnimationClip__Int32__Single(DewAnimationClip clip, int index, float speed)
	{
		if (clip == null)
		{
			return;
		}
		DewAnimationClip dewAnimationClip = ResolveAbilityClip(clip);
		if (dewAnimationClip != clip && dewAnimationClip.entries != null)
		{
			index = Mathf.Clamp(index, 0, dewAnimationClip.entries.Length - 1);
		}
		DewAnimationEntry entry = dewAnimationClip.GetEntry(index);
		if (entry == null)
		{
			return;
		}
		ReplaceAnimationLocal(ReplaceableAnimationType.Ability, entry.rawClip);
		if (speed < 0.01f)
		{
			speed = 0.01f;
		}
		if (Dew.IsOkay(speed) && Dew.IsOkay(entry.duration) && !(entry.duration <= 0f))
		{
			abilityAnimStatus.isPlaying = true;
			abilityAnimStatus.duration = Mathf.Max(entry.duration / speed, 0.0001f);
			abilityAnimStatus.easeFunction = EasingFunction.GetEasingFunction(entry.timeCurve);
			abilityAnimStatus.elapsedTime = 0f;
			abilityAnimStatus.rawClipDuration = entry.rawClip.length;
			abilityAnimStatus.trimRange = entry.trimRange;
			abilityAnimStatus.currentClip = clip;
			abilityAnimStatus.overrideWalkNormalizedDuration = clip.overrideWalkNormalizedDuration;
			if (animator.isHuman && (clip.overrideWalk == DewAnimationClip.OverrideWalkBehavior.UpperBody || clip.onlyUpperBody))
			{
				animator.SetLayerWeight(1, 1f);
			}
			animator.SetFloat(AbilityAnimationSpeed_ID, entry.rawClip.length / entry.duration * speed);
			animator.SetFloat(AbilityAnimationNormalizedTime_ID, abilityAnimStatus.normalizedTimeParameter);
			if (!clip.onlyUpperBody)
			{
				animator.SetTrigger(StartAbilityAnimation_ID);
			}
			if ((UnityEngine.Object)(object)_abilitySampler != null)
			{
				_abilitySampler.SetFloat(AbilityAnimationSpeed_ID, entry.rawClip.length / entry.duration * speed);
				_abilitySampler.SetFloat(AbilityAnimationNormalizedTime_ID, abilityAnimStatus.normalizedTimeParameter);
				_abilitySampler.SetTrigger(StartAbilityAnimation_ID);
			}
			abilityAnimStatus.currentClip = clip;
		}
	}

	protected static void InvokeUserCode_RpcPlayAbilityAnimation__DewAnimationClip__Int32__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayAbilityAnimation called on server.");
		}
		else
		{
			((EntityAnimation)(object)obj).UserCode_RpcPlayAbilityAnimation__DewAnimationClip__Int32__Single(reader.ReadDewAnimationClip(), NetworkReaderExtensions.ReadInt(reader), NetworkReaderExtensions.ReadFloat(reader));
		}
	}

	protected void UserCode_RpcStopAbilityAnimation__DewAnimationClip(DewAnimationClip clip)
	{
		if (!(abilityAnimStatus.currentClip != clip))
		{
			abilityAnimStatus.elapsedTime = abilityAnimStatus.duration;
			animator.SetTrigger(StopAbilityAnimation_ID);
		}
	}

	protected static void InvokeUserCode_RpcStopAbilityAnimation__DewAnimationClip(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcStopAbilityAnimation called on server.");
		}
		else
		{
			((EntityAnimation)(object)obj).UserCode_RpcStopAbilityAnimation__DewAnimationClip(reader.ReadDewAnimationClip());
		}
	}

	protected void UserCode_RpcStopAbilityAnimation()
	{
		abilityAnimStatus.elapsedTime = abilityAnimStatus.duration;
		animator.SetTrigger(StopAbilityAnimation_ID);
	}

	protected static void InvokeUserCode_RpcStopAbilityAnimation(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcStopAbilityAnimation called on server.");
		}
		else
		{
			((EntityAnimation)(object)obj).UserCode_RpcStopAbilityAnimation();
		}
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		((NetworkBehaviour)this).SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteCounterBool(isDeathAnimationForced__BackingField);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteCounterBool(isDeathAnimationForced__BackingField);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		((NetworkBehaviour)this).DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<CounterBool>(ref isDeathAnimationForced__BackingField, (Action<CounterBool, CounterBool>)null, reader.ReadCounterBool());
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 1L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<CounterBool>(ref isDeathAnimationForced__BackingField, (Action<CounterBool, CounterBool>)null, reader.ReadCounterBool());
		}
	}
}
