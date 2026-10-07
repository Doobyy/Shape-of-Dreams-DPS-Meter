using System.Collections.Generic;
using UnityEngine;

public static class PolygonTools
{
	public static float[] _areas = new float[128];

	public static void GetVerticesArray(IReadOnlyList<Vector2> points, ref Vector2[] verts)
	{
		for (int i = 0; i < points.Count - 2; i++)
		{
			verts[i * 3] = points[i];
			verts[i * 3 + 1] = points[i + 1];
			verts[i * 3 + 2] = points[i + 2];
		}
	}

	public static float GetArea(IReadOnlyList<Vector2> vertices)
	{
		float num = 0f;
		for (int i = 0; i < vertices.Count / 3; i++)
		{
			Vector2 v = vertices[i * 3 + 1] - vertices[i * 3];
			Vector2 v2 = vertices[i * 3 + 2] - vertices[i * 3];
			float num2 = Mathf.Abs(v.Cross(v2));
			_areas[i] = num2;
			num += num2;
		}
		return num;
	}

	public static int PickRandomTriangleWeightedByArea(IReadOnlyList<Vector2> vertices, ref Vector2[] triVerts)
	{
		float area = GetArea(vertices);
		float num = Random.Range(0f, area);
		for (int i = 0; i < vertices.Count / 3; i++)
		{
			if (num < _areas[i])
			{
				triVerts[0] = vertices[i * 3];
				triVerts[1] = vertices[i * 3 + 1];
				triVerts[2] = vertices[i * 3 + 2];
				return i;
			}
			num -= _areas[i];
		}
		triVerts[0] = vertices[vertices.Count - 3];
		triVerts[1] = vertices[vertices.Count - 2];
		triVerts[2] = vertices[vertices.Count - 1];
		return vertices.Count / 3 - 1;
	}

	public static Vector2 GetRandomPositionInTriangle(Vector2[] triVerts, DewRandom random = null)
	{
		if (random == null)
		{
			random = DewRandom.instance;
		}
		float num = Mathf.Sqrt(random.Range(0f, 1f));
		float num2 = random.Range(0f, 1f);
		float num3 = 1f - num;
		float num4 = num * (1f - num2);
		float num5 = num2 * num;
		return num3 * triVerts[0] + num4 * triVerts[1] + num5 * triVerts[2];
	}
}
