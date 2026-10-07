using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using IngameDebugConsole;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ConsoleManager : NetworkedManagerBase<ConsoleManager>
{
	public enum AutoExecKey
	{
		Global = 0,
		Network = 100,
		NetworkServer = 101,
		NetworkClient = 102,
		Game = 200,
		GameServer = 201,
		GameClient = 202
	}

	public struct ExecutionContext
	{
		public DewPlayer player;

		public Entity selection;

		public Vector3 cursorWorldPos;
	}

	public Entity localSelectedEntity;

	public ExecutionContext executionContext;

	public Material glDrawMaterial;

	[NonSerialized]
	public List<ConsoleBindItem> activeCommandBinds = new List<ConsoleBindItem>();

	public SafeAction<bool> ClientEvent_OnCheatEnabledChanged;

	[SyncVar(hook = "CheatEnabledChanged")]
	private bool _isCheatEnabled;

	private CanvasGroup _consoleWindowCanvas;

	private Camera _mainCamera;

	public Action<bool, bool> _Mirror_SyncVarHookDelegate__isCheatEnabled;

	public bool isCheatEnabled => _isCheatEnabled;

	public bool isConsoleWindowOpen
	{
		get
		{
			if ((UnityEngine.Object)(object)_consoleWindowCanvas != null)
			{
				return _consoleWindowCanvas.alpha > 0.1f;
			}
			return false;
		}
	}

	public bool Network_isCheatEnabled
	{
		get
		{
			return _isCheatEnabled;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref _isCheatEnabled, 1uL, _Mirror_SyncVarHookDelegate__isCheatEnabled);
		}
	}

	private void CheatEnabledChanged(bool oldVal, bool newVal)
	{
		try
		{
			ClientEvent_OnCheatEnabledChanged?.Invoke(newVal);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		if (newVal)
		{
			Debug.Log("Cheat is now on");
		}
		else
		{
			Debug.Log("Cheat is now off");
		}
		if (newVal && ManagerBase<InGameAnalyticsManager>.instance != null)
		{
			ManagerBase<InGameAnalyticsManager>.instance.DisableAnalyticsLocal();
		}
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		_mainCamera = Camera.main;
		_consoleWindowCanvas = ((Component)(object)UnityEngine.Object.FindObjectOfType<DebugLogManager>(true)).transform.Find("DebugLogWindow").GetComponent<CanvasGroup>();
		DebugLogConsole.ServerCommandHandler = (string command) =>
		{
			if ((UnityEngine.Object)(object)NetworkedManagerBase<GameManager>.instance != null)
			{
				ExecuteServerCommand(command.ToCharArray(), localSelectedEntity, ControlManager.GetWorldPositionOnGroundOnCursor());
			}
			else if (NetworkClient.active)
			{
				ExecuteServerCommand(command.ToCharArray(), localSelectedEntity, Vector3.zero);
			}
			else
			{
				Debug.Log("You're not connected to a game.");
			}
		};
		DebugLogConsole.GameContextValidator = () => (UnityEngine.Object)(object)NetworkedManagerBase<GameManager>.instance != null;
		DebugLogConsole.CheatContextValidator = () => (UnityEngine.Object)(object)NetworkedManagerBase<ConsoleManager>.instance != null && NetworkedManagerBase<ConsoleManager>.instance.isCheatEnabled;
		Camera.onPostRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPostRender, new Camera.CameraCallback(DrawEntityBounds));
	}

	public override void OnLateStart()
	{
		base.OnLateStart();
		ExecuteAutoExec(AutoExecKey.Global);
	}

	public bool ShouldSkipAutoExec()
	{
		if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftControl))
		{
			return Input.GetKey(KeyCode.LeftAlt);
		}
		return false;
	}

	public void ExecuteAutoExec(AutoExecKey key)
	{
		if (ShouldSkipAutoExec())
		{
			Debug.Log("Ctrl+Alt+Shift detected, skipping auto-exec of " + key);
			return;
		}
		string[] array = PlayerPrefs.GetString("AutoExec_" + key, "").Split('\n', StringSplitOptions.None);
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (!string.IsNullOrWhiteSpace(text.Trim()))
			{
				Debug.Log($"AutoExec for ({key}) will execute {text}");
			}
		}
		array2 = array;
		foreach (string text2 in array2)
		{
			if (!string.IsNullOrWhiteSpace(text2.Trim()))
			{
				DebugLogConsole.ExecuteCommand(text2, false);
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Camera.onPostRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPostRender, new Camera.CameraCallback(DrawEntityBounds));
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (ControlManager.IsInputFieldFocused())
		{
			return;
		}
		for (int i = 0; i < activeCommandBinds.Count; i++)
		{
			ConsoleBindItem consoleBindItem = activeCommandBinds[i];
			if ((consoleBindItem.type == ConsoleBindItemType.Down && Input.GetKeyDown(consoleBindItem.key)) || (consoleBindItem.type == ConsoleBindItemType.Up && Input.GetKeyUp(consoleBindItem.key)))
			{
				DebugLogConsole.ExecuteCommand(consoleBindItem.command, false);
			}
		}
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (((UnityEngine.Object)(object)localSelectedEntity == null || !localSelectedEntity.isActive) && (UnityEngine.Object)(object)DewPlayer.local != null && (UnityEngine.Object)(object)DewPlayer.local.hero != null)
		{
			localSelectedEntity = DewPlayer.local.hero;
		}
	}

	private void DrawEntityBounds(Camera cam)
	{
		if (_mainCamera == null)
		{
			_mainCamera = Camera.main;
		}
		if (cam != _mainCamera || _consoleWindowCanvas.alpha < 0.1f)
		{
			return;
		}
		Entity entityOnCursor = ControlManager.GetEntityOnCursor();
		foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
		{
			Color gray = Color.gray;
			if ((UnityEngine.Object)(object)allEntity == (UnityEngine.Object)(object)localSelectedEntity)
			{
				gray = Color.green;
			}
			else
			{
				if (!((UnityEngine.Object)(object)allEntity == (UnityEngine.Object)(object)entityOnCursor))
				{
					continue;
				}
				gray = Color.gray;
			}
			GLDrawCircle(allEntity.position, allEntity.Control.outerRadius, gray);
		}
	}

	public void OnGameAreaPointerDown(PointerEventData eventData)
	{
		if ((UnityEngine.Object)(object)NetworkedManagerBase<GameManager>.instance == null || !isConsoleWindowOpen)
		{
			return;
		}
		Entity entityOnCursor = ControlManager.GetEntityOnCursor();
		if (Input.GetKeyDown(KeyCode.Mouse0) && (UnityEngine.Object)(object)entityOnCursor != null)
		{
			localSelectedEntity = entityOnCursor;
			((Component)(object)_consoleWindowCanvas).transform.Find("CommandInputField").GetComponent<InputField>().ActivateInputField();
		}
		if (Input.GetKeyDown(KeyCode.Mouse2))
		{
			if ((UnityEngine.Object)(object)localSelectedEntity == null)
			{
				localSelectedEntity = DewPlayer.local.hero;
			}
			DebugLogConsole.ExecuteCommand("teleport", false);
			((Component)(object)_consoleWindowCanvas).transform.Find("CommandInputField").GetComponent<InputField>().ActivateInputField();
		}
	}

	private void GLDrawCircle(Vector3 pos, float radius, Color color)
	{
		GL.PushMatrix();
		glDrawMaterial.SetPass(0);
		GL.LoadOrtho();
		GL.Begin(1);
		GL.Color(color);
		for (int i = 0; i < 20; i++)
		{
			Vector3 start = pos + Vector3.right * Mathf.Cos((float)Math.PI / 10f * (float)i) * radius + Vector3.forward * Mathf.Sin((float)Math.PI / 10f * (float)i) * radius;
			Vector3 end = pos + Vector3.right * Mathf.Cos((float)Math.PI / 10f * (float)(i + 1)) * radius + Vector3.forward * Mathf.Sin((float)Math.PI / 10f * (float)(i + 1)) * radius;
			Line(start, end);
		}
		GL.End();
		GL.PopMatrix();
		void Line(Vector3 position, Vector3 position2)
		{
			Vector3 v = _mainCamera.WorldToViewportPoint(position);
			Vector3 v2 = _mainCamera.WorldToViewportPoint(position2);
			v.z = 0f;
			v2.z = 0f;
			GL.Vertex(v);
			GL.Vertex(v2);
		}
	}

	[Server]
	public void EnableCheats()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ConsoleManager::EnableCheats()' called when server was not active");
		}
		else
		{
			Network_isCheatEnabled = true;
		}
	}

	[Server]
	public void DisableCheats()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ConsoleManager::DisableCheats()' called when server was not active");
		}
		else
		{
			Network_isCheatEnabled = false;
		}
	}

	[Command(requiresAuthority = false)]
	public void ExecuteServerCommand(char[] charCommand, Entity selected, Vector3 cursorWorldPos, NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_System_002EChar_005B_005D((NetworkWriter)(object)val, charCommand);
		NetworkWriterExtensions.WriteNetworkBehaviour((NetworkWriter)(object)val, (NetworkBehaviour)(object)selected);
		NetworkWriterExtensions.WriteVector3((NetworkWriter)(object)val, cursorWorldPos);
		((NetworkBehaviour)this).SendCommandInternal("System.Void ConsoleManager::ExecuteServerCommand(System.Char[],Entity,UnityEngine.Vector3,Mirror.NetworkConnectionToClient)", -1792973414, (NetworkWriter)(object)val, 0, false);
		NetworkWriterPool.Return(val);
	}

	public ConsoleManager()
	{
		_Mirror_SyncVarHookDelegate__isCheatEnabled = CheatEnabledChanged;
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_ExecuteServerCommand__Char_005B_005D__Entity__Vector3__NetworkConnectionToClient(char[] charCommand, Entity selected, Vector3 cursorWorldPos, NetworkConnectionToClient sender)
	{
		executionContext.player = sender.GetPlayer();
		executionContext.selection = selected;
		executionContext.cursorWorldPos = cursorWorldPos;
		string text = new string(charCommand);
		Debug.Log(((UnityEngine.Object)(object)executionContext.player).name + ": " + text);
		try
		{
			DebugLogConsole.ExecuteCommand(text, true);
		}
		catch (Exception ex)
		{
			Exception ex2 = ex;
			while (ex2.InnerException != null)
			{
				ex2 = ex2.InnerException;
			}
			if ((UnityEngine.Object)(object)executionContext.player != (UnityEngine.Object)(object)DewPlayer.local)
			{
				executionContext.player.SendLogWarning("Exception was thrown on server while executing above command");
				executionContext.player.SendLogWarning($"{ex}\n\n");
			}
			Debug.LogWarning("Exception was thrown while executing above command");
			Debug.LogException(ex);
		}
		executionContext = default;
	}

	protected static void InvokeUserCode_ExecuteServerCommand__Char_005B_005D__Entity__Vector3__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command ExecuteServerCommand called on client.");
		}
		else
		{
			((ConsoleManager)(object)obj).UserCode_ExecuteServerCommand__Char_005B_005D__Entity__Vector3__NetworkConnectionToClient(GeneratedNetworkCode._Read_System_002EChar_005B_005D(reader), NetworkReaderExtensions.ReadNetworkBehaviour<Entity>(reader), NetworkReaderExtensions.ReadVector3(reader), senderConnection);
		}
	}

	static ConsoleManager()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterCommand(typeof(ConsoleManager), "System.Void ConsoleManager::ExecuteServerCommand(System.Char[],Entity,UnityEngine.Vector3,Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_ExecuteServerCommand__Char_005B_005D__Entity__Vector3__NetworkConnectionToClient, false);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		((NetworkBehaviour)this).SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteBool(writer, _isCheatEnabled);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 1L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, _isCheatEnabled);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		((NetworkBehaviour)this).DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isCheatEnabled, _Mirror_SyncVarHookDelegate__isCheatEnabled, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 1L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isCheatEnabled, _Mirror_SyncVarHookDelegate__isCheatEnabled, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
