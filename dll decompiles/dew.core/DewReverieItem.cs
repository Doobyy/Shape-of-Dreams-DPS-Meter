using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class DewReverieItem : DewGameObserverWithProgress, IExcludeFromPool
{
	public const int ReverieSlotCount = 3;

	public int index;

	bool IExcludeFromPool.excludeFromPool => excludeFromPool;

	public abstract int grantedStardust { get; }

	public virtual string[] grantedItems { get; }

	public virtual bool excludeFromPool => false;

	public virtual string reverieListButtonText => null;

	public virtual void OnReverieListButtonClick()
	{
	}

	public virtual void OnSetupReverie()
	{
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		try
		{
			LoadState(DewSave.profileMain.reverieSlots[index].persistentVariables);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public override void OnStopLocalClient()
	{
		base.OnStopLocalClient();
		try
		{
			SaveReverieStateToData(DewSave.profileMain.reverieSlots[index]);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public void SaveReverieStateToData(DewProfile.ReverieDataBase data)
	{
		try
		{
			if (data.persistentVariables == null)
			{
				data.persistentVariables = new Dictionary<string, string>();
			}
			data.currentProgress = GetCurrentProgress();
			data.maxProgress = GetMaxProgress();
			SaveState(data.persistentVariables);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public override void OnComplete()
	{
		base.OnComplete();
		if (ManagerBase<InGameReverieManager>.instance != null)
		{
			ManagerBase<InGameReverieManager>.instance.CompleteReverie(this);
			return;
		}
		DewProfile.DailyReverieData dailyReverieData = DewSave.profileMain.reverieSlots[index];
		if (dailyReverieData.isComplete)
		{
			Debug.Log(name + " already complete");
			return;
		}
		Debug.Log(name + " completed");
		dailyReverieData.isComplete = true;
		dailyReverieData.maxProgress = GetMaxProgress();
		dailyReverieData.currentProgress = dailyReverieData.maxProgress;
		dailyReverieData.persistentVariables = null;
		try
		{
			OnStopLocalClient();
		}
		catch (Exception exception)
		{
			Debug.LogError("Exception occured while stopping reverie: " + name);
			Debug.LogException(exception);
		}
		DewSave.SaveProfileMain();
	}
}
