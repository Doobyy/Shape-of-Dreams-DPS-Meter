using System;
using System.Collections.Generic;
using UnityEngine;

public class ComponentCache<TKey, TValue> where TKey : UnityEngine.Object where TValue : class
{
	public enum ComponentSource
	{
		Self,
		Children,
		Parent
	}

	public ComponentSource componentSource;

	private readonly Dictionary<TKey, TValue> _cache;

	private readonly int _capacity;

	private List<TKey> _sweepBuffer;

	public ComponentCache(int capacity, ComponentSource source)
	{
		if (capacity <= 0)
		{
			throw new ArgumentOutOfRangeException("capacity", "Capacity must be greater than zero.");
		}
		_capacity = capacity;
		_cache = new Dictionary<TKey, TValue>(capacity);
		componentSource = source;
	}

	public TValue GetOrAdd(TKey key)
	{
		if (key == null)
		{
			return null;
		}
		if (_cache.TryGetValue(key, out var value))
		{
			if (!(value is UnityEngine.Object obj) || !(obj == null))
			{
				return value;
			}
			_cache.Remove(key);
		}
		TValue val = null;
		if (key is Component component)
		{
			val = componentSource switch
			{
				ComponentSource.Self => component.GetComponent<TValue>(), 
				ComponentSource.Children => component.GetComponentInChildren<TValue>(), 
				ComponentSource.Parent => component.GetComponentInParent<TValue>(), 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}
		else if (key is GameObject gameObject)
		{
			val = componentSource switch
			{
				ComponentSource.Self => gameObject.GetComponent<TValue>(), 
				ComponentSource.Children => gameObject.GetComponentInChildren<TValue>(), 
				ComponentSource.Parent => gameObject.GetComponentInParent<TValue>(), 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}
		if (_cache.Count >= _capacity)
		{
			if (_sweepBuffer == null)
			{
				_sweepBuffer = new List<TKey>();
			}
			_sweepBuffer.Clear();
			foreach (KeyValuePair<TKey, TValue> item in _cache)
			{
				if (item.Key == null || (item.Value is UnityEngine.Object obj2 && obj2 == null))
				{
					_sweepBuffer.Add(item.Key);
				}
			}
			for (int i = 0; i < _sweepBuffer.Count; i++)
			{
				_cache.Remove(_sweepBuffer[i]);
			}
			_sweepBuffer.Clear();
			if (_cache.Count >= _capacity)
			{
				_cache.Clear();
			}
		}
		_cache[key] = val;
		return val;
	}

	public void Clear()
	{
		_cache.Clear();
	}
}
