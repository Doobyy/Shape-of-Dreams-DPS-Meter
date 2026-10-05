using Mirror;
using UnityEngine;

public class Quest_StarSeekersJournal : DewQuest
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
					desiredDistance = new Vector2Int(1, 5),
					preferCloserToExit = true,
					avoidMainModifier = true
				},
				addedModifiers = new AddedModifierData[1]
				{
					new AddedModifierData
					{
						type = "RoomMod_StardustEverywhere",
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
