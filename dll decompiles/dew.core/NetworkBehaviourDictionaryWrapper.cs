using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;

public class NetworkBehaviourDictionaryWrapper<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>> where TValue : NetworkBehaviour
{
	private readonly IDictionary<TKey, SyncedNetworkBehaviour> _wrapped;

	public int Count => _wrapped.Count;

	public bool IsReadOnly => _wrapped.IsReadOnly;

	public TValue this[TKey key]
	{
		get
		{
			return (TValue)(object)(NetworkBehaviour)_wrapped[key];
		}
		set
		{
			_wrapped[key] = (NetworkBehaviour)(object)value;
		}
	}

	IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => ((IDictionary<TKey, TValue>)this).Keys;

	IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => ((IDictionary<TKey, TValue>)this).Values;

	public ICollection<TKey> Keys => _wrapped.Keys;

	public ICollection<TValue> Values
	{
		get
		{
			TValue[] array = new TValue[Count];
			int num = 0;
			foreach (SyncedNetworkBehaviour value in _wrapped.Values)
			{
				array[num] = (TValue)(object)(NetworkBehaviour)value;
				num++;
			}
			return array;
		}
	}

	public NetworkBehaviourDictionaryWrapper(IDictionary<TKey, SyncedNetworkBehaviour> wrapped)
	{
		_wrapped = wrapped;
	}

	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		return _wrapped.Select((KeyValuePair<TKey, SyncedNetworkBehaviour> e) => new KeyValuePair<TKey, TValue>(e.Key, (TValue)(object)(NetworkBehaviour)e.Value)).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Add(KeyValuePair<TKey, TValue> item)
	{
		_wrapped.Add(new KeyValuePair<TKey, SyncedNetworkBehaviour>(item.Key, (NetworkBehaviour)(object)item.Value));
	}

	public void Clear()
	{
		_wrapped.Clear();
	}

	public bool Contains(KeyValuePair<TKey, TValue> item)
	{
		return _wrapped.Contains(new KeyValuePair<TKey, SyncedNetworkBehaviour>(item.Key, (NetworkBehaviour)(object)item.Value));
	}

	public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
	{
		int num = 0;
		using IEnumerator<KeyValuePair<TKey, TValue>> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<TKey, TValue> current = enumerator.Current;
			if (arrayIndex + num >= array.Length)
			{
				break;
			}
			array[arrayIndex + num] = current;
			num++;
		}
	}

	public bool Remove(KeyValuePair<TKey, TValue> item)
	{
		return _wrapped.Remove(new KeyValuePair<TKey, SyncedNetworkBehaviour>(item.Key, (NetworkBehaviour)(object)item.Value));
	}

	public void Add(TKey key, TValue value)
	{
		_wrapped.Add(key, (NetworkBehaviour)(object)value);
	}

	public bool ContainsKey(TKey key)
	{
		return _wrapped.ContainsKey(key);
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		bool result = _wrapped.TryGetValue(key, out var value2);
		value = (TValue)(object)(NetworkBehaviour)value2;
		return result;
	}

	public bool Remove(TKey key)
	{
		return _wrapped.Remove(key);
	}
}
