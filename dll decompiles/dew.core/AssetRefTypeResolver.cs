using System;
using System.Collections.Generic;
using UnityEngine;

internal static class AssetRefTypeResolver
{
	public static Type Resolve(string guid, ref string typeName, ref string typeAssemblyQualifiedName)
	{
		Type type = null;
		if (!string.IsNullOrEmpty(typeAssemblyQualifiedName))
		{
			type = Type.GetType(typeAssemblyQualifiedName);
		}
		if (type == null && !string.IsNullOrEmpty(typeName))
		{
			type = CollectionExtensions.GetValueOrDefault<string, Type>((IReadOnlyDictionary<string, Type>)DewResources.database.typeNameToType, typeName);
		}
		if (type == null && !string.IsNullOrEmpty(guid))
		{
			type = CollectionExtensions.GetValueOrDefault<string, Type>((IReadOnlyDictionary<string, Type>)DewResources.database.guidToType, guid);
			if (type != null)
			{
				Debug.LogWarning("AssetRef typeName '" + typeName + "' is stale; resolved '" + type.Name + "' by guid " + guid + ". Re-save the referencing asset.");
				typeName = type.Name;
				typeAssemblyQualifiedName = type.AssemblyQualifiedName;
			}
		}
		return type;
	}
}
