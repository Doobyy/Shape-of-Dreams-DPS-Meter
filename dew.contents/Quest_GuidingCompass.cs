using Mirror;
using UnityEngine;

public class Quest_GuidingCompass : DewQuest
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
					desiredDistance = new Vector2Int(3, 5),
					preferCloserToExit = true,
					avoidMainModifier = true
				},
				addedModifiers = new AddedModifierData[1] { "RoomMod_GuidingCompass_SpawnRift" },
				onReachDestination = CompleteQuest,
				onFail = FailQuest
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
