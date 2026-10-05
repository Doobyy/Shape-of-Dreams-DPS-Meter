using System;
using UnityEngine;

public class UI_GamepadNavigationHint : MonoBehaviour, IGamepadNavigationHint
{
	[Serializable]
	public struct NavigationSettings
	{
		public GamepadNavigation navigation;

		public GameObject customTarget;

		public bool limitToChildren;

		public bool canFallbackToGeneric;
	}

	public NavigationSettings navigation;

	public bool useDifferentNavigationsPerDirection;

	[Header("Up")]
	public NavigationSettings up;

	[Header("Left")]
	public NavigationSettings left;

	[Header("Down")]
	public NavigationSettings down;

	[Header("Right")]
	public NavigationSettings right;

	public bool TryGetUp(out IGamepadFocusable next)
	{
		return TryGet(Vector3.up, up, out next);
	}

	public bool TryGetLeft(out IGamepadFocusable next)
	{
		return TryGet(Vector3.left, left, out next);
	}

	public bool TryGetRight(out IGamepadFocusable next)
	{
		return TryGet(Vector3.right, right, out next);
	}

	public bool TryGetDown(out IGamepadFocusable next)
	{
		return TryGet(Vector3.down, down, out next);
	}

	private bool TryGet(Vector3 direction, NavigationSettings nav, out IGamepadFocusable next)
	{
		if (!useDifferentNavigationsPerDirection)
		{
			nav = navigation;
		}
		next = null;
		Transform t = transform;
		Func<IGamepadFocusable, bool> condition = (nav.limitToChildren ? ((Func<IGamepadFocusable, bool>)((IGamepadFocusable focusable) => focusable.GetTransform().IsSelfOrDescendantOf(t))) : null);
		GetFocusTargetSettings settings = new GetFocusTargetSettings
		{
			direction = direction,
			condition = condition
		};
		ManagerBase<GlobalUIManager>.instance.PopulateCacheData(ref settings, out var handle);
		switch (nav.navigation)
		{
		case GamepadNavigation.Generic:
			ManagerBase<GlobalUIManager>.instance.TryGetFocusTargetGeneric(direction, condition, out next);
			break;
		case GamepadNavigation.Grid:
			settings.angleLimit = new Vector2(0f, 1f);
			settings.normalizedPerpendicularDistLimit = new Vector2(0f, 0.005f);
			settings.normalizedDistLimit = new Vector2(0f, 0.1f);
			if (!ManagerBase<GlobalUIManager>.instance.TryGetFocusTarget(settings, out next))
			{
				settings.angleLimit = new Vector2(0f, 45f);
				settings.normalizedPerpendicularDistLimit = new Vector2(0f, 0.05f);
				settings.normalizedDistLimit = new Vector2(0f, 0.2f);
				ManagerBase<GlobalUIManager>.instance.TryGetFocusTarget(settings, out next);
			}
			break;
		case GamepadNavigation.GridWrapAround:
			settings.angleLimit = new Vector2(0f, 0.5f);
			settings.normalizedPerpendicularDistLimit = new Vector2(0f, 0.005f);
			settings.normalizedDistLimit = new Vector2(0f, 0.1f);
			if (!ManagerBase<GlobalUIManager>.instance.TryGetFocusTarget(settings, out next))
			{
				Vector3 focused = ManagerBase<GlobalUIManager>.instance.focused.GetTransform().position;
				settings.direction = -direction;
				settings.angleLimit = new Vector2(0f, 1f);
				settings.normalizedPerpendicularDistLimit = new Vector2(0f, 0.01f);
				settings.normalizedDistLimit = new Vector2(0f, float.PositiveInfinity);
				settings.customScoreFunc = (IGamepadFocusable f) => Vector3.Distance(f.GetTransform().position, focused);
				ManagerBase<GlobalUIManager>.instance.TryGetFocusTarget(settings, out next);
			}
			break;
		case GamepadNavigation.GridLong:
			settings.angleLimit = new Vector2(0f, 0.5f);
			settings.normalizedPerpendicularDistLimit = new Vector2(0f, 0.005f);
			settings.normalizedDistLimit = new Vector2(0f, 0.3f);
			if (!ManagerBase<GlobalUIManager>.instance.TryGetFocusTarget(settings, out next))
			{
				settings.angleLimit = new Vector2(0f, 45f);
				settings.normalizedPerpendicularDistLimit = new Vector2(0f, 0.05f);
				settings.normalizedDistLimit = new Vector2(0f, 0.2f);
				ManagerBase<GlobalUIManager>.instance.TryGetFocusTarget(settings, out next);
			}
			break;
		case GamepadNavigation.Wide:
			settings.angleLimit = new Vector2(0f, 80f);
			settings.normalizedPerpendicularDistLimit = new Vector2(0f, 1f);
			settings.normalizedDistLimit = new Vector2(0.2f, 1f);
			ManagerBase<GlobalUIManager>.instance.TryGetFocusTarget(settings, out next);
			break;
		case GamepadNavigation.HeroInfoBar:
		{
			settings.angleLimit = new Vector2(0f, 80f);
			settings.normalizedDistLimit = new Vector2(0f, 0.05f);
			Vector3 current = ManagerBase<GlobalUIManager>.instance.focused.GetTransform().position;
			settings.customScoreFunc = (IGamepadFocusable f) =>
			{
				Vector3 position = f.GetTransform().position;
				return 1f / (position.x + Mathf.Abs(position.y - current.y) * 50f);
			};
			ManagerBase<GlobalUIManager>.instance.TryGetFocusTarget(settings, out next);
			break;
		}
		case GamepadNavigation.Custom:
		{
			if (nav.customTarget != null && nav.customTarget.TryGetComponent<IGamepadFocusable>(out var component) && component.CanBeFocused())
			{
				next = component;
			}
			if (nav.customTarget != null && nav.customTarget.TryGetComponent<UI_GamepadFocusableGroup>(out var component2) && component2.IsValid())
			{
				next = component2.GetEnterFocusable(direction);
			}
			break;
		}
		default:
			throw new ArgumentOutOfRangeException("navigation", nav.navigation, null);
		}
		handle();
		if (next == null && nav.canFallbackToGeneric)
		{
			return false;
		}
		return true;
	}
}
