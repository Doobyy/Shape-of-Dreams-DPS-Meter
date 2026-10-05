using UnityEngine;

public static class IInteractableUnityNullCheckExtension
{
	public static bool IsUnityNull(this IInteractable interactable)
	{
		if (interactable is Object obj)
		{
			return obj == null;
		}
		return interactable == null;
	}
}
