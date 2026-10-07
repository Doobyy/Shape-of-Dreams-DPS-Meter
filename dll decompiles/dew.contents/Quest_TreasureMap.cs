using Mirror;
using UnityEngine;

public class Quest_TreasureMap : DewQuest
{
	protected override void OnStepStarted(bool isLoadedFromSave)
	{
		base.OnStepStarted(isLoadedFromSave);
		if (((NetworkBehaviour)this).isServer)
		{
			AddStepGoal_ReachNode(new NextGoalSettings
			{
				nodeIndexSettings = new GetNodeIndexSettings
				{
					desiredDistance = new Vector2Int(3, 4),
					preferCloserToExit = true,
					avoidMainModifier = false
				},
				addedModifiers = new AddedModifierData[1]
				{
					new AddedModifierData
					{
						type = "RoomMod_SpawnHiddenStash",
						isForceRevealed = true
					}
				},
				onReachDestination = CompleteQuest,
				onFail = FailQuest
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
