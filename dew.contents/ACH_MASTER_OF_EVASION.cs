using System;
using UnityEngine;

[AchUnlockOnComplete(typeof(LucidDream_MadLife))]
public class ACH_MASTER_OF_EVASION : DewAchievementItem
{
	[SaveVar(SaveVarFlags.Default)]
	private bool _didFail;

	[SaveVar(SaveVarFlags.Default)]
	private bool _didStartForest;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchOnTakeDamage((EventInfoDamage dmg) =>
		{
			if (!((UnityEngine.Object)(object)dmg.actor == null))
			{
				Entity firstEntity = dmg.actor.firstEntity;
				if (!((UnityEngine.Object)(object)firstEntity == null) && firstEntity is Monster monster && !((UnityEngine.Object)(object)monster.owner != (UnityEngine.Object)(object)DewPlayer.creep))
				{
					_didFail = true;
				}
			}
		});
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoaded += new Action<EventInfoLoadZone>(ClientEventOnZoneLoaded);
		if (NetworkedManagerBase<ZoneManager>.instance.currentZone != null && NetworkedManagerBase<ZoneManager>.instance.currentZone.name == "Zone_Forest" && NetworkedManagerBase<ZoneManager>.instance.currentNode.type == WorldNodeType.Start)
		{
			_didStartForest = true;
			_didFail = false;
		}
	}

	public override void OnStopLocalClient()
	{
		base.OnStopLocalClient();
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoaded -= new Action<EventInfoLoadZone>(ClientEventOnZoneLoaded);
		}
	}

	private void ClientEventOnZoneLoaded(EventInfoLoadZone obj)
	{
		if (_didFail || !obj.isTraveling)
		{
			return;
		}
		NetworkedManagerBase<ZoneManager>.instance.CallOnReadyAfterTransition(() =>
		{
			if (_didStartForest && NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex > 0)
			{
				Complete();
			}
			else if (NetworkedManagerBase<ZoneManager>.instance.currentZone.name == "Zone_Forest")
			{
				_didStartForest = true;
				_didFail = false;
			}
		});
	}
}
