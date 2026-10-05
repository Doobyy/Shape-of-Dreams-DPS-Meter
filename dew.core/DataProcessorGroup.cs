using System;
using System.Collections.Generic;
using UnityEngine;

public class DataProcessorGroup<TData, TFrom, TTo> : IPoolClearable where TData : struct
{
	private struct DataProcessorEntry
	{
		public int priority;

		public DataProcessor<TData, TFrom, TTo> processor;
	}

	private List<DataProcessorEntry> _entries;

	public int count
	{
		get
		{
			if (_entries != null)
			{
				return _entries.Count;
			}
			return 0;
		}
	}

	public void Add(DataProcessor<TData, TFrom, TTo> processor, int priority = 0)
	{
		if (_entries == null)
		{
			_entries = new List<DataProcessorEntry>();
		}
		int index = _entries.Count;
		for (int i = 0; i < _entries.Count; i++)
		{
			if (_entries[i].priority > priority)
			{
				index = i;
				break;
			}
		}
		_entries.Insert(index, new DataProcessorEntry
		{
			priority = priority,
			processor = processor
		});
	}

	public void Remove(DataProcessor<TData, TFrom, TTo> processor)
	{
		if (_entries == null || processor == null)
		{
			return;
		}
		for (int num = _entries.Count - 1; num >= 0; num--)
		{
			if (_entries[num].processor == processor)
			{
				_entries.RemoveAt(num);
			}
		}
	}

	public void Process(ref TData data, TFrom from, TTo to)
	{
		if (_entries == null)
		{
			return;
		}
		for (int i = 0; i < _entries.Count; i++)
		{
			try
			{
				if (_entries[i].processor != null)
				{
					_entries[i].processor(ref data, from, to);
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	public unsafe static void ProcessMerged(List<DataProcessorGroup<TData, TFrom, TTo>> groups, ref TData data, TFrom from, TTo to)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		int num = groups.Count;
		switch (num)
		{
		case 0:
			return;
		case 1:
			groups[0].Process(ref data, from, to);
			return;
		}
		int num2 = num;
		Span<int> val = new Span<int>((void*)stackalloc byte[(int)checked(unchecked((nuint)(uint)num2) * (nuint)4u)], num2);
		while (true)
		{
			int num3 = -1;
			int num4 = int.MaxValue;
			for (int i = 0; i < num; i++)
			{
				List<DataProcessorEntry> entries = groups[i]._entries;
				if (entries != null && val[i] < entries.Count)
				{
					int priority = entries[val[i]].priority;
					if (priority < num4)
					{
						num4 = priority;
						num3 = i;
					}
				}
			}
			if (num3 < 0)
			{
				break;
			}
			List<DataProcessorEntry> entries2 = groups[num3]._entries;
			int index = val[num3]++;
			try
			{
				if (entries2[index].processor != null)
				{
					entries2[index].processor(ref data, from, to);
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	public void Clear()
	{
		if (_entries != null)
		{
			_entries.Clear();
		}
	}
}
public class DataProcessorGroup<T> : IPoolClearable where T : struct
{
	private struct DataProcessorEntry
	{
		public int priority;

		public DataProcessor<T> processor;
	}

	private List<DataProcessorEntry> _entries;

	public Action onChanged;

	public int count
	{
		get
		{
			if (_entries != null)
			{
				return _entries.Count;
			}
			return 0;
		}
	}

	public void Add(DataProcessor<T> processor, int priority = 0)
	{
		if (_entries == null)
		{
			_entries = new List<DataProcessorEntry>();
		}
		int index = _entries.Count;
		for (int i = 0; i < _entries.Count; i++)
		{
			if (_entries[i].priority > priority)
			{
				index = i;
				break;
			}
		}
		_entries.Insert(index, new DataProcessorEntry
		{
			priority = priority,
			processor = processor
		});
		onChanged?.Invoke();
	}

	public void Remove(DataProcessor<T> processor)
	{
		if (_entries == null || processor == null)
		{
			return;
		}
		for (int num = _entries.Count - 1; num >= 0; num--)
		{
			if (_entries[num].processor == processor)
			{
				_entries.RemoveAt(num);
			}
		}
		onChanged?.Invoke();
	}

	public void Process(ref T data)
	{
		if (_entries == null)
		{
			return;
		}
		for (int i = 0; i < _entries.Count; i++)
		{
			try
			{
				if (_entries[i].processor != null)
				{
					_entries[i].processor(ref data);
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	public void Clear()
	{
		if (_entries != null)
		{
			_entries.Clear();
		}
		onChanged?.Invoke();
	}
}
