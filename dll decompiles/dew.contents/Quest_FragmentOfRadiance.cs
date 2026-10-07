using Mirror;
using UnityEngine;

public class Quest_FragmentOfRadiance : DewQuest
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
					desiredDistance = new Vector2Int(2, 4),
					preferCloserToExit = true,
					avoidMainModifier = true
				},
				addedModifiers = new AddedModifierData[1] { "RoomMod_FragmentOfRadiance_MysteriousPlace" },
				roomOverride = "Room_Special_FragmentOfRadiance_MysteriousPlace",
				onReachDestination = CompleteQuest,
				onFail = FailQuest
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
