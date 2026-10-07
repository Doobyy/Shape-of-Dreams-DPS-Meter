using System;
using DG.Tweening;
using DG.Tweening.Core;
using UnityEngine;
using UnityEngine.UI.Extensions;

[DewResourceLink(ResourceLinkBy.Name)]
public class Emote : MonoBehaviour, ICosmetic
{
	private enum InitMode
	{
		Normal,
		Preview,
		Stationary
	}

	public static readonly string[] DefaultUnlocks = new string[7] { "Emote_LeafPuppy_ThumbsUp", "Emote_LeafPuppy_GG", "Emote_LeafPuppy_Happy", "Emote_LeafPuppy_Hi", "Emote_LeafPuppy_Negative", "Emote_LeafPuppy_Sad", "Emote_LeafPuppy_Shocked" };

	public string category;

	public string heroType;

	public bool generatedFromServer;

	public string[] dlcIds;

	public CosmeticPurchasePrerequisite condition;

	public string conditionType;

	public int conditionValue;

	public int price = 200;

	public float customDuration;

	[NonSerialized]
	public float? durationOverride;

	public Func<Vector3> posGetter;

	private InitMode _mode;

	bool ICosmetic.generatedFromServer => generatedFromServer;

	string[] ICosmetic.dlcIds => dlcIds;

	private void Start()
	{
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected Obj, but got Unknown
		if (_mode == InitMode.Stationary)
		{
			return;
		}
		DewEffect.Play(gameObject);
		transform.localScale *= 0.85f;
		if (_mode == InitMode.Normal)
		{
			UpdatePosition();
			TweenSettingsExtensions.SetUpdate<Sequence>(TweenSettingsExtensions.AppendCallback(TweenSettingsExtensions.Append(TweenSettingsExtensions.AppendInterval(DOTween.Sequence(), durationOverride ?? ((customDuration > 0.0001f) ? customDuration : 1.6f)), (Tween)(object)ShortcutExtensions.DOScale(transform, Vector3.zero, 0.15f)), (TweenCallback)(() =>
			{
				UnityEngine.Object.Destroy(gameObject);
			})), true);
		}
	}

	public void SetupPreview()
	{
		if (!Application.IsPlaying(this))
		{
			throw new InvalidOperationException();
		}
		_mode = InitMode.Preview;
	}

	public void SetupStationary()
	{
		if (!Application.IsPlaying(this))
		{
			throw new InvalidOperationException();
		}
		_mode = InitMode.Stationary;
		RotateTransform[] componentsInChildren = GetComponentsInChildren<RotateTransform>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			UnityEngine.Object.Destroy(componentsInChildren[i]);
		}
		UIParticleSystem[] componentsInChildren2 = GetComponentsInChildren<UIParticleSystem>(includeInactive: true);
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)componentsInChildren2[i]);
		}
		ParticleSystem[] componentsInChildren3 = GetComponentsInChildren<ParticleSystem>(includeInactive: true);
		for (int i = 0; i < componentsInChildren3.Length; i++)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)componentsInChildren3[i]);
		}
		DOTweenAnimation[] componentsInChildren4 = GetComponentsInChildren<DOTweenAnimation>(includeInactive: true);
		foreach (DOTweenAnimation obj in componentsInChildren4)
		{
			((ABSAnimationComponent)obj).DOComplete();
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)obj);
		}
		Transform transform = base.transform.FindDeepChild("Glow0");
		Transform transform2 = base.transform.FindDeepChild("Glow1");
		Transform transform3 = base.transform.FindDeepChild("GlowShadow");
		if (transform != null)
		{
			transform.localScale *= 0.75f;
		}
		if (transform2 != null)
		{
			transform2.localScale *= 0.75f;
		}
		if (transform3 != null)
		{
			transform3.localScale *= 0.75f;
		}
		foreach (Transform item in base.transform.FindDeepChildren("Extra Layer"))
		{
			item.gameObject.SetActive(value: false);
		}
	}

	private void LateUpdate()
	{
		if (_mode == InitMode.Normal)
		{
			UpdatePosition();
		}
	}

	private void UpdatePosition()
	{
		try
		{
			transform.position = posGetter();
		}
		catch (Exception)
		{
			UnityEngine.Object.Destroy(gameObject);
		}
	}

	public void CheckPrerequisites(out bool canPurchase, out int currentProgress, out int maxProgress, out string conditionText)
	{
		Dew.CheckCosmeticPrerequisites(name, category, heroType, condition, conditionType, conditionValue, out canPurchase, out currentProgress, out maxProgress, out conditionText);
	}
}
