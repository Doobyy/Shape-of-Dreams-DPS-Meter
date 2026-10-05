using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using IngameDebugConsole;
using Mirror;
using UnityEngine;

public class ModBehaviour : MonoBehaviour
{
	internal class ManagerPatchItem
	{
		public Type type;

		public Func<Action> onStart;

		public Action onCleanup;

		public bool hasStarted;
	}

	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
	public class ConsoleCommandAttribute : Attribute
	{
		public readonly string Description;

		public readonly string CustomName;

		public ConsoleCommandAttribute(string description)
		{
			Description = description;
			CustomName = null;
		}

		public ConsoleCommandAttribute(string description, string customName)
		{
			Description = description;
			CustomName = customName;
		}
	}

	[NonSerialized]
	public LoadedModInstance instance;

	private Harmony _harmony;

	internal List<ManagerPatchItem> _managerPatches = new List<ManagerPatchItem>();

	public ModItem mod => instance.mod;

	public Harmony harmony
	{
		get
		{
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Expected Obj, but got Unknown
			if (_harmony == null)
			{
				_harmony = new Harmony(mod.metadata.id + Guid.NewGuid().ToString());
			}
			return _harmony;
		}
	}

	public FieldInfo[] modConfigFields => (from f in GetType().GetFields(BindingFlags.Instance | BindingFlags.Public)
		where f.FieldType.IsSubclassOf(typeof(ModConfig))
		select f).ToArray();

	public void CallOnManager<T>(Action onStart, Action onCleanup = null) where T : ManagerBase<T>
	{
		ManagerPatchItem item = new ManagerPatchItem
		{
			type = typeof(T),
			onStart = () =>
			{
				onStart?.Invoke();
				return (Action)null;
			},
			onCleanup = onCleanup
		};
		InitManagerPatchItem(item, ManagerBase<T>.instance);
	}

	public void CallOnNetworkedManager<T>(Action onStart, Action onCleanup = null) where T : NetworkedManagerBase<T>
	{
		ManagerPatchItem item = new ManagerPatchItem
		{
			type = typeof(T),
			onStart = () =>
			{
				onStart?.Invoke();
				return (Action)null;
			},
			onCleanup = onCleanup
		};
		InitManagerPatchItem(item, (UnityEngine.Object)(object)NetworkedManagerBase<T>.instance);
	}

	public void CallOnManager<T>(Func<T, Action> onStart) where T : ManagerBase<T>
	{
		ManagerPatchItem item = new ManagerPatchItem
		{
			type = typeof(T),
			onStart = () => onStart?.Invoke(ManagerBase<T>.instance),
			onCleanup = null
		};
		InitManagerPatchItem(item, ManagerBase<T>.instance);
	}

	public void CallOnNetworkedManager<T>(Func<T, Action> onStart) where T : NetworkedManagerBase<T>
	{
		ManagerPatchItem item = new ManagerPatchItem
		{
			type = typeof(T),
			onStart = () => onStart?.Invoke(NetworkedManagerBase<T>.instance),
			onCleanup = null
		};
		InitManagerPatchItem(item, (UnityEngine.Object)(object)NetworkedManagerBase<T>.instance);
	}

	public void CallOnManager<T>(Action<T> onStart) where T : ManagerBase<T>
	{
		ManagerPatchItem item = new ManagerPatchItem
		{
			type = typeof(T),
			onStart = () =>
			{
				onStart?.Invoke(ManagerBase<T>.instance);
				return (Action)null;
			},
			onCleanup = null
		};
		InitManagerPatchItem(item, ManagerBase<T>.instance);
	}

	public void CallOnNetworkedManager<T>(Action<T> onStart) where T : NetworkedManagerBase<T>
	{
		ManagerPatchItem item = new ManagerPatchItem
		{
			type = typeof(T),
			onStart = () =>
			{
				onStart?.Invoke(NetworkedManagerBase<T>.instance);
				return (Action)null;
			},
			onCleanup = null
		};
		InitManagerPatchItem(item, (UnityEngine.Object)(object)NetworkedManagerBase<T>.instance);
	}

	private void InitManagerPatchItem(ManagerPatchItem item, bool shouldStartNow)
	{
		_managerPatches.Add(item);
		if (!shouldStartNow)
		{
			return;
		}
		try
		{
			item.hasStarted = true;
			Action action = item.onStart?.Invoke();
			if (action != null)
			{
				item.onCleanup = (Action)Delegate.Combine(item.onCleanup, action);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public virtual void OnConfigChanged()
	{
	}

	public virtual void LoadConfigsToDisk()
	{
		FieldInfo[] array = modConfigFields;
		foreach (FieldInfo fieldInfo in array)
		{
			string modConfigFilePath = GetModConfigFilePath(fieldInfo);
			string directoryName = Path.GetDirectoryName(modConfigFilePath);
			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			if (!File.Exists(modConfigFilePath))
			{
				string contents = DewPersistence.ToJson(fieldInfo.GetValue(this));
				File.WriteAllText(modConfigFilePath, contents);
			}
			else
			{
				object value = DewPersistence.FromJson(File.ReadAllText(modConfigFilePath), fieldInfo.FieldType);
				fieldInfo.SetValue(this, value);
			}
		}
	}

	public virtual void SaveConfigsToDisk()
	{
		FieldInfo[] array = modConfigFields;
		foreach (FieldInfo fieldInfo in array)
		{
			string modConfigFilePath = GetModConfigFilePath(fieldInfo);
			string directoryName = Path.GetDirectoryName(modConfigFilePath);
			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			string contents = DewPersistence.ToJson(fieldInfo.GetValue(this));
			File.WriteAllText(modConfigFilePath, contents);
		}
	}

	public virtual string GetModConfigFilePath(FieldInfo modConfigInfo)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		string text = Path.Join(string.op_Implicit(Path.Join(string.op_Implicit(Application.persistentDataPath), string.op_Implicit("QuickSave"), string.op_Implicit("Mods"))), string.op_Implicit(mod.metadata.id));
		string text2 = GetType().Name + "." + modConfigInfo.Name + ".json";
		return Path.Join(string.op_Implicit(text), string.op_Implicit(text2));
	}

	protected static void PrintNoEntitySelected()
	{
		Debug.Log("No entity selected");
	}

	protected static Entity GetConsoleSelectedEntity()
	{
		if (!((UnityEngine.Object)(object)NetworkedManagerBase<ConsoleManager>.instance == null))
		{
			return NetworkedManagerBase<ConsoleManager>.instance.localSelectedEntity;
		}
		return null;
	}

	protected static bool TryGetConsoleSelectedEntity(out Entity ent)
	{
		ent = (((UnityEngine.Object)(object)NetworkedManagerBase<ConsoleManager>.instance == null) ? null : NetworkedManagerBase<ConsoleManager>.instance.localSelectedEntity);
		return (UnityEngine.Object)(object)ent != null;
	}

	protected static bool MakeSureNetworked()
	{
		if (NetworkServer.active || NetworkClient.active)
		{
			return true;
		}
		Debug.Log("This command can only be used in networking context");
		return false;
	}

	protected static bool MakeSureServer()
	{
		if (NetworkServer.active)
		{
			return true;
		}
		Debug.Log("This command can only be used in servers");
		return false;
	}

	protected static bool MakeSureClientNonServer()
	{
		if (!NetworkServer.active && NetworkClient.active)
		{
			return true;
		}
		Debug.Log("This command can only be used in non-server clients");
		return false;
	}

	protected static bool MakeSureInGame()
	{
		if ((UnityEngine.Object)(object)NetworkedManagerBase<GameManager>.instance != null)
		{
			return true;
		}
		Debug.Log("This command can only be used in games");
		return false;
	}

	protected static bool MakeSureNotInGame()
	{
		if ((UnityEngine.Object)(object)NetworkedManagerBase<GameManager>.instance != null)
		{
			return true;
		}
		Debug.Log("This command can only be used outside games");
		return false;
	}

	public void RegisterConsoleCommand(string methodName, string customName = null, string description = null)
	{
		MethodInfo[] methods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (MethodInfo m in methods)
		{
			RegisterConsoleCommand(m, customName, description);
		}
	}

	public void RegisterConsoleCommand(MethodInfo m, string customName = null, string description = null)
	{
		if (description == null)
		{
			description = "No description provided. (" + mod.metadata.name + ")";
		}
		string text = (string.IsNullOrEmpty(customName) ? m.Name : customName);
		string text2 = text;
		ParameterInfo[] parameters = m.GetParameters();
		int num = 0;
		while (HasCommand(text2))
		{
			text2 = $"{text}{num}";
			num++;
		}
		if (m.IsStatic)
		{
			DebugLogConsole.AddCommand(text2, description, m, (object)null, (string[])null, (CommandType)0);
		}
		else
		{
			DebugLogConsole.AddCommand(text2, description, m, (object)this, (string[])null, (CommandType)0);
		}
		foreach (LoadedModInstance loadedInstance in DewMod.loadedInstances)
		{
			loadedInstance.registeredCommands.Remove(text2);
		}
		instance.registeredCommands.Add(text2);
		bool HasCommand(string commandName)
		{
			for (int num2 = DebugLogConsole.methods.Count - 1; num2 >= 0; num2--)
			{
				if (DebugLogConsole.caseInsensitiveComparer.Compare(DebugLogConsole.methods[num2].command, commandName, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) == 0 && DebugLogConsole.methods[num2].parameters.Length == parameters.Length)
				{
					return true;
				}
			}
			return false;
		}
	}
}
