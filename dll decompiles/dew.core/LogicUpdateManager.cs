using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unity.Profiling;
using UnityEngine;

public class LogicUpdateManager : ManagerBase<LogicUpdateManager>
{
	private sealed class ReferenceTypeComparer : IEqualityComparer<Type>
	{
		public static readonly ReferenceTypeComparer Instance = new ReferenceTypeComparer();

		public bool Equals(Type x, Type y)
		{
			return (object)x == y;
		}

		public int GetHashCode(Type obj)
		{
			return RuntimeHelpers.GetHashCode(obj);
		}
	}

	private static readonly ProfilerMarker FrameUpdateMarker = new ProfilerMarker("LogicUpdate.FrameUpdate");

	private static readonly ProfilerMarker LogicUpdateMarker = new ProfilerMarker("LogicUpdate.LogicUpdate");

	public const float MinLogicUpdateInterval = 1f / 30f;

	public static float logicDeltaTime = 1f / 30f;

	public const int DefaultExecutionPriority = 0;

	internal readonly HashSet<ILogicUpdate> _removedObjects = new HashSet<ILogicUpdate>();

	internal readonly HashSet<ILogicUpdate> _addedObjects = new HashSet<ILogicUpdate>();

	private double _lastLogicUpdateTime;

	internal HashSet<ILogicUpdate>[] _logicObjectSets;

	private ILogicUpdate[][] _iterArrays;

	private int[] _iterCounts;

	private bool[] _bucketDirty;

	internal static Dictionary<int, int> _priorityToArrayIndex;

	internal static Dictionary<int, int> _arrayIndexToPriority;

	private static Dictionary<Type, int> _typeToPriorityCache;

	private static List<int> _allPriorities;

	public override bool shouldRegisterUpdates => false;

	protected override void Awake()
	{
		base.Awake();
		InitIfNecessary();
		_lastLogicUpdateTime = Time.time;
	}

	private void InitIfNecessary()
	{
		if (_priorityToArrayIndex == null)
		{
			_priorityToArrayIndex = new Dictionary<int, int>();
			_arrayIndexToPriority = new Dictionary<int, int>();
			_typeToPriorityCache = new Dictionary<Type, int>(ReferenceTypeComparer.Instance);
			_allPriorities = new List<int>();
			_allPriorities.Add(0);
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			for (int i = 0; i < assemblies.Length; i++)
			{
				Type[] types = assemblies[i].GetTypes();
				foreach (Type type in types)
				{
					LogicUpdatePriorityAttribute logicUpdatePriorityAttribute = (LogicUpdatePriorityAttribute)Attribute.GetCustomAttribute(type, typeof(LogicUpdatePriorityAttribute), inherit: true);
					if (logicUpdatePriorityAttribute != null)
					{
						if (!_allPriorities.Contains(logicUpdatePriorityAttribute.priority))
						{
							_allPriorities.Add(logicUpdatePriorityAttribute.priority);
						}
						_typeToPriorityCache.Add(type, logicUpdatePriorityAttribute.priority);
					}
				}
			}
			_allPriorities.Sort();
			for (int k = 0; k < _allPriorities.Count; k++)
			{
				_priorityToArrayIndex.Add(_allPriorities[k], k);
				_arrayIndexToPriority.Add(k, _allPriorities[k]);
			}
		}
		if (_logicObjectSets == null)
		{
			_logicObjectSets = new HashSet<ILogicUpdate>[_allPriorities.Count];
			_iterArrays = new ILogicUpdate[_allPriorities.Count][];
			_iterCounts = new int[_allPriorities.Count];
			_bucketDirty = new bool[_allPriorities.Count];
			for (int l = 0; l < _allPriorities.Count; l++)
			{
				_logicObjectSets[l] = new HashSet<ILogicUpdate>();
				_iterArrays[l] = Array.Empty<ILogicUpdate>();
			}
		}
	}

	private void PrintErrorOfObjectName(object obj)
	{
		try
		{
			if (obj is Actor actor)
			{
				Debug.LogError(actor.GetActorReadableName());
				return;
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		try
		{
			if (obj is MonoBehaviour message)
			{
				Debug.LogError(message);
				return;
			}
		}
		catch (Exception exception2)
		{
			Debug.LogException(exception2);
		}
		try
		{
			if (obj is UnityEngine.Object message2)
			{
				Debug.LogError(message2);
				return;
			}
		}
		catch (Exception exception3)
		{
			Debug.LogException(exception3);
		}
		try
		{
			Debug.LogError($"{obj} ({obj.GetType().Name})");
		}
		catch (Exception exception4)
		{
			Debug.LogException(exception4);
		}
	}

	private void Update()
	{
		DoNodeRemoval();
		DoLogicUpdates();
		EnsureIterationArrays();
		for (int i = 0; i < _iterArrays.Length; i++)
		{
			ILogicUpdate[] array = _iterArrays[i];
			int num = _iterCounts[i];
			for (int j = 0; j < num; j++)
			{
				try
				{
					array[j].FrameUpdate();
				}
				catch (Exception exception)
				{
					PrintErrorOfObjectName(array[j]);
					Debug.LogException(exception, array[j] as UnityEngine.Object);
				}
			}
		}
		DoNodeRemoval();
		DoNodeAddition();
	}

	private void DoNodeRemoval()
	{
		if (_removedObjects.Count <= 0)
		{
			return;
		}
		foreach (ILogicUpdate removedObject in _removedObjects)
		{
			int priority = GetPriority(removedObject);
			int num = _priorityToArrayIndex[priority];
			if (!_logicObjectSets[num].Remove(removedObject))
			{
				if (removedObject as UnityEngine.Object != null)
				{
					Debug.LogWarning($"Object to remove not found in logic objects: {removedObject as UnityEngine.Object}");
				}
				else
				{
					Debug.LogWarning($"Object to remove not found in logic objects: {removedObject}");
				}
			}
			else
			{
				_bucketDirty[num] = true;
			}
		}
		_removedObjects.Clear();
	}

	private void EnsureIterationArrays()
	{
		for (int i = 0; i < _logicObjectSets.Length; i++)
		{
			if (!_bucketDirty[i])
			{
				continue;
			}
			HashSet<ILogicUpdate> hashSet = _logicObjectSets[i];
			int count = hashSet.Count;
			if (_iterArrays[i].Length < count)
			{
				int num;
				for (num = ((_iterArrays[i].Length < 4) ? 4 : _iterArrays[i].Length); num < count; num *= 2)
				{
				}
				_iterArrays[i] = new ILogicUpdate[num];
			}
			hashSet.CopyTo(_iterArrays[i]);
			_iterCounts[i] = count;
			_bucketDirty[i] = false;
		}
	}

	private void DoLogicUpdates()
	{
		if ((double)Time.time - _lastLogicUpdateTime < 0.03333333507180214)
		{
			return;
		}
		logicDeltaTime = (float)(Time.timeAsDouble - _lastLogicUpdateTime);
		_lastLogicUpdateTime = Time.timeAsDouble;
		EnsureIterationArrays();
		for (int i = 0; i < _iterArrays.Length; i++)
		{
			ILogicUpdate[] array = _iterArrays[i];
			int num = _iterCounts[i];
			for (int j = 0; j < num; j++)
			{
				try
				{
					array[j].LogicUpdate(logicDeltaTime);
				}
				catch (Exception exception)
				{
					PrintErrorOfObjectName(array[j]);
					if (array[j] is UnityEngine.Object context)
					{
						Debug.LogException(exception, context);
					}
					else
					{
						Debug.LogException(exception);
					}
				}
			}
		}
		DoNodeRemoval();
	}

	private int GetPriority(ILogicUpdate lobj)
	{
		Type type = lobj.GetType();
		if (_typeToPriorityCache.TryGetValue(type, out var value))
		{
			return value;
		}
		return 0;
	}

	private void DoNodeAddition()
	{
		foreach (ILogicUpdate addedObject in _addedObjects)
		{
			int priority = GetPriority(addedObject);
			int num = _priorityToArrayIndex[priority];
			if (_logicObjectSets[num].Add(addedObject))
			{
				_bucketDirty[num] = true;
			}
		}
		_addedObjects.Clear();
	}

	public static void Register(ILogicUpdate lobj)
	{
		if (ManagerBase<LogicUpdateManager>.instance == null)
		{
			Debug.LogError("LogicUpdateManager.Register failed, LogicUpdateManager instance not found.", lobj as UnityEngine.Object);
			return;
		}
		ManagerBase<LogicUpdateManager>.instance.InitIfNecessary();
		ManagerBase<LogicUpdateManager>.instance._addedObjects.Add(lobj);
	}

	public static void Unregister(ILogicUpdate lobj)
	{
		if (!(ManagerBase<LogicUpdateManager>.instance == null) && !ManagerBase<LogicUpdateManager>.instance._addedObjects.Remove(lobj))
		{
			ManagerBase<LogicUpdateManager>.instance.InitIfNecessary();
			ManagerBase<LogicUpdateManager>.instance._removedObjects.Add(lobj);
		}
	}
}
