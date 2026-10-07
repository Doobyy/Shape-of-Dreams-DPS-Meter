using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Cells2D<T>
{
	public Vector2 center;

	public Vector2 min;

	public Vector2 max;

	public Vector2 size;

	public float cellSize;

	public T[] data;

	public int dataWidth;

	public int dataHeight;

	public Cells2D()
	{
	}

	public Cells2D(Vector2 worldCenter, float cellSize, int dataWidth, int dataHeight)
	{
		center = worldCenter;
		this.cellSize = cellSize;
		data = new T[dataWidth * dataHeight];
		this.dataWidth = dataWidth;
		this.dataHeight = dataHeight;
		min = center - new Vector2((float)dataWidth / 2f * cellSize, (float)dataHeight / 2f * cellSize);
		max = center + new Vector2((float)dataWidth / 2f * cellSize, (float)dataHeight / 2f * cellSize);
		size = max - min;
	}

	public Cells2D<T> GetCropped(int startX, int endX, int startY, int endY)
	{
		Vector2 worldPos = GetWorldPos((startX, startY));
		Vector2 worldPos2 = GetWorldPos((endX, endY));
		Cells2D<T> cells2D = new Cells2D<T>((worldPos + worldPos2) / 2f, cellSize, endX - startX + 1, endY - startY + 1);
		for (int i = startX; i <= endX; i++)
		{
			for (int j = startY; j <= endY; j++)
			{
				cells2D.Set((i - startX, j - startY), Get((i, j)));
			}
		}
		return cells2D;
	}

	public Vector2 GetWorldPos((int, int) indices)
	{
		int item = indices.Item1;
		int item2 = indices.Item2;
		float x = center.x + (float)(item - dataWidth / 2) * cellSize;
		float y = center.y + (float)(item2 - dataHeight / 2) * cellSize;
		return new Vector2(x, y);
	}

	public Vector2 GetNormalizedPos(Vector2 worldPos)
	{
		float x = (worldPos.x - min.x) / ((float)dataWidth * cellSize);
		float y = (worldPos.y - min.y) / ((float)dataHeight * cellSize);
		return new Vector2(x, y);
	}

	public T Get((int, int) indices)
	{
		var (num, num2) = indices;
		return data[num + num2 * dataWidth];
	}

	public void Set((int, int) indices, T value)
	{
		var (num, num2) = indices;
		data[num + num2 * dataWidth] = value;
	}

	public (int, int) GetClosestCell(Vector2 worldPos)
	{
		int value = Mathf.RoundToInt((worldPos.x - center.x + (float)dataWidth * cellSize * 0.5f) / cellSize);
		int value2 = Mathf.RoundToInt((worldPos.y - center.y + (float)dataHeight * cellSize * 0.5f) / cellSize);
		int item = Mathf.Clamp(value, 0, dataWidth - 1);
		value2 = Mathf.Clamp(value2, 0, dataHeight - 1);
		return (item, value2);
	}

	public bool IsInBounds((int, int) indices)
	{
		if (indices.Item1 >= 0 && indices.Item2 >= 0 && indices.Item1 < dataWidth)
		{
			return indices.Item2 < dataHeight;
		}
		return false;
	}

	public void FloodFill((int, int) start, Func<(int, int), bool> func)
	{
		Stack<(int, int)> stack = new Stack<(int, int)>();
		stack.Push(start);
		while (stack.Count > 0)
		{
			(int, int) tuple = stack.Pop();
			if (IsInBounds(tuple) && func(tuple))
			{
				stack.Push((tuple.Item1 + 1, tuple.Item2));
				stack.Push((tuple.Item1 - 1, tuple.Item2));
				stack.Push((tuple.Item1, tuple.Item2 + 1));
				stack.Push((tuple.Item1, tuple.Item2 - 1));
			}
		}
	}
}
