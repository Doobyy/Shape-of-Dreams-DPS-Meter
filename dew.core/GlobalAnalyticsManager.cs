using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;

public class GlobalAnalyticsManager : ManagerBase<GlobalAnalyticsManager>
{
	public override bool shouldRegisterUpdates => false;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Init()
	{
		if (IsAnalyticsEnabled())
		{
			Analytics.initializeOnStartup = true;
			Analytics.enabled = true;
			Analytics.deviceStatsEnabled = true;
			PerformanceReporting.enabled = true;
		}
		else
		{
			Analytics.initializeOnStartup = false;
			Analytics.enabled = false;
			Analytics.deviceStatsEnabled = false;
			PerformanceReporting.enabled = false;
			if (DewBuildProfile.current.disableAnalytics)
			{
				Debug.Log("Analytics disabled by build profile settings");
			}
		}
		SceneManager.activeSceneChanged += SceneManagerOnactiveSceneChanged;
	}

	public static bool IsAnalyticsEnabled()
	{
		float num = 0.1f;
		float num2 = (float)(SystemInfo.deviceUniqueIdentifier.GetHashCode() & 0x7FFFFFFF) / 2.1474836E+09f;
		if (!DewBuildProfile.current.disableAnalytics)
		{
			return num2 <= num;
		}
		return false;
	}

	private static async void SceneManagerOnactiveSceneChanged(Scene arg0, Scene arg1)
	{
		if (arg1.name != "Title")
		{
			return;
		}
		await UniTask.WaitForSeconds(1f, true, (PlayerLoopTiming)8, default(CancellationToken));
		if (!(ManagerBase<GlobalAnalyticsManager>.instance == null))
		{
			ManagerBase<GlobalAnalyticsManager>.instance.SendUGSStats();
			if (DewBuildProfile.current.platform != PlatformType.STEAM || !DewBuildProfile.current.useSteamLobbyAndRelay)
			{
				ManagerBase<GlobalAnalyticsManager>.instance.SendEOSStats();
			}
		}
	}

	private async void SendUGSStats()
	{
		int num;
		if (num == 0)
		{
			Awaiter val2 = default;
			Awaiter val = val2;
			val.GetResult();
			bool c_success = ManagerBase<UGSManager>.instance.status == ServiceStatus.Ready;
			InvokeEvent((Event)(object)new Event_SpecialUGSConnectivity
			{
				c_success = c_success
			});
		}
	}

	private async void SendEOSStats()
	{
		int num;
		if (num == 0)
		{
			Awaiter val2 = default;
			Awaiter val = val2;
			val.GetResult();
			bool c_success = ManagerBase<EOSManager>.instance.status == ServiceStatus.Ready;
			InvokeEvent((Event)(object)new Event_SpecialEOSConnectivity
			{
				c_success = c_success
			});
		}
	}

	private static void InvokeEvent(Event ev)
	{
		try
		{
			AnalyticsService.Instance.RecordEvent(ev);
		}
		catch (Exception message)
		{
			Debug.Log(message);
		}
	}
}
