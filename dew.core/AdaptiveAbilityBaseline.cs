using System.Collections.Generic;
using UnityEngine;

public static class AdaptiveAbilityBaseline
{
	private struct Entry
	{
		public int maxUsedCount;

		public int lastUsedCount;

		public int roomsSinceLastSeen;

		public int sampleCount;

		public int monsterCountAtMax;
	}

	private static readonly Dictionary<(uint assetId, uint ownerNetId), Entry> _data = new Dictionary<(uint, uint), Entry>();

	private static readonly Dictionary<(uint assetId, uint ownerNetId), int> _currentRoomUsage = new Dictionary<(uint, uint), int>();

	private const int kDropAfterRoomsNotSeen = 3;

	private const int kMaxAdaptivePrewarmPerPrefab = 1000;

	private static int _currentRoomMonsterCount;

	public static void SetCurrentRoomMonsterCount(int count)
	{
		_currentRoomMonsterCount = count;
	}

	public static void IncrementCurrentRoomUsage(uint assetId, uint ownerNetId)
	{
		if (assetId != 0)
		{
			(uint, uint) key = (assetId, ownerNetId);
			_currentRoomUsage.TryGetValue(key, out var value);
			_currentRoomUsage[key] = value + 1;
		}
	}

	public static void RecordPeakActiveUsage(uint assetId, uint ownerNetId, int activeCount)
	{
		if (assetId != 0)
		{
			(uint, uint) key = (assetId, ownerNetId);
			_currentRoomUsage.TryGetValue(key, out var value);
			if (activeCount > value)
			{
				_currentRoomUsage[key] = activeCount;
			}
		}
	}

	public static void CommitCurrentRoomToLastUsage(bool wasBossRoom)
	{
		if (wasBossRoom)
		{
			_currentRoomUsage.Clear();
			return;
		}
		foreach (KeyValuePair<(uint, uint), int> item in _currentRoomUsage)
		{
			RecordRoom(item.Key, item.Value, _currentRoomMonsterCount);
		}
		List<(uint, uint)> list = null;
		foreach (var item2 in new List<(uint, uint)>(_data.Keys))
		{
			if (_currentRoomUsage.ContainsKey(item2))
			{
				continue;
			}
			Entry value = _data[item2];
			value.roomsSinceLastSeen++;
			if (value.roomsSinceLastSeen >= 3)
			{
				if (list == null)
				{
					list = new List<(uint, uint)>();
				}
				list.Add(item2);
			}
			else
			{
				_data[item2] = value;
			}
		}
		if (list != null)
		{
			foreach (var item3 in list)
			{
				_data.Remove(item3);
			}
		}
		_currentRoomUsage.Clear();
	}

	public static void PopulateFromPeakUsage(Dictionary<(uint assetId, uint owner), int> output, float margin, int newRoomMonsterCount)
	{
		if (output == null)
		{
			return;
		}
		foreach (KeyValuePair<(uint, uint), Entry> datum in _data)
		{
			if (datum.Value.maxUsedCount > 0 && datum.Key.Item1 != 0)
			{
				float num = (float)datum.Value.maxUsedCount * margin;
				if (datum.Value.monsterCountAtMax > 0 && newRoomMonsterCount > datum.Value.monsterCountAtMax)
				{
					float num2 = (float)newRoomMonsterCount / (float)datum.Value.monsterCountAtMax;
					num *= num2;
				}
				int num3 = Mathf.CeilToInt(num);
				if (num3 > 1000)
				{
					num3 = 1000;
				}
				output.TryGetValue(datum.Key, out var value);
				if (num3 > value)
				{
					output[datum.Key] = num3;
				}
			}
		}
	}

	private static void RecordRoom((uint, uint) key, int usedCount, int monsterCount)
	{
		if (!_data.TryGetValue(key, out var value))
		{
			value = default;
		}
		if (usedCount > value.maxUsedCount)
		{
			value.maxUsedCount = usedCount;
			value.monsterCountAtMax = monsterCount;
		}
		value.lastUsedCount = usedCount;
		value.roomsSinceLastSeen = 0;
		value.sampleCount++;
		_data[key] = value;
	}

	public static void Clear()
	{
		_data.Clear();
		_currentRoomUsage.Clear();
		_currentRoomMonsterCount = 0;
	}
}
