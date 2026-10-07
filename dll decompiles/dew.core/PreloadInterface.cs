using System.Collections.Generic;
using UnityEngine;

public class PreloadInterface
{
	internal HashSet<string> _guids = new HashSet<string>();

	public void AddAssetRef<T>(AssetRef<T> assetRef) where T : Object
	{
		if (!string.IsNullOrEmpty(assetRef.guid))
		{
			AddGuid(assetRef.guid);
		}
	}

	public void AddGuid(string guid)
	{
		_guids.Add(guid);
	}

	public void AddFromMonsterPool(MonsterPool pool)
	{
		if (pool == null)
		{
			return;
		}
		foreach (MonsterPool.SpawnRuleEntry entry in pool.entries)
		{
			if (!string.IsNullOrEmpty(entry.monster.guid))
			{
				if (DewResources.database.guidToType.TryGetValue(entry.monster.guid, out var value))
				{
					AddType(value.Name);
				}
				else
				{
					AddGuid(entry.monster.guid);
				}
			}
		}
	}

	public void AddType(string type, bool includeDependencies = true)
	{
		if (!DewResources.database.typeNameToGuid.TryGetValue(type, out var value))
		{
			return;
		}
		AddGuid(value);
		if (!includeDependencies)
		{
			return;
		}
		ListReturnHandle<string> handle;
		foreach (string allDependency in DewResources.GetAllDependencies(out handle, type))
		{
			if (DewResources.database.typeNameToGuid.TryGetValue(allDependency, out var value2))
			{
				AddGuid(value2);
			}
		}
		handle.Return();
	}

	public void KeepEverything()
	{
		foreach (string loadedGuid in DewResources.loadedGuids)
		{
			AddGuid(loadedGuid);
		}
	}
}
