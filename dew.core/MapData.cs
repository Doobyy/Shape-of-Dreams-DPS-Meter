using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MapData
{
	private const float MapMarginDistance = 5f;

	private const float NodeDistanceFromWall = 2.5f;

	public Cells2D<MapCellType> cells;

	private FlatTupleListWrapper<int> _ipnWrapper;

	[SerializeField]
	private List<int> _ipnFlat;

	private FlatTupleListWrapper<int> _opnWrapper;

	[SerializeField]
	private List<int> _opnFlat;

	public float area;

	public IReadOnlyList<(int, int)> innerPropNodeIndices
	{
		get
		{
			if (_ipnWrapper == null)
			{
				_ipnWrapper = new FlatTupleListWrapper<int>();
			}
			if (_ipnWrapper.list != _ipnFlat)
			{
				_ipnWrapper.list = _ipnFlat;
			}
			return _ipnWrapper;
		}
	}

	public IReadOnlyList<(int, int)> outerPropNodeIndices
	{
		get
		{
			if (_opnWrapper == null)
			{
				_opnWrapper = new FlatTupleListWrapper<int>();
			}
			if (_opnWrapper.list != _opnFlat)
			{
				_opnWrapper.list = _opnFlat;
			}
			return _opnWrapper;
		}
	}

	public MapData()
	{
	}

	public MapData(Cells2D<MapCellType> raw, int minX, int maxX, int minY, int maxY)
	{
		int num = Mathf.RoundToInt(5f / raw.cellSize);
		cells = raw.GetCropped(minX - num, maxX + num, minY - num, maxY + num);
		_ipnFlat = new List<int>();
		_opnFlat = new List<int>();
		float num2 = cells.cellSize * cells.cellSize;
		int a = Mathf.RoundToInt(2.5f / cells.cellSize);
		a = Mathf.Max(a, 1);
		for (int i = 0; i < cells.dataHeight; i++)
		{
			for (int j = 0; j < cells.dataWidth; j++)
			{
				if (cells.Get((j, i)) != MapCellType.Playable)
				{
					continue;
				}
				area += num2;
				bool flag = false;
				for (int k = i - a; k <= i + a; k++)
				{
					for (int l = j - a; l <= j + a; l++)
					{
						if (cells.Get((l, k)) != MapCellType.Playable)
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						break;
					}
				}
				if (!flag)
				{
					_ipnFlat.Add(j);
					_ipnFlat.Add(i);
				}
				else
				{
					_opnFlat.Add(j);
					_opnFlat.Add(i);
				}
			}
		}
	}
}
