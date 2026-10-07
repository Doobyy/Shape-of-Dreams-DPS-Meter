using System;
using System.Collections.Generic;
using UnityEngine;

public class Extras_CharacterPoser : MonoBehaviour
{
	public AnimationClip clip;

	public float normalizedTime;

	public bool enableLobbyLights;

	private Animator _animator;

	private List<KeyValuePair<AnimationClip, AnimationClip>> _overrides;

	private AnimatorOverrideController _controller;

	private void Awake()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected Obj, but got Unknown
		_animator = GetComponentInChildren<Animator>();
		_controller = new AnimatorOverrideController(_animator.runtimeAnimatorController);
		_animator.runtimeAnimatorController = (RuntimeAnimatorController)(object)_controller;
		string[] names = Enum.GetNames(typeof(EntityAnimation.ReplaceableAnimationType));
		AnimationClip[] animationClips = _animator.runtimeAnimatorController.animationClips;
		int[] array = (int[])Enum.GetValues(typeof(EntityAnimation.ReplaceableAnimationType));
		AnimationClip[] array2 = new AnimationClip[12];
		for (int i = 0; i < animationClips.Length; i++)
		{
			for (int j = 0; j < names.Length; j++)
			{
				if (names[j] == ((UnityEngine.Object)(object)animationClips[i]).name)
				{
					array2[array[j]] = animationClips[i];
					break;
				}
			}
		}
		_overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
		_overrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(array2[9], clip));
	}

	private void OnEnable()
	{
		_animator.SetLayerWeight(1, 0f);
		_animator.SetTrigger("StartAbilityAnimation");
		if (enableLobbyLights)
		{
			FocusedLobbyCharacterEffect componentInChildren = GetComponentInChildren<FocusedLobbyCharacterEffect>(includeInactive: true);
			if (componentInChildren != null)
			{
				DewEffect.Play(componentInChildren.gameObject);
			}
		}
	}

	private void Update()
	{
		_overrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(_overrides[0].Key, clip));
		_overrides.RemoveAt(0);
		_controller.ApplyOverrides((IList<KeyValuePair<AnimationClip, AnimationClip>>)_overrides);
		_animator.SetFloat("abilityAnimationNormalizedTime", normalizedTime);
	}
}
