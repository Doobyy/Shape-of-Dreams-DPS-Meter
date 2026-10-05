using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DewInternal;

[CreateAssetMenu(fileName = "New Dew Resource Database", menuName = "Dew Resource Database", order = 1)]
public class DewResourceDatabase : SerializedScriptableObject
{
	private static readonly string[] Paths = new string[3] { "Assets/Dew", "Assets/DewCore", "Assets/Res/Animations" };

	public Dictionary<string, string> typeAssemblyQualifiedNameToGuid = new Dictionary<string, string>();

	public Dictionary<string, string> nameToGuid = new Dictionary<string, string>();

	public Dictionary<string, string> guidToName = new Dictionary<string, string>();

	public Dictionary<uint, string> netObjectAssetIdToGuid = new Dictionary<uint, string>();

	public List<string> sceneNames = new List<string>();

	public Dictionary<UnityEngine.Object, string> objectToGuidFallback = new Dictionary<UnityEngine.Object, string>();

	public List<string> excludedFromPoolObjects = new List<string>();

	public Dictionary<string, List<string>> dependencyConnections = new Dictionary<string, List<string>>();

	public Dictionary<string, List<string>> deepDependencyConnections = new Dictionary<string, List<string>>();

	public List<string> strippedTypes = new List<string>();

	public Dictionary<string, string> lightToHeavyGuidMap = new Dictionary<string, string>();

	public Dictionary<string, string> heavyToLightGuidMap = new Dictionary<string, string>();

	public Dictionary<string, long> lightGuidToHeavyLastWriteTimeTicks = new Dictionary<string, long>();

	public Dictionary<string, string> lightGuidToHeavyMd5 = new Dictionary<string, string>();

	public List<string> allGuids = new List<string>();

	[NonSerialized]
	public Dictionary<Type, string> typeToGuid = new Dictionary<Type, string>();

	[NonSerialized]
	public Dictionary<string, Type> guidToType = new Dictionary<string, Type>();

	[NonSerialized]
	public Dictionary<string, string> typeNameToGuid = new Dictionary<string, string>();

	[NonSerialized]
	public Dictionary<string, Type> typeNameToType = new Dictionary<string, Type>();

	[NonSerialized]
	public Dictionary<string, uint> guidToNetAssetId = new Dictionary<string, uint>();

	public bool InitForRuntime()
	{
		typeToGuid.Clear();
		guidToType.Clear();
		typeNameToGuid.Clear();
		typeNameToType.Clear();
		guidToNetAssetId.Clear();
		foreach (KeyValuePair<uint, string> item in netObjectAssetIdToGuid)
		{
			guidToNetAssetId[item.Value] = item.Key;
		}
		bool flag = false;
		foreach (KeyValuePair<string, string> item2 in typeAssemblyQualifiedNameToGuid)
		{
			try
			{
				Type type = Type.GetType(item2.Key);
				if (type == null)
				{
					Debug.LogWarning("Type '" + item2.Key + "' not found in current domain, possibly stale database?");
					continue;
				}
				typeToGuid.Add(type, item2.Value);
				guidToType.Add(item2.Value, type);
				typeNameToGuid.Add(type.Name, item2.Value);
				typeNameToType.Add(type.Name, type);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception, (UnityEngine.Object)(object)this);
				flag = true;
			}
		}
		return !flag;
	}
}
