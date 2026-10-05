using UnityEngine;

public static class DewDebug
{
	public static void DrawRect(Rect rect, Color color, float duration)
	{
		Vector2 vector = new Vector2(rect.xMin, rect.yMin);
		Vector2 vector2 = new Vector2(rect.xMin, rect.yMax);
		Vector2 vector3 = new Vector2(rect.xMax, rect.yMax);
		Vector2 vector4 = new Vector2(rect.xMax, rect.yMin);
		Debug.DrawLine(vector, vector2, color, duration);
		Debug.DrawLine(vector2, vector3, color, duration);
		Debug.DrawLine(vector3, vector4, color, duration);
		Debug.DrawLine(vector4, vector, color, duration);
	}
}
