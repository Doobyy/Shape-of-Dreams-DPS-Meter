using System;
using System.Collections.Generic;
using System.Reflection;
using Steamworks;
using UnityEngine;

public class DewAchievementItem : DewGameObserverWithProgress
{
	private static readonly Dictionary<Type, bool> _hasPersistentVarsByType = new Dictionary<Type, bool>();

	public virtual int grantedStardust => 50;

	private bool hasPersistentVars
	{
		get
		{
			Type type = GetType();
			if (_hasPersistentVarsByType.TryGetValue(type, out var value))
			{
				return value;
			}
			bool flag = false;
			FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			for (int i = 0; i < fields.Length; i++)
			{
				if (fields[i].GetCustomAttributes(typeof(AchPersistentVarAttribute), inherit: false).Length != 0)
				{
					flag = true;
					break;
				}
			}
			_hasPersistentVarsByType[type] = flag;
			return flag;
		}
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		if (DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth) || !DewSave.profileMain.achievements.TryGetValue(GetType().Name, out var value))
		{
			return;
		}
		try
		{
			LoadState(value.persistentVariables);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public override void OnStopLocalClient()
	{
		base.OnStopLocalClient();
		FlushProgressToProfile();
	}

	public bool FlushProgressToProfile(bool midRun = false)
	{
		if (DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
		{
			return false;
		}
		if (midRun && !hasPersistentVars)
		{
			return false;
		}
		if (!DewSave.profileMain.achievements.TryGetValue(GetType().Name, out var value))
		{
			return false;
		}
		if (value.persistentVariables == null)
		{
			value.persistentVariables = new Dictionary<string, string>();
		}
		try
		{
			int currentProgress = GetCurrentProgress();
			int maxProgress = GetMaxProgress();
			if (midRun && currentProgress < value.currentProgress)
			{
				Debug.LogWarning($"Achievement {GetType().Name} progress went backwards ({value.currentProgress} -> {currentProgress}); " + "keeping the saved value. Relax the guard in FlushProgressToProfile if this counter is meant to decrease.");
				return false;
			}
			bool result = currentProgress != value.currentProgress || maxProgress != value.maxProgress;
			value.currentProgress = currentProgress;
			value.maxProgress = maxProgress;
			if (DewSteam.isInitialized)
			{
				SteamUserStats.SetStat("STAT_" + name, value.currentProgress);
			}
			SaveState(value.persistentVariables);
			return result;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return false;
		}
	}

	public override void OnComplete()
	{
		base.OnComplete();
		ManagerBase<AchievementManager>.instance.CompleteAchievement(this);
	}
}
