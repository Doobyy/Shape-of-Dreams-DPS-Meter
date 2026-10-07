using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointsToTriangleVertices : IReadOnlyList<Vector2>, IEnumerable<Vector2>, IEnumerable, IReadOnlyCollection<Vector2>
{
	private IReadOnlyList<Vector2> _points;

	private Vector2 _center;

	public int Count => _points.Count * 3;

	public Vector2 this[int index]
	{
		get
		{
			if (index >= Count || index < 0)
			{
				throw new IndexOutOfRangeException("index");
			}
			switch (index % 3)
			{
			case 0:
				return _center;
			case 1:
				return _points[index / 3];
			default:
			{
				int num = index / 3 + 1;
				if (num >= _points.Count)
				{
					num = 0;
				}
				return _points[num];
			}
			}
		}
	}

	public PointsToTriangleVertices(IReadOnlyList<Vector2> points)
	{
		_points = points;
		Vector2 center = default;
		for (int i = 0; i < _points.Count; i++)
		{
			Vector2 vector = _points[i];
			center += vector;
		}
		center /= (float)_points.Count;
		_center = center;
	}

	public IEnumerator<Vector2> GetEnumerator()
	{
		for (int i = 0; i < Count; i++)
		{
			yield return this[i];
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
