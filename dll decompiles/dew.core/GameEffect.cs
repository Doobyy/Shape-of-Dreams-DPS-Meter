using UnityEngine;

[SaveActor(true)]
public class GameEffect : Actor
{
	public GameObject startEffect;

	public GameObject loopEffect;

	public GameObject endEffect;

	public override bool isDestroyedOnRoomChange => false;

	public override bool ShouldBeSavedWithRoom()
	{
		return false;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (isNewInstance)
		{
			FxPlay(startEffect);
		}
		FxPlay(loopEffect);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		FxStop(loopEffect);
		FxPlay(endEffect);
	}

	private void MirrorProcessed()
	{
	}
}
