using System;
using System.Collections.Generic;
using UnityEngine;

public class UI_GamepadFocusableGroup : MonoBehaviour
{
	public EnterGroupBehavior enterBehavior;

	public GameObject customTarget;

	private void Start()
	{
		ManagerBase<GlobalUIManager>.instance._allGroups.Add(this);
	}

	private void OnDestroy()
	{
		if (ManagerBase<GlobalUIManager>.instance != null)
		{
			ManagerBase<GlobalUIManager>.instance._allGroups.Remove(this);
		}
	}

	public static IGamepadFocusable GetEnterFocusable(GameObject parent, EnterGroupBehavior enterBehavior, Vector2 direction)
	{
		if (enterBehavior == EnterGroupBehavior.Custom)
		{
			return null;
		}
		int num = 0;
		int num2 = 0;
		List<IGamepadFocusable> componentsInChildrenNonAlloc = parent.GetComponentsInChildrenNonAlloc(out ListReturnHandle<IGamepadFocusable> handle);
		IGamepadFocusable result = null;
		float num3 = float.NegativeInfinity;
		switch (enterBehavior)
		{
		case EnterGroupBehavior.UseDistanceFromPrevious:
			if (ManagerBase<GlobalUIManager>.instance.focused is Component component2 && component2 != null)
			{
				Vector2 center2 = ((RectTransform)component2.transform).GetScreenSpaceRect().center;
				foreach (IGamepadFocusable item in componentsInChildrenNonAlloc)
				{
					Vector2 center3 = item.GetTransform().GetScreenSpaceRect().center;
					float num6 = 1f / Vector2.Distance(center2, center3);
					if (num6 > num3 && item.CanBeFocused())
					{
						num3 = num6;
						result = item;
					}
				}
				handle.Return();
				return result;
			}
			num = -1;
			num2 = 1;
			break;
		case EnterGroupBehavior.UseEnterDirection:
			if (ManagerBase<GlobalUIManager>.instance.focused is Component component && component != null)
			{
				Vector2 center = ((RectTransform)component.transform).GetScreenSpaceRect().center;
				foreach (IGamepadFocusable item2 in componentsInChildrenNonAlloc)
				{
					Vector2 rhs = item2.GetTransform().GetScreenSpaceRect().center - center;
					float num4 = Vector2.Dot(direction.normalized, rhs);
					float num5 = 1f / num4;
					if (num5 > num3 && item2.CanBeFocused())
					{
						num3 = num5;
						result = item2;
					}
				}
				handle.Return();
				return result;
			}
			num = -1;
			num2 = 1;
			break;
		case EnterGroupBehavior.PrioritizeLT:
			num = -1;
			num2 = 1;
			break;
		case EnterGroupBehavior.PrioritizeRT:
			num = 1;
			num2 = 1;
			break;
		case EnterGroupBehavior.PrioritizeLB:
			num = -1;
			num2 = -1;
			break;
		case EnterGroupBehavior.PrioritizeRB:
			num = 1;
			num2 = -1;
			break;
		case EnterGroupBehavior.PrioritizeTop:
			num2 = 1;
			break;
		case EnterGroupBehavior.PrioritizeLeft:
			num = -1;
			break;
		case EnterGroupBehavior.PrioritizeRight:
			num = 1;
			break;
		case EnterGroupBehavior.PrioritizeBottom:
			num2 = -1;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		foreach (IGamepadFocusable item3 in componentsInChildrenNonAlloc)
		{
			Vector3 position = item3.GetTransform().position;
			float num7 = position.x * (float)num + position.y * (float)num2;
			if (num7 > num3 && item3.CanBeFocused())
			{
				num3 = num7;
				result = item3;
			}
		}
		handle.Return();
		return result;
	}

	public virtual IGamepadFocusable GetEnterFocusable(Vector2 direction)
	{
		if (enterBehavior == EnterGroupBehavior.Custom)
		{
			return customTarget.GetComponent<IGamepadFocusable>();
		}
		return GetEnterFocusable(gameObject, enterBehavior, direction);
	}

	public bool IsValid()
	{
		if (isActiveAndEnabled)
		{
			return ManagerBase<GlobalUIManager>.instance.IsUIElementClickable((RectTransform)transform);
		}
		return false;
	}
}
