using System;
using UnityEngine;

public abstract class DewSpecialReverieItem : DewReverieItem
{
	public virtual Type nextReverie { get; }

	public override void OnStartLocalClient()
	{
		try
		{
			LoadState(DewSave.profileMain.specialReverie.persistentVariables);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public override void OnStopLocalClient()
	{
		try
		{
			SaveReverieStateToData(DewSave.profileMain.specialReverie);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}
}
