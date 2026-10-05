using System;
using UnityEngine;

public static class RectTransformSetWorldPositionExtension
{
	public static void SetWorldPosition(this RectTransform rt, Vector3 worldPosition)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Invalid comparison between Unknown and I4
		Canvas componentInParent = rt.GetComponentInParent<Canvas>();
		if ((UnityEngine.Object)(object)componentInParent == null)
		{
			throw new NullReferenceException("Cannot set the world position of a RectTransform without a parent canvas");
		}
		if ((int)componentInParent.renderMode == 0)
		{
			rt.SetWorldPositionForScreenSpaceOverlay(worldPosition, Camera.main);
			return;
		}
		if ((int)componentInParent.renderMode == 1)
		{
			rt.SetWorldPositionForScreenSpaceCamera(worldPosition, componentInParent);
			return;
		}
		throw new InvalidOperationException("Cannot set the world position of a RectTransform in a World Space canvas");
	}

	public static void SetWorldPositionForScreenSpaceOverlay(this RectTransform rt, Vector3 position, Camera camera)
	{
		rt.position = camera.WorldToScreenPoint(position);
		rt.rotation = Quaternion.identity;
	}

	public static void SetWorldPositionForScreenSpaceCamera(this RectTransform rt, Vector3 position, Canvas canvas)
	{
		if (canvas.worldCamera == null)
		{
			throw new NullReferenceException("Canvas in Screen Space - Camera doesn't have its world camera assigned");
		}
		Vector3 position2 = default;
		RectTransformUtility.ScreenPointToWorldPointInRectangle((RectTransform)((Component)(object)canvas).transform, (Vector2)canvas.worldCamera.WorldToScreenPoint(position), canvas.worldCamera, ref position2);
		rt.position = position2;
		rt.rotation = ((Component)(object)canvas).transform.rotation;
	}
}
