using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Build Profile", menuName = "Build Profile")]
public class DewBuildProfile : ScriptableObject
{
	public static string CurrentBuildProfileAssetPath = "Assets/Resources/CurrentBuild.asset";

	public static string EditorBuildProfileDefault = "Assets/Res/BuildProfiles/Profile - Release.asset";

	public BuildType buildType;

	public PlatformType platform;

	public DewGameContentSettings content;

	public int defaultStardustAmount;

	public float killGoldMultiplier = 1f;

	public float dismantleDreamDustMultiplier = 1f;

	public float stardustGainMultiplier = 1f;

	public int worldNodeCountOffset;

	public RoomRewardFlowItemType[] customRewardFlowFirstTime;

	public RoomRewardFlowItemType[] customRewardFlow;

	public float bonusMemoryHaste;

	public int startGold;

	public int startDreamDust;

	public int luckShrineMinZoneIndex;

	public float luckShrineInPoolChance;

	public string savePrefix;

	public string codenameSuffix;

	public BuildFeatureTag[] featureTags = new BuildFeatureTag[0];

	[FormerlySerializedAs("isDevelopmentBuild")]
	public DevelopmentBuildMode developmentMode;

	public bool includeCheatCommands;

	public bool disableAnalytics;

	public bool useSteamLobbyAndRelay;

	public static DewBuildProfile current
	{
		get
		{
			DewBuildProfile dewBuildProfile = Resources.Load<DewBuildProfile>("CurrentBuild");
			if (dewBuildProfile.content.zoneCountByTier == null || dewBuildProfile.content.zoneCountByTier.Count == 0)
			{
				dewBuildProfile.Init();
			}
			return dewBuildProfile;
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void PrepareBuildProfile()
	{
		current.Init();
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void LogCurrentBuildProfile()
	{
		Debug.Log("Using Build Profile: " + current.name);
	}

	public string GetBundleVersion(string codename)
	{
		if (!string.IsNullOrEmpty(codenameSuffix))
		{
			codename += codenameSuffix;
		}
		DateTime now = DateTime.Now;
		string text = buildType.ToString().ToLower().Substring(0, 1);
		string text2 = platform.ToString().ToLower().Substring(0, 1);
		if (string.IsNullOrEmpty(codename))
		{
			return $"{text}.{now:yyMMddHH}_{text2}";
		}
		return text + "." + codename + "_" + text2;
	}

	public string GetFolderName(string codename)
	{
		if (!string.IsNullOrEmpty(codenameSuffix))
		{
			codename += codenameSuffix;
		}
		DateTime now = DateTime.Now;
		string text = buildType switch
		{
			BuildType.Indev => "i", 
			BuildType.DemoLite => "dl", 
			BuildType.DemoPrivate => "dp", 
			BuildType.Release => "r", 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		string text2 = platform.ToString().ToLower();
		if (string.IsNullOrEmpty(codename))
		{
			return $"ShapeOfDreams-{text}-{now:yyMMddHH}_{text2}";
		}
		return $"ShapeOfDreams-{text}-{now:yyMMddHH}-{codename}_{text2}";
	}

	public string[] GetExtraScriptingDefines()
	{
		List<string> list = new List<string>();
		switch (buildType)
		{
		case BuildType.Indev:
			list.Add("DEW_INDEV");
			break;
		case BuildType.DemoLite:
			list.Add("DEW_DEMOLITE");
			break;
		case BuildType.DemoPrivate:
			list.Add("DEW_DEMOPRIVATE");
			break;
		case BuildType.Release:
			list.Add("DEW_RELEASE");
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		switch (platform)
		{
		case PlatformType.DRMFREE:
			list.Add("DEW_DRMFREE");
			break;
		case PlatformType.STEAM:
			list.Add("DEW_STEAM");
			break;
		case PlatformType.STOVE:
			list.Add("DEW_STOVE");
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		if (includeCheatCommands || featureTags.Contains(BuildFeatureTag.Booth))
		{
			list.Add("DEW_CONSOLE_CHEATS");
		}
		return list.ToArray();
	}

	public bool HasFeature(BuildFeatureTag tag)
	{
		if (featureTags == null)
		{
			return false;
		}
		return featureTags.Contains(tag);
	}

	public void Validate()
	{
		if (!Application.isPlaying)
		{
			if (content == null)
			{
				throw new Exception("Game content not set");
			}
			content.Validate();
		}
	}

	private void PrintValidationResult()
	{
		try
		{
			Validate();
			Debug.Log("No problem has been found in profile: " + name);
		}
		catch (Exception exception)
		{
			Debug.LogError("Problem has been found in profile: " + name);
			Debug.LogException(exception);
		}
	}

	private void OnValidate()
	{
		Validate();
	}

	public void Init()
	{
		content.Init();
	}
}
