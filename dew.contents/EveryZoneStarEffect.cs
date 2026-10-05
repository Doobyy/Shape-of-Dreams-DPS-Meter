using System;
using Mirror;
using UnityEngine;

public abstract class EveryZoneStarEffect : StarEffect
{
	[SaveVar(SaveVarFlags.Default)]
	private int _lastZoneIndex = -1;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			ClientEventOnZoneLoaded(default);
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoaded += new Action<EventInfoLoadZone>(ClientEventOnZoneLoaded);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoaded -= new Action<EventInfoLoadZone>(ClientEventOnZoneLoaded);
		}
	}

	private void ClientEventOnZoneLoaded(EventInfoLoadZone obj)
	{
		if (NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex == _lastZoneIndex || NetworkedManagerBase<ZoneManager>.instance.currentZone == null)
		{
			return;
		}
		_lastZoneIndex = NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex;
		Dew.CallDelayed(() =>
		{
			if (!this.IsNullOrInactive())
			{
				OnNewZoneReached();
			}
		});
	}

	public abstract void OnNewZoneReached();

	private void MirrorProcessed()
	{
	}
}
