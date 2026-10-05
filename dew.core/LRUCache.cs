using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class LRUCache<TKey, TValue>
{
	private readonly int _capacity;

	private readonly Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>> _dictionary;

	private readonly LinkedList<KeyValuePair<TKey, TValue>> _list;

	public int Count => _list.Count;

	public LRUCache(int capacity)
	{
		if (capacity <= 0)
		{
			throw new ArgumentOutOfRangeException("capacity", "Capacity must be greater than zero.");
		}
		_capacity = capacity;
		_dictionary = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>(capacity);
		_list = new LinkedList<KeyValuePair<TKey, TValue>>();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGet(TKey key, out TValue value)
	{
		if (_dictionary.TryGetValue(key, out var value2))
		{
			_list.Remove(value2);
			_list.AddFirst(value2);
			value = value2.Value.Value;
			return true;
		}
		value = default;
		return false;
	}

	public void Set(TKey key, TValue value)
	{
		if (_dictionary.TryGetValue(key, out var value2))
		{
			_list.Remove(value2);
		}
		else if (_list.Count >= _capacity)
		{
			LinkedListNode<KeyValuePair<TKey, TValue>> last = _list.Last;
			_dictionary.Remove(last.Value.Key);
			_list.RemoveLast();
		}
		LinkedListNode<KeyValuePair<TKey, TValue>> value3 = _list.AddFirst(new KeyValuePair<TKey, TValue>(key, value));
		_dictionary[key] = value3;
	}

	public void Clear()
	{
		_list.Clear();
		_dictionary.Clear();
	}
}
