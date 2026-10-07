using UnityEngine;

public static class RectTransformGetWorldRectExtension
{
	public static Rect GetWorldRect(this RectTransform rt, Vector2 scale)
	{
		Vector3[] array = new Vector3[4];
		rt.GetWorldCorners(array);
		Vector3 vector = array[0];
		Vector2 size = new Vector2(scale.x * rt.rect.size.x, scale.y * rt.rect.size.y);
		return new Rect(vector, size);
	}

	public static Rect GetScreenSpaceRect(this RectTransform transform)
	{
		Vector2 vector = Vector2.Scale(transform.rect.size, transform.lossyScale);
		return new Rect((Vector2)transform.position - vector * transform.pivot, vector);
	}
}
