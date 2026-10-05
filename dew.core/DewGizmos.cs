using System;
using UnityEngine;

public static class DewGizmos
{
	private const float RiftGizmosStarEdgeLength = 1.5f;

	private const float RiftGizmosDirectionLineLength = 2f;

	private const float RiftGizmosPositionY = 2.35f;

	public static void DrawLine(Vector3 start, Vector3 end, Color color)
	{
	}

	public static void DrawArrow(Vector3 start, Vector3 end, Color color, float headSize)
	{
	}

	public static void DrawText(string text, Vector3 pos, Color color, int fontSize)
	{
	}

	public static void DrawCircle(Vector3 center, Vector3 normal, float radius, Color color, int segments = 32)
	{
	}

	public static void DrawRift(Color color, Vector3 position, Quaternion rotation)
	{
		Color color2 = Gizmos.color;
		Gizmos.color = color;
		Vector3 vector = position + new Vector3(0f, 2.35f, 0f);
		int num = 10;
		Vector3[] array = new Vector3[num];
		for (int i = 0; i < num; i++)
		{
			float num2 = ((i % 2 == 0) ? 1.5f : 0.75f);
			float f = (float)i * (float)Math.PI / (float)(num / 2) + (float)Math.PI / 2f;
			array[i] = vector + rotation * new Vector3(Mathf.Cos(f), Mathf.Sin(f), 0f) * num2;
		}
		for (int j = 0; j < num; j++)
		{
			int num3 = (j + 1) % num;
			Gizmos.DrawLine(array[j], array[num3]);
		}
		Vector3 vector2 = rotation * Vector3.forward;
		for (int k = 0; k < num; k += 2)
		{
			Gizmos.DrawLine(array[k], array[k] + vector2 * 2f);
		}
		Gizmos.color = color2;
	}
}
