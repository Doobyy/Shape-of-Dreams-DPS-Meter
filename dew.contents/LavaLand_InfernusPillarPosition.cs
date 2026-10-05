using System.Collections.Generic;
using UnityEngine;

public class LavaLand_InfernusPillarPosition : SingletonBehaviour<LavaLand_InfernusPillarPosition>
{
	private List<int> _indices = new List<int>();

	public Vector3 GetRandomPosition()
	{
		if (_indices.Count == 0)
		{
			for (int i = 0; i < transform.childCount; i++)
			{
				_indices.Add(i);
			}
			_indices.Shuffle();
		}
		int index = _indices[0];
		_indices.RemoveAt(0);
		return transform.GetChild(index).position;
	}
}
