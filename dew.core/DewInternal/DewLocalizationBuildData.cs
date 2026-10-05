using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DewInternal;

[CreateAssetMenu(fileName = "New Dew Localization Build Data", menuName = "Dew Localization Build Data", order = 1)]
public class DewLocalizationBuildData : SerializedScriptableObject
{
	public Dictionary<string, PerLanguageLocalizationData> dataByLanguage;

	private Dictionary<string, DewFieldInfo> _fieldInfos;

	public IReadOnlyDictionary<string, DewFieldInfo> fieldInfos => _fieldInfos;

	public void InitForRuntime()
	{
		_fieldInfos = new Dictionary<string, DewFieldInfo>();
		foreach (KeyValuePair<string, PerLanguageLocalizationData> item in dataByLanguage)
		{
			foreach (KeyValuePair<string, SkillData> skill in item.Value.skills)
			{
				foreach (SkillConfigData config in skill.Value.configs)
				{
					foreach (LocaleNode item2 in config.description)
					{
						if (item2.type != LocaleNodeType.Expression)
						{
							continue;
						}
						foreach (ExpressionChildNode node in item2.expressionData.nodes)
						{
							if (node.type == ExpressionChildNodeType.FieldName && !_fieldInfos.ContainsKey(node.value))
							{
								InitFieldData(node);
							}
						}
					}
				}
			}
			foreach (KeyValuePair<string, GemData> gem in item.Value.gems)
			{
				foreach (LocaleNode item3 in gem.Value.description)
				{
					if (item3.type != LocaleNodeType.Expression)
					{
						continue;
					}
					foreach (ExpressionChildNode node2 in item3.expressionData.nodes)
					{
						if (node2.type == ExpressionChildNodeType.FieldName && !_fieldInfos.ContainsKey(node2.value))
						{
							InitFieldData(node2);
						}
					}
				}
			}
			foreach (KeyValuePair<string, StarData> star in item.Value.stars)
			{
				foreach (LocaleNode item4 in star.Value.description)
				{
					if (item4.type != LocaleNodeType.Expression)
					{
						continue;
					}
					foreach (ExpressionChildNode node3 in item4.expressionData.nodes)
					{
						if (node3.type == ExpressionChildNodeType.FieldName && !_fieldInfos.ContainsKey(node3.value))
						{
							InitFieldData(node3);
						}
					}
				}
			}
			foreach (KeyValuePair<string, CurseData> curse in item.Value.curses)
			{
				foreach (LocaleNode item5 in curse.Value.description)
				{
					if (item5.type != LocaleNodeType.Expression)
					{
						continue;
					}
					foreach (ExpressionChildNode node4 in item5.expressionData.nodes)
					{
						if (node4.type == ExpressionChildNodeType.FieldName && !_fieldInfos.ContainsKey(node4.value))
						{
							InitFieldData(node4);
						}
					}
				}
				foreach (LocaleNode item6 in curse.Value.shortDesc)
				{
					if (item6.type != LocaleNodeType.Expression)
					{
						continue;
					}
					foreach (ExpressionChildNode node5 in item6.expressionData.nodes)
					{
						if (node5.type == ExpressionChildNodeType.FieldName && !_fieldInfos.ContainsKey(node5.value))
						{
							InitFieldData(node5);
						}
					}
				}
			}
			foreach (KeyValuePair<string, TreasureData> treasure in item.Value.treasures)
			{
				foreach (LocaleNode item7 in treasure.Value.description)
				{
					if (item7.type != LocaleNodeType.Expression)
					{
						continue;
					}
					foreach (ExpressionChildNode node6 in item7.expressionData.nodes)
					{
						if (node6.type == ExpressionChildNodeType.FieldName && !_fieldInfos.ContainsKey(node6.value))
						{
							InitFieldData(node6);
						}
					}
				}
			}
		}
		Debug.Log($"DewLocalization init for runtime: {fieldInfos.Count} Field Infos");
		void InitFieldData(ExpressionChildNode expData)
		{
			try
			{
				string[] array = expData.value.Split("::", StringSplitOptions.None);
				if (Dew.TryGetTypeFromShortName(array[0], out var type))
				{
					FieldInfo field = type.GetField(array[1], BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
					if (field != null)
					{
						_fieldInfos[expData.value] = new DewFieldInfo
						{
							requiresTarget = false,
							valueGetters = new Func<object, object>[1]
							{
								(object _) => field.GetValue(null)
							},
							isProperty = false
						};
						return;
					}
				}
				string[] array2 = array[1].Split('.', StringSplitOptions.None);
				_fieldInfos[expData.value] = new DewFieldInfo
				{
					requiresTarget = true,
					valueGetters = new Func<object, object>[array2.Length],
					isProperty = false
				};
				Type type2 = Dew.GetTypeFromShortName(array[0]);
				for (int num = 0; num < array2.Length; num++)
				{
					FieldInfo field2 = type2.GetField(array2[num], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (field2 != null)
					{
						_fieldInfos[expData.value].valueGetters[num] = (object obj) =>
						{
							if (obj == null)
							{
								return "!nullobj";
							}
							try
							{
								return field2.GetValue(obj);
							}
							catch (Exception exception2)
							{
								Debug.LogException(exception2);
								return "!exception";
							}
						};
						type2 = field2.FieldType;
					}
					else
					{
						PropertyInfo property = type2.GetProperty(array2[num], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
						if (!(property != null))
						{
							throw new Exception("Cannot resolve field or property '" + array2[num] + "' in type '" + type2.Name + "'");
						}
						DewFieldInfo value = _fieldInfos[expData.value];
						value.isProperty = true;
						_fieldInfos[expData.value] = value;
						_fieldInfos[expData.value].valueGetters[num] = (object obj) =>
						{
							if (obj == null)
							{
								return "!nullobj";
							}
							try
							{
								return property.GetValue(obj);
							}
							catch (Exception exception2)
							{
								Debug.LogException(exception2);
								return "!exception";
							}
						};
						type2 = property.PropertyType;
					}
				}
			}
			catch (Exception exception)
			{
				Debug.LogWarning("Below exception occurred while processing: " + expData.value);
				Debug.LogException(exception);
			}
		}
	}
}
