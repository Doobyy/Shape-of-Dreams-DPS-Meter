using System;
using System.Buffers;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Pool;

public static class DewPool
{
	private static readonly HashSet<Type> _virtualClearedTypes = new HashSet<Type>
	{
		typeof(Actor),
		typeof(Entity),
		typeof(Projectile),
		typeof(DamageInstance),
		typeof(AbilityTrigger),
		typeof(Pickup_DreamDust),
		typeof(CurseStatusEffect)
	};

	private static readonly Dictionary<Type, FieldInfo[]> _extraClearFields = new Dictionary<Type, FieldInfo[]>();

	public static T[] GetArray<T>(out ArrayReturnHandle<T> handle, int minSize = 128)
	{
		T[] array = ArrayPool<T>.Shared.Rent(minSize);
		handle = new ArrayReturnHandle<T>(array);
		return array;
	}

	public static List<T> GetList<T>(out ListReturnHandle<T> handle)
	{
		List<T> list = CollectionPool<List<T>, T>.Get();
		handle = new ListReturnHandle<T>(list);
		return list;
	}

	public static Dictionary<TKey, TValue> GetDictionary<TKey, TValue>(out DictionaryReturnHandle<TKey, TValue> handle)
	{
		Dictionary<TKey, TValue> dictionary = CollectionPool<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>.Get();
		handle = new DictionaryReturnHandle<TKey, TValue>(dictionary);
		return dictionary;
	}

	public static void ClearEventsAndProcessors(Component obj)
	{
		if (ManagerBase<SpawnManager>.softInstance == null)
		{
			return;
		}
		if (obj is Actor actor)
		{
			if (ManagerBase<SpawnManager>.softInstance.usePooling || actor.reuseInRoom)
			{
				try
				{
					actor.ClearPooledEventsAndProcessors();
					ClearExtraFields(actor);
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
		}
		else if (obj is EntityComponent { entity: var entity } entityComponent && (ManagerBase<SpawnManager>.softInstance.usePooling || (!((UnityEngine.Object)(object)entity == null) && entity.reuseInRoom)))
		{
			try
			{
				entityComponent.ClearPooledEventsAndProcessors();
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
			}
		}
	}

	private static void ClearExtraFields(Actor obj)
	{
		Type type = ((object)obj).GetType();
		if (!_extraClearFields.TryGetValue(type, out var value))
		{
			FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			List<FieldInfo> list = new List<FieldInfo>();
			foreach (FieldInfo fieldInfo in fields)
			{
				if ((IsSafeActionField(fieldInfo) || IsDataProcessorsField(fieldInfo)) && !_virtualClearedTypes.Contains(fieldInfo.DeclaringType))
				{
					list.Add(fieldInfo);
				}
			}
			value = list.ToArray();
			_extraClearFields[type] = value;
		}
		for (int j = 0; j < value.Length; j++)
		{
			if (value[j].GetValue(obj) is IPoolClearable poolClearable)
			{
				poolClearable.Clear();
			}
		}
	}

	private static bool IsSafeActionField(FieldInfo field)
	{
		Type fieldType = field.FieldType;
		if (fieldType == typeof(SafeAction))
		{
			return true;
		}
		if (!fieldType.IsGenericType)
		{
			return false;
		}
		Type genericTypeDefinition = fieldType.GetGenericTypeDefinition();
		if (!(genericTypeDefinition == typeof(SafeAction<>)) && !(genericTypeDefinition == typeof(SafeAction<, >)) && !(genericTypeDefinition == typeof(SafeAction<, , >)))
		{
			return genericTypeDefinition == typeof(SafeAction<, , , >);
		}
		return true;
	}

	private static bool IsDataProcessorsField(FieldInfo field)
	{
		Type fieldType = field.FieldType;
		if (!fieldType.IsGenericType)
		{
			return false;
		}
		Type genericTypeDefinition = fieldType.GetGenericTypeDefinition();
		if (!(genericTypeDefinition == typeof(DataProcessorGroup<>)))
		{
			return genericTypeDefinition == typeof(DataProcessorGroup<, , >);
		}
		return true;
	}
}
