using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CharacterModelDisplay : LogicBehaviour
{
	private static readonly int DissolveAmount = Shader.PropertyToID("_DissolveStrength");

	private static readonly int CMBaseColor = Shader.PropertyToID("_CMBaseColor");

	private static readonly int CMOpacity = Shader.PropertyToID("_CMOpacity");

	private static readonly int CMDissolveColor = Shader.PropertyToID("_CMDissolveColor");

	public float appearDissolveAnimateTime = 0.2f;

	public GameObject fxCharacterChangeEffect;

	public string skinType;

	public List<string> accessories = new List<string>();

	public float opacity = 1f;

	public bool isFocused;

	private EntityModel _currentModel;

	private List<Renderer> _currentRenderers = new List<Renderer>();

	private List<Renderer> _currentEffectRenderers = new List<Renderer>();

	private string _currentSkinName;

	private Bounds _bounds;

	private bool _wasFocused;

	private ILobbyCharacterModelOnFocus[] _focusHandlers;

	private List<Accessory> _instantiatedAccessories = new List<Accessory>();

	private float _lastOpacity = -1f;

	private List<Renderer> _propertyRenderers;

	private Dictionary<int, float> _floatProperties;

	private MaterialPropertyBlock _propertyBlock;

	private void Awake()
	{
		_propertyRenderers = new List<Renderer>();
		_floatProperties = new Dictionary<int, float>();
		_propertyBlock = new MaterialPropertyBlock();
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (_currentModel == null || opacity == _lastOpacity)
		{
			return;
		}
		SetFloat(CMOpacity, opacity);
		foreach (Renderer currentEffectRenderer in _currentEffectRenderers)
		{
			if ((bool)currentEffectRenderer)
			{
				currentEffectRenderer.enabled = opacity > 0.5f;
			}
		}
		_lastOpacity = opacity;
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (skinType != _currentSkinName)
		{
			Setup(skinType);
		}
		if (_wasFocused != isFocused)
		{
			UpdateFocused();
		}
		if (_currentRenderers != null && _currentRenderers.Count != 0)
		{
			_bounds = _currentRenderers[0].bounds;
			for (int i = 1; i < _currentRenderers.Count; i++)
			{
				_bounds.Encapsulate(_currentRenderers[i].bounds);
			}
		}
	}

	private void UpdateFocused()
	{
		_wasFocused = isFocused;
		if (_focusHandlers == null)
		{
			return;
		}
		ILobbyCharacterModelOnFocus[] focusHandlers = _focusHandlers;
		foreach (ILobbyCharacterModelOnFocus lobbyCharacterModelOnFocus in focusHandlers)
		{
			if (lobbyCharacterModelOnFocus != null && lobbyCharacterModelOnFocus is UnityEngine.Object obj && !(obj == null))
			{
				try
				{
					lobbyCharacterModelOnFocus.OnLobbyCharacterFocus(isFocused);
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
		}
	}

	public void Setup(string skinName)
	{
		_propertyRenderers.Clear();
		_floatProperties.Clear();
		if (_currentModel != null)
		{
			_currentModel.GetComponent<Coroutiner>().StopAllCoroutines();
			UnityEngine.Object.Destroy(_currentModel.gameObject);
			_currentRenderers.Clear();
			_currentEffectRenderers.Clear();
			_currentModel = null;
			_instantiatedAccessories.Clear();
		}
		skinType = skinName;
		_currentSkinName = skinName;
		if (string.IsNullOrEmpty(skinName))
		{
			return;
		}
		EntityModel component = DewResources.GetByName<Skin>(skinName).GetComponent<EntityModel>();
		_currentModel = UnityEngine.Object.Instantiate(component, transform);
		_currentModel.gameObject.SetLayerRecursive(gameObject.layer);
		if (_currentModel.holsteredWeapon != null)
		{
			UnityEngine.Object.Destroy(_currentModel.holsteredWeapon.gameObject);
		}
		_wasFocused = false;
		_focusHandlers = _currentModel.GetComponentsInChildren<ILobbyCharacterModelOnFocus>();
		ILobbyCharacterModelSetup[] componentsInChildren = _currentModel.GetComponentsInChildren<ILobbyCharacterModelSetup>();
		foreach (ILobbyCharacterModelSetup lobbyCharacterModelSetup in componentsInChildren)
		{
			try
			{
				lobbyCharacterModelSetup.OnLobbyCharacterSetup();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		_currentRenderers.Clear();
		_currentRenderers.AddRange(_currentModel.GetComponentsInChildren<SkinnedMeshRenderer>());
		_currentRenderers.AddRange(_currentModel.GetComponentsInChildren<MeshRenderer>());
		Renderer[] bodyRenderers = component.GetComponentInChildren<EntityModel>().bodyRenderers;
		List<string> list = new List<string>();
		Renderer[] array = bodyRenderers;
		foreach (Renderer renderer in array)
		{
			list.Add(renderer.name);
		}
		for (int num = _currentRenderers.Count - 1; num >= 0; num--)
		{
			if (!list.Contains(_currentRenderers[num].name))
			{
				_currentRenderers.RemoveAt(num);
			}
		}
		DewEffect.Play(fxCharacterChangeEffect);
		SetupAnimation();
		SetupVisual();
		UpdateAccessories();
		_lastOpacity = -1f;
	}

	private void SetupVisual()
	{
		List<Renderer> list = new List<Renderer>();
		list.AddRange(_currentModel.GetComponentsInChildren<MeshRenderer>());
		list.AddRange(_currentModel.GetComponentsInChildren<SkinnedMeshRenderer>());
		_propertyRenderers.AddRange(list);
		SetFloat(CMOpacity, opacity);
		_currentModel.gameObject.AddComponent<Coroutiner>().StartCoroutine(AnimateAppear());
		IEnumerator AnimateAppear()
		{
			SetFloat(DissolveAmount, 1f);
			for (float t = 0f; t < 1f; t += Time.deltaTime / appearDissolveAnimateTime)
			{
				SetFloat(DissolveAmount, 1f - t);
				yield return null;
			}
			SetFloat(DissolveAmount, 0f);
		}
	}

	private void SetupAnimation()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected Obj, but got Unknown
		Animator componentInChildren = _currentModel.GetComponentInChildren<Animator>();
		if ((UnityEngine.Object)(object)componentInChildren == null)
		{
			return;
		}
		AnimatorOverrideController val = (AnimatorOverrideController)(object)(componentInChildren.runtimeAnimatorController = (RuntimeAnimatorController)new AnimatorOverrideController(componentInChildren.runtimeAnimatorController));
		string[] names = Enum.GetNames(typeof(EntityAnimation.ReplaceableAnimationType));
		AnimationClip[] animationClips = componentInChildren.runtimeAnimatorController.animationClips;
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
		List<KeyValuePair<AnimationClip, AnimationClip>> list = new List<KeyValuePair<AnimationClip, AnimationClip>>();
		AnimationClipWithSpeed animationClipWithSpeed = (((UnityEngine.Object)(object)_currentModel.lobby.clip != null) ? _currentModel.lobby : _currentModel.idle);
		list.Add(new KeyValuePair<AnimationClip, AnimationClip>(array2[0], animationClipWithSpeed.clip));
		val.ApplyOverrides((IList<KeyValuePair<AnimationClip, AnimationClip>>)list);
		componentInChildren.SetFloat("speedIdle", animationClipWithSpeed.speed);
		componentInChildren.SetLayerWeight(1, 0f);
	}

	private void SetFloat(int nameID, float value)
	{
		if (!_floatProperties.TryGetValue(nameID, out var value2) || !(Mathf.Abs(value2 - value) < 0.0001f))
		{
			_floatProperties[nameID] = value;
			ApplyPropertyBlocks();
		}
	}

	private float GetFloat(int nameID)
	{
		if (!_floatProperties.TryGetValue(nameID, out var value))
		{
			return 0f;
		}
		return value;
	}

	private void ApplyPropertyBlocks()
	{
		if (_propertyRenderers == null || _propertyRenderers.Count == 0 || _propertyBlock == null)
		{
			return;
		}
		for (int i = 0; i < _propertyRenderers.Count; i++)
		{
			Renderer renderer = _propertyRenderers[i];
			if (renderer == null)
			{
				continue;
			}
			renderer.GetPropertyBlock(_propertyBlock);
			foreach (KeyValuePair<int, float> floatProperty in _floatProperties)
			{
				_propertyBlock.SetFloat(floatProperty.Key, floatProperty.Value);
			}
			renderer.SetPropertyBlock(_propertyBlock);
			_propertyBlock.Clear();
		}
	}

	public Vector3 GetCenterPosition()
	{
		_ = transform.position;
		return _bounds.center;
	}

	public Vector3 GetAbovePosition()
	{
		Vector3 position = transform.position;
		Vector3 result = _bounds.center + Vector3.up * _bounds.extents.y;
		result.x = position.x;
		result.z = position.z;
		return result;
	}

	public void UpdateAccessories()
	{
		for (int num = _instantiatedAccessories.Count - 1; num >= 0; num--)
		{
			if (_instantiatedAccessories[num] == null)
			{
				_instantiatedAccessories.RemoveAt(num);
			}
			else if (!accessories.Contains(_instantiatedAccessories[num].name))
			{
				List<Renderer> list = new List<Renderer>();
				list.AddRange(_instantiatedAccessories[num].GetComponentsInChildren<MeshRenderer>());
				list.AddRange(_instantiatedAccessories[num].GetComponentsInChildren<SkinnedMeshRenderer>());
				foreach (Renderer item in list)
				{
					_propertyRenderers.Remove(item);
				}
				UnityEngine.Object.Destroy(_instantiatedAccessories[num].gameObject);
				_instantiatedAccessories.RemoveAt(num);
			}
		}
		_currentEffectRenderers.Clear();
		if (_currentModel == null)
		{
			return;
		}
		foreach (string a in accessories)
		{
			if (!(_instantiatedAccessories.Find((Accessory acc) => acc.name == a) != null))
			{
				Accessory byName = DewResources.GetByName<Accessory>(a);
				if (!(byName == null))
				{
					Accessory accessory = UnityEngine.Object.Instantiate(byName);
					accessory.Setup(_currentModel.transform, _currentSkinName);
					accessory.name = a;
					ShortcutExtensions.DOPunchScale(accessory.transform, Vector3.one * 0.3f * accessory.transform.localScale.x, 0.4f, 10, 1f);
					_instantiatedAccessories.Add(accessory);
					List<Renderer> list2 = new List<Renderer>();
					list2.AddRange(accessory.GetComponentsInChildren<MeshRenderer>());
					list2.AddRange(accessory.GetComponentsInChildren<SkinnedMeshRenderer>());
					_propertyRenderers.AddRange(list2);
					ApplyPropertyBlocks();
				}
			}
		}
		_currentEffectRenderers.AddRange((IEnumerable<Renderer>)(object)_currentModel.GetComponentsInChildren<ParticleSystemRenderer>());
	}
}
