using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class RoomRifts : RoomComponent
{
	public List<string> openedSidetrackRifts = new List<string>();

	private List<(Vector3, Quaternion)> _sidetrackRiftPositions = new List<(Vector3, Quaternion)>();

	public override void OnRoomStartServer()
	{
		base.OnRoomStartServer();
		if (!Rift_RoomExit.instance.IsNullOrInactive())
		{
			Rift_RoomExit.instance.GetSidetrackPortalPositions(_sidetrackRiftPositions);
		}
		DewResources.PreloadAllByNameSubstring("Rift_Sidetrack_");
	}

	public void OpenRifts()
	{
		if (!Rift_RoomExit.instance.IsNullOrInactive())
		{
			Rift_RoomExit.instance.Open();
		}
		if (!Rift_Sidetrack.instance.IsNullOrInactive())
		{
			Rift_Sidetrack.instance.Open();
		}
		List<Rift_Sidetrack> wantsToSpawn = new List<Rift_Sidetrack>();
		if (NetworkedManagerBase<ZoneManager>.instance.currentNode.type == WorldNodeType.Combat)
		{
			Rift_Sidetrack[] array = DewResources.FindAllByNameSubstring<Rift_Sidetrack>("Rift_Sidetrack_", default(ResourceLoadSettings)).ToArray();
			foreach (Rift_Sidetrack rift_Sidetrack in array)
			{
				if (rift_Sidetrack.isValid && (openedSidetrackRifts.Contains(((UnityEngine.Object)(object)rift_Sidetrack).name) || (!(room.GetRoomRandom(-9132).Value() >= rift_Sidetrack.spawnChance) && !NetworkedManagerBase<ZoneManager>.instance.bannedSidetracksForCurrentLoop.Contains(((UnityEngine.Object)(object)rift_Sidetrack).name) && (rift_Sidetrack.allowedZones == null || rift_Sidetrack.allowedZones.Length == 0 || rift_Sidetrack.allowedZones.Contains(NetworkedManagerBase<ZoneManager>.instance.currentZone.name)))))
				{
					wantsToSpawn.Add(rift_Sidetrack);
				}
			}
		}
		int num = DewBuildProfile.current.content.zoneCountByTier.Sum();
		if (!DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth) && NetworkedManagerBase<ZoneManager>.instance.currentNode.type == WorldNodeType.ExitBoss && NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex % num == num - 1)
		{
			wantsToSpawn.Add(DewResources.GetByName<Rift_Sidetrack>("Rift_Sidetrack_TheDream", default(ResourceLoadSettings)));
		}
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(1.8f);
			for (int j = 0; j < wantsToSpawn.Count; j++)
			{
				Rift_Sidetrack source = wantsToSpawn[j];
				CreateSidetrackRift(source);
				yield return new WaitForSeconds(0.35f);
			}
		}
	}

	[Server]
	public void CreateSidetrackRift(Rift_Sidetrack source)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void RoomRifts::CreateSidetrackRift(Rift_Sidetrack)' called when server was not active");
			return;
		}
		if (_sidetrackRiftPositions.Count == 0)
		{
			Debug.LogWarning("No space to create " + ((UnityEngine.Object)(object)source).name + " in " + ((UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.instance).name);
			return;
		}
		(Vector3, Quaternion) tuple = _sidetrackRiftPositions[0];
		_sidetrackRiftPositions.RemoveAt(0);
		if (source.oncePerLoop)
		{
			NetworkedManagerBase<ZoneManager>.instance.bannedSidetracksForCurrentLoop.Add(((UnityEngine.Object)(object)source).name);
		}
		Debug.Log("Creating " + ((UnityEngine.Object)(object)source).name + " in " + ((UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.instance).name);
		Rift_Sidetrack rift_Sidetrack = Dew.InstantiateAndSpawn<Rift_Sidetrack>(DewResources.GetByName<Rift_Sidetrack>(((UnityEngine.Object)(object)source).name, default(ResourceLoadSettings)), tuple.Item1, (Quaternion?)tuple.Item2, (Action<Rift_Sidetrack>)null);
		if (!Rift_RoomExit.instance.IsNullOrInactive() && Rift_RoomExit.instance.isLocked)
		{
			rift_Sidetrack.isLocked = true;
		}
		rift_Sidetrack.Open();
	}

	private void MirrorProcessed()
	{
	}
}
