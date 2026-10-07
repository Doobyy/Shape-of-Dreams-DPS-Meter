using System;
using UnityEngine;

public class NextGoalSettings
{
	public string localizedTitleKey;

	public string localizedDescriptionKey;

	public GetNodeIndexSettings nodeIndexSettings;

	public AddedModifierData[] addedModifiers;

	public string roomOverride;

	public bool dontChangeTitle;

	public bool dontChangeDescription;

	public bool ignoreSuboptimalSituation;

	[NonSerialized]
	public Action onReachDestination;

	[NonSerialized]
	public Action<FailReason> onFail;

	internal void InvokeOnFail(FailReason failReason)
	{
		try
		{
			onFail?.Invoke(failReason);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	internal void InvokeOnReachDestination()
	{
		try
		{
			onReachDestination?.Invoke();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}
}
