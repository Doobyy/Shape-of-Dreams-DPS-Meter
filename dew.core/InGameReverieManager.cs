using System;
using System.Collections.Generic;
using UnityEngine;

public class InGameReverieManager : ManagerBase<InGameReverieManager>
{
	public const int RerollMaxCount = 3;

	public const int SpecialReverieIndex = -99;

	private List<DewReverieItem> _trackedReveries = new List<DewReverieItem>();

	public bool disableTrackingReveries;

	public bool isTrackingReveries { get; private set; }

	private void Start()
	{
		if (disableTrackingReveries)
		{
			return;
		}
		NetworkedManagerBase<GameResultManager>.instance.ClientEvent_OnGameConcluded += (Action<DewGameResult>)((DewGameResult obj) =>
		{
			if (isTrackingReveries)
			{
				for (int num = _trackedReveries.Count - 1; num >= 0; num--)
				{
					_trackedReveries[num].FeedGameResult(obj);
				}
				StopTrackingReveries();
			}
		});
		DewNetworkManager.instance.ClientEvent_OnSessionEnd += (Action)(() =>
		{
			if (isTrackingReveries)
			{
				StopTrackingReveries();
			}
		});
		GameManager.CallOnReady(() =>
		{
			StartTrackingReveries();
			DewPlayer.local.ClientEvent_OnHeroChanged += (Action<Hero, Hero>)((Hero _, Hero to) =>
			{
				if (isTrackingReveries)
				{
					StopTrackingReveries();
					StartTrackingReveries();
				}
			});
		});
	}

	private void OnDestroy()
	{
		if (isTrackingReveries)
		{
			StopTrackingReveries();
		}
	}

	internal void StartTrackingReveries()
	{
		if (isTrackingReveries)
		{
			return;
		}
		isTrackingReveries = true;
		if ((UnityEngine.Object)(object)DewPlayer.local == null || (UnityEngine.Object)(object)DewPlayer.local.hero == null)
		{
			return;
		}
		for (int i = 0; i < DewSave.profileMain.reverieSlots.Count; i++)
		{
			try
			{
				DewProfile.DailyReverieData dailyReverieData = DewSave.profileMain.reverieSlots[i];
				if (!dailyReverieData.IsEmpty() && !dailyReverieData.isComplete)
				{
					DewReverieItem dewReverieItem = (DewReverieItem)Activator.CreateInstance(Dew.reveriesByName[dailyReverieData.type].GetType());
					dewReverieItem.index = i;
					_trackedReveries.Add(dewReverieItem);
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		try
		{
			if (!DewSave.profileMain.specialReverie.IsEmpty() && !DewSave.profileMain.specialReverie.isComplete)
			{
				DewSpecialReverieItem dewSpecialReverieItem = (DewSpecialReverieItem)Activator.CreateInstance(Dew.reveriesByName[DewSave.profileMain.specialReverie.type].GetType());
				dewSpecialReverieItem.index = -99;
				_trackedReveries.Add(dewSpecialReverieItem);
			}
		}
		catch (Exception exception2)
		{
			Debug.LogException(exception2);
		}
		for (int num = _trackedReveries.Count - 1; num >= 0; num--)
		{
			DewReverieItem dewReverieItem2 = _trackedReveries[num];
			try
			{
				dewReverieItem2.OnStartLocalClient();
			}
			catch (Exception exception3)
			{
				Debug.LogError("Exception occured while starting reverie: " + dewReverieItem2.name);
				Debug.LogException(exception3, this);
				_trackedReveries.RemoveAt(num);
			}
		}
		Debug.Log($"Started tracking {_trackedReveries.Count} reveries");
	}

	internal void StopTrackingReveries()
	{
		if (!isTrackingReveries)
		{
			return;
		}
		isTrackingReveries = false;
		for (int num = _trackedReveries.Count - 1; num >= 0; num--)
		{
			DewReverieItem dewReverieItem = _trackedReveries[num];
			try
			{
				dewReverieItem.OnStopLocalClient();
			}
			catch (Exception exception)
			{
				Debug.LogError("Exception occured while stopping reverie: " + dewReverieItem.name);
				Debug.LogException(exception, this);
			}
		}
		Debug.Log($"Stopped tracking {_trackedReveries.Count} reveries");
		_trackedReveries.Clear();
	}

	public void CompleteReverie(DewReverieItem item)
	{
		if (item == null)
		{
			return;
		}
		DewProfile.ReverieDataBase reverieDataBase = ((item.index == -99) ? ((DewProfile.ReverieDataBase)DewSave.profileMain.specialReverie) : ((DewProfile.ReverieDataBase)DewSave.profileMain.reverieSlots[item.index]));
		if (reverieDataBase.isComplete)
		{
			Debug.Log(item.name + " already complete");
			return;
		}
		Debug.Log(item.name + " completed");
		reverieDataBase.isComplete = true;
		reverieDataBase.maxProgress = item.GetMaxProgress();
		reverieDataBase.currentProgress = reverieDataBase.maxProgress;
		reverieDataBase.persistentVariables = null;
		try
		{
			item.OnStopLocalClient();
		}
		catch (Exception exception)
		{
			Debug.LogError("Exception occured while stopping reverie: " + item.name);
			Debug.LogException(exception, this);
		}
		DewSave.SaveProfileMain();
		_trackedReveries.Remove(item);
	}
}
