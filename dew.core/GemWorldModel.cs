using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class GemWorldModel : ItemWorldModel
{
	protected override void Awake()
	{
		base.Awake();
		Gem gem = (Gem)item;
		gem.onDismantleProgressChanged = (Action<float, float>)Delegate.Combine(gem.onDismantleProgressChanged, new Action<float, float>(OnDismantleProgressChanged));
		EditSkillManager instance = ManagerBase<EditSkillManager>.instance;
		instance.OnGemVisibleClientState = (Action<Gem, bool>)Delegate.Combine(instance.OnGemVisibleClientState, new Action<Gem, bool>(GemVisibleClientState));
		gem.ClientEvent_OnTempOwnerChanged += (Action<DewPlayer, DewPlayer>)((DewPlayer _, DewPlayer _) =>
		{
			UpdateVisibility();
		});
		gem.ClientEvent_OnHandOwnerChanged += (Action<Hero, Hero>)((Hero _, Hero _) =>
		{
			UpdateVisibility();
		});
		gem.ClientEvent_OnOwnerChanged += (Action<Hero, Hero>)((Hero _, Hero _) =>
		{
			UpdateVisibility();
		});
	}

	private void GemVisibleClientState(Gem gem, bool visible)
	{
		if (!((UnityEngine.Object)(object)(Gem)item != (UnityEngine.Object)(object)gem))
		{
			UpdateVisibilityLocal(!visible);
		}
	}

	protected override void OnAppearInAnimation()
	{
		base.OnAppearInAnimation();
		Vector3 localPosition = iconQuad.transform.localPosition;
		iconQuad.transform.localPosition = Vector3.up * 2f;
		TweenSettingsExtensions.Append(DOTween.Sequence(), (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOLocalMoveY(iconQuad.transform, localPosition.y, 0.8f, false), (Ease)30));
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		ShortcutExtensions.DOKill((Component)iconQuad.transform, true);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (ManagerBase<EditSkillManager>.instance != null)
		{
			EditSkillManager instance = ManagerBase<EditSkillManager>.instance;
			instance.OnGemVisibleClientState = (Action<Gem, bool>)Delegate.Remove(instance.OnGemVisibleClientState, new Action<Gem, bool>(GemVisibleClientState));
		}
	}

	protected override Texture GetIconTexture()
	{
		return ((Gem)item).icon.texture;
	}
}
