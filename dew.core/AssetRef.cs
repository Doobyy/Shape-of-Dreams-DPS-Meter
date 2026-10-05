using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public struct AssetRef<T> where T : UnityEngine.Object
{
	public string guid = null;

	public string typeName = null;

	public string typeAssemblyQualifiedName = null;

	public bool isMonoBehaviour = false;

	public bool isActor = false;

	[NonSerialized]
	private T _asset = null;

	[NonSerialized]
	private T _lightAsset = null;

	[NonSerialized]
	private Type _type = null;

	[JsonIgnore]
	public T asset
	{
		get
		{
			return GetAsset(guid, ref _asset);
		}
		set
		{
			_asset = null;
			_type = null;
			if (value == null)
			{
				guid = null;
				typeName = null;
				typeAssemblyQualifiedName = null;
				isMonoBehaviour = false;
				isActor = false;
				return;
			}
			guid = DewResources.GetGuidOfAsset(value);
			if (DewResources.database.lightToHeavyGuidMap.TryGetValue(guid, out var value2))
			{
				guid = value2;
			}
			typeName = value.GetType().Name;
			typeAssemblyQualifiedName = value.GetType().AssemblyQualifiedName;
			isMonoBehaviour = value is Component;
			isActor = value is Actor;
		}
	}

	[JsonIgnore]
	public Type type
	{
		get
		{
			if (_type == null)
			{
				_type = AssetRefTypeResolver.Resolve(guid, ref typeName, ref typeAssemblyQualifiedName);
			}
			return _type;
		}
	}

	[JsonIgnore]
	public T lightAsset => GetAsset(CollectionExtensions.GetValueOrDefault<string, string>((IReadOnlyDictionary<string, string>)DewResources.database.heavyToLightGuidMap, guid, guid), ref _lightAsset);

	private T GetAsset(string g, ref T cached)
	{
		if (string.IsNullOrEmpty(g))
		{
			return null;
		}
		if (_type == null)
		{
			_type = AssetRefTypeResolver.Resolve(guid, ref typeName, ref typeAssemblyQualifiedName);
		}
		if (cached == null && _type != null)
		{
			cached = DewResources.Convert(DewResources.GetByGuid<T>(g), _type) as T;
		}
		if (cached == null && isActor)
		{
			cached = DewResources.Convert(DewResources.GetByGuid<T>(g), typeof(Actor)) as T;
		}
		if (cached == null && isMonoBehaviour)
		{
			cached = DewResources.Convert(DewResources.GetByGuid<T>(g), typeof(MonoBehaviour)) as T;
		}
		if (cached == null)
		{
			cached = DewResources.GetByGuid<T>(g);
		}
		return cached;
	}

	public void ClearCache()
	{
		_asset = null;
		_lightAsset = null;
		_type = null;
	}

	public AssetRef(T v = null)
	{
		asset = v;
	}

	public static implicit operator T(AssetRef<T> assetRef)
	{
		return assetRef.asset;
	}

	public static implicit operator AssetRef<T>(T t)
	{
		return new AssetRef<T>(t);
	}

	public bool Equals(AssetRef<T> other)
	{
		return guid == other.guid;
	}

	public override bool Equals(object obj)
	{
		if (obj is AssetRef<T> other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (guid == null)
		{
			return 0;
		}
		return guid.GetHashCode();
	}

	public static bool operator ==(AssetRef<T> left, AssetRef<T> right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(AssetRef<T> left, AssetRef<T> right)
	{
		return !(left == right);
	}

	[Obsolete("Using GetType() on AssetRefs is typically a mistake", true)]
	public new Type GetType()
	{
		return null;
	}
}
