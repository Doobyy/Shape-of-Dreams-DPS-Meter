using Mirror;
using UnityEngine;

public class Quest_SuspiciousTreasureMap : DewQuest
{
	protected override void OnStepStarted(bool isLoadedFromSave)
	{
		base.OnStepStarted(isLoadedFromSave);
		if (((NetworkBehaviour)this).isServer)
		{
			float value = Random.value;
			string text;
			if (value < 0.2f)
			{
				text = "RoomMod_Ambush";
			}
			else
			{
				text = ((!(value < 0.4f)) ? "RoomMod_SpawnHiddenStash" : "RoomMod_GoldEverywhere");
			}
			AddStepGoal_ReachNode(new NextGoalSettings
			{
				nodeIndexSettings = new GetNodeIndexSettings
				{
					desiredDistance = new Vector2Int(2, 4),
					preferCloserToExit = true,
					avoidMainModifier = true
				},
				addedModifiers = new AddedModifierData[2] { "RoomMod_SuspiciousTreasureLocation", text },
				onReachDestination = CompleteQuest,
				onFail = FailQuest
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
