using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DewLaunchOptions
{
	public static bool forceKeyboardAndMouse;

	public static bool forceGamepad;

	public static bool steamNoRestart;

	public static string connectLobby;

	public static string modDir;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Init()
	{
		List<string> list = Environment.GetCommandLineArgs().ToList();
		Debug.Log("[LaunchOptions] Received: " + list.JoinToString(", "));
		forceKeyboardAndMouse = list.Contains("-forcepc");
		forceGamepad = list.Contains("-forcegamepad");
		steamNoRestart = list.Contains("-steamnorestart");
		connectLobby = null;
		modDir = null;
		if (list.Contains("+connect_lobby"))
		{
			try
			{
				int num = list.IndexOf("+connect_lobby");
				connectLobby = list[num + 1];
			}
			catch (Exception)
			{
			}
		}
		if (list.Contains("-moddir"))
		{
			try
			{
				int num2 = list.IndexOf("-moddir");
				modDir = list[num2 + 1];
			}
			catch (Exception)
			{
			}
		}
		Debug.Log(string.Format("[LaunchOptions] {0}: {1}", "forceKeyboardAndMouse", forceKeyboardAndMouse));
		Debug.Log(string.Format("[LaunchOptions] {0}: {1}", "forceGamepad", forceGamepad));
		Debug.Log(string.Format("[LaunchOptions] {0}: {1}", "steamNoRestart", steamNoRestart));
		Debug.Log("[LaunchOptions] connectLobby: " + connectLobby);
		Debug.Log("[LaunchOptions] modDir: " + modDir);
	}
}
