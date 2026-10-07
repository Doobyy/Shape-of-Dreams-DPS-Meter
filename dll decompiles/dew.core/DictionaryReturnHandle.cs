using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public struct DictionaryReturnHandle<TKey, TValue>
{
	private readonly Dictionary<TKey, TValue> _resource;

	private bool _isReturned;

	public bool needToReturn
	{
		get
		{
			if (_resource != null)
			{
				return !_isReturned;
			}
			return false;
		}
	}

	internal DictionaryReturnHandle(Dictionary<TKey, TValue> resource)
	{
		_resource = resource;
		_isReturned = false;
	}

	public void Return()
	{
		if (_isReturned)
		{
			Debug.LogWarning("Tried to return a resource twice");
			return;
		}
		_resource.Clear();
		CollectionPool<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>.Release(_resource);
		_isReturned = true;
	}
}
