using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public struct SyncableAssetRef : IEquatable<SyncableAssetRef>
{
	public string guid = null;

	public string typeName = null;

	public string typeAssemblyQualifiedName = null;

	public bool isMonoBehaviour = false;

	public bool isActor = false;

	[NonSerialized]
	private UnityEngine.Object _asset = null;

	[NonSerialized]
	private UnityEngine.Object _lightAsset = null;

	[NonSerialized]
	private Type _type = null;

	[JsonIgnore]
	public UnityEngine.Object asset
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
			}
			else
			{
				guid = DewResources.GetGuidOfAsset(value);
				typeName = value.GetType().Name;
				typeAssemblyQualifiedName = value.GetType().AssemblyQualifiedName;
				isMonoBehaviour = value is Component;
				isActor = value is Actor;
			}
		}
	}

	[JsonIgnore]
	public UnityEngine.Object lightAsset => GetAsset(CollectionExtensions.GetValueOrDefault<string, string>((IReadOnlyDictionary<string, string>)DewResources.database.heavyToLightGuidMap, guid, guid), ref _lightAsset);

	private UnityEngine.Object GetAsset(string g, ref UnityEngine.Object cached)
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
			cached = DewResources.Convert(DewResources.GetByGuid(g), _type);
		}
		if (cached == null && isActor)
		{
			cached = DewResources.Convert(DewResources.GetByGuid(g), typeof(Actor));
		}
		if (cached == null && isMonoBehaviour)
		{
			cached = DewResources.Convert(DewResources.GetByGuid(g), typeof(MonoBehaviour));
		}
		if (cached == null)
		{
			cached = DewResources.GetByGuid(g);
		}
		return cached;
	}

	public void ClearCache()
	{
		_asset = null;
	}

	public SyncableAssetRef(UnityEngine.Object v = null)
	{
		asset = v;
	}

	public static implicit operator UnityEngine.Object(SyncableAssetRef assetRef)
	{
		return assetRef.asset;
	}

	public static implicit operator SyncableAssetRef(UnityEngine.Object t)
	{
		return new SyncableAssetRef(t);
	}

	public bool Equals(SyncableAssetRef other)
	{
		return guid == other.guid;
	}

	public override bool Equals(object obj)
	{
		if (obj is SyncableAssetRef other)
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

	public static bool operator ==(SyncableAssetRef left, SyncableAssetRef right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SyncableAssetRef left, SyncableAssetRef right)
	{
		return !(left == right);
	}
}
