using Mirror;
using UnityEngine;

public class Quest_CallOfTheRavenous : DewQuest
{
	private Ge_CallOfTheRavenous _ge;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_ge = Dew.FindActorOfType<Ge_CallOfTheRavenous>();
		}
	}

	protected override void OnStepStarted(bool isLoadedFromSave)
	{
		base.OnStepStarted(isLoadedFromSave);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		AddStepGoal_ReachNode(new NextGoalSettings
		{
			nodeIndexSettings = new GetNodeIndexSettings
			{
				desiredDistance = new Vector2Int(3, 5),
				preferCloserToExit = true,
				avoidMainModifier = true
			},
			addedModifiers = new AddedModifierData[1] { "RoomMod_CallOfTheRavenous" },
			onReachDestination = CompleteQuest,
			onFail = (FailReason fail) =>
			{
				if (_ge.IsNullOrInactive())
				{
					_ge = Dew.FindActorOfType<Ge_CallOfTheRavenous>();
				}
				if (!_ge.IsNullOrInactive())
				{
					_ge.Destroy();
				}
				FailQuest(fail);
			}
		});
	}

	private void MirrorProcessed()
	{
	}
}
